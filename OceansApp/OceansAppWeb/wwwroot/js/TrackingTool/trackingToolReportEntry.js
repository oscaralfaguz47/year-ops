let quantityInput = document.getElementById('quantityInput');
let notesInput = document.getElementById('notesInput');
let movementIdNormalHoursInput = document.getElementById('movementIdNormalHoursInput');
let movementIdOnCallFlateRateInput = document.getElementById('movementIdOnCallFlateRateInput');
let movementIdOnCallTimeWorkedInput = document.getElementById('movementIdOnCallTimeWorkedInput');
let onCallFlateRateSelect = document.getElementById('onCallFlateRateSelect');
let onCallTimeWorkedInput = document.getElementById('onCallTimeWorkedInput');
let projectIdInput = document.getElementById('projectId');

const dropArea = document.querySelector('.file-upload-wrapper');
const fileList = [];
const maxFileSize = 10 * 1024 * 1024; // 10 MB

// Highlight drop zone when dragging files
dropArea.addEventListener('dragover', (event) => {
    event.stopPropagation();
    event.preventDefault();
    dropArea.classList.add('dragover');
});

// Revert highlighting when files are no longer dragged over the area
dropArea.addEventListener('dragleave', (event) => {
    dropArea.classList.remove('dragover');
});

// Handle drop event and file selection change
dropArea.addEventListener('drop', handleFiles);
document.getElementById('file-upload').addEventListener('change', handleFiles);
document.getElementById('file-upload').setAttribute('accept',
    '.pdf, .jpg, .jpeg, .png, .gif, .svg, .doc, .docx, .xls, .xlsx, .csv, .txt');

// Handle file paste from clipboard
document.addEventListener('paste', (event) => {
    const items = event.clipboardData || event.originalEvent.clipboardData;
    if (items) {
        let files = [];
        if (items.files && items.files.length) {
            files = Array.from(items.files);
        } else if (items.items) {
            files = Array.from(items.items)
                .filter(item => item.kind === 'file')
                .map(item => item.getAsFile());
        }

        if (files.length > 0) {
            processFiles(files);
        }
    }
});


async function handleFiles(event) {
    event.stopPropagation();
    event.preventDefault();
    dropArea.classList.remove('dragover');
    const newFiles = event.dataTransfer ? event.dataTransfer.files : event.target.files;
    for (const file of newFiles) {
        if (isValidFileType(file) && isValidFileSize(file) && !isDuplicate(file)) {
            fileList.push(file);
            updateFileDisplay(file);
        }
    }
    updateInfoText();
}

function processFiles(newFiles) {
    newFiles.forEach(file => {
        if (isValidFileType(file) && isValidFileSize(file) && !isDuplicate(file)) {
            fileList.push(file);
            updateFileDisplay(file);
        }
    });
    updateInfoText();
}

function isValidFileType(file) {
    const fileExtension = file.name.split('.').pop().toLowerCase();
    const validExtensions = ['pdf', 'jpg', 'jpeg', 'png', 'gif', 'svg', 'doc', 'docx', 'xls', 'xlsx', 'csv', 'txt'];
    return validExtensions.includes(fileExtension);
}

function isValidFileSize(file) {
    return file.size <= maxFileSize;
}

function isDuplicate(file) {
    return fileList.some(f => f.name === file.name && f.size === file.size);
}

function updateInfoText() {
    const infoText = document.getElementById('info-text');
    infoText.style.display = fileList.length > 0 ? 'none' : 'block';
}

let isCreatingMovement = false;
let movementCreationPromise = null;

function updateFileDisplay(file) {
    const uploadArea = document.getElementById('file-upload-name');
    const fileElement = document.createElement('div');
    fileElement.className = 'row-selected-file';

    const deleteBtn = document.createElement('button');
    deleteBtn.className = 'delete-btn';
    deleteBtn.innerHTML = '<i class="fa-solid fa-trash-can"></i>';
    deleteBtn.onclick = function () {
        fileList.splice(fileList.indexOf(file), 1);
        fileElement.remove();
        updateInfoText();
    };

    const fileName = document.createElement('span');
    fileName.textContent = file.name;

    const statusLabel = document.createElement('span');
    statusLabel.textContent = '';
    statusLabel.className = 'span-status';
    fileElement.appendChild(deleteBtn);
    fileElement.appendChild(fileName);
    fileElement.appendChild(statusLabel);
    uploadArea.appendChild(fileElement);

    // Ensure movementIdNormalHoursInput is actually null and no creation is currently in progress
    if (!movementIdNormalHoursInput.value && !isCreatingMovement) {
        isCreatingMovement = true;
        movementCreationPromise = createFirstMovementIfDoesNotExist()
            .then(data => {
                if (data && data.createdMovementId !== undefined) {
                    movementIdNormalHoursInput.value = data.createdMovementId;
                    displayToasterSuccess('Movement created successfully');
                } else {
                    throw new Error('Invalid response data');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                statusLabel.textContent = 'Movement creation failed';
                displayToasterError(error.message);
            })
            .finally(() => {
                isCreatingMovement = false;
            });
    }


    if (movementCreationPromise) {
        movementCreationPromise.then(() => {
            uploadFile(file, statusLabel);
        });
    } else {
        uploadFile(file, statusLabel);
    }
}

async function uploadFile(file, statusLabel) {
    var token = $('[name="__RequestVerificationToken"]').val();
    const formData = new FormData();
    formData.append('files', file);
    formData.append('movementId', movementIdNormalHoursInput.value);
    statusLabel.innerHTML = '<div class="spinner loading-file-spinner"></div>';

    try {
        const response = await fetch('/TrackingTool/ReportingMyTime/UploadFilesClientNoTrackingTool', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'RequestVerificationToken': token
            },
            body: formData
        });
        if (!response.ok) throw new Error('Upload failed');
        const data = await response.json();
        statusLabel.innerHTML = '<i class="fa-solid fa-check uploaded-check-icon green-label"></i>';
        displayToasterSuccess(data.message);
    } catch (error) {
        console.error('Error:', error);
        statusLabel.textContent = 'Upload failed';
        displayToasterError(error.message);
    }
}

async function createFirstMovementIfDoesNotExist() {
    var token = $('[name="__RequestVerificationToken"]').val();
    let actionDateData = new Date(dateFromInput.value).toISOString();
    const formData = new FormData();
    formData.append('uploadFilesData.ProjectId', projectIdInput.value);
    formData.append('uploadFilesData.MovementId', movementIdNormalHoursInput.value);
    formData.append('uploadFilesData.ActionDate', actionDateData);

    const response = await fetch('/TrackingTool/ReportingMyTime/CreateMovementClientNoTrackingTool', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'RequestVerificationToken': token
        },
        body: formData
    });
    if (!response.ok) {
        throw new Error('Creation failed');
    }
    const data = await response.json();
    if (!data.success) {
        throw new Error(data.message || 'Unknown error');
    }
    return data;
}






//CREATE, UPDATE TIME ENTRY MOVEMENT
async function createUpdateTimeEntry() {
    var token = $('[name="__RequestVerificationToken"]').val();
    let actionDateData = new Date(dateFromInput.value).toISOString();
    const formData = new FormData();
    function appendIfValid(key, value) {
        if (value) {
            formData.append(key, value);
        }
    }

    let dataItems = [];

    let normalHoursData = {
        MovementId: movementIdNormalHoursInput.value,
        ProjectId: projectIdInput.value,
        Quantity: quantityInput.value,
        ActionDate: actionDateData,
        Notes: notesInput.value,
        MovementType: 'Normal Hours'
    };
    let onCallFlateRateData = {
        MovementId: movementIdOnCallFlateRateInput.value,
        ProjectId: projectIdInput.value,
        Quantity: onCallFlateRateSelect.value,
        ActionDate: actionDateData,
        Notes: null,
        MovementType: 'On Call Flate Rate'
    };
    let onCallTimeWorkedData = {
        MovementId: movementIdOnCallTimeWorkedInput.value,
        ProjectId: projectIdInput.value,
        Quantity: onCallTimeWorkedInput.value,
        ActionDate: actionDateData,
        Notes: null,
        MovementType: 'On Call Time Worked'
    };

    dataItems.push(normalHoursData);
    dataItems.push(onCallFlateRateData);
    dataItems.push(onCallTimeWorkedData);


    dataItems.forEach((item, index) => {
        Object.keys(item).forEach(key => {
            appendIfValid(`reportMovementListData[${index}].${key}`, item[key]);
        });
    });

    fetch('/TrackingTool/ReportingMyTime/CreateUpdateTimeEntryClientNoTrackingTool', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            RequestVerificationToken: token
        },
        body: formData
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw response;
            }
        })
        .then(data => {
            console.log('Success:', data);
            data.createdMovementList.forEach(function (el, index) {
                console.log("ELEMENT: " + el.elementType);
                if (el.elementType === 'Normal Hours') {
                    console.log("YES!!");
                    movementIdNormalHoursInput.value = el.idElement;
                }
                if (el.elementType === 'On Call Flate Rate') {
                    movementIdOnCallFlateRateInput.value = el.idElement;
                }
                if (el.elementType === 'On Call Time Worked') {
                    movementIdOnCallTimeWorkedInput.value = el.idElement;
                }
            });
            displayToasterSuccess(data.message);
            // Successful response management
        })
        .catch(errorResponse => {
            if (errorResponse.status === 400) {
                errorResponse.json().then(body => {
                    if (body.errors) {
                        for (const field in body.errors) {
                            console.error(`${field}: ${body.errors[field]}`);
                            // Puedes aquí agregar lógica para mostrar errores en campos específicos del formulario
                        }
                    } else if (body.error) {
                        // Handle other types of 400 errors
                        console.error("Error:", body.error);
                        displayToasterError(body.error);
                    }
                });
            } else {
                console.error('Something went wrong with the request.');
            }
        });
}


// INPUT VALIDATIONS
document.getElementById('notesInput').addEventListener('input', function (e) {
    if (this.value.length > 200) {
        this.value = this.value.slice(0, 200);
    }
});
document.addEventListener("DOMContentLoaded", function () {
    validateInputTypeNumber('onCallTimeWorkedInput');
    validateInputTypeNumber('quantityInput');
});
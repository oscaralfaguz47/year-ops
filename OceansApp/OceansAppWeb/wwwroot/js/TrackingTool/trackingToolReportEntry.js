let quantityInput = document.getElementById('quantityInput');
let notesInput = document.getElementById('notesInput');
let movementIdNormalHoursInput = document.getElementById('movementIdNormalHoursInput');
let movementIdOnCallFlateRateInput = document.getElementById('movementIdOnCallFlateRateInput');
let movementIdOnCallTimeWorkedInput = document.getElementById('movementIdOnCallTimeWorkedInput');
let onCallFlateRateSelect = document.getElementById('onCallFlateRateSelect');
let onCallTimeWorkedInput = document.getElementById('onCallTimeWorkedInput');
let projectIdInput = document.getElementById('projectId');
const uploadArea = document.getElementById('file-upload-name');

const dropArea = document.querySelector('.file-upload-wrapper');
let fileList = [];
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
            updateFileDisplay(file, true, null);
        }
    }
    updateInfoText();
}

function processFiles(newFiles) {
    newFiles.forEach(file => {
        if (isValidFileType(file) && isValidFileSize(file) && !isDuplicate(file)) {
            fileList.push(file);
            updateFileDisplay(file, true, null);
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
    infoText.style.display = uploadArea.textContent.trim() === '' && uploadArea.childNodes.length === 0 ? 'block' : 'none';
}

let isCreatingMovement = false;
let movementCreationPromise = null;

function updateFileDisplay(file, isUploading, fileNameFromDb) {
    const fileElement = document.createElement('div');
    fileElement.className = 'row-selected-file';

    const deleteBtn = document.createElement('button');
    deleteBtn.className = 'delete-btn';
    deleteBtn.innerHTML = '<i class="fa-solid fa-trash-can"></i>';
    if (isUploading) {
        deleteBtn.onclick = function () {
            fileList.splice(fileList.indexOf(file), 1);
            fileElement.remove();
            updateInfoText();
        };
    } else {
        deleteBtn.onclick = async function () {
            await deleteFile(fileNameFromDb);
            fileElement.remove();
            updateInfoText();
        };
    }

    const fileName = document.createElement('span');
    if (isUploading) {
        fileName.textContent = file.name;
    } else {
        fileName.textContent = cleanFileName(fileNameFromDb);
    }


    const statusLabel = document.createElement('span');
    statusLabel.textContent = '';
    statusLabel.className = 'span-status';
    fileElement.appendChild(deleteBtn);
    fileElement.appendChild(fileName);
    fileElement.appendChild(statusLabel);
    uploadArea.appendChild(fileElement);

    // Ensure movementIdNormalHoursInput is actually null and no creation is currently in progress
    if (isUploading) {
        if (!movementIdNormalHoursInput.value && !isCreatingMovement) {
            isCreatingMovement = true;
            movementCreationPromise = createFirstMovementIfDoesNotExist()
                .then(data => {
                    if (data && data.createdMovementId !== undefined) {
                        movementIdNormalHoursInput.value = data.createdMovementId;
                    } else {
                        throw new Error('Invalid response data');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
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
    let startActionDateData = new Date(dateFromInput.value).toISOString();
    let actionDateData = new Date(dateToInput.value).toISOString();
    const formData = new FormData();
    formData.append('uploadFilesData.ProjectId', projectIdInput.value);
    formData.append('uploadFilesData.MovementId', movementIdNormalHoursInput.value);
    formData.append('uploadFilesData.StartActionDate', startActionDateData);
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
    let actionDateData = new Date(dateToInput.value).toISOString();
    let startActionDateData = new Date(dateFromInput.value).toISOString();
    const formData = new FormData();
    function appendIfValid(key, value) {
        if (value) {
            formData.append(key, value);
        }
    }

    let dataItems = [];

    let normalHoursData = {
        ProjectId: projectIdInput.value,
        Quantity: quantityInput.value,
        StartActionDate: startActionDateData,
        ActionDate: actionDateData,
        Notes: notesInput.value,
        MovementType: 'Normal Hours'
    };
    let onCallFlateRateData = {
        ProjectId: projectIdInput.value,
        Quantity: onCallFlateRateSelect.value,
        StartActionDate: startActionDateData,
        ActionDate: actionDateData,
        Notes: null,
        MovementType: 'On Call Flate Rate'
    };
    let onCallTimeWorkedData = {
        ProjectId: projectIdInput.value,
        Quantity: onCallTimeWorkedInput.value,
        StartActionDate: startActionDateData,
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
            movementIdNormalHoursInput.value = data.movementIdNormalHours;
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

//GET PROJECT MOVEMENTS
async function getProjectMovementsClientHasTrackTool() {
    fileList = [];
    uploadArea.innerHTML = '';
    loadingBoxIntern.style.display = 'block';
    errorMessageIntern.style.display = 'none';
    let noTackingToolSection = document.getElementById('no-tracking-tool-sec');
    noTackingToolSection.style.display = 'none';
    var startDateValue = encodeURIComponent(dateFromInput.value);
    var endDateValue = encodeURIComponent(dateToInput.value);
    var url = "/TrackingTool/ReportingMyTime/GetProjectMovements?projectId=" + encodeURIComponent(projectIdInput.value) +
        "&startDate=" + startDateValue + "&endDate=" + endDateValue;

    fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    errorMessageIntern.style.display = 'block';
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .then(data => {
            console.log(data);
            let normalHoursQuantity = 0;
            let onCallFlateRateQuantity = 0;
            let onCallTimeWorkedQuantity = 0;
            let notes = '';
            let blobNames = [];
            if (data.movementsList.length > 0) {
                movementIdNormalHoursInput.value = data.movementsList[0].movementId;
            } else {
                movementIdNormalHoursInput.value = null;
            }
            data.movementsList.forEach(function (obj) {
                if (obj.movementTypeName == 'Normal Hours') {
                    notes += obj.notes === null ? '' : obj.notes;
                    normalHoursQuantity += obj.quantity;
                    JSON.parse(obj.blobNames).forEach(function (blobName) {
                        blobNames.push(blobName);
                    });
                } if (obj.movementTypeName == 'On Call Flate Rate') {
                    onCallFlateRateQuantity += obj.quantity;
                }
                if (obj.movementTypeName == 'On Call Time Worked') {
                    onCallTimeWorkedQuantity += obj.quantity;
                }
            });
            quantityInput.value = normalHoursQuantity;
            onCallFlateRateSelect.value = onCallFlateRateQuantity;
            onCallTimeWorkedInput.value = onCallTimeWorkedQuantity;
            notesInput.value = notes;
            blobNames.forEach(function (objName) {
                updateFileDisplay(null, false, objName);
            });
            updateInfoText();
            noTackingToolSection.style.display = 'block';
        }).finally(() => {
            loadingBoxIntern.style.display = 'none';
        });
}
function cleanFileName(fileName) {
    const regex = /^[a-f0-9]+_\d+_/i;
    return fileName.replace(regex, '');
}
// DELETE FILE
async function deleteFile(fileName) {
    if (!fileName) {
        console.error('File name must be provided.');
        return;
    }

    const token = $('[name="__RequestVerificationToken"]').val();
    const formData = new FormData();
    formData.append('fileName', fileName);

    try {
        const response = await fetch("/TrackingTool/ReportingMyTime/DeleteBlob", {
            method: 'POST',
            headers: {
                RequestVerificationToken: token
            },
            body: formData
        });

        const data = await response.json();
        if (response.ok) {
            toastr.success(data.message);
        } else {
            displayToasterError(data.error || 'Failed to delete the file.');
            console.error('There has been a problem with the fetch operation:', data.detail);
        }
    } catch (error) {
        console.error('Network error:', error);
        displayToasterError('Network error occurred. Please try again.');
    }
}

// INPUT VALIDATIONS
document.getElementById('notesInput').addEventListener('input', function (e) {
    if (this.value.length > 400) {
        this.value = this.value.slice(0, 400);
    }
});
document.addEventListener("DOMContentLoaded", function () {
    validateInputTypeNumber('onCallTimeWorkedInput');
    validateInputTypeNumber('quantityInput');
});
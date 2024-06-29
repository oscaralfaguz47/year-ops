let quantityInput = document.getElementById('quantityInput');
let notesInput = document.getElementById('notesInput');
let movementIdNormalHoursInput = document.getElementById('movementIdNormalHoursInput');
let movementIdOnCallFlateRateInput = document.getElementById('movementIdOnCallFlateRateInput');
let movementIdOnCallTimeWorkedInput = document.getElementById('movementIdOnCallTimeWorkedInput');
let onCallFlateRateSelect = document.getElementById('onCallFlateRateSelect');
let onCallTimeWorkedInput = document.getElementById('onCallTimeWorkedInput');
let projectIdInput = document.getElementById('projectId');
const uploadArea = document.getElementById('file-upload-name');
var fileInput = document.getElementById('file-upload');
let saveReportBtn = document.getElementById('save-btn');
let transactionStatus = 'No actions';
let uploadBtn = document.getElementById('upload-btn');

function handleChangeData() {
    saveReportBtn.style.display = 'block';
}

const dropArea = document.querySelector('.file-upload-wrapper');
let fileList = [];
const maxFileSize = 10 * 1024 * 1024; // 10 MB

// Highlight drop zone when dragging files
dropArea.addEventListener('dragover', (event) => {
    if (transactionStatus === 'No actions' || transactionStatus === 'Rejected') {
        event.stopPropagation();
        event.preventDefault();
        dropArea.classList.add('dragover');
    }
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
    if (transactionStatus === 'No actions' || transactionStatus === 'Rejected') {
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
            updateFileDisplay(file, true, null, 'No actions');
        }
    }
    updateInfoText();
}
function reUploadFile(fileElement) {
    fileElement.remove();
    updateFileDisplay(fileList[0], true, null, 'No actions');
}
function processFiles(newFiles) {
    newFiles.forEach(file => {
        if (isValidFileType(file) && isValidFileSize(file) && !isDuplicate(file)) {
            fileList.push(file);
            updateFileDisplay(file, true, null, 'No actions');
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

function updateFileDisplay(file, isUploading, fileNameFromDb, transactionStatus) {
    const fileElement = document.createElement('div');
    fileElement.className = 'row-selected-file';

    const deleteBtn = document.createElement('button');
    const spinnerLabel = document.createElement('label');
    spinnerLabel.className = 'spinner-label';
    spinnerLabel.innerHTML = '<i class="fa-solid fa-spinner saving-icon"></i>';
    spinnerLabel.style.display = 'block';
    deleteBtn.className = 'delete-btn';
    deleteBtn.innerHTML = '<i class="fa-solid fa-trash-can"></i>';

    const fileName = document.createElement('span');
    if (isUploading) {
        fileName.textContent = file.name;
    } else {
        fileName.textContent = cleanFileName(fileNameFromDb);
    }

    const statusLabel = document.createElement('span');
    statusLabel.textContent = '';
    statusLabel.className = 'span-status';
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
                uploadFile(file, statusLabel, fileElement).then(data => {
                    deleteBtn.onclick = function () {
                        deleteFile(data.fileNamesUploaded[0], statusLabel, deleteBtn, spinnerLabel).then(() => {
                            fileList.splice(fileList.indexOf(file), 1);
                            fileElement.remove();
                            updateInfoText();
                        });
                    };
                    fileElement.appendChild(deleteBtn);
                    fileElement.appendChild(spinnerLabel);
                    spinnerLabel.style.display = 'none';
                }).catch(error => {
                    console.error("Error uploading the file:", error);
                });
            });
        } else {
            uploadFile(file, statusLabel, fileElement).then(data => {
                deleteBtn.onclick = function () {
                    deleteFile(data.fileNamesUploaded[0], statusLabel, deleteBtn, spinnerLabel).then(() => {
                        fileList.splice(fileList.indexOf(file), 1);
                        fileElement.remove();
                        updateInfoText();
                    });
                };
                fileElement.appendChild(deleteBtn);
                fileElement.appendChild(spinnerLabel);
                spinnerLabel.style.display = 'none';
            }).catch(error => {
                console.error("Error uploading the file:", error);
            });
        }
    } else {
        if (transactionStatus === 'No actions' || transactionStatus === 'Rejected') {
            fileElement.appendChild(deleteBtn);
        }
        fileElement.appendChild(spinnerLabel);
        spinnerLabel.style.display = 'none';
        deleteBtn.onclick = async function () {
            await deleteFile(fileNameFromDb, statusLabel, deleteBtn, spinnerLabel);
            fileElement.remove();
            updateInfoText();
        };
        statusLabel.innerHTML = '<i class="fa-solid fa-check uploaded-check-icon green-label"></i>';
    }
}

async function uploadFile(file, statusLabel, fileElement) {
    submissionError.innerHTML = '';
    var token = $('[name="__RequestVerificationToken"]').val();
    const formData = new FormData();
    formData.append('files', file);
    formData.append('movementId', movementIdNormalHoursInput.value);
    statusLabel.innerHTML = '<i class="fa-solid fa-spinner saving-icon"></i>';

    try {
        const response = await fetch('/TrackingTool/ReportingMyTime/UploadFilesClientNoTrackingTool', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'RequestVerificationToken': token
            },
            body: formData
        });
        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
                        return acc.concat(current);
                    }, []);
                    fileElement.remove();
                    displayToasterWarningArray(allErrors);
                    break;
                case "Not Found":
                    displayToasterError(errorData.detail);
                    createReuploadBtn(fileElement, statusLabel);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
                    createReuploadBtn(fileElement, statusLabel);
            }
            return null;
        }
        const data = await response.json();
        statusLabel.innerHTML = '<i class="fa-solid fa-check uploaded-check-icon green-label"></i>';
        return data;
    } catch (error) {
        validateSessionExpiration(error.message);
        console.error('Network or fetch error:', error);
        createReuploadBtn(fileElement, statusLabel);
        displayToasterError(error.message);
        return null;
    }
}
function createReuploadBtn(fileElement, statusLabel) {
    statusLabel.innerHTML = '';
    const errorSpan = document.createElement('span');
    errorSpan.innerHTML = 'Upload Failed <i class="fa-solid fa-upload"></i>';
    errorSpan.className = 'reupload-label';
    errorSpan.addEventListener('click', () => reUploadFile(fileElement));
    statusLabel.appendChild(errorSpan);
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
    submissionError.innerHTML = '';
    let saveBtn = document.getElementById('save-btn');
    const savingLabel = `<i class="fa-solid fa-spinner saving-icon"></i> Saving Changes...`;
    const saveLabel = `<i class="fa-solid fa-floppy-disk"></i> Please save your changes`;
    saveBtn.disabled = true;
    saveBtn.innerHTML = savingLabel;
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
    try {
        const response = await fetch('/TrackingTool/ReportingMyTime/CreateUpdateTimeEntryClientNoTrackingTool', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                RequestVerificationToken: token
            },
            body: formData
        });
        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
                        return acc.concat(current);
                    }, []);
                    displayToasterWarningArray(allErrors);
                    break;
                case "Not Found":
                    displayToasterError(errorData.error);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            saveBtn.disabled = false;
            saveBtn.innerHTML = saveLabel;
            return null;
        }
        const dataFromApi = await response.json();
        movementIdNormalHoursInput.value = dataFromApi.movementIdNormalHours;
        saveBtn.disabled = false;
        saveBtn.innerHTML = saveLabel;
        displayToasterSuccess(dataFromApi.message);
        saveReportBtn.style.display = 'none';
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        saveBtn.disabled = false;
        saveBtn.innerHTML = saveLabel;
        return null;
    }
}

function initializeUploadProcess() {
    fileList = [];

    if (uploadArea) {
        uploadArea.innerHTML = '';
    }
    if (fileInput) {
        fileInput.value = '';
    }
}
function updateStatusReportSubmittedClientHasTrackingTool() {
    submissionInfo.innerHTML = `<button style="background-color: ${getStatusColor(transactionStatus)}" id="submitBtn" onclick="submitReportToBePaid()">${getStatusWhiteIcon(transactionStatus)} 
                ${transactionStatus === 'Waiting to be approved' ? 'Pending approval' : transactionStatus === 'Approved' ? 'Timesheet approved' : transactionStatus}</button>`;
    let submitBtn = document.getElementById('submitBtn');
    submitBtn.disabled = true;
    submitBtn.className = 'submit-button-disabled';
    quantityInput.disabled = true;
    notesInput.disabled = true;
    onCallFlateRateSelect.disabled = true;
    onCallTimeWorkedInput.disabled = true;
    fileInput.disabled = true;
    saveReportBtn.style.display = 'none';
    uploadBtn.style.display = 'none';
}
//GET PROJECT MOVEMENTS
async function getProjectMovementsClientHasTrackTool() {
    saveReportBtn.style.display = 'none';
    initializeUploadProcess();
    loadingBoxIntern.style.display = 'block';
    errorMessageIntern.style.display = 'none';
    let noTackingToolSection = document.getElementById('no-tracking-tool-sec');
    noTackingToolSection.style.display = 'none';

    var startDateValue = encodeURIComponent(dateFromInput.value);
    var endDateValue = encodeURIComponent(dateToInput.value);
    var url = "/TrackingTool/ReportingMyTime/GetProjectMovements?projectId=" + encodeURIComponent(projectIdInput.value) +
        "&startDate=" + startDateValue + "&endDate=" + endDateValue;

    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorData = await response.json();
            errorMessageIntern.style.display = 'block';
            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
        }

        const data = await response.json();
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
        submissionInfo.innerHTML = `<button style="background-color: ${getStatusColor('No Actions')}" id="submitBtn" onclick="submitReportToBePaid()">${getStatusWhiteIcon('No Actions')} Submit your time</button>`;
        quantityInput.disabled = false;
        notesInput.disabled = false;
        onCallFlateRateSelect.disabled = false;
        onCallTimeWorkedInput.disabled = false;
        fileInput.disabled = false;
        transactionStatus = 'No actions';
        uploadBtn.style.display = 'block';

        data.movementsList.forEach(function (obj) {
            transactionStatus = obj.transactionStatus;
            if (transactionStatus !== 'No actions' && transactionStatus !== 'Rejected') {
                updateStatusReportSubmittedClientHasTrackingTool();
            }
            if (obj.movementTypeName === 'Normal Hours') {
                notes += obj.notes === null ? '' : obj.notes;
                normalHoursQuantity += obj.quantity;
                JSON.parse(obj.blobNames).forEach(function (blobName) {
                    blobNames.push(blobName);
                });
            }
            if (obj.movementTypeName === 'On Call Flate Rate') {
                onCallFlateRateQuantity += obj.quantity;
            }
            if (obj.movementTypeName === 'On Call Time Worked') {
                onCallTimeWorkedQuantity += obj.quantity;
            }
        });

        quantityInput.value = normalHoursQuantity;
        onCallFlateRateSelect.value = onCallFlateRateQuantity;
        onCallTimeWorkedInput.value = onCallTimeWorkedQuantity;
        notesInput.value = notes;
        blobNames.forEach(function (objName) {
            updateFileDisplay(null, false, objName, transactionStatus);
        });
        updateInfoText();
        noTackingToolSection.style.display = 'block';
    } catch (error) {
        validateSessionExpiration(error.message);
        console.error(error);
        // Handle any errors here
    } finally {
        loadingBoxIntern.style.display = 'none';
    }
}

function cleanFileName(fileName) {
    const regex = /^[a-f0-9]+_\d+_/i;
    return fileName.replace(regex, '');
}
// DELETE FILE
async function deleteFile(fileName, statusLabel, deleteBtn, spinnerLabel) {
    if (!fileName) {
        console.error('File name must be provided.');
        return;
    }
    fileInput.value = '';
    deleteBtn.style.display = 'none';
    statusLabel.innerHTML = '';
    spinnerLabel.style.display = 'block';
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
        } else {
            statusLabel.textContent = 'Delete failed';
            deleteBtn.style.display = 'block';
            spinnerLabel.style.display = 'none';
            displayToasterError(data.error || 'Failed to delete the file.');
            console.error('There has been a problem with the fetch operation:', data.detail);
        }
    } catch (error) {
        validateSessionExpiration(error.message);
        statusLabel.textContent = 'Delete failed';
        deleteBtn.style.display = 'block';
        spinnerLabel.style.display = 'none';
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

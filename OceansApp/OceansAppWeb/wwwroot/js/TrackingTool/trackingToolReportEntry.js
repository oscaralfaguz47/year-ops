document.addEventListener('DOMContentLoaded', async function () {
    paymentPeriod = getElementById('PaymentPeriodInput').value;
    let currentDateNoChange = new Date();
    calculatePeriod(currentDateNoChange, paymentPeriod); 
});

// Element selectors
const quantityInput = getElementById('quantityInput');
const notesInput = getElementById('notesInput');
const movementIdNormalHoursInput = getElementById('movementIdNormalHoursInput');
const movementIdOnCallFlateRateInput = getElementById('movementIdOnCallFlateRateInput');
const movementIdOnCallTimeWorkedInput = getElementById('movementIdOnCallTimeWorkedInput');
const onCallFlateRateSelect = getElementById('onCallFlateRateSelect');
const onCallTimeWorkedInput = getElementById('onCallTimeWorkedInput');
const uploadArea = getElementById('file-upload-name');
const fileInput = getElementById('file-upload');
const saveReportBtn = getElementById('save-btn');
const uploadBtn = getElementById('upload-btn');
const holidaysContainer = getElementById('holidaysContainer');
const dropArea = document.querySelector('.file-upload-wrapper');
const noTrackingToolSection = getElementById('no-tracking-tool-sec');
const onCallSectionEl = getElementById('on-call-section');
const previewFilesElement = document.querySelector('.preview-files-section');
let blobNames = [];
const previewContainer = getElementById("previewContainer");

const maxFileSize = 10 * 1024 * 1024; // 10 MB
let transactionStatus = 'No actions';
let fileList = [];
let isCreatingMovement = false;
let movementCreationPromise = null;

const displayElement = (element, displayStyle) => element.style.display = displayStyle;

function handleChangeData() {
    saveReportBtn.style.display = 'block';
}
// Event listeners
dropArea.addEventListener('dragover', event => {
    if (transactionStatus === 'No actions' || transactionStatus === 'Rejected') {
        event.preventDefault();
        dropArea.classList.add('dragover');
    }
});

dropArea.addEventListener('dragleave', () => {
    dropArea.classList.remove('dragover');
});

dropArea.addEventListener('drop', handleFiles);
fileInput.addEventListener('change', handleFiles);
fileInput.setAttribute('accept', '.pdf, .jpg, .jpeg, .png, .gif, .svg, .doc, .docx, .xls, .xlsx, .csv, .txt');

document.addEventListener('paste', event => {
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

// File handling functions
async function handleFiles(event) {
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
    const infoText = getElementById('info-text');
    infoText.style.display = uploadArea.textContent.trim() === '' && uploadArea.childNodes.length === 0 ? 'block' : 'none';
}

// File display functions
function updateFileDisplay(file, isUploading, fileNameFromDb, transactionStatus, blobUrl) {
    const fileElement = document.createElement('div');
    fileElement.className = 'row-selected-file';

    const deleteBtn = document.createElement('button');
    const spinnerLabel = document.createElement('label');
    spinnerLabel.className = 'spinner-label';
    spinnerLabel.innerHTML = '<i class="fa-solid fa-spinner saving-icon"></i>';
    displayElement(spinnerLabel, 'block');
    deleteBtn.className = 'delete-btn';
    deleteBtn.innerHTML = '<i class="fa-solid fa-trash-can"></i>';

    const fileName = document.createElement('a');
    fileName.textContent = isUploading ? file.name : cleanFileName(fileNameFromDb);

    if (blobUrl) {
        fileName.href = blobUrl;
        fileName.target = '_blank';
        fileName.rel = 'noopener noreferrer'; 
    } else if (file) {
        const localUrl = URL.createObjectURL(file); 
        fileName.href = localUrl;
        fileName.target = '_blank';
        fileName.rel = 'noopener noreferrer'; 

        fileName.onclick = function () {
            setTimeout(() => URL.revokeObjectURL(localUrl), 5000);
        };
    } else {
        fileName.href = '#';
        fileName.onclick = function (event) {
            event.preventDefault();
            console.error('No file available to redirect.');
        };
    }


    const statusLabel = document.createElement('span');
    statusLabel.textContent = '';
    statusLabel.className = 'span-status';
    fileElement.appendChild(fileName);
    fileElement.appendChild(statusLabel);
    uploadArea.appendChild(fileElement);

    if (isUploading) {
        handleFileUpload(file, statusLabel, fileElement, deleteBtn, spinnerLabel);
    } else {
        finalizeFileDisplay(fileNameFromDb, fileElement, statusLabel, deleteBtn, spinnerLabel, transactionStatus);
    }
}
async function handleFileUpload(file, statusLabel, fileElement, deleteBtn, spinnerLabel) {
    if (!movementIdNormalHoursInput.value && !isCreatingMovement) {
        isCreatingMovement = true;
        try {
            movementCreationPromise = createFirstMovementIfDoesNotExist();
            const data = await movementCreationPromise;
            if (data && data.createdMovementId !== undefined) {
                movementIdNormalHoursInput.value = data.createdMovementId;
            } else {
                throw new Error('Invalid response data');
            }
        } catch (error) {
            console.error('Error:', error);
            displayToasterError(error.message);
        } finally {
            isCreatingMovement = false;
        }
    }

    if (movementCreationPromise) {
        await movementCreationPromise;
    }
    uploadFile(file, statusLabel, fileElement).then(data => {
        const blobUrl = URL.createObjectURL(file);
        const newBlob = {
            BlobName: data.fileNamesUploaded[0].blobName,
            BlobUrl: data.fileNamesUploaded[0].blobUrl
        };
        blobNames.push(newBlob);
        deleteBtn.onclick = async function () {
            await deleteFile(data.fileNamesUploaded[0].blobName, statusLabel, deleteBtn, spinnerLabel);
            fileList.splice(fileList.indexOf(file), 1);

            const blobIndex = blobNames.findIndex(blob => blob.BlobName === data.fileNamesUploaded[0].blobName);
            if (blobIndex !== -1) {
                blobNames.splice(blobIndex, 1);
            }

            fileElement.remove();
            updateInfoText();
            validateUploadedFilesToRemovePreviewBtn();
            URL.revokeObjectURL(blobUrl); 
        };
        fileElement.appendChild(deleteBtn);
        fileElement.appendChild(spinnerLabel);
        displayElement(spinnerLabel, 'none');
        validateUploadedFilesToRemovePreviewBtn();

        console.log("Lista de blobs actualizada:", blobNames);
    }).catch(error => {
        console.error("Error uploading the file:", error);
    });
}
function finalizeFileDisplay(fileNameFromDb, fileElement, statusLabel, deleteBtn, spinnerLabel, transactionStatus) {
    if (transactionStatus === 'No actions' || transactionStatus === 'Rejected') {
        fileElement.appendChild(deleteBtn);
    }
    fileElement.appendChild(spinnerLabel);
    displayElement(spinnerLabel, 'none');
    deleteBtn.onclick = async function () {
        await deleteFile(fileNameFromDb, statusLabel, deleteBtn, spinnerLabel);
        const blobIndex = blobNames.findIndex(blob => blob.BlobName === fileNameFromDb);
        if (blobIndex !== -1) {
            blobNames.splice(blobIndex, 1);
        }
        fileElement.remove();
        updateInfoText();
        validateUploadedFilesToRemovePreviewBtn();
        console.log(blobNames);
    };
    statusLabel.innerHTML = '<i class="fa-solid fa-check uploaded-check-icon green-label"></i>';
}

// File upload functions
async function uploadFile(file, statusLabel, fileElement) {
    submissionError.innerHTML = '';
    const token = $('[name="__RequestVerificationToken"]').val();
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
            handleUploadError(errorData, fileElement, statusLabel);
            return null;
        }
        const data = await response.json();
        console.log(data);
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
function handleUploadError(errorData, fileElement, statusLabel) {
    switch (errorData.messageType) {
        case "Validation Error":
            const allErrors = Object.values(errorData.errors).reduce((acc, current) => acc.concat(current), []);
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
    const token = $('[name="__RequestVerificationToken"]').val();
    const startActionDateData = getNormalizedOneDate(dateFromInput).normalizedDate;
    const actionDateData = getNormalizedOneDate(dateToInput).normalizedDate;
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

// Time entry creation/update
async function createUpdateTimeEntry() {
    submissionError.innerHTML = '';
    const saveBtn = getElementById('save-btn');
    const savingLabel = `<i class="fa-solid fa-spinner saving-icon"></i> Saving Changes...`;
    const saveLabel = `<i class="fa-solid fa-floppy-disk"></i> Please save your changes`;
    saveBtn.disabled = true;
    saveBtn.innerHTML = savingLabel;

    const token = $('[name="__RequestVerificationToken"]').val();
    const actionDateData = getNormalizedOneDate(dateToInput).normalizedDate;;
    const startActionDateData = getNormalizedOneDate(dateFromInput).normalizedDate;
    const formData = new FormData();
    const appendIfValid = (key, value) => {
        if (value) {
            formData.append(key, value);
        }
    };

    const dataItems = [
        {
            ProjectId: projectIdInput.value,
            Quantity: quantityInput.value,
            StartActionDate: startActionDateData,
            ActionDate: actionDateData,
            Notes: notesInput.value,
            MovementType: 'Normal Hours'
        },
        {
            ProjectId: projectIdInput.value,
            Quantity: onCallFlateRateSelect.value,
            StartActionDate: startActionDateData,
            ActionDate: actionDateData,
            Notes: null,
            MovementType: 'On Call Flate Rate'
        },
        {
            ProjectId: projectIdInput.value,
            Quantity: onCallTimeWorkedInput.value,
            StartActionDate: startActionDateData,
            ActionDate: actionDateData,
            Notes: null,
            MovementType: 'On Call Time Worked'
        }
    ];

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
            handleCreateUpdateError(errorData);
            saveBtn.disabled = false;
            saveBtn.innerHTML = saveLabel;
            return null;
        }

        const dataFromApi = await response.json();
        movementIdNormalHoursInput.value = dataFromApi.movementIdNormalHours;
        saveBtn.disabled = false;
        saveBtn.innerHTML = saveLabel;
        displayToasterSuccess(dataFromApi.message);
        displayElement(saveReportBtn, 'none');
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
function handleCreateUpdateError(errorData) {
    switch (errorData.messageType) {
        case "Validation Error":
            const allErrors = Object.values(errorData.errors).reduce((acc, current) => acc.concat(current), []);
            displayToasterWarningArray(allErrors);
            break;
        case "Not Found":
            displayToasterError(errorData.error);
            break;
        default:
            displayToasterError('An unexpected error occurred: ' + errorData.error);
    }
}

// Initialization and update functions
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
    const submitBtn = getElementById('submitBtn');
    submitBtn.disabled = true;
    submitBtn.className = 'submit-button-disabled';
    quantityInput.disabled = true;
    notesInput.disabled = true;
    onCallFlateRateSelect.disabled = true;
    onCallTimeWorkedInput.disabled = true;
    fileInput.disabled = true;
    displayElement(saveReportBtn, 'none');
    displayElement(uploadBtn, 'none');
}

// Fetch project movements
async function getProjectMovementsClientHasTrackTool(participatesOnCall) {
    initializeUploadProcess();
    participatesOnCall ? onCallSectionEl.style.display = 'block' : onCallSectionEl.style.display = 'none';
    const startDateValue = encodeURIComponent(dateFromInput.value);
    const endDateValue = encodeURIComponent(dateToInput.value);
    const url = `/TrackingTool/ReportingMyTime/GetProjectMovements?projectId=${encodeURIComponent(projectIdInput.value)}&startDate=${startDateValue}&endDate=${endDateValue}`;

    try {
        const response = await fetch(url);

        if (!response.ok) {
            const errorData = await response.json();
            displayElement(errorMessageIntern, 'block');
            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
        }

        const data = await response.json();
        updateProjectMovements(data);
    } catch (error) {
        validateSessionExpiration(error.message);
        console.error(error);
    } finally {
        displayElement(loadingBoxIntern, 'none');
    }
}
function updateProjectMovements(data) {
    let normalHoursQuantity = 0;
    let onCallFlateRateQuantity = 0;
    let onCallTimeWorkedQuantity = 0;
    let notes = '';
    blobNames = [];
    previewFilesElement.style.display = 'none';

    if (data.movementsList.length > 0) {
        const normalMovement = data.movementsList.find(movement => movement.movementTypeName !== 'Holidays');
        movementIdNormalHoursInput.value = normalMovement ? normalMovement.movementId : null;
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
    displayElement(uploadBtn, 'block');
    let holidaysCount = 0;
    let holidaysHtmlList = ``;

    data.movementsList.forEach(function (obj) {
        if (obj.movementTypeName === 'Normal Hours') {
            notes += obj.notes === null ? '' : obj.notes;
            normalHoursQuantity += obj.quantity;
            JSON.parse(obj.blobData).forEach(blobName => blobNames.push(blobName));
            if (blobNames.length > 0) {
                previewFilesElement.style.display = 'flex';
            }
        }
        if (obj.movementTypeName === 'On Call Flate Rate') {
            onCallFlateRateQuantity += obj.quantity;
        }
        if (obj.movementTypeName === 'On Call Time Worked') {
            onCallTimeWorkedQuantity += obj.quantity;
        }
        if (obj.movementTypeName === 'Holidays') {
            holidaysCount++;
            const holidayDate = new Date(obj.actionDate);
            holidaysHtmlList += `<div data-tooltip="You will be paid ${obj.quantity} hours for this holiday, you don't need to report this." class="holiday-Item tooltip-target"><span class="holiday-name">${obj.notes}<i class="fa-solid fa-gift"></i></span><span>${getMonthName(holidayDate.getMonth())} ${holidayDate.getDate()}</span></div>`;
        } else {
            transactionStatus = obj.transactionStatus;
            if (transactionStatus !== 'No actions' && transactionStatus !== 'Rejected') {
                updateStatusReportSubmittedClientHasTrackingTool();
            }
        }
    });

    if (holidaysCount > 0) {
        holidaysContainer.innerHTML = `<label>You have ${holidaysCount} holiday${holidaysCount === 1 ? '' : 's'} to be reimbursed for this period</label> <div style="display:flex; justify-content:center">${holidaysHtmlList}</div>`;
        displayElement(holidaysContainer, 'block');
        initializeTooltips();
    } else {
        holidaysContainer.style.display = 'none';
    }

    quantityInput.value = normalHoursQuantity;
    onCallFlateRateSelect.value = onCallFlateRateQuantity;
    onCallTimeWorkedInput.value = onCallTimeWorkedQuantity;
    notesInput.value = notes;
    blobNames.forEach(blobName => updateFileDisplay(null, false, blobName.BlobName, transactionStatus, blobName.BlobUrl));
    updateInfoText();
    displayElement(noTrackingToolSection, 'block');
}

// File deletion
async function deleteFile(fileName, statusLabel, deleteBtn, spinnerLabel) {
    if (!fileName) {
        console.error('File name must be provided.');
        return;
    }

    fileInput.value = '';
    displayElement(deleteBtn, 'none');
    statusLabel.innerHTML = '';
    displayElement(spinnerLabel, 'block');

    const token = $('[name="__RequestVerificationToken"]').val();
    const formData = new FormData();
    formData.append('fileName', fileName);

    try {
        const response = await fetch("/TrackingTool/ReportingMyTime/DeleteBlob", {
            method: 'POST',
            headers: { RequestVerificationToken: token },
            body: formData
        });

        const data = await response.json();
        if (!response.ok) {
            handleDeleteError(data, statusLabel, deleteBtn, spinnerLabel);
        }
    } catch (error) {
        validateSessionExpiration(error.message);
        handleDeleteError({ error: 'Network error occurred. Please try again.' }, statusLabel, deleteBtn, spinnerLabel);
    }
}
function validateUploadedFilesToRemovePreviewBtn() {
    if (dropArea.querySelectorAll('.row-selected-file').length > 0) {
        previewFilesElement.style.display = 'flex';
    } else {
        previewFilesElement.style.display = 'none';
    }
}
function handleDeleteError(data, statusLabel, deleteBtn, spinnerLabel) {
    statusLabel.textContent = 'Delete failed';
    displayElement(deleteBtn, 'block');
    displayElement(spinnerLabel, 'none');
    displayToasterError(data.error || 'Failed to delete the file.');
    console.error('There has been a problem with the fetch operation:', data.detail);
}

// Input validation
notesInput.addEventListener('input', function () {
    if (this.value.length > 400) {
        this.value = this.value.slice(0, 400);
    }
});

function cleanFileName(fileName) {
    const regex = /^[a-f0-9]+_\d+_/i;
    return fileName.replace(regex, '');
}

function previewUploadedFiles(modalId) {
    previewContainer.innerHTML = "";

    blobNames.forEach(blob => {
        const fileType = getFileType(blob.BlobName);

        if (fileType === "image" || fileType === "svg") {
            previewImage(blob);
        } else if (fileType === "pdf") {
            previewPDF(blob);
        } else if (fileType === "word" || fileType === "excel") {
            previewTextOrOffice(blob);
        } else {
            previewOther(blob);
        }
    });

    showModal(modalId);
}

function getFileType(fileName) {
    const lowerCaseName = fileName.toLowerCase();
    if (lowerCaseName.endsWith(".jpg") || lowerCaseName.endsWith(".jpeg") || lowerCaseName.endsWith(".png") || lowerCaseName.endsWith(".gif")) {
        return "image";
    } else if (lowerCaseName.endsWith(".svg")) {
        return "svg";
    } else if (lowerCaseName.endsWith(".pdf")) {
        return "pdf";
    } else if (lowerCaseName.endsWith(".doc") || lowerCaseName.endsWith(".docx")) {
        return "word";
    } else if (lowerCaseName.endsWith(".xls") || lowerCaseName.endsWith(".xlsx")) {
        return "excel";
    } else {
        return "other";
    }
}

function previewImage(blob) {
    const container = createPreviewContainer(blob);
    const img = document.createElement("img");
    img.src = blob.BlobUrl;
    img.alt = blob.BlobName;
    img.style.maxWidth = "90%";
    img.style.margin = "0 auto";
    img.style.display = "block";
    container.appendChild(img);
    previewContainer.appendChild(container);
}

function previewPDF(blob) {
    const container = createPreviewContainer(blob);
    const iframe = document.createElement("iframe");
    iframe.src = blob.BlobUrl;
    iframe.width = "100%";
    iframe.height = "500px";
    iframe.style.border = "1px solid #ccc";
    container.appendChild(iframe);
    previewContainer.appendChild(container);
}

function previewTextOrOffice(blob) {
    const container = createPreviewContainer(blob);
    const iframe = document.createElement("iframe");
    iframe.src = `https://view.officeapps.live.com/op/view.aspx?src=${encodeURIComponent(blob.BlobUrl)}`;
    iframe.width = "100%";
    iframe.height = "500px";
    iframe.style.border = "1px solid #ccc";
    container.appendChild(iframe);
    previewContainer.appendChild(container);
}

function previewOther(blob) {
    const container = document.createElement("div");
    container.className = 'item-container';

    const linkContainer = document.createElement('div');
    linkContainer.className = 'link-container';
    const link = document.createElement("a");
    link.href = blob.BlobUrl;
    link.target = "_blank";
    link.textContent = cleanFileName(blob.BlobName);
    link.className = 'item-title';

    linkContainer.appendChild(link);
    container.appendChild(linkContainer);
    previewContainer.appendChild(container);
}

function createPreviewContainer(blob) {
    const container = document.createElement("div");
    container.className = 'item-container';

    const linkContainer = document.createElement('div');
    linkContainer.className = 'link-container';
    const titleLink = document.createElement("a");
    titleLink.href = blob.BlobUrl;
    titleLink.target = "_blank";
    titleLink.textContent = cleanFileName(blob.BlobName);
    titleLink.className = 'item-title';
    titleLink.style.textDecoration = "underline";

    linkContainer.appendChild(titleLink);
    container.appendChild(linkContainer);
    return container;
}
document.addEventListener("keydown", function (event) {
    hideModal('modal-preview-files');
});
function closeModalOnOutsideClick(event, modalId) {
    const modal = document.getElementById(modalId);
    const modalContent = modal.querySelector(".global-modal-content");

    if (!modalContent.contains(event.target)) {
        hideModal(modalId);
    }
}
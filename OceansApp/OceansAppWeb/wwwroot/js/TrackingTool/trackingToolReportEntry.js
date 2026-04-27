

// Element selectors
const quantityInput = getElementById('quantityInput');
const notesInput = getElementById('notesInput');
const movementIdNormalHoursInput = getElementById('movementIdNormalHoursInput');
const movementIdOnCallFlateRateInput = getElementById('movementIdOnCallFlateRateInput');
const movementIdOnCallTimeWorkedInput = getElementById('movementIdOnCallTimeWorkedInput');
const onCallFlateRateSelect = getElementById('onCallFlateRateSelect');
const onCallTimeWorkedInput = getElementById('onCallTimeWorkedInput');
const uploadArea = getElementById('file-upload-name');
const saveReportBtn = getElementById('save-btn');
const uploadBtnPrimary = document.getElementById('upload-btn-primary');
const primaryFileInput = document.getElementById('file-upload-primary');
const uploadBtnSecond = document.getElementById('upload-btn-second');
const secondFileInput = document.getElementById('file-upload-second');
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
let lastFocusedUploadInput = null;


const displayElement = (element, displayStyle) => element.style.display = displayStyle;

const fileUploadStates = {
    'file-upload-primary': { fileList: [], blobNames: [] },
    'file-upload-second': { fileList: [], blobNames: [] }
};

document.addEventListener('DOMContentLoaded', async function () {
    const paymentPeriod = getElementById('PaymentPeriodInput').value;

    initializeCurrentDateFromUrl();

    const urlParams = new URLSearchParams(window.location.search);
    const startDateParam = urlParams.get('startDate');
    const endDateParam = urlParams.get('endDate');

    let baseDate = new Date();
    if (startDateParam) {
        const parsed = parseLocalDate(startDateParam);
        if (!isNaN(parsed)) baseDate = parsed;
    }

    const hasValidParams = startDateParam && endDateParam;

    await calculatePeriod(
        baseDate,
        paymentPeriod,
        undefined,
        hasValidParams ? parseLocalDate(startDateParam) : null,
        hasValidParams ? parseLocalDate(endDateParam) : null,
        true
    );

    initializeFileUpload({
        inputId: 'file-upload-primary',
        dropAreaSelector: '#primary-upload-files-input .file-upload-wrapper',
        uploadAreaId: 'file-upload-name-primary',
        infoTextId: 'info-text-primary',
        previewSectionClass: '#primary-upload-files-input .preview-files-section'
    });

    initializeFileUpload({
        inputId: 'file-upload-second',
        dropAreaSelector: '#second-upload-files-input .file-upload-wrapper',
        uploadAreaId: 'file-upload-name-second',
        infoTextId: 'info-text-second',
        previewSectionClass: '#second-upload-files-input .preview-files-section'
    });
    registerPasteListener();
});


function initializeFileUpload({ inputId, dropAreaSelector, uploadAreaId, infoTextId, previewSectionClass }) {
    const input = document.getElementById(inputId);
    const dropArea = document.querySelector(dropAreaSelector);
    const uploadArea = document.getElementById(uploadAreaId);
    const infoText = document.getElementById(infoTextId);
    const previewSection = document.querySelector(previewSectionClass);
    const state = fileUploadStates[inputId];
    if (!state) {
        console.warn(`Upload state for inputId '${inputId}' is not defined.`);
        return;
    }

    input.setAttribute('accept', '.pdf, .jpg, .jpeg, .png');

    dropArea.addEventListener('dragover', event => {
        if (transactionStatus === 'No actions' || transactionStatus === 'Rejected') {
            event.preventDefault();
            dropArea.classList.add('dragover');
        }
    });

    dropArea.addEventListener('dragleave', () => {
        dropArea.classList.remove('dragover');
    });

    dropArea.addEventListener('drop', event => {
        event.preventDefault();
        dropArea.classList.remove('dragover');
        handleFilesInput(event.dataTransfer.files, inputId, uploadArea, infoText, previewSection);
    });
    dropArea.addEventListener('click', () => {
        lastFocusedUploadInput = { inputId, uploadArea, infoText, previewSection };
    });


    input.addEventListener('change', event => {
        handleFilesInput(event.target.files, inputId, uploadArea, infoText, previewSection);
    });

}


let pasteListenerRegistered = false; 

function registerPasteListener() {
    if (pasteListenerRegistered) return; 
    pasteListenerRegistered = true;

    document.addEventListener('paste', event => {
        if ((transactionStatus === 'No actions' || transactionStatus === 'Rejected') && lastFocusedUploadInput) {
            const items = event.clipboardData?.items;
            if (items) {
                const files = Array.from(items)
                    .filter(item => item.kind === 'file')
                    .map(item => item.getAsFile());
                if (files.length > 0) {
                    handleFilesInput(
                        files,
                        lastFocusedUploadInput.inputId,
                        lastFocusedUploadInput.uploadArea,
                        lastFocusedUploadInput.infoText,
                        lastFocusedUploadInput.previewSection
                    );
                }
            }
        }
    });
}
async function handleFilesInput(files, inputId, uploadArea, infoText, previewSection) {
    const state = fileUploadStates[inputId];
    const MAX_FILES = 20;

    const currentUploadArea = document.getElementById(
        inputId === 'file-upload-primary' ? 'file-upload-name-primary' : 'file-upload-name-second'
    );
    const currentFileCount = currentUploadArea ? currentUploadArea.querySelectorAll('.row-selected-file').length : 0;

    const validFiles = [];
    for (const file of files) {
        if (!isValidFileType(file)) {
            alert('The file you are trying to upload is not valid. Only PDF, JPG, JPEG, and PNG formats are allowed.');
            continue;
        }
        if (!isValidFileSize(file)) {
            alert('The file is too large. Maximum allowed size is 10 MB.');
            continue;
        }
        if (isDuplicate(file, state?.fileList || [])) {
            continue;
        }
        validFiles.push(file);
    }

    if (currentFileCount >= MAX_FILES) {
        displayToasterWarning(`You have reached the maximum limit of ${MAX_FILES} files.`);
        return;
    }

    const availableSlots = MAX_FILES - currentFileCount;
    if (validFiles.length > availableSlots) {
        displayToasterWarning(`You can only upload ${availableSlots} more file(s). Only the first ${availableSlots} will be uploaded.`);
    }

    const filesToProcess = validFiles.slice(0, availableSlots);

    for (const file of filesToProcess) {
        state.fileList.push(file);
        await waitForFileDisplay(file, inputId, uploadArea, state, previewSection, infoText);
    }

    updateInfoText(uploadArea, infoText);
}


function waitForFileDisplay(file, inputId, uploadArea, state, previewSection, infoText) {
    return new Promise((resolve) => {
        updateFileDisplay(file, true, null, 'No actions', null, uploadArea, state, previewSection, inputId, infoText, resolve);
    });
}
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

primaryFileInput.setAttribute('accept', '.pdf, .jpg, .jpeg, .png');
secondFileInput.setAttribute('accept', '.pdf, .jpg, .jpeg, .png');


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
    const ext = file.name.split('.').pop().toLowerCase();
    return ['pdf', 'jpg', 'jpeg', 'png'].includes(ext);
}
function isValidFileSize(file) {
    return file.size <= maxFileSize;
}
function isDuplicate(file, fileList) {
    if (!Array.isArray(fileList)) return false;
    return fileList.some(f => f.name === file.name && f.size === file.size);
}
function updateInfoText(uploadArea, infoText) {
    if (uploadArea.children.length === 0) {
        infoText.style.display = 'block';
    } else {
        infoText.style.display = 'none';
    }
}

// File display functions
function updateFileDisplay(file, isUploading, fileNameFromDb, transactionStatus, blobUrl, uploadArea, state, previewSection, inputId, infoText, onUploadComplete) {
    const fileElement = document.createElement('div');
    fileElement.className = 'row-selected-file';

    const deleteBtn = document.createElement('button');
    deleteBtn.className = 'delete-btn';
    deleteBtn.innerHTML = '<i class="fa-solid fa-trash-can"></i>';

    const spinnerLabel = document.createElement('label');
    spinnerLabel.className = 'spinner-label';
    spinnerLabel.innerHTML = '<i class="fa-solid fa-spinner saving-icon"></i>';
    displayElement(spinnerLabel, 'block');

    const fileName = document.createElement('a');
    fileName.textContent = isUploading ? file.name : cleanFileName(fileNameFromDb);

    if (blobUrl) {
        fileName.href = blobUrl;
    } else if (file) {
        const localUrl = URL.createObjectURL(file);
        fileName.href = localUrl;
        fileName.onclick = () => setTimeout(() => URL.revokeObjectURL(localUrl), 5000);
    } else {
        fileName.href = '#';
        fileName.onclick = e => { e.preventDefault(); };
    }

    fileName.target = '_blank';
    fileName.rel = 'noopener noreferrer';

    const statusLabel = document.createElement('span');
    statusLabel.className = 'span-status';

    fileElement.appendChild(fileName);
    fileElement.appendChild(statusLabel);
    uploadArea.appendChild(fileElement);

    if (isUploading) {
        handleFileUpload(file, statusLabel, fileElement, deleteBtn, spinnerLabel, state, uploadArea, previewSection, inputId, infoText, onUploadComplete);
    }
}

async function handleFileUpload(file, statusLabel, fileElement, deleteBtn, spinnerLabel, state, uploadArea, previewSection, inputId, infoText, onUploadComplete) {
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
            if (onUploadComplete) onUploadComplete();
            return;
        } finally {
            isCreatingMovement = false;
        }
    }

    if (movementCreationPromise) {
        await movementCreationPromise;
    }

    try {
        const data = await uploadFile(file, statusLabel, fileElement, inputId);

        if (!data) {
            if (onUploadComplete) onUploadComplete(); 
            return;
        }

        const blobUrl = URL.createObjectURL(file);
        const newBlob = {
            BlobName: data.fileNamesUploaded[0].blobName,
            BlobUrl: data.fileNamesUploaded[0].blobUrl,
            PrimaryReportTrackingToolName: inputId.includes('primary') ? 'yes' : null,
            SecondReportTrackingToolName: inputId.includes('second') ? 'yes' : null
        };

        const currentState = fileUploadStates[inputId];
        currentState.blobNames.push(newBlob);

        deleteBtn.onclick = async function () {
            await deleteFile(data.fileNamesUploaded[0].blobName, statusLabel, deleteBtn, spinnerLabel);
            const currentState = fileUploadStates[inputId];
            const indexInFileList = currentState.fileList.findIndex(f => f.name === file.name && f.size === file.size);
            if (indexInFileList !== -1) currentState.fileList.splice(indexInFileList, 1);
            const blobIndex = currentState.blobNames.findIndex(blob => blob.BlobName === data.fileNamesUploaded[0].blobName);
            const globalIndex = blobNames.findIndex(b => b.BlobName === data.fileNamesUploaded[0].blobName);
            if (globalIndex !== -1) blobNames.splice(globalIndex, 1);
            if (blobIndex !== -1) currentState.blobNames.splice(blobIndex, 1);
            fileElement.remove();
            updateInfoText(uploadArea, infoText);
            validateUploadedFilesToTogglePreviewBtn();
            URL.revokeObjectURL(blobUrl);
        };

        fileElement.appendChild(deleteBtn);
        fileElement.appendChild(spinnerLabel);
        displayElement(spinnerLabel, 'none');
        validateUploadedFilesToTogglePreviewBtn();

    } catch (error) {
        console.error("Error uploading the file:", error);
    } finally {
        if (onUploadComplete) onUploadComplete(); 
    }
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
        validateUploadedFilesToTogglePreviewBtn();
    };
    statusLabel.innerHTML = '<i class="fa-solid fa-check uploaded-check-icon green-label"></i>';
}

// File upload functions
async function uploadFile(file, statusLabel, fileElement, inputId) {
    submissionError.innerHTML = '';
    const token = $('[name="__RequestVerificationToken"]').val();
    const formData = new FormData();

    // Determines whether it is 'primary' or 'second' based on the input ID
    const primarySecond = inputId === 'file-upload-primary' ? 'primary' : 'second';

    formData.append('files', file);
    formData.append('movementId', movementIdNormalHoursInput.value);
    formData.append('primarySecond', primarySecond);

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
    noHoursError.innerHTML = '';
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
    if (primaryFileInput) {
        primaryFileInput.value = '';
    }
    if (secondFileInput) {
        secondFileInput.value = '';
    }
}
function updateStatusReportSubmittedClientHasTrackingTool() {
    submissionInfo.innerHTML = `<button style="background-color: ${getStatusColor(transactionStatus)}" id="submitBtn" onclick="submitReportToBePaid()">${getStatusWhiteIcon(transactionStatus)} 
                ${transactionStatus === 'Waiting to be approved' ? 'Pending approval' : transactionStatus === 'Approved' ? 'Timesheet approved' : transactionStatus}</button>`;
    const submitBtn = getElementById('submitBtn');
    submitBtn.disabled = true;
    noHoursSection.style.display = 'none';
    submitBtn.className = 'submit-button-disabled';
    quantityInput.disabled = true;
    notesInput.disabled = true;
    onCallFlateRateSelect.disabled = true;
    onCallTimeWorkedInput.disabled = true;
    primaryFileInput.disabled = true;
    secondFileInput.disabled = true;
    displayElement(saveReportBtn, 'none');
    displayElement(uploadBtnPrimary, 'none');
    displayElement(uploadBtnSecond, 'none');

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
    let holidaysCount = 0;
    let holidaysHtmlList = ``;

    const primaryUploadArea = document.getElementById('file-upload-name-primary');
    const primaryInfoText = document.getElementById('info-text-primary');
    const primaryPreviewSection = document.querySelector('#primary-upload-files-input .preview-files-section');
    const primaryUploadBtn = document.getElementById('upload-btn-primary');
    const primaryFileInput = document.getElementById('file-upload-primary');
    const primaryState = fileUploadStates['file-upload-primary'];

    const secondUploadArea = document.getElementById('file-upload-name-second');
    const secondInfoText = document.getElementById('info-text-second');
    const secondPreviewSection = document.querySelector('#second-upload-files-input .preview-files-section');
    const secondUploadBtn = document.getElementById('upload-btn-second');
    const secondFileInput = document.getElementById('file-upload-second');
    const secondState = fileUploadStates['file-upload-second'];

    // Reset previous data
    primaryState.blobNames = [];
    secondState.blobNames = [];
    primaryUploadArea.innerHTML = '';
    secondUploadArea.innerHTML = '';
    primaryPreviewSection.style.display = 'none';
    secondPreviewSection.style.display = 'none';
    holidaysContainer.style.display = 'none'; 
    holidaysContainer.innerHTML = '';          

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
    primaryFileInput.disabled = false;
    secondFileInput.disabled = false;
    transactionStatus = 'No actions';
    displayElement(primaryUploadBtn, 'block');
    displayElement(secondUploadBtn, 'block');

    data.movementsList.forEach(obj => {
        if (obj.movementTypeName === 'Normal Hours') {
            notes += obj.notes || '';
            normalHoursQuantity += obj.quantity;

            const blobs = JSON.parse(obj.blobData);
            blobs.forEach(blob => {
                const isPrimary = blob.PrimaryReportTrackingToolName !== null;
                const uploadArea = isPrimary ? primaryUploadArea : secondUploadArea;
                const state = isPrimary ? primaryState : secondState;
                const infoText = isPrimary ? primaryInfoText : secondInfoText;

                state.blobNames.push(blob);          
                blobNames.push(blob);                 

                const fileElement = document.createElement('div');
                fileElement.className = 'row-selected-file';

                const deleteBtn = document.createElement('button');
                deleteBtn.className = 'delete-btn';
                deleteBtn.innerHTML = '<i class="fa-solid fa-trash-can"></i>';

                const spinnerLabel = document.createElement('label');
                spinnerLabel.className = 'spinner-label';
                spinnerLabel.style.display = 'none';
                spinnerLabel.innerHTML = '<i class="fa fa-spinner fa-spin"></i>'; 


                const fileName = document.createElement('a');
                fileName.textContent = cleanFileName(blob.BlobName);
                fileName.href = blob.BlobUrl;
                fileName.target = '_blank';
                fileName.rel = 'noopener noreferrer';

                const statusLabel = document.createElement('span');
                statusLabel.className = 'span-status';
                statusLabel.innerHTML = '<i class="fa-solid fa-check uploaded-check-icon green-label"></i>';

                deleteBtn.onclick = async function () {
                    displayElement(deleteBtn, 'none');
                    displayElement(spinnerLabel, 'block');
                    await deleteFile(blob.BlobName, statusLabel, deleteBtn, spinnerLabel);
                    const index = state.blobNames.findIndex(b => b.BlobName === blob.BlobName);
                    if (index !== -1) state.blobNames.splice(index, 1);
                    const globalIndex = blobNames.findIndex(b => b.BlobName === blob.BlobName);
                    if (globalIndex !== -1) blobNames.splice(globalIndex, 1);
                    fileElement.remove();
                    updateInfoText(uploadArea, infoText);
                    validateUploadedFilesToTogglePreviewBtn();
                };

                fileElement.appendChild(fileName);
                fileElement.appendChild(statusLabel);
                fileElement.appendChild(spinnerLabel); 
                fileElement.appendChild(deleteBtn);   
                uploadArea.appendChild(fileElement);

            });
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
            if (obj.transactionStatus && obj.transactionStatus !== 'No actions') {
                transactionStatus = obj.transactionStatus;
            } else if (transactionStatus === 'No actions') {
                transactionStatus = obj.transactionStatus;
            }
        }
    });

    if (transactionStatus !== 'No actions' && transactionStatus !== 'Rejected') {
        updateStatusReportSubmittedClientHasTrackingTool();
    }

    if (holidaysCount > 0) {
        holidaysContainer.innerHTML = `<label>You have ${holidaysCount} holiday${holidaysCount === 1 ? '' : 's'} to be reimbursed for this period</label> <div style="display:flex; justify-content:center">${holidaysHtmlList}</div>`;
        displayElement(holidaysContainer, 'block');
        initializeTooltips();
    } else {
        holidaysContainer.style.display = 'none';
    }

    updateInfoText(primaryUploadArea, primaryInfoText);
    updateInfoText(secondUploadArea, secondInfoText);
    validateUploadedFilesToTogglePreviewBtn();

    quantityInput.value = normalHoursQuantity;
    onCallFlateRateSelect.value = onCallFlateRateQuantity;
    onCallTimeWorkedInput.value = onCallTimeWorkedQuantity;
    notesInput.value = notes;
    displayElement(noTrackingToolSection, 'block');
}



// File deletion
async function deleteFile(fileName, statusLabel, deleteBtn, spinnerLabel) {
    if (!fileName) {
        console.error('File name must be provided.');
        return;
    }

    primaryFileInput.value = '';
    secondFileInput.value = '';
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

function validateUploadedFilesToTogglePreviewBtn() {
    const previewSectionPrimary = document.querySelector('#primary-upload-files-input .preview-files-section');
    const previewSectionSecond = document.querySelector('#second-upload-files-input .preview-files-section');
    const primaryUploadArea = document.getElementById('file-upload-name-primary');
    const secondUploadArea = document.getElementById('file-upload-name-second');
    const primaryInfoText = document.getElementById('info-text-primary');
    const secondInfoText = document.getElementById('info-text-second');
    const globalPreviewBtn = document.getElementById('preview-uploaded-files-btn');

    const primaryState = fileUploadStates['file-upload-primary'];
    const secondState = fileUploadStates['file-upload-second'];

    const primaryDOMFiles = primaryUploadArea ? primaryUploadArea.querySelectorAll('.row-selected-file') : [];
    const secondDOMFiles = secondUploadArea ? secondUploadArea.querySelectorAll('.row-selected-file') : [];

    // Update state blobNames arrays based on current DOM content
    primaryState.blobNames = Array.from(primaryDOMFiles).map(fileEl => {
        const anchor = fileEl.querySelector('a');
        const blobName = anchor ? anchor.textContent : '';
        const blobUrl = anchor ? anchor.href : '';
        return {
            BlobName: blobName,
            BlobUrl: blobUrl,
            PrimaryReportTrackingToolName: 'yes',
            SecondReportTrackingToolName: null
        };
    });

    secondState.blobNames = Array.from(secondDOMFiles).map(fileEl => {
        const anchor = fileEl.querySelector('a');
        const blobName = anchor ? anchor.textContent : '';
        const blobUrl = anchor ? anchor.href : '';
        return {
            BlobName: blobName,
            BlobUrl: blobUrl,
            PrimaryReportTrackingToolName: null,
            SecondReportTrackingToolName: 'yes'
        };
    });

    // Hide delete buttons if status is not editable
    const deleteBtns = document.querySelectorAll('.row-selected-file .delete-btn');
    deleteBtns.forEach(btn => {
        btn.style.display = (transactionStatus === 'No actions' || transactionStatus === 'Rejected') ? 'block' : 'none';
    });

    // Sync info text visibility with DOM
    if (primaryInfoText && primaryUploadArea) {
        primaryInfoText.style.display = primaryUploadArea.children.length === 0 ? 'block' : 'none';
    }
    if (secondInfoText && secondUploadArea) {
        secondInfoText.style.display = secondUploadArea.children.length === 0 ? 'block' : 'none';
    }

    // Sync preview section visibility
    if (previewSectionPrimary) {
        previewSectionPrimary.style.display = primaryUploadArea && primaryUploadArea.children.length > 0 ? 'flex' : 'none';
    }
    if (previewSectionSecond) {
        previewSectionSecond.style.display = secondUploadArea && secondUploadArea.children.length > 0 ? 'flex' : 'none';
    }

    // Also check actual state lists (edge cases where DOM not yet synced)
    const primaryHasFiles = (primaryUploadArea?.children.length ?? 0) > 0 || (primaryState?.fileList?.length ?? 0) > 0 || (primaryState?.blobNames?.length ?? 0) > 0;
    const secondHasFiles = (secondUploadArea?.children.length ?? 0) > 0 || (secondState?.fileList?.length ?? 0) > 0 || (secondState?.blobNames?.length ?? 0) > 0;

    // Control global preview button
    if (globalPreviewBtn) {
        globalPreviewBtn.style.display = (primaryHasFiles || secondHasFiles) ? 'inline-block' : 'none';
    }

    // Also reset file inputs if no files left to allow re-upload of same file
    if (primaryUploadArea?.children.length === 0 && primaryFileInput) primaryFileInput.value = '';
    if (secondUploadArea?.children.length === 0 && secondFileInput) secondFileInput.value = '';

    const primaryCount = primaryUploadArea ? primaryUploadArea.querySelectorAll('.row-selected-file').length : 0;
    const secondCount = secondUploadArea ? secondUploadArea.querySelectorAll('.row-selected-file').length : 0;

    const primaryUploadBtn = document.getElementById('upload-btn-primary');
    const secondUploadBtn = document.getElementById('upload-btn-second');
    const primaryFileInputEl = document.getElementById('file-upload-primary');
    const secondFileInputEl = document.getElementById('file-upload-second');

    if (primaryUploadBtn && primaryFileInputEl) {
        const primaryDisabled = primaryCount >= MAX_FILES;
        primaryUploadBtn.disabled = primaryDisabled;
        primaryFileInputEl.disabled = primaryDisabled;
        primaryUploadBtn.title = primaryDisabled ? `Maximum of ${MAX_FILES} files reached.` : '';
    }

    if (secondUploadBtn && secondFileInputEl) {
        const secondDisabled = secondCount >= MAX_FILES;
        secondUploadBtn.disabled = secondDisabled;
        secondFileInputEl.disabled = secondDisabled;
        secondUploadBtn.title = secondDisabled ? `Maximum of ${MAX_FILES} files reached.` : '';
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
    let cleaned = fileName.replace(regex, '');

    cleaned = cleaned.replace(/(?:^|_)(primary|second)_?/, '');

    return cleaned;
}


function previewPrimaryUploadedFiles(modalId) {
    previewContainer.innerHTML = "";
    const primaryState = fileUploadStates['file-upload-primary'];

    primaryState.blobNames.forEach(blob => {
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

function previewSecondUploadedFiles(modalId) {
    previewContainer.innerHTML = "";
    const secondState = fileUploadStates['file-upload-second'];

    secondState.blobNames.forEach(blob => {
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
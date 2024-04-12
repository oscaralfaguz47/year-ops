let quantityInput = document.getElementById('quantityInput');
let notesInput = document.getElementById('notesInput');
let movementIdInput = document.getElementById('movementIdInput');
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

// Handle drop event
dropArea.addEventListener('drop', handleFiles);

// Function to process files when dropping or selecting
function handleFiles(event) {
    event.stopPropagation();
    event.preventDefault();
    dropArea.classList.remove('dragover');
    const newFiles = event.dataTransfer ? event.dataTransfer.files : event.target.files;
    processFiles(newFiles);
}

function processFiles(newFiles) {
    Array.from(newFiles).forEach(file => {
        if (isValidFileType(file) && isValidFileSize(file) && !isDuplicate(file)) {
            addFileToList(file);
        }
    });
    updateFileDisplay();
}

function addFileToList(file) {
    fileList.push(file);
}

function isDuplicate(newFile) {
    return fileList.some(file => file.name === newFile.name && file.size === newFile.size);
}

// Validate file type
function isValidFileType(file) {
    const fileExtension = file.name.split('.').pop().toLowerCase();
    const validExtensions = ['pdf', 'jpg', 'jpeg', 'png', 'gif', 'svg', 'doc', 'docx', 'xls', 'xlsx', 'csv', 'txt'];
    if (!validExtensions.includes(fileExtension)) {
        alert(`Only PDF, images, Word, Excel, and TXT files are allowed. You tried to upload a file with extension .${fileExtension}.`);
        return false;
    }
    return true;
}

// Validate file size
function isValidFileSize(file) {
    if (file.size > maxFileSize) {
        alert(`File size should not exceed 10MB. You tried to upload a file of size ${Math.round(file.size / 1024 / 1024)}MB.`);
        return false;
    }
    return true;
}

// Update file display and instruction message
function updateFileDisplay() {
    const uploadArea = document.getElementById('file-upload-name');
    uploadArea.innerHTML = '';  

    fileList.forEach((file, index) => {
        const fileElement = document.createElement('div');
        fileElement.className = 'row-selected-file'; 

        const deleteBtn = document.createElement('button');
        deleteBtn.className = 'delete-btn';
        deleteBtn.innerHTML = '<i class="fa-solid fa-trash-can"></i>';  
        deleteBtn.onclick = function () {
            fileList.splice(index, 1);
            updateFileDisplay();
        };

        const fileName = document.createElement('span');
        fileName.textContent = file.name;

        fileElement.appendChild(deleteBtn);
        fileElement.appendChild(fileName);
        uploadArea.appendChild(fileElement);
    });

    const infoText = document.getElementById('info-text');
    infoText.style.display = fileList.length > 0 ? 'none' : 'block';
}


document.getElementById('file-upload').setAttribute('accept',
    '.pdf, .jpg, .jpeg, .png, .gif, .svg, .doc, .docx, .xls, .xlsx, .csv, .txt');

document.getElementById('file-upload').addEventListener('change', handleFiles);

document.addEventListener('paste', (event) => {
    const items = (event.clipboardData || event.originalEvent.clipboardData).items;
    for (const item of items) {
        if (item.kind === 'file') {
            const file = item.getAsFile();
            if (file && file.type.startsWith('image/')) { // Only allows pasting if it is an image
                if (!isDuplicate(file)) {
                    addFileToList(file);
                    updateFileDisplay();
                }
            }
        }
    }
});



//CREATE, UPDATE TIME ENTRY MOVEMENT
async function createUpdateTimeEntry() {
    const actionDate = new Date(dateFromInput.value);
    var token = $('[name="__RequestVerificationToken"]').val();
    let movementIdData = movementIdInput.value || null  ;
    const formData = new FormData();
    function appendIfValid(key, value) {
        if (value) {
            formData.append(key, value);
        }
    }


    // Add each file to formData
    fileList.forEach(file => {
        formData.append('files', file);
    });
    appendIfValid('reportMovementData.MovementId', movementIdInput.value);
    appendIfValid('reportMovementData.ProjectId', document.getElementById('projectId').value);
    appendIfValid('reportMovementData.Quantity', quantityInput.value);
    appendIfValid('reportMovementData.ActionDate', new Date(dateFromInput.value).toISOString());
    appendIfValid('reportMovementData.Notes', notesInput.value);

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
            movementIdInput.value = data.createdMovement;
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
    validateInputTypeNumber('onCallTimeWorked');
    validateInputTypeNumber('quantityInput');
});
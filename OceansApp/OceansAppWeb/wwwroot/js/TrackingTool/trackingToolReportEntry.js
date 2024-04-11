const dropArea = document.querySelector('.file-upload-wrapper');
let quantityInput = document.getElementById('quantityInput');
let notesInput = document.getElementById('notesInput');
let movementIdInput = document.getElementById('movementIdInput');

// Highlight the drop zone when dragging files over it
dropArea.addEventListener('dragover', (event) => {
    event.stopPropagation();
    event.preventDefault();
    dropArea.classList.add('dragover');
});

// Revert highlighting when files are no longer being dragged over the area
dropArea.addEventListener('dragleave', (event) => {
    dropArea.classList.remove('dragover');
});

// Handle the file drop event
dropArea.addEventListener('drop', (event) => {
    event.stopPropagation();
    event.preventDefault();
    dropArea.classList.remove('dragover');

    const files = event.dataTransfer.files;
    document.getElementById('file-upload').files = files;

    updateFileNames(files);
});

function updateFileNames(files) {
    var fileNames = Array.from(files).map(file => file.name).join(', ');
    document.getElementById('file-upload-name').textContent = fileNames || 'Ningún archivo seleccionado...';
}

document.getElementById('file-upload').addEventListener('change', function () {
    updateFileNames(this.files);
});

document.addEventListener('paste', (event) => {
    const items = (event.clipboardData || event.originalEvent.clipboardData).items;
    for (const item of items) {
        if (item.kind === 'file') {
            const file = item.getAsFile();
            const fileList = [file];
            processFiles(fileList);
        }
    }
});

function processFiles(files) {
    const fileNames = Array.from(files).map(file => file.name).join(', ');
    document.getElementById('file-upload-name').textContent = fileNames || 'Ningún archivo seleccionado...';
}

dropArea.addEventListener('drop', (event) => {
    event.stopPropagation();
    event.preventDefault();
    dropArea.classList.remove('dragover');

    const files = event.dataTransfer.files;
    processFiles(files);
});

document.getElementById('file-upload').addEventListener('change', function () {
    processFiles(this.files);
});

//CREATE, UPDATE TIME ENTRY MOVEMENT
async function createUpdateTimeEntry() {
    const actionDate = new Date(dateFromInput.value);
    var token = $('[name="__RequestVerificationToken"]').val();
    let movementIdData = movementIdInput.value || null  ;

    var data = {
        MovementId: movementIdData,
        ProjectId: document.getElementById('projectId').value || null,
        Quantity: quantityInput.value || null,
        ActionDate: actionDate.toISOString(),
        Notes: notesInput.value || null
    };
    console.log(data);
    fetch('/TrackingTool/ReportingMyTime/CreateUpdateTimeEntryClientNoTrackingTool', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            RequestVerificationToken: token
        },
        body: JSON.stringify(data)
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
                    console.log('BODY: ' + body);
                    if (body.errors) {
                        console.error("Validation errors:", body.errors);
                        // Iterates and shows validation errors
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
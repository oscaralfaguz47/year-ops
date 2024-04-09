const dropArea = document.querySelector('.file-upload-wrapper');

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


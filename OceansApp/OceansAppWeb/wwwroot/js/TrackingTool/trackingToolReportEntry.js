const dropArea = document.querySelector('.file-upload-wrapper');

// Resaltar la zona de arrastre al arrastrar archivos sobre ella
dropArea.addEventListener('dragover', (event) => {
    event.stopPropagation();
    event.preventDefault();
    dropArea.classList.add('dragover');
});

// Revertir el resaltado cuando los archivos ya no estén siendo arrastrados sobre la zona
dropArea.addEventListener('dragleave', (event) => {
    dropArea.classList.remove('dragover');
});

// Manejar el evento de soltar los archivos
dropArea.addEventListener('drop', (event) => {
    event.stopPropagation();
    event.preventDefault();
    dropArea.classList.remove('dragover');

    // Obtener los archivos del evento
    const files = event.dataTransfer.files;
    // Actualizar el input de archivo manualmente
    document.getElementById('file-upload').files = files;

    updateFileNames(files);
});

// Función para actualizar los nombres de los archivos en la UI
function updateFileNames(files) {
    var fileNames = Array.from(files).map(file => file.name).join(', ');
    document.getElementById('file-upload-name').textContent = fileNames || 'Ningún archivo seleccionado...';
}

// Evento de cambio para el input de archivo
document.getElementById('file-upload').addEventListener('change', function () {
    updateFileNames(this.files);
});

// Añadir el evento paste al documento o a un área específica
document.addEventListener('paste', (event) => {
    const items = (event.clipboardData || event.originalEvent.clipboardData).items;
    for (const item of items) {
        if (item.kind === 'file') {
            const file = item.getAsFile();

            // Simula un objeto FileList ya que no se puede modificar directamente
            const fileList = [file];
            processFiles(fileList);
        }
    }
});

// Función para procesar los archivos
function processFiles(files) {
    const fileNames = Array.from(files).map(file => file.name).join(', ');
    document.getElementById('file-upload-name').textContent = fileNames || 'Ningún archivo seleccionado...';
    // Opcionalmente, actualiza el input de archivo o realiza acciones adicionales con los archivos aquí
    // Nota: No es posible asignar directamente a input.files aquí debido a restricciones de seguridad
}

// Reutilizar la función processFiles para el evento drop y el cambio del input de archivo
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


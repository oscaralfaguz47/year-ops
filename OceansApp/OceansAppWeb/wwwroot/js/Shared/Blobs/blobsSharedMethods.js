function getFileTypeFromUrl(url) {
    const lastSegment = url.split('/').pop();

    const parts = lastSegment.split('.');

    if (parts.length > 1) {
        return parts.pop().split('?')[0]; 
    } else {
        return 'Unknown'; 
    }
}

function getFileType(fileType) {
    const imageTypes = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'tiff', 'svg', 'webp', 'ico', 'heic'];

    if (fileType === 'pdf') {
        return 'pdf';
    }
    else if (imageTypes.includes(fileType)) {
        return 'image';
    }
    else {
        return 'other';
    }
}


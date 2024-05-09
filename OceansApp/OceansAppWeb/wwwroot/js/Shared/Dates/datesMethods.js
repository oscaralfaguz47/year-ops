function formatDateMmDdYyyy(date) {
    let dateToConvert = new Date(date);
    let day = dateToConvert.getUTCDate().toString().padStart(2, '0');
    let month = (dateToConvert.getUTCMonth() + 1).toString().padStart(2, '0');
    let year = dateToConvert.getUTCFullYear();
    return `${month}/${day}/${year}`;
}

function formatDateMmDdYyyy(date) {
    let dateToConvert = new Date(date);
    let day = dateToConvert.getUTCDate().toString().padStart(2, '0');
    let month = (dateToConvert.getUTCMonth() + 1).toString().padStart(2, '0');
    let year = dateToConvert.getUTCFullYear();
    return `${month}/${day}/${year}`;
}

function formatUtcToLocalMmDdYyyyTime(dateToFormat) {
    var formattedDate = new Date(dateToFormat);
    if (formattedDate instanceof Date && !isNaN(formattedDate)) {
        let differenceMinutes = formattedDate.getTimezoneOffset();
        let localDateSent = new Date(formattedDate.getTime() - differenceMinutes * 60000);
        dateToReturn = localDateSent.toLocaleString('en-US', {
            month: '2-digit',
            day: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            hour12: true,
        });
        return dateToReturn;
    }
}
function getMonthName(monthNumber) {
    const monthNames = [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];
    const monthName = monthNames[monthNumber];
    return monthName;
}
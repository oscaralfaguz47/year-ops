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
function formatDateMonthDateSuffix(date) {
    const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    const day = date.getDate();
    const month = months[date.getMonth()];
    function getDaySuffix(day) {
        if (day > 3 && day < 21) return 'th';
        switch (day % 10) {
            case 1: return 'st';
            case 2: return 'nd';
            case 3: return 'rd';
            default: return 'th';
        }
    }
    return `${month} ${day}${getDaySuffix(day)}`;
}

// Ejemplo de uso:
const date = new Date('2024-12-25');
console.log(formatDate(date));  // Output: "Dec 25th"

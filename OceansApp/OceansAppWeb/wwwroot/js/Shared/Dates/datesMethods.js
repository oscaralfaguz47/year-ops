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

function getNormalizedDates(dateFromInputEl, dateToInputEl) {
    if (!dateFromInputEl.value || !dateToInputEl.value) {
        throw new Error("Date values ​​cannot be empty.");
    }

    const startDateParts = dateFromInputEl.value.split('/');
    const endDateParts = dateToInputEl.value.split('/');

    const startDateData = new Date(
        Date.UTC(
            parseInt(startDateParts[2]),
            parseInt(startDateParts[0]) - 1, 
            parseInt(startDateParts[1]) 
        )
    );

    const endDateData = new Date(
        Date.UTC(
            parseInt(endDateParts[2]), 
            parseInt(endDateParts[0]) - 1, 
            parseInt(endDateParts[1]) 
        )
    );

    return { startDate: startDateData.toISOString(), endDate: endDateData.toISOString() };
}

function getNormalizedOneDate(dateInputEl) {
    if (!dateInputEl.value) {
        throw new Error("Date value ​​cannot be empty.");
    }

    const dateParts = dateInputEl.value.split('/');

    const dateData = new Date(
        Date.UTC(
            parseInt(dateParts[2]),
            parseInt(dateParts[0]) - 1,
            parseInt(dateParts[1])
        )
    );

    return { normalizedDate: dateData.toISOString() };
}
function convertDateStringToMMDDYYYY(dateString) {
    const date = new Date(dateString);

    if (isNaN(date)) {
        throw new Error("Invalid date string format");
    }

    const month = (date.getMonth() + 1).toString().padStart(2, '0'); 
    const day = date.getDate().toString().padStart(2, '0'); 
    const year = date.getFullYear(); 

    return `${month}/${day}/${year}`;
}

function convertNormalizedDate(dateInputEl) {
    if (!dateInputEl.value) {
        throw new Error("Date value ​​cannot be empty.");
    }

    const dateParts = dateInputEl.value.split('/');

    const dateData = new Date(
        Date.UTC(
            parseInt(dateParts[2]),
            parseInt(dateParts[0]) - 1,
            parseInt(dateParts[1])
        )
    );

    return { normalizedDate: dateData };
}
// Helper to avoid UTC misinterpretation of date strings like '2025-06-16'
function parseLocalDate(yyyyMmDd) {
    const [year, month, day] = yyyyMmDd.split('-').map(Number);
    return new Date(year, month - 1, day);
}

function formatDateYyyyMmDd(date) {
    let day = date.getDate().toString().padStart(2, '0');
    let month = (date.getMonth() + 1).toString().padStart(2, '0');
    let year = date.getFullYear();
    return `${year}-${month}-${day}`;
}
let paymentPeriod = 0;
let currentDate = new Date();


// Function to calculate the new period based on direction and mode.
function adjustDate(direction, mode) {
    const dayAdjustment = mode === 1 ? 15 : new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 0).getDate();

    if (direction === 'left') {
        currentDate = new Date(currentDate.setDate(currentDate.getDate() - dayAdjustment));
    } else if (direction === 'right') {
        currentDate = new Date(currentDate.setDate(currentDate.getDate() + dayAdjustment));
    }
}

const formatDate = (date) => {
    let month = '' + (date.getMonth() + 1),
        day = '' + date.getDate(),
        year = date.getFullYear();

    if (month.length < 2)
        month = '0' + month;
    if (day.length < 2)
        day = '0' + day;

    return [month, day, year].join('/');
};
function formatDateYyyyMmDd(date) {
    let day = date.getDate().toString().padStart(2, '0');
    let month = (date.getMonth() + 1).toString().padStart(2, '0');
    let year = date.getFullYear();
    return `${year}-${month}-${day}`;
}
// Calculates and displays start and end dates based on the click direction.
const handleButtonClick = (direction) => {
    adjustDate(direction, paymentPeriod, null);
    let buttons = [document.getElementById('previousBtn'), document.getElementById('nextBtn')];
    buttons.forEach(btn => {
        if (btn) btn.disabled = true;
    });
    let { startDate, endDate } = calculatePeriod(currentDate, paymentPeriod, buttons);
};

const calculatePeriod = (date, mode, buttons) => {
    let startDate, endDate;
    if (mode === 1) { // Biweekly
        // Adjusts to the nearest fortnight before the current date
        const dayOfMonth = date.getDate();
        if (dayOfMonth <= 15) {
            startDate = new Date(date.getFullYear(), date.getMonth(), 1);
            endDate = new Date(date.getFullYear(), date.getMonth(), 15);
        } else {
            startDate = new Date(date.getFullYear(), date.getMonth(), 16);
            endDate = new Date(date.getFullYear(), date.getMonth() + 1, 0);
        }
    } else if (mode === 2) { // Montly
        startDate = new Date(date.getFullYear(), date.getMonth(), 1);
        endDate = new Date(date.getFullYear(), date.getMonth() + 1, 0);
    }

    document.getElementById('previous-date').innerHTML = `<span> ${getMonthName(startDate.getMonth())} ${startDate.getDate()}, ${startDate.getFullYear()}</span>`;
    document.getElementById('next-date').innerHTML = `<span>${getMonthName(endDate.getMonth())} ${endDate.getDate()}, ${startDate.getFullYear()}</span>`;
    dateToInput.value = formatDate(endDate);
    dateFromInput.value = formatDate(startDate);
    navitateBetweenDates(formatDateYyyyMmDd(startDate), formatDateYyyyMmDd(endDate), buttons);
    return { startDate, endDate };
};

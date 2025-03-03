let paymentPeriod = 0;
let currentDate = new Date();


// Calculate the new period based on direction and mode.
function adjustDate(direction, mode) {
    let year = currentDate.getFullYear();
    let month = currentDate.getMonth();
    let day = currentDate.getDate();

    if (Number(mode) === 1) {
        if (day <= 15) {
            if (direction === 'left') {
                month -= 1; 
                day = 16; 
            } else {
                day = 16; 
            }
        } else {
            if (direction === 'left') {
                day = 1; 
            } else {
                month += 1; 
                day = 1; 
            }
        }
    } else { 
        month = (direction === 'left') ? month - 1 : month + 1;
        day = 1; 
    }

    let lastDay = new Date(year, month + 1, 0).getDate();
    if (day > lastDay) {
        day = lastDay; 
    }

    currentDate = new Date(year, month, day);
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
const handleButtonClick = async (direction) => {
    adjustDate(direction, paymentPeriod, null);
    let buttons = [getElementById('previousBtn'), getElementById('nextBtn')];
    buttons.forEach(btn => {
        if (btn) btn.disabled = true;
    });
    let { startDate, endDate } = await calculatePeriod(currentDate, paymentPeriod, buttons);
};

const calculatePeriod = async (date, mode, buttons) => {
    let startDate, endDate;
    const dayOfMonth = date.getDate();

    if (Number(mode) === 1) { //Beweekly
        if (dayOfMonth <= 15) {
            startDate = new Date(date.getFullYear(), date.getMonth(), 1);
            endDate = new Date(date.getFullYear(), date.getMonth(), 15);
        }
        else {
            startDate = new Date(date.getFullYear(), date.getMonth(), 16);
            endDate = new Date(date.getFullYear(), date.getMonth() + 1, 0);
        }
    } else if (Number(mode) === 2) { //Monthly
        startDate = new Date(date.getFullYear(), date.getMonth(), 1);
        endDate = new Date(date.getFullYear(), date.getMonth() + 1, 0);
    }

    getElementById('previous-date').innerHTML = `<span class="month-date-lb"> ${getMonthName(startDate.getMonth()).slice(0, 3) } ${startDate.getDate()}</span>`;
    getElementById('next-date').innerHTML = `<span class="month-date-lb">${getMonthName(endDate.getMonth()).slice(0, 3)} ${endDate.getDate()}</span><span>, (${startDate.getFullYear()})</span>`;
    dateToInput.value = formatDate(endDate);
    dateFromInput.value = formatDate(startDate);
    await navitateBetweenDates(formatDateYyyyMmDd(startDate), formatDateYyyyMmDd(endDate), buttons);


    return { startDate, endDate };
};


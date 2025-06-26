let paymentPeriod = 0;
let currentDate = new Date();

let lastAppliedStartDate = null;
let lastAppliedEndDate = null;

function parseLocalDate(dateStr) {
    const [year, month, day] = dateStr.split('-').map(Number);
    return new Date(year, month - 1, day);
}

function isValidDateString(dateStr) {
    const regex = /^\d{4}-\d{2}-\d{2}$/;
    if (!regex.test(dateStr)) return false;

    const date = new Date(dateStr);
    return !isNaN(date.getTime());
}
function initializeCurrentDateFromUrl() {
    const urlParams = new URLSearchParams(window.location.search);
    const startDateParam = urlParams.get('startDate');
    const endDateParam = urlParams.get('endDate');

    const hasValidStart = startDateParam && isValidDateString(startDateParam);
    const hasValidEnd = endDateParam && isValidDateString(endDateParam);

    if (hasValidStart && hasValidEnd) {
        const start = parseLocalDate(startDateParam);
        const end = parseLocalDate(endDateParam);
        const mode = Number(document.getElementById('PaymentPeriodInput')?.value || 1);

        if (isValidPeriod(start, end, mode)) {
            currentDate = start;
            lastAppliedStartDate = start;
            lastAppliedEndDate = end;
            return;
        }
    }

    // If the parameters are incorrect or manipulated, clean the URL
    const cleanUrl = new URL(window.location.href);
    cleanUrl.searchParams.delete('startDate');
    cleanUrl.searchParams.delete('endDate');
    window.history.replaceState({}, '', cleanUrl);

    // Reset to current date
    currentDate = new Date();
}

function isValidPeriod(start, end, mode) {
    const startDay = start.getDate();
    const endDay = end.getDate();
    const startMonth = start.getMonth();
    const endMonth = end.getMonth();
    const startYear = start.getFullYear();
    const endYear = end.getFullYear();

    if (start > end) return false;
    if (startMonth !== endMonth || startYear !== endYear) return false;

    const lastDay = new Date(startYear, startMonth + 1, 0).getDate();

    if (Number(mode) === 1) {
        return (startDay === 1 && endDay === 15) || (startDay === 16 && endDay === lastDay);
    } else {
        return startDay === 1 && endDay === lastDay;
    }
}


const formatDate = (date) => {
    let month = '' + (date.getMonth() + 1),
        day = '' + date.getDate(),
        year = date.getFullYear();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return [month, day, year].join('/');
};

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
    if (day > lastDay) day = lastDay;

    currentDate = new Date(year, month, day);
}

const handleButtonClick = async (direction) => {
    adjustDate(direction, paymentPeriod);
    let buttons = [getElementById('previousBtn'), getElementById('nextBtn')];
    buttons.forEach(btn => {
        if (btn) btn.disabled = true;
    });
    let { startDate, endDate } = await calculatePeriod(currentDate, paymentPeriod, buttons);
};

// Calculate the period and update UI + URL
const calculatePeriod = async (date, mode, buttons, overrideStartDate = null, overrideEndDate = null, forceInitialLoad = false) => {
    let startDate, endDate;

    if (overrideStartDate && overrideEndDate) {
        startDate = overrideStartDate;
        endDate = overrideEndDate;

        if (
            !forceInitialLoad &&
            lastAppliedStartDate &&
            lastAppliedEndDate &&
            formatDateYyyyMmDd(startDate) === formatDateYyyyMmDd(lastAppliedStartDate) &&
            formatDateYyyyMmDd(endDate) === formatDateYyyyMmDd(lastAppliedEndDate)
        ) {
            console.log('⏭️ Same period detected, skipping recalculation...');
            return;
        }
    } else {
        const dayOfMonth = date.getDate();

        if (Number(mode) === 1) {
            if (dayOfMonth <= 15) {
                startDate = new Date(date.getFullYear(), date.getMonth(), 1);
                endDate = new Date(date.getFullYear(), date.getMonth(), 15);
            } else {
                startDate = new Date(date.getFullYear(), date.getMonth(), 16);
                endDate = new Date(date.getFullYear(), date.getMonth() + 1, 0);
            }
        } else if (Number(mode) === 2) {
            startDate = new Date(date.getFullYear(), date.getMonth(), 1);
            endDate = new Date(date.getFullYear(), date.getMonth() + 1, 0);
        }
    }

    lastAppliedStartDate = new Date(startDate);
    lastAppliedEndDate = new Date(endDate);

    getElementById('previous-date').innerHTML = `<span class="month-date-lb">${getMonthName(startDate.getMonth()).slice(0, 3)} ${startDate.getDate()}</span>`;
    getElementById('next-date').innerHTML = `<span class="month-date-lb">${getMonthName(endDate.getMonth()).slice(0, 3)} ${endDate.getDate()}</span><span>, (${startDate.getFullYear()})</span>`;
    dateToInput.value = formatDate(endDate);
    dateFromInput.value = formatDate(startDate);

    await navitateBetweenDates(formatDateYyyyMmDd(startDate), formatDateYyyyMmDd(endDate), buttons);

    const newUrl = new URL(window.location.href);
    newUrl.searchParams.set('startDate', formatDateYyyyMmDd(startDate));
    newUrl.searchParams.set('endDate', formatDateYyyyMmDd(endDate));
    window.history.replaceState({}, '', newUrl);

    return { startDate, endDate };
};


function generateDateList(startDateString, endDateString) {
    const startDate = new Date(startDateString + 'T00:00:00');
    const endDate = new Date(endDateString + 'T23:59:59');
    const dateListContainer = document.getElementById('dateList');
    dateListContainer.innerHTML = '';
    let currentDate = new Date(startDate.getTime());

    const hoursCountDiv = document.createElement('div');
    hoursCountDiv.id = 'total-hours-label'; // Añadir un ID para identificar fácilmente este div
    hoursCountDiv.innerHTML = '<span class="total">TOTAL TIME: <span class="hours-minutes">0 hours - 0 minutes</span></span>';
    dateListContainer.insertBefore(hoursCountDiv, dateListContainer.firstChild);

    while (currentDate <= endDate) {
        const dayItem = document.createElement('div');
        dayItem.className = 'day-item';
        const formattedDate = currentDate.toLocaleDateString('en-US', { weekday: 'long', day: 'numeric', month: 'long' });
        const weekday = currentDate.toLocaleDateString('en-US', { weekday: 'long' });
        const noReportNeededLabel = weekday === 'Sunday' || weekday === 'Saturday' ? '<label class="non-rep-label">- Generally Non-reportable day</label>' : '';
        const dateValue = `${currentDate.getFullYear()}-${String(currentDate.getMonth() + 1).padStart(2, '0')}-${String(currentDate.getDate()).padStart(2, '0')}`;
        dayItem.innerHTML = `<label class="day-label">${formattedDate}</label> <button class="btn-add-time" onclick="addTimeEntry(this, '${dateValue}')">+ Add Time</button> <label class="count-day-label" data-value="0">0 h - 0 m</label> ${noReportNeededLabel}`;
        dateListContainer.appendChild(dayItem);
        currentDate.setDate(currentDate.getDate() + 1); 
    }
}

function addTimeEntry(button, date) {
    const timeEntryDiv = document.createElement('div');
    timeEntryDiv.className = 'time-entry';
    timeEntryDiv.innerHTML = `
        <span>From</span><input type="time" class="time-from input-time" value="08:00"/><span>To</span>
        <input type="time" class="time-to input-time" value="16:00"/>
        <label class="count-time"></label>
        <input type="text" placeholder="Detail" class="time-detail input-time"/>
        <button class="btn-delete-time" onclick="deleteTimeEntry(this)"><i class="bi bi-trash"></i></button>
        <button class="btn-save-time" onclick="saveTimeEntry(this, '${date}')"><i class="fa-solid fa-floppy-disk"></i></button>
        <label class="time-label"></label>
    `;
    button.parentElement.appendChild(timeEntryDiv);

    const timeFromInput = timeEntryDiv.querySelector('.time-from');
    const timeToInput = timeEntryDiv.querySelector('.time-to');
    const timeLabel = timeEntryDiv.querySelector('.count-time');

    const updateTimeDifference = () => {
        const fromTime = timeFromInput.value;
        const toTime = timeToInput.value;
        const validFromTime = fromTime === '' ? '00:00' : fromTime;
        const validToTime = toTime === '' ? '00:00' : toTime;

        const difference = calculateTimeDifference(validFromTime, validToTime);
        if (difference.hours >= 0 && difference.minutes >= 0) {
            timeLabel.textContent = `${difference.hours} h - ${difference.minutes} m`;
        } else {
            timeLabel.textContent = '0 h - 0 m';
        }
        const dayItem = button.closest('.day-item');
        updateDayTotal(dayItem);
        updateTotalHours();
    };

    timeFromInput.addEventListener('input', updateTimeDifference);
    timeToInput.addEventListener('input', updateTimeDifference);

    updateTimeDifference();
}


function calculateTimeDifference(startTime, endTime) {
    const startTimeDate = new Date(`1970-01-01T${startTime}:00`);
    const endTimeDate = new Date(`1970-01-01T${endTime}:00`);
    let difference = endTimeDate - startTimeDate;
    if (difference < 0) {
        difference += 24 * 60 * 60 * 1000;
    }
    const hours = Math.floor(difference / (1000 * 60 * 60));
    const minutes = Math.round((difference % (1000 * 60 * 60)) / (1000 * 60));
    return { hours, minutes };
}


function deleteTimeEntry(button) {
    const dayItem = button.closest('.day-item');
    button.parentElement.remove();
    updateDayTotal(dayItem);
    updateTotalHours();
}

function saveTimeEntry(button, date) {
    const timeFrom = button.parentElement.querySelector('.time-from').value;
    const timeTo = button.parentElement.querySelector('.time-to').value;
    const detail = button.parentElement.querySelector('.time-detail').value;
    const timeLabel = button.parentElement.querySelector('.time-label');

    timeLabel.textContent = `Saved: ${date}, From: ${timeFrom}, To: ${timeTo}, Detail: ${detail}`;
}

function updateDayTotal(dayElement) {
    const timeEntries = dayElement.querySelectorAll('.time-entry');
    let totalMinutes = 0;

    timeEntries.forEach(entry => {
        const timeFrom = entry.querySelector('.time-from').value || "00:00";
        const timeTo = entry.querySelector('.time-to').value || "00:00";
        const difference = calculateTimeDifference(timeFrom, timeTo);
        totalMinutes += (difference.hours * 60) + difference.minutes;
    });

    const totalHours = Math.floor(totalMinutes / 60);
    const totalMinutesLeft = totalMinutes % 60;
    const countDayLabel = dayElement.querySelector('.count-day-label');
    countDayLabel.textContent = `${totalHours} h - ${totalMinutesLeft} m`;
}


function updateTotalHours() {
    const allDayLabels = document.querySelectorAll('.count-day-label');
    let totalMinutes = 0;

    allDayLabels.forEach(label => {
        const parts = label.textContent.match(/(\d+)\s*h\s*-\s*(\d+)\s*m/);
        if (parts && parts.length === 3) {
            const hours = parseInt(parts[1], 10);
            const minutes = parseInt(parts[2], 10);
            if (!isNaN(hours) && !isNaN(minutes)) { 
                totalMinutes += hours * 60 + minutes;
            }
        }
    });

    const totalHours = Math.floor(totalMinutes / 60);
    const totalMinutesLeft = totalMinutes % 60;
    const totalHoursLabel = document.getElementById('total-hours-label');
    if (totalHoursLabel) {
        totalHoursLabel.innerHTML = `<span class="total">TOTAL TIME: <span class="hours-minutes">${totalHours} hours - ${totalMinutesLeft} minutes</span></span>`;
    }
}

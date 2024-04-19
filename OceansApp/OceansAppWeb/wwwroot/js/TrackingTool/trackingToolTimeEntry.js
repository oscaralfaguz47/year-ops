//GET PROJECT MOVEMENTS
async function getTrackingToolProjectMovements() {
    loadingBoxIntern.style.display = 'block';
    errorMessageIntern.style.display = 'none';
    let tackingToolSection = document.getElementById('tracking-tool-sec');
    tackingToolSection.style.display = 'none';
    var startDateValue = encodeURIComponent(dateFromInput.value);
    var endDateValue = encodeURIComponent(dateToInput.value);
    var url = "/TrackingTool/ReportingMyTime/GetTrackingToolProjectMovements?projectId=" + encodeURIComponent(projectIdInput.value) +
        "&startDate=" + startDateValue + "&endDate=" + endDateValue;

    return fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    errorMessageIntern.style.display = 'block';
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .then(data => {
            tackingToolSection.style.display = 'block';
            return data;
        }).finally(() => {
            loadingBoxIntern.style.display = 'none';
        });
}
function generateDateList(startDateString, endDateString, movements) {
    console.log(movements);
    const startDate = new Date(startDateString + 'T00:00:00');
    const endDate = new Date(endDateString + 'T23:59:59');
    const dateListContainer = document.getElementById('dateList');
    dateListContainer.innerHTML = '';
    let currentDate = new Date(startDate.getTime());

    const hoursCountDiv = document.createElement('div');
    hoursCountDiv.id = 'total-hours-label';
    hoursCountDiv.innerHTML = '<span class="total">TOTAL TIME: <span class="hours-minutes">0 hours - 0 minutes</span></span>';
    dateListContainer.insertBefore(hoursCountDiv, dateListContainer.firstChild);

    while (currentDate <= endDate) {
        const dayItem = document.createElement('div');
        dayItem.className = 'day-item';

        const formattedDate = currentDate.toLocaleDateString('en-US', { weekday: 'long', day: 'numeric', month: 'long' });
        const dateLabel = document.createElement('label');
        dateLabel.className = 'day-label';
        dateLabel.textContent = formattedDate;

        const addButton = document.createElement('button');
        addButton.className = 'btn-add-time';
        addButton.textContent = '+ Add Time';
        attachOnClick(addButton, currentDate.toISOString().split('T')[0]); // Utiliza la función auxiliar

        const countLabel = document.createElement('label');
        countLabel.className = 'count-day-label';
        countLabel.setAttribute('data-value', '0');
        countLabel.textContent = '0 h - 0 m';

        const weekday = currentDate.toLocaleDateString('en-US', { weekday: 'long' });
        const nonReportNeededLabel = document.createElement('label');
        nonReportNeededLabel.className = 'non-rep-label';
        nonReportNeededLabel.textContent = weekday === 'Sunday' || weekday === 'Saturday' ? '- Generally Non-reportable day' : '';

        dayItem.appendChild(dateLabel);
        dayItem.appendChild(addButton);
        dayItem.appendChild(countLabel);
        dayItem.appendChild(nonReportNeededLabel);
        dateListContainer.appendChild(dayItem);
        movements.forEach(function (movement) {
            const movementDate = new Date(movement.actionDate);
            if (movementDate.toISOString().split('T')[0] === currentDate.toISOString().split('T')[0]) {
                addTimeEntry(addButton, currentDate.toISOString().split('T')[0], movement.movementId, movement.timeFrom, movement.timeTo,
                    movement.notes);
            }
        });
        currentDate.setDate(currentDate.getDate() + 1);
    }
}
function attachOnClick(button, date) {
    button.onclick = function () {
        addTimeEntry(this, date, null, null, null, null);
    };
}


function addTimeEntry(button, date, movementId, timeFrom, timeTo, notes) {
    const timeEntryDiv = document.createElement('div');
    timeEntryDiv.className = 'time-entry';
    timeEntryDiv.innerHTML = `
        <span>From</span><input type="time" class="time-from input-time" value="${timeFrom === null ? '08:00' : timeFrom}"/><span>To</span>
        <input type="hidden" class="movement-id" ${movementId === null ? 'value' : 'value="'+movementId+'"'}"/>
        <input type="time" class="time-to input-time" value="${timeTo === null ? '16:00' : timeTo}"/>
        <label class="count-time"></label>
        <input type="text" placeholder="Detail" class="time-detail input-time" value="${notes === null ? '' : notes}"/>
        <button class="btn-delete-time" onclick="deleteTimeEntry(this)"><i class="fa-solid fa-trash-can"></i></button>
        <button class="btn-save-time" onclick="saveTimeEntry(this, '${date}')"><i class="fa-solid fa-floppy-disk"></i></button>
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
    const notes = button.parentElement.querySelector('.time-detail').value;
    const movementId = button.parentElement.querySelector('.movement-id').value === '' ? null : button.parentElement.querySelector('.movement-id').value;

    createUpdateTimeEntryTrackingTool(movementId, notes, timeFrom, timeTo, date, button.parentElement.querySelector('.movement-id'));
}
//CREATE, UPDATE TIME ENTRY
async function createUpdateTimeEntryTrackingTool(movementId, notes, timeFrom, timeTo, date, movementIdInput) {
    let actionDateData = new Date(date).toISOString();

    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        MovementId: movementId,
        ProjectId: Number(projectIdInput.value),
        ActionDate: actionDateData,
        Notes: notes,
        TimeFrom: timeFrom,
        TimeTo: timeTo
    };
    console.log(data);
    fetch('/TrackingTool/ReportingMyTime/CreateUpdateTimeEntryTrackingTool', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            RequestVerificationToken: token
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    if (errorData.messageType === "Validation Error") {
                        displayToasterWarningArray(errorData.errors);
                        throw new Error('Validation errors!');
                    } else {
                        displayToasterError(errorData.error);
                        throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                    }
                });
            }
        })
        .then(data => {
            displayToasterSuccess(data.message);
            movementIdInput.value = data.movementId;
        });
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

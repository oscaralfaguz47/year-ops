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
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            loadingBoxIntern.style.display = 'none';
        });
}
function generateDateList(startDateString, endDateString, movements) {
    const startDate = new Date(startDateString + 'T00:00:00');
    const endDate = new Date(endDateString + 'T23:59:59');
    const dateListContainer = document.getElementById('dateList');
    dateListContainer.innerHTML = '';
    let currentDate = new Date(startDate.getTime());

    const hoursCountDiv = document.getElementById('total-hours-label');
    hoursCountDiv.innerHTML = `<span class="strong-label">Total Time Reported</span> <span class="gray-bold-span mb-2">0 Hours, 0 Minutes</span>`;

    const dateListBox = document.createElement('div');
    dateListBox.className = 'date-list-box';
    dateListContainer.append(dateListBox);

    while (currentDate <= endDate) {
        const dayItem = document.createElement('div');
        dayItem.className = 'day-item';

        const formattedDate = currentDate.toLocaleDateString('en-US', { weekday: 'short', day: 'numeric', month: 'long' });
        const dateLabel = document.createElement('label');
        dateLabel.className = 'day-label';
        const highlightedText = `<span class="highlight">${formattedDate.slice(0, 3)}</span>${formattedDate.slice(3)}`;
        dateLabel.innerHTML = highlightedText;
        dayItem.appendChild(dateLabel);

        const dayItemBox = document.createElement('div');
        dayItemBox.className = 'day-item-box';
        dayItem.appendChild(dayItemBox);

        const addButton = document.createElement('button');
        addButton.className = 'btn-add-time';
        addButton.textContent = '+ Add';
        addButton.addEventListener('click', function () {
            displayCreateUpdateTime('modal-update-create-time', formattedDate, null, addButton, null);
        });

        const countLabel = document.createElement('label');
        countLabel.className = 'count-day-label';
        countLabel.setAttribute('data-value', '0');
        countLabel.textContent = '0 h - 0 m';

        const weekday = currentDate.toLocaleDateString('en-US', { weekday: 'long' });
        if (weekday === 'Sunday' || weekday === 'Saturday') {
            addButton.style.backgroundColor = 'gray';
        }

        dayItemBox.appendChild(addButton);
        dayItem.appendChild(countLabel);
        dateListBox.appendChild(dayItem);
        submissionInfo.innerHTML = `<button style="background-color: ${getStatusColor('No Actions')}" id="submitBtn" onclick="submitReportToBePaid()">${getStatusWhiteIcon('No Actions')} Submit your time</button>`;
        movements.forEach(function (movement) {
            const movementDate = new Date(movement.actionDate);
            if (movement.transactionStatusName !== 'No actions' && movement.transactionStatusName !== 'Rejected') {
                submissionInfo.innerHTML = `<button style="background-color: ${getStatusColor(movement.transactionStatusName)}" id="submitBtn" onclick="submitReportToBePaid()">${getStatusWhiteIcon(movement.transactionStatusName)} 
                ${movement.transactionStatusName === 'Waiting to be approved' ? 'Pending approval' : movement.transactionStatusName === 'Approved' ? 'Timesheet approved' : movement.transactionStatusName}</button>`;
                addButton.style.display = 'none';
                let submitBtn = document.getElementById('submitBtn');
                submitBtn.disabled = true;
                submitBtn.className = 'submit-button-disabled';
            }
            if (movementDate.toISOString().split('T')[0] === currentDate.toISOString().split('T')[0]) {
                addTimeEntry(addButton, movement.movementId, movement.timeFrom, movement.timeTo, movement.transactionStatusName);
            }
        });
        currentDate.setDate(currentDate.getDate() + 1);
    }
}

function addTimeEntry(button, movementId, timeFrom, timeTo, transactionStatus) {
    const hoursMinutes = calculateTimeDifference(timeFrom, timeTo);
    const reportedTimeLabel = document.createElement('span');
    reportedTimeLabel.className = 'reported-time-span';
    reportedTimeLabel.innerHTML = `<span id="reportedTimeSpan-${ movementId }">${hoursMinutes.hours} h - ${hoursMinutes.minutes} m</span>`;

    const timeEntryDiv = document.createElement('div');
    timeEntryDiv.className = 'time-entry';
    timeEntryDiv.innerHTML = `
        <input type="hidden" class="time-from" value="${timeFrom}"/>
        <input type="hidden" class="movement-id" value="${movementId}"/>
        <input type="hidden" class="time-to" value="${timeTo}"/>
    `;

    const editBtn = document.createElement('button');
    editBtn.addEventListener('click', function () {
        displayCreateUpdateTime('modal-update-create-time', null, movementId, button, timeEntryDiv);
    });
    editBtn.className = 'edit-time-btn';
    editBtn.innerHTML = `<i class="fa-solid fa-pencil"></i>`;

    button.parentElement.appendChild(timeEntryDiv);
    timeEntryDiv.appendChild(reportedTimeLabel);
    if (transactionStatus === 'No actions' || transactionStatus === 'Rejected') {
        reportedTimeLabel.appendChild(editBtn);
    }

    const timeFromInput = timeEntryDiv.querySelector('.time-from');
    const timeToInput = timeEntryDiv.querySelector('.time-to');

    const updateTimeDifference = () => {

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

function saveTimeEntry(button, date) {
    var spinnerLabel = button.parentElement.querySelector('.spinner-time-actions');
    const deleteBtn = button.parentElement.querySelector('.btn-delete-time');
    const timeFrom = button.parentElement.querySelector('.time-from').value;
    const timeTo = button.parentElement.querySelector('.time-to').value;
    const notes = button.parentElement.querySelector('.time-detail').value;
    let movementId = button.parentElement.querySelector('.movement-id').value === '' ? null : button.parentElement.querySelector('.movement-id').value;
    const checkSavedIcon = button.parentElement.querySelector('.check-saved-icon');

    createUpdateTimeEntryTrackingTool(movementId, notes, timeFrom, timeTo, date, button.parentElement.querySelector('.movement-id'),
        spinnerLabel, button, checkSavedIcon).then(data => {
            if (data) {
                movementId = data.movementId; 
                button.parentElement.querySelector('.movement-id').value = movementId;
                deleteBtn.setAttribute('onclick', ''); 
                deleteBtn.setAttribute('onclick', `deleteTimeEntry(this, ${movementId})`); 
            }
        }).catch(error => {
            console.error("Error in saveTimeEntry:", error);
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
        totalHoursLabel.innerHTML = `<span class="strong-label">Total Time Reported</span> <span class="gray-bold-span mb-2">${totalHours} Hours, ${totalMinutesLeft} Minutes</span>`;
    }
}

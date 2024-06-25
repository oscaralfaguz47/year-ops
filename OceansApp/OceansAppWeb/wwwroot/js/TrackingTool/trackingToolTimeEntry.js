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
            displayCreateUpdateTime('modal-update-create-time', formattedDate, null, null);
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
        submissionInfo.innerHTML = `<strong>Have you reported all your hours accurately?</strong> <button onclick="submitReportToBePaid()"><i class="fa-regular fa-paper-plane"></i> Submit Report to get paid</button>`;
        movements.forEach(function (movement) {
            const movementDate = new Date(movement.actionDate);
            if (movement.transactionStatusName !== 'No actions' && movement.transactionStatusName !== 'Rejected') {
                submissionInfo.innerHTML = `<div style="margin-bottom:10px"> You have already submitted your report, and the current status is:</div > <span class="status-span">${getStatusLabel(movement.transactionStatusName)}</span>`;
                addButton.disabled = true;
            }
            if (movementDate.toISOString().split('T')[0] === currentDate.toISOString().split('T')[0]) {
                addTimeEntry(addButton, currentDate.toISOString().split('T')[0], movement.movementId, movement.timeFrom, movement.timeTo,
                    movement.notes, movement.transactionStatusName);
            }
        });
        currentDate.setDate(currentDate.getDate() + 1);
    }
}

function addTimeEntry(button, date, movementId, timeFrom, timeTo, notes, transactionStatus) {
    const hoursMinutes = calculateTimeDifference(timeFrom, timeTo);
    const reportedTimeLabel = document.createElement('span');
    reportedTimeLabel.className = 'reported-time-span';
    reportedTimeLabel.innerHTML = `<span>${hoursMinutes.hours} h - ${hoursMinutes.minutes} m</span>`;

    const editBtn = document.createElement('button');
    editBtn.className = 'edit-time-btn';
    editBtn.innerHTML = `<i class="fa-solid fa-pencil"></i>`;
    const timeEntryDiv = document.createElement('div');
    timeEntryDiv.className = 'time-entry';
    timeEntryDiv.innerHTML = `
        <input ${transactionStatus !== 'No actions' && transactionStatus !== 'Rejected' ? 'disabled' : ''} type="hidden" class="time-from" value="${timeFrom === null ? '08:00' : timeFrom}"/>
        <input type="hidden" class="movement-id" ${movementId === null ? 'value' : 'value="' + movementId + '"'}"/>
        <input ${transactionStatus !== 'No actions' && transactionStatus !== 'Rejected' ? 'disabled' : ''} type="hidden" class="time-to" value="${timeTo === null ? '16:00' : timeTo}"/>
    `;

    button.parentElement.appendChild(timeEntryDiv);
    button.parentElement.appendChild(reportedTimeLabel);
    reportedTimeLabel.appendChild(editBtn);


    const btnSaveTime = timeEntryDiv.querySelector('.btn-save-time');
    const checkSavedIcon = timeEntryDiv.querySelector('.check-saved-icon');
    const inputs = timeEntryDiv.querySelectorAll('.input-time, .movement-id');

    inputs.forEach(input => {
        if (input.type === 'text') {
            input.addEventListener('input', () => {
                btnSaveTime.style.display = 'block';
                checkSavedIcon.style.display = 'none';
            });
        } else {
            input.addEventListener('change', () => {
                btnSaveTime.style.display = 'block';
                checkSavedIcon.style.display = 'none';
            });
        }
        if (input.type === 'time') {
            input.addEventListener('keydown', (event) => {
                if (event.key === 'Backspace' || event.key === 'Delete') {
                    event.preventDefault();
                }
            });
        }
    });

    const timeFromInput = timeEntryDiv.querySelector('.time-from');
    const timeToInput = timeEntryDiv.querySelector('.time-to');

    const updateTimeDifference = () => {
        const fromTime = timeFromInput.value;
        const toTime = timeToInput.value;
        const validFromTime = fromTime === '' ? '00:00' : fromTime;
        const validToTime = toTime === '' ? '00:00' : toTime;

        const difference = calculateTimeDifference(validFromTime, validToTime);

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

function deleteTimeEntry(deleteBtn, movementId) {
    if (movementId !== null) {
        var spinnerLabel = deleteBtn.parentElement.querySelector('.spinner-time-actions');
        const checkSavedIcon = deleteBtn.parentElement.querySelector('.check-saved-icon');
        deleteTrackingToolTimeEntry(movementId, deleteBtn, spinnerLabel, checkSavedIcon).then(success => {
            if (success) {
                const dayItem = deleteBtn.closest('.day-item');
                deleteBtn.parentElement.remove();
                updateDayTotal(dayItem);
                updateTotalHours();
            }
        });
    } else {
        const dayItem = deleteBtn.closest('.day-item');
        deleteBtn.parentElement.remove();
        updateDayTotal(dayItem);
        updateTotalHours();
    }
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


// DELETE TRACKING TOOL TIME ENTRY
async function deleteTrackingToolTimeEntry(movementId, deleteBtn, spinnerLabel, checkSavedIcon) {
    if (!movementId) {
        return false;
    }
    deleteBtn.style.display = 'none';
    spinnerLabel.style.display = 'block';
    const token = $('[name="__RequestVerificationToken"]').val();
    const formData = new FormData();
    formData.append('movementId', movementId);

    try {
        const response = await fetch("/TrackingTool/ReportingMyTime/DeleteTrackingToolTimeEntry", {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body: formData
        });

        if (!response.ok) {
            // If the response is not successful, we extract the JSON containing the error message
            const err = await response.json();
            console.error('There has been a problem with your fetch operation:', err.error);
            switch (err.messageType) {
                case 'Validation Error':
                    displayToasterWarning('Validation Error: ' + err.error);
                    break;
                case 'Not Found':
                    return true;
                    break;
                default:
                    displayToasterError('Error: ' + err.error);
            }
            deleteBtn.style.display = 'block';
            checkSavedIcon.style.display = 'block';
            spinnerLabel.style.display = 'none';
            return false;
        }
        //If the answer is successful
        const data = await response.json();
        return true;
    } catch (err) {
        // Handling network errors or fetch failures
        validateSessionExpiration(err.message);
        displayToasterError('There has been a problem with your fetch operation:', err);
        console.error('There has been a problem with your fetch operation:', err);
        deleteBtn.style.display = 'block';
        checkSavedIcon.style.display = 'block';
        spinnerLabel.style.display = 'none';
        return false;
    }
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

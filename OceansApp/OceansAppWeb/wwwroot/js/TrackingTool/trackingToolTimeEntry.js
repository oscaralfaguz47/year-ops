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
        attachOnClick(addButton, currentDate.toISOString().split('T')[0]); 

        const arrowSpan = document.createElement('span');
        arrowSpan.textContent = '→';
        arrowSpan.style.display = 'none';

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
        dayItem.appendChild(arrowSpan);
        dayItem.appendChild(countLabel);
        dayItem.appendChild(nonReportNeededLabel);
        dateListContainer.appendChild(dayItem);
        submissionInfo.innerHTML = `<strong>Have you reported all your hours accurately?</strong> <button onclick="submitReportToBePaid()"><i class="fa-regular fa-paper-plane"></i> Submit Report to get paid</button>`;
        movements.forEach(function (movement) {
            const movementDate = new Date(movement.actionDate);
            if (movement.transactionStatusName !== 'No actions' && movement.transactionStatusName !== 'Rejected') {
                submissionInfo.innerHTML = `<div style="margin-bottom:10px"> You have already submitted your report, and the current status is:</div > <span class="status-span">${getStatusLabel(movement.transactionStatusName)}</span>`;
                addButton.style.display = 'none';
                arrowSpan.style.display = 'unset';
            }
            if (movementDate.toISOString().split('T')[0] === currentDate.toISOString().split('T')[0]) {
                addTimeEntry(addButton, currentDate.toISOString().split('T')[0], movement.movementId, movement.timeFrom, movement.timeTo,
                    movement.notes, movement.transactionStatusName);
            }
        });
        currentDate.setDate(currentDate.getDate() + 1);
    }
}
function attachOnClick(button, date) {
    button.onclick = function () {
        addTimeEntry(this, date, null, null, null, null, 'No actions');
    };
}

function addTimeEntry(button, date, movementId, timeFrom, timeTo, notes, transactionStatus) {
    const timeEntryDiv = document.createElement('div');
    timeEntryDiv.className = 'time-entry';
    timeEntryDiv.innerHTML = `
        <span>From</span><input ${transactionStatus !== 'No actions' && transactionStatus !== 'Rejected' ? 'disabled' : ''} type="time" class="time-from input-time" value="${timeFrom === null ? '08:00' : timeFrom}"/><span>To</span>
        <input type="hidden" class="movement-id" ${movementId === null ? 'value' : 'value="' + movementId + '"'}"/>
        <input ${transactionStatus !== 'No actions' && transactionStatus !== 'Rejected' ? 'disabled' : ''} type="time" class="time-to input-time" value="${timeTo === null ? '16:00' : timeTo}"/>
        <label class="count-time"></label>
        <input ${transactionStatus !== 'No actions' && transactionStatus !== 'Rejected' ? 'disabled' : ''} type="text" placeholder="Detail" class="time-detail input-time" maxlength="400" value="${notes === null ? '' : notes}"/>
        <button class="btn-delete-time ${transactionStatus !== 'No actions' && transactionStatus !== 'Rejected' ? 'hidden' : ''}" onclick="deleteTimeEntry(this, ${movementId})"><i class="fa-solid fa-trash-can"></i></button>
        <i class="fa-solid fa-spinner spinner-time-actions"></i>
        <i class="fa-solid fa-check uploaded-check-icon green-label check-saved-icon" ${movementId === null ? 'style="display:none"' : 'style="display:block"'}></i>
        <button class="btn-save-time" ${movementId !== null ? 'style="display:none"' : 'style="display:block"'} onclick="saveTimeEntry(this, '${date}')"><i class="fa-solid fa-floppy-disk"></i></button>
    `;
    button.parentElement.appendChild(timeEntryDiv);

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
                // Previene la eliminación usando Backspace y Delete
                if (event.key === 'Backspace' || event.key === 'Delete') {
                    event.preventDefault();
                }
            });
        }
    });

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




//CREATE, UPDATE TIME ENTRY
async function createUpdateTimeEntryTrackingTool(movementId, notes, timeFrom, timeTo, date, movementIdInput, spinnerLabel, button, checkSavedIcon) {
    submissionError.innerHTML = '';
    button.style.display = 'none';
    spinnerLabel.style.display = 'block';
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

    try {
        const response = await fetch('/TrackingTool/ReportingMyTime/CreateUpdateTimeEntryTrackingTool', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: token
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
                        return acc.concat(current); 
                    }, []);
                    displayToasterWarningArray(allErrors);
                    break;
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    button.style.display = 'block';
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            spinnerLabel.style.display = 'none';
            checkSavedIcon.style.display = 'none';
            button.style.display = 'block';
            return null; 
        }

        const dataFromApi = await response.json();
        movementIdInput.value = dataFromApi.movementId;
        spinnerLabel.style.display = 'none';
        checkSavedIcon.style.display = 'block';
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        spinnerLabel.style.display = 'none';
        checkSavedIcon.style.display = 'none';
        button.style.display = 'block';
        return null; // Return null to signify an error that prevented a successful fetch
    }
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
        totalHoursLabel.innerHTML = `<span class="total">TOTAL TIME: <span class="hours-minutes">${totalHours} hours - ${totalMinutesLeft} minutes</span></span>`;
    }
}

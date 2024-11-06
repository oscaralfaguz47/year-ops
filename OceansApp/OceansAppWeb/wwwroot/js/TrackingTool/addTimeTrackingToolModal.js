let modalTitle = document.getElementById('create-update-time-modal-title');
let timeToInput = document.getElementById('timeToInput');
let timeFromInput = document.getElementById('timeFromInput');
let additionalNotesInput = document.getElementById('addNotesInput');
let actionDateInput = document.getElementById('actionDateInput');
let movementIdInput = document.getElementById('movementIdInput');
let validationMessageTimeZero = document.getElementById('time-zero-val-message');
let validationMessageNotes = document.getElementById('notes-val-message');
let timeClassificationSelect = document.getElementById('timeClassification');
let addBtn = null;
let htmlReportedTimeElement = null;
let timeClasifications = [];

timeFromInput.addEventListener('change', () => {
    let hoursMinutes = calculateTimeDifference(timeFromInput.value, timeToInput.value);
    if (hoursMinutes.hours === 0 && hoursMinutes.minutes === 0) {
        validationMessageTimeZero.style.display = 'block';
    } else {
        validationMessageTimeZero.style.display = 'none';
    }
});
timeToInput.addEventListener('change', () => {
    let hoursMinutes = calculateTimeDifference(timeFromInput.value, timeToInput.value);
    if (hoursMinutes.hours === 0 && hoursMinutes.minutes === 0) {
        validationMessageTimeZero.style.display = 'block';
    } else {
        validationMessageTimeZero.style.display = 'none';
    }
});
additionalNotesInput.addEventListener('input', () => {
    if (timeClassificationSelect.selectedOptions[0].text.includes('(Non-payable)') && additionalNotesInput.value === '') {
        validationMessageNotes.style.display = 'block';
    } else {
        validationMessageNotes.style.display = 'none';
    }
});

timeFromInput.addEventListener('keydown', (event) => {
    if (event.key === 'Backspace' || event.key === 'Delete') {
        event.preventDefault();
    }
});
timeToInput.addEventListener('keydown', (event) => {
    if (event.key === 'Backspace' || event.key === 'Delete') {
        event.preventDefault();
    }
});
document.getElementById('addNotesInput').addEventListener('input', function (e) {
    if (this.value.length > 400) {
        this.value = this.value.slice(0, 400);
    }
});

function displayCreateUpdateTime(modalId, selectedDate, movementId, button, htmlElement) {
    const tooltip = document.querySelector('.tooltip');
    if (tooltip) {
        tooltip.style.opacity = '0';
        tooltip.style.pointerEvents = 'none';
    }
    fillMovementTypesSelect(timeClassificationSelect, timeClasifications);
    htmlReportedTimeElement = htmlElement;
    const currentYear = new Date(dateFromInput.value).getFullYear();
    const fullDateString = `${selectedDate} ${currentYear}`;
    const dateObject = new Date(fullDateString);
    addBtn = button;
    hideValidationMessage(validationMessageTimeZero);
    hideValidationMessage(validationMessageNotes);
    const submitBtns = [{ id: 'btn-cancel', text: 'Delete' }, { id: 'btn-saving', text: 'Confirm' }];
    const otherBtns = ['close-modal-x-btn'];
    enableModalButtons(submitBtns, otherBtns, 'spinner-border');
    resetForm('form-create-update');
    if (movementId === null) {
        movementIdInput.value = movementId;
        modalTitle.textContent = selectedDate;
        timeFromInput.value = '08:00';
        timeToInput.value = '16:00';
        actionDateInput.value = dateObject;
        showModal(modalId);
    } else {
        displaySpinner();
        var url = "/TrackingTool/ReportingMyTime/GetTrackingToolMovementDataById?movementId=" + encodeURIComponent(movementId);
        return fetch(url)
            .then(response => {
                if (response.ok) {
                    return response.json();
                } else {
                    return response.json().then(errorData => {

                        if (errorData.messageType === 'Not Found') {
                            hideModal(modalId);
                            const reportedSpan = document.getElementById('reportedTimeSpan-' + movementId);
                            reportedSpan.closest('.time-entry').remove();
                            const dayItem = addBtn.closest('.day-item');
                            updateDayTotal(dayItem);
                            updateTotalHours();
                        }
                        throw new Error('The request to the server failed!. More details: ' + errorData.messageType);
                    });
                }
            })
            .then(data => {
                let date = new Date(data.movementData.actionDate);
                let options = {
                    weekday: 'short',
                    month: 'long',   
                    day: 'numeric'
                };

                let formattedDate = date.toLocaleDateString('en-US', options);
                movementIdInput.value = movementId;
                modalTitle.textContent = formattedDate;
                timeFromInput.value = data.movementData.timeFrom;
                timeToInput.value = data.movementData.timeTo;
                actionDateInput.value = data.movementData.actionDate;
                timeClassificationSelect.value = data.movementData.movementTypeName === 'Normal Hours' ? 'Normal Hours' : data.movementData.movementTypeId;
                additionalNotesInput.value = data.movementData.notes;
                showModal(modalId);
                return data;
            })
            .catch(error => {
                validateSessionExpiration(error.message);
            })
            .finally(() => {
                hideSpinner();
            });
    }
}

//CREATE, UPDATE TIME ENTRY
async function createUpdateTimeEntryTrackingTool(modalId) {
    let hoursMinutes = calculateTimeDifference(timeFromInput.value, timeToInput.value);
    if ((hoursMinutes.hours === 0 && hoursMinutes.minutes === 0)
        || (timeClassificationSelect.selectedOptions[0].text.includes('(Non-payable)') && additionalNotesInput.value === '')) {
        validationMessageNotes.style.display = 'block';
        return;
    }
    const submitBtnsInitialize = [{ id: 'btn-cancel', text: 'Delete' }, { id: 'btn-saving', text: 'Confirm' }];
    const otherBtnsInitialize = ['close-modal-x-btn', 'btn-cancel'];

    const otherBtns = ['btn-cancel', 'close-modal-x-btn'];
    disableButtonsWaitingForPostMethod('btn-saving', otherBtns, 'spinner-border')
    let actionDateData = new Date(actionDateInput.value).toISOString();

    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        MovementId: movementIdInput.value === '' ? null : Number(movementIdInput.value),
        ProjectId: Number(projectIdInput.value),
        ActionDate: actionDateData,
        Notes: additionalNotesInput.value,
        TimeFrom: timeFromInput.value,
        TimeTo: timeToInput.value,
        MovementTypeId: timeClassificationSelect.value === 'Normal Hours' ? null : Number(timeClassificationSelect.value)
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
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            enableModalButtons(submitBtnsInitialize, otherBtnsInitialize, 'spinner-border');
            return null;
        }

        const dataFromApi = await response.json();
        hideModal(modalId);
        enableModalButtons(submitBtnsInitialize, otherBtnsInitialize, 'spinner-border');
        if (movementIdInput.value !== null && movementIdInput.value !== '') {
            const timeFromInputFromDiv = htmlReportedTimeElement.querySelector('.time-from');
            const timeToInputFromDiv = htmlReportedTimeElement.querySelector('.time-to');
            const movementIdInputFromDiv = htmlReportedTimeElement.querySelector('.movement-id');
            const editBtnFromDiv = htmlReportedTimeElement.querySelector('.reported-time-span');
            editBtnFromDiv.setAttribute('data-tooltip', `<div class="tooltip-container">
    <label>${!timeClassificationSelect.selectedOptions[0].text.includes('(Non-payable)') ? 'Payable Hours <i class="i-payable">$</i>' : 'Non-Payable Hours <i class="i-non-payable">$</i>'}</label>
    <p>${additionalNotesInput.value}</p>
    </div>`);
            editBtnFromDiv.onclick = function () {
                let date = new Date(actionDateInput.value);
                let options = {
                    weekday: 'short',
                    month: 'long',
                    day: 'numeric'
                };
                let formattedDate = date.toLocaleDateString('en-US', options);
                displayCreateUpdateTime(modalId, formattedDate, dataFromApi.movementId, addBtn, htmlReportedTimeElement);
            };

            timeFromInputFromDiv.value = timeFromInput.value;
            timeToInputFromDiv.value = timeToInput.value;
            movementIdInputFromDiv.value = dataFromApi.movementId;

            const hoursMinutes = calculateTimeDifference(timeFromInput.value, timeToInput.value);
            const reportedTimeFromToById = document.getElementById('time-from-to-span-' + movementIdInput.value);
            const reportedTimeSpanById = document.getElementById('reportedTimeSpan-' + movementIdInput.value);
            reportedTimeFromToById.innerHTML = `${formatTimeTo12Hour(timeFromInput.value)} - ${formatTimeTo12Hour(timeToInput.value)}`;
            reportedTimeSpanById.innerHTML = `<span id="reportedTimeSpan-${movementIdInput.value}">${hoursMinutes.hours} h - ${hoursMinutes.minutes} m</span>`;
            if (timeClassificationSelect.selectedOptions[0].text.includes('(Non-payable)')) {
                reportedTimeSpanById.parentElement.classList.add('non-payable');
            } else {
                reportedTimeSpanById.parentElement.classList.remove('non-payable');
            }
            const dayItem = addBtn.closest('.day-item');
            updateDayTotal(dayItem);
            updateTotalHours();
        } else {
            let payable = timeClassificationSelect.selectedOptions[0].text.includes('(Non-payable)') ? false : true;
            addTimeEntry(addBtn, dataFromApi.movementId, timeFromInput.value, timeToInput.value, 'No actions',
                payable, additionalNotesInput.value);
        }
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        enableModalButtons(submitBtnsInitialize, otherBtnsInitialize, 'spinner-border');
        return null;
    }
}

// DELETE TRACKING TOOL TIME ENTRY
async function deleteTrackingToolTimeEntry(modalId) {
    if (!movementIdInput.value) {
        hideModal(modalId);
        return false;
    }
    const submitBtnsInitialize = [{ id: 'btn-cancel', text: 'Delete' }, { id: 'btn-saving', text: 'Confirm' }];
    const otherBtnsInitialize = ['close-modal-x-btn', 'btn-cancel'];

    const otherBtns = ['btn-saving', 'close-modal-x-btn'];
    disableButtonsWaitingForPostMethod('btn-cancel', otherBtns, 'spinner-border')
 
    const token = $('[name="__RequestVerificationToken"]').val();
    const formData = new FormData();
    formData.append('movementId', movementIdInput.value);

    try {
        const response = await fetch("/TrackingTool/ReportingMyTime/DeleteTrackingToolTimeEntry", {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body: formData
        });

        if (!response.ok) {
            const err = await response.json();
            console.error('There has been a problem with your fetch operation:', err.error);
            switch (err.messageType) {
                case 'Validation Error':
                    displayToasterWarning('Validation Error: ' + err.error);
                    break;
                case 'Not Found':
                    hideModal(modalId);
                    const reportedSpan = document.getElementById('reportedTimeSpan-' + movementIdInput.value);
                    reportedSpan.closest('.time-entry').remove();
                    const dayItem = addBtn.closest('.day-item');
                    updateDayTotal(dayItem);
                    updateTotalHours();
                    return true;
                    break;
                default:
                    displayToasterError('Error: ' + err.error);
            }
            enableModalButtons(submitBtnsInitialize, otherBtnsInitialize, 'spinner-border');
            return false;
        }
        hideModal(modalId);
        const reportedSpan = document.getElementById('reportedTimeSpan-' + movementIdInput.value);
        reportedSpan.closest('.time-entry').remove();
        const dayItem = addBtn.closest('.day-item');
        updateDayTotal(dayItem);
        updateTotalHours();
        return true;
    } catch (err) {
        // Handling network errors or fetch failures
        validateSessionExpiration(err.message);
        displayToasterError('There has been a problem with your fetch operation:', err);
        console.error('There has been a problem with your fetch operation:', err);
        enableModalButtons(submitBtnsInitialize, otherBtnsInitialize, 'spinner-border');
        return false;
    }
}
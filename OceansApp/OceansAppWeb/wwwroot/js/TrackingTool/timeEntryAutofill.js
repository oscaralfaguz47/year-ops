const autoFillFromInput = getElementById('autofillTimeFromInput');
const autoFillToInput = getElementById('autoFillTimeToInput');
const autoFillTimeClassificationSelect = getElementById('autofillTimeClassification');
const autofillAdditionalNotesInput = document.getElementById('autoFillAddNotesInput');
let autofillValidationMessageTimeZero = document.getElementById('time-zero-val-message-autofill');
let autofillValidationMessageNotes = document.getElementById('notes-val-message-autofill');

autoFillFromInput.addEventListener('change', () => {
    let hoursMinutes = calculateTimeDifference(autoFillFromInput.value, autoFillToInput.value);
    if (hoursMinutes.hours === 0 && hoursMinutes.minutes === 0) {
        autofillValidationMessageTimeZero.style.display = 'block';
    } else {
        autofillValidationMessageTimeZero.style.display = 'none';
    }
});
autoFillToInput.addEventListener('change', () => {
    let hoursMinutes = calculateTimeDifference(autoFillFromInput.value, autoFillToInput.value);
    if (hoursMinutes.hours === 0 && hoursMinutes.minutes === 0) {
        autofillValidationMessageTimeZero.style.display = 'block';
    } else {
        autofillValidationMessageTimeZero.style.display = 'none';
    }
});
autofillAdditionalNotesInput.addEventListener('input', () => {
    if (timeClassificationSelect.selectedOptions[0].text.includes('(Non-payable)') && autofillAdditionalNotesInput.value === '') {
        autofillValidationMessageNotes.style.display = 'block';
    } else {
        autofillValidationMessageNotes.style.display = 'none';
    }
});

autoFillFromInput.addEventListener('keydown', (event) => {
    if (event.key === 'Backspace' || event.key === 'Delete') {
        event.preventDefault();
    }
});
autoFillToInput.addEventListener('keydown', (event) => {
    if (event.key === 'Backspace' || event.key === 'Delete') {
        event.preventDefault();
    }
});
autofillAdditionalNotesInput.addEventListener('input', function (e) {
    if (this.value.length > 400) {
        this.value = this.value.slice(0, 400);
    }
});
function displayAutofillModal(modalId) {
    fillMovementTypesSelect(autoFillTimeClassificationSelect, timeClasifications);

    hideValidationMessage(autofillValidationMessageTimeZero);
    hideValidationMessage(autofillValidationMessageNotes);

    const submitBtns = [{ id: 'btn-save-autofill', text: 'Save' }];
    const otherBtns = ['autofill-close-modal-x-btn', 'btn-cancel-autofill'];
    enableModalButtons(submitBtns, otherBtns, 'spinner-border');

    resetForm('form-autofill');
    autoFillFromInput.value = '08:00';
    autoFillToInput.value = '16:00';
    showModal(modalId);
}

async function saveAutofill(modalId) {
    let hoursMinutes = calculateTimeDifference(autoFillFromInput.value, autoFillToInput.value);
    if ((hoursMinutes.hours === 0 && hoursMinutes.minutes === 0)
        || (autoFillTimeClassificationSelect.selectedOptions[0].text.includes('(Non-payable)') && autofillAdditionalNotesInput.value === '')) {
        autofillValidationMessageNotes.style.display = 'block';
        return;
    }
    const submitBtnsInitialize = [{ id: 'btn-save-autofill', text: 'Save' }];
    const otherBtnsInitialize = ['autofill-close-modal-x-btn', 'btn-cancel-autofill'];

    disableButtonsWaitingForPostMethod('btn-save-autofill', otherBtnsInitialize, 'spinner-border')

    var token = $('[name="__RequestVerificationToken"]').val();

    const startDateToSubmit = getNormalizedOneDate(dateFromInput).normalizedDate;
    const endDateToSubmit = getNormalizedOneDate(dateToInput).normalizedDate;

    var data = {
        ProjectId: Number(projectIdInput.value),
        Notes: autofillAdditionalNotesInput.value,
        TimeFrom: autoFillFromInput.value,
        TimeTo: autoFillToInput.value,
        MovementTypeId: autoFillTimeClassificationSelect.value === 'Normal Hours' ? null : Number(autoFillTimeClassificationSelect.value)
    };

    try {
        const response = await fetch(`/TrackingTool/ReportingMyTime/AutofillTimeEntryTrackingTool?startDate=${startDateToSubmit}&endDate=${endDateToSubmit}`, {
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
        displayToasterSuccess(dataFromApi.message);
        initializeNavigation();
        const movements = await getTrackingToolProjectMovements();

        const startDateGenerateList = convertToIsoDate(dateFromInput.value); 
        const endDateGenerateList = convertToIsoDate(dateToInput.value);    

        function convertToIsoDate(dateInput) {
            const [month, day, year] = dateInput.split('/');
            return `${year}-${month}-${day}`;
        }
        generateDateList(startDateGenerateList, endDateGenerateList, movements.movementsList);
        trackingToolTimeEntrySection.style.display = 'block';
        loadingBoxIntern.style.display = 'none';
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        enableModalButtons(submitBtnsInitialize, otherBtnsInitialize, 'spinner-border');
        return null;
    }
}
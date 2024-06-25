let modalTitle = document.getElementById('create-update-time-modal-title');
let timeToInput = document.getElementById('timeToInput');
let timeFromInput = document.getElementById('timeFromInput');
let additionalNotesInput = document.getElementById('addNotesInput');
let actionDateInput = document.getElementById('actionDateInput');
let movementIdInput = document.getElementById('movementIdInput');
function displayCreateUpdateTime(modalId, selectedDate, movementId) {
    modalTitle.textContent = selectedDate;
    inicializeModalButtons(modalId, 'Confirm');
    resetForm('form-create-update')
    if (movementId === null) {
        timeFromInput.value = '08:00';
        timeToInput.value = '16:00';
    }
    showModal(modalId);
}

//CREATE, UPDATE TIME ENTRY
async function createUpdateTimeEntryTrackingTool() {
    waitingForPostMethod();
    let actionDateData = new Date(actionDateInput.value).toISOString();

    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        MovementId: movementIdInput.value,
        ProjectId: Number(projectIdInput.value),
        ActionDate: actionDateData,
        Notes: additionalNotesInput.value,
        TimeFrom: timeFromInput.value,
        TimeTo: timeToInput.Value
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
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            inicializeModalButtons(modalId, 'Confirm');
            return null;
        }

        const dataFromApi = await response.json();
        movementIdInput.value = dataFromApi.movementId;
        inicializeModalButtons(modalId, 'Confirm');
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        inicializeModalButtons(modalId, 'Confirm');
        return null;
    }
}
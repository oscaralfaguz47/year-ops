
document.addEventListener("DOMContentLoaded", function () {
    var actionDate = document.getElementById('ActionDate');
    var today = new Date();
    var todayFormatted = today.toISOString().substr(0, 10);
    actionDate.min = todayFormatted;
    function validateDate() {
        if (actionDate.value < actionDate.min) {
            actionDate.value = actionDate.min;
        }
    }
    actionDate.addEventListener('change', validateDate);
});

//CREATE / UPDATE INTERVIEW
async function displayUpdateCreateInterviewModal(modalId, id) {
    var modalTitle = document.getElementById('create-interview-modal-title');
    modalTitle.textContent = "REGISTER NEW INTERVIEW";
    var createUpdateForm = $('#form-create-update');
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    createUpdateForm.find('[name="interviewId"]').val("");
    createUpdateForm.find('[name="consultantIdFromSearch"]').val("");

    if (id !== null) {
        modalTitle.textContent = "UPDATE INTERVIEW";
        var url = "/Recruiting/Interviews/GetInterviewDataById?interviewId=" + encodeURIComponent(id);
        displaySpinner();
        fetch(url)
            .then(response => {
                if (response.ok) {
                    return response.json();
                } else {
                    return response.json().then(errorData => {
                        displayToasterError(errorData.error);
                        hideModal(modalId);
                        getListOfResults(false, false);
                        throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                    });
                }
            })
            .then(data => {
                createUpdateForm.find('[name="interviewId"]').val(data.interviewData.interviewId);
                createUpdateForm.find('[name="consultantIdFromSearch"]').val(data.interviewData.consultantId);
                createUpdateForm.find('[name="consultantNameInput"]').val(data.interviewData.consultantName);
                createUpdateForm.find('[name="consultantEmailInput"]').val(data.interviewData.consultantEmail);
                createUpdateForm.find('[name="durationMinutes"]').val(data.interviewData.durationMinutes);
                updateHours();
                let actionDateFormat = new Date(data.interviewData.date);
                createUpdateForm.find('[name="date"]').val(actionDateFormat.toISOString().split('T')[0]);
                showModal(modalId);
            })
            .catch(error => {
                validateSessionExpiration(error.message);
            })
            .finally(() => {
                hideSpinner();
            });
    } else {
        showModal(modalId);
    }
}


//CREATE, UPDATE INTERVIEW
async function createUpdateInterview(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var interviewIdData = createUpdateForm.find('[name="interviewId"]').val() || null;
    var consultantIdData = createUpdateForm.find('[name="consultantIdFromSearch"]').val() || null;
    var durationMinutesData = createUpdateForm.find('[name="durationMinutes"]').val() || null;
    var actionDateData = createUpdateForm.find('[name="date"]').val() || null;

    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        InterviewId: interviewIdData,
        ConsultantId: consultantIdData,
        DurationMinutes: durationMinutesData,
        Date: actionDateData
    };
    fetch('/Recruiting/Interviews/CreateUpdateInterview', {
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
                        inicializeModalButtons(modalId);
                        throw new Error('Validation errors!');
                    } else {
                        displayToasterError(errorData.error);
                        hideModal(modalId);
                        throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                    }
                });
            }
        })
        .then(data => {
            inicializeModalButtons(modalId);
            displayToasterSuccess(data.message);
            hideModal(modalId);
            getListOfResults(false, false);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        });
}

// INPUT VALIDATIONS
document.addEventListener("DOMContentLoaded", function () {
    validateInputTypeNumber('DurationMinutesInput');
});
document.addEventListener("DOMContentLoaded", function () {
    validateInputTypeNumber('DurationHoursInput');
});

//UPDATE HOURS 
function updateHours() {
    var durationHoursInput = document.getElementById('DurationHoursInput');
    var durationMinutesInput = document.getElementById('DurationMinutesInput');
    var durationHours = (1/60) * parseFloat(durationMinutesInput.value);

    var total = durationHours || 0;

    durationHoursInput.value = total.toFixed(2);
}
//UPDATE MINUTES
function updateMinutes() {
    var durationHoursInput = document.getElementById('DurationHoursInput');
    var durationMinutesInput = document.getElementById('DurationMinutesInput');
    var durationHours = parseFloat(durationHoursInput.value) / (1 / 60);

    var total = durationHours || 0;

    durationMinutesInput.value = total.toFixed(2);
}

updateHours();
updateMinutes();


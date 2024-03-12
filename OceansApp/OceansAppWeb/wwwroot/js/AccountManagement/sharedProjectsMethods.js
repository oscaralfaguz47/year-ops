
//document.addEventListener("DOMContentLoaded", function () {
//    var actionDate = document.getElementById('actionDate');
//    var today = new Date();
//    var todayFormatted = today.toISOString().substr(0, 10);
//    actionDate.min = todayFormatted;
//    function validateDate() {
//        if (actionDate.value < actionDate.min) {
//            actionDate.value = actionDate.min;
//        }
//    }
//    actionDate.addEventListener('change', validateDate);
//});

var createUpdateForm = $('#form-add-update-consultant');

function hideConsultantResults() {
    let resultsContainer = document.getElementById('consultant-search-results');
    resultsContainer.style.display = 'none';
}

async function searchConsultantsBySearchText(searchTextInput, hiddenInputForId, consultantNameInput, consultantEmailInput) {
    if (searchTextInput.value.length > 100) {
        searchTextInput.value = searchTextInput.value.slice(0, 100);
    } else {
        let resultsContainer = document.getElementById('consultant-search-results');
        resultsContainer.innerHTML = '';
        resultsContainer.innerHTML = `<div class="text-center"><div class="spinner-border" role="status">
        <span class="sr-only" ></span>
                </div></div>`;
        let data = await getConsultantsBySearchText(searchTextInput.value);
        resultsContainer.innerHTML = '';
        resultsContainer.style.display = 'block';
        if (data.consultants.length > 0) {
            let resultList = document.createElement('ul');
            for (let item of data.consultants) {
                let listItem = document.createElement('li');
                listItem.innerHTML = '<strong>' + item.consultantName + '</strong> ' + (item.userCategoryName === "Administrative" ? '<span class="green-label">(' : '<span class="blue-label">(') + item.userCategoryName + ')</span>';
                listItem.onclick = function () {
                    document.getElementById(hiddenInputForId).value = item.consultantId;
                    document.getElementById(consultantNameInput).value = item.consultantName;
                    document.getElementById(consultantEmailInput).value = item.email;
                    hideConsultantResults();
                };
                resultList.appendChild(listItem);
            }
            resultsContainer.appendChild(resultList);
        } else {
            resultsContainer.innerHTML = '<div class="red-label text-center">No results found</div>';
        }
        document.addEventListener('click', function (event) {
            let isClickInside = resultsContainer.contains(event.target);
            if (!isClickInside) {
                hideConsultantResults();
            }
        });
        document.addEventListener('keydown', function (event) {
            if (event.key === "Escape") {
                hideConsultantResults();
            }
        });
    }
}
function validateRatesInputs() {
    var clientRateMethod = document.querySelector('input[name="client-rate-model"]:checked').value;
    var consultantRateMethod = document.querySelector('input[name="consultant-rate-model"]:checked').value;

    if (clientRateMethod === 'H') {
        document.getElementById('hourlyClientRateEl').style.display = 'block';
        document.getElementById('monthlyClientRateEl').style.display = 'none';
        document.getElementById('monthlyClientRate').value = null;
    } else {
        document.getElementById('hourlyClientRateEl').style.display = 'none';
        document.getElementById('monthlyClientRateEl').style.display = 'block';
        document.getElementById('hourlyClientRate').value = null;
    }
    if (consultantRateMethod === 'M') {
        document.getElementById('monthlyConsultantSalaryEl').style.display = 'block';
        document.getElementById('hourlyConsultantSalaryEl').style.display = 'none';
        document.getElementById('hourlySalary').value = null;
        document.getElementById("isMonthlySalaryCalculatedPerHour").style.display = 'block';
        document.getElementById("calculationMethod").value = true;
        document.getElementById("calculationMethod").checked = true;
    } else {
        document.getElementById('monthlyConsultantSalaryEl').style.display = 'none';
        document.getElementById('hourlyConsultantSalaryEl').style.display = 'block';
        document.getElementById('monthlySalary').value = null;
        document.getElementById("isMonthlySalaryCalculatedPerHour").style.display = 'none';
        document.getElementById("calculationMethod").value = false;
        document.getElementById("calculationMethod").checked = false;
    }
}
//DISPLAY MODAL
async function displayAddUpdateConsultant(modalId, id) {
    inicializeSecondModalButtons(modalId);
    var modalTitle = document.getElementById('add-consultant-modal-title');
    modalTitle.textContent = id === null ? 'ADD CONSULTANT TO THE PROJECT' : 'EDIT CONSULTANT ASSIGNATION PROJECT';
    resetForm('form-add-update-consultant');
    createUpdateForm.find('[name="proConsAssignedId"]').val("");
    createUpdateForm.find('[name="consultantIdFromSearch"]').val("");
    validateRatesInputs();
    document.getElementById('search-input-cont').style.display = 'block';
    var clientRateSection = document.getElementById("client-rate-section");
    var clientRateInputs = document.getElementById("client-rate-inputs");
    if (document.getElementById('external-pt').checked) {
        clientRateSection.style.display = 'block';
        clientRateInputs.style.display = 'flex';
    } else {
        clientRateSection.style.display = 'none';
        clientRateInputs.style.display = 'none';
    }

    if (id !== null) {
        createUpdateForm.find('[name="proConsAssignedId"]').val(id);
        document.getElementById('search-input-cont').style.display = 'none';

        var url = "/AccountManagement/Projects/GetAssignedConsultantToProjectById?consultantProjectAssignedtId=" + encodeURIComponent(id);
        displaySpinner();
        fetch(url)
            .then(response => {
                if (response.ok) {
                    return response.json();
                } else {
                    return response.json().then(errorData => {
                        displayToasterError(errorData.error);
                        hideModal(modalId);
                        throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                    });
                }
            })
            .then(data => {
                console.log(data);
                createUpdateForm.find('[name="consultantNameInput"]').val(data.consultantAssignation.consultantName);
                document.getElementById('consultantEmailInput').value = data.consultantAssignation.email;
                createUpdateForm.find('[name="positionDetail"]').val(data.consultantAssignation.positionDetail);

                if (data.consultantAssignation.hourlyClientRate !== 0) document.getElementsByName('client-rate-model')[0].checked = true;
                if (data.consultantAssignation.monthlyClientRate !== 0) document.getElementsByName('client-rate-model')[1].checked = true;
                if (data.consultantAssignation.monthlySalary !== 0) document.getElementsByName('consultant-rate-model')[0].checked = true;
                if (data.consultantAssignation.hourlySalary !== 0) document.getElementsByName('consultant-rate-model')[1].checked = true;
                validateRatesInputs();
                createUpdateForm.find('[name="monthlyClientRate"]').val(data.consultantAssignation.monthlyClientRate);
                createUpdateForm.find('[name="hourlyClientRate"]').val(data.consultantAssignation.hourlyClientRate);
                createUpdateForm.find('[name="monthlySalary"]').val(data.consultantAssignation.monthlySalary);
                createUpdateForm.find('[name="hourlySalary"]').val(data.consultantAssignation.hourlySalary);
                createUpdateForm.find('[name="isMonthlySalaryCalculatedPerHour"]').val(data.consultantAssignation.isMonthlySalaryCalculatedPerHour);
                createUpdateForm.find('[name="isMonthlySalaryCalculatedPerHour"]').prop('checked', data.consultantAssignation.isMonthlySalaryCalculatedPerHour);
                showModal(modalId);
            })
            .finally(() => {
                hideSpinner();
            });
    } else {
        showModal(modalId);
    }
}
//ADD CONSULTANT TO PROJECT 
function addConsultantToProject(modalId) {
    var projectConsultantAssignedValue = createUpdateForm.find('[name="proConsAssignedId"]').val();
    var hourlyClientRateValue = createUpdateForm.find('[name="hourlyClientRate"]').val();
    var monthlyClientRateValue = createUpdateForm.find('[name="monthlyClientRate"]').val();
    var hourlyConsultantRateValue = createUpdateForm.find('[name="hourlySalary"]').val();
    var monthlyConsultantRateValue = createUpdateForm.find('[name="monthlySalary"]').val();
    var clientRateMethodRb = document.querySelector('input[name="client-rate-model"]:checked').value;
    var consultantRateMethodRb = document.querySelector('input[name="consultant-rate-model"]:checked').value;
    var positionDetailValue = createUpdateForm.find('[name="positionDetail"]').val();
    var actionDateValue = createUpdateForm.find('[name="actionDate"]').val();
    var isBillableValue = document.getElementById("IsBillable").value;
    var isMonthlySalaryCalculatedPerHourVal = createUpdateForm.find('[name="isMonthlySalaryCalculatedPerHour"]').prop('checked');
    
    var modelState = true;
    if ((createUpdateForm.find('[name="consultantIdFromSearch"]').val() === null
        || createUpdateForm.find('[name="consultantIdFromSearch"]').val() === '') && projectConsultantAssignedValue === "") {
        modelState = false;
        displayToasterWarning('You must search and select a Consultant.');
    }
    if (positionDetailValue.length === 0) {
        modelState = false;
        displayToasterWarning('The Position Description is required.');
    }

    if (isBillableValue === "true" && (Number(hourlyClientRateValue) === 0 && clientRateMethodRb === 'H')) {
        modelState = false;
        displayToasterWarning('The Hourly Client Rate is required.');
    }
    if (isBillableValue === "true" && (Number(monthlyClientRateValue) === 0 && clientRateMethodRb === 'M')) {
        modelState = false;
        displayToasterWarning('The Monthly Client Rate is required.');
    }
    if (Number(hourlyConsultantRateValue) === 0 && consultantRateMethodRb === 'H') {
        modelState = false;
        displayToasterWarning('The Hourly Consultant Salary is required.');
    }
    if (Number(monthlyConsultantRateValue) === 0 && consultantRateMethodRb === 'M') {
        modelState = false;
        displayToasterWarning('The Monthly Consultant Salary is required.');
    }
    if (actionDateValue === '') {
        modelState = false;
        displayToasterWarning('The Action Date is required.');
    } else {
        if (!isValidDate(actionDateValue.toString())) {
            modelState = false;
            displayToasterWarning('The Action Date is not a valid date.');
        }
    }
    if (modelState) {
        addConsultantToModalCreateUpdateProject(modalId);
        //EDIT CONSULTANT PARAMETERS
        if (projectConsultantAssignedValue !== "") {
            displaySpinner();

            var token = $('[name="__RequestVerificationToken"]').val();
            var data = {
                ProjectConsultantAssignedId: Number(projectConsultantAssignedValue),
                HourlyClientRate: Number(hourlyClientRateValue),
                HourlySalary: Number(hourlyConsultantRateValue),
                MonthlyClientRate: Number(monthlyClientRateValue),
                MonthlySalary: Number(monthlyConsultantRateValue),
                PositionDetail: positionDetailValue,
                ActionDate: actionDateValue ? actionDateValue.toString() : null,
                IsMonthlySalaryCalculatedPerHour: Boolean(isMonthlySalaryCalculatedPerHourVal)
            };
            fetch('/AccountManagement/Projects/UpdateConsultantParameters', {
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
                                hideModal(modalId);
                                throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                            }
                        });
                    }
                })
                .then(data => {
                    hideModal(modalId);
                    createUpdateForm[0].reset();
                    displayToasterSuccess(data.message);
                    getListOfResults(false, false);
                })
                .finally(() => {
                    hideSpinner();
                })
        }
    }
}
// INPUT VALIDATIONS
document.getElementById('positionDetail').addEventListener('input', function (e) {
    if (this.value.length > 130) {
        this.value = this.value.slice(0, 130);
    }
});

document.addEventListener("DOMContentLoaded", function () {
    validateInputTypeNumber('monthlyClientRate');
    validateInputTypeNumber('hourlyClientRate');
    validateInputTypeNumber('monthlySalary');
    validateInputTypeNumber('hourlySalary');
});

//Activate and deactivate Consultant from project
async function activateDeactivateConsultantFromProject(projectConsultantAssignedId, name, status) {
    var title = status ? "Deactivate Consultant" : "Activate Consultant";
    var textAction = status ? "Deactivate" : "Activate";
    var textSpan = status ? "Deactivated" : "Activated";
    var validationMessage = status ? "The Deactivation Date is required." : "The Activation Date is required.";

    try {
        const result = await Swal.fire({
            title: title,
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: textAction,
            cancelButtonText: 'Cancel',
            html: `<div><span>Select a date when ${name} will be ${textSpan}</span></div>
            <input type="date" id="swal-input-action-date" class="swal2-input" required>`,
            focusConfirm: false,
            preConfirm: () => {
                const actionDate = document.getElementById('swal-input-action-date').value;
                if (!actionDate) {
                    Swal.showValidationMessage(validationMessage);
                    return false;
                }
                return [actionDate];
            }//,
            //didOpen: () => {
            //    const today = new Date();
            //    const localDate = new Date(today.getTime() - (today.getTimezoneOffset() * 60000)).toISOString().split('T')[0];
            //    document.getElementById('swal-input-action-date').setAttribute('min', localDate);
            //    document.getElementById('swal-input-action-date').onkeydown = (e) => {
            //        e.preventDefault();
            //    };
            //}
        });

        if (result.isConfirmed) {
            displaySpinner();
            var actionDate = document.getElementById('swal-input-action-date').value;
            const data = await activateDeactivateConsultantFromProjectHttps(projectConsultantAssignedId, actionDate);
            toastr.success(data.message);
            hideSpinner();
            return data.success;
        } else {
            return false;
        }
    } catch (error) {
        console.error(error);
        hideSpinner();
        return false;
    }
}

// GET CONSULTANT HISTORY
async function getProjectConsultantHistory(projectConsultantAssignedId, modalId) {
    displaySpinner();
    var bodyList = document.getElementById('consultant-history-body');
    var actionIcon = "";
    await getProjectConsultantHistoryHttps(projectConsultantAssignedId).then((data) => {
        var count = 0;
        var firstRow = "";
        var row = "";
        var titleLabelClass = "";
        data.historyList.result.forEach(function (obj) {
            if (obj.action === 'Consultant Assigned First Time') {
                actionIcon = '<i class="bi bi-plus-square green-label"></i>';
                titleLabelClass = 'consultant-added';
            } else if (obj.action === 'Hourly Client Rate updated' || obj.action === 'Monthly Salary updated' || obj.action === 'Position Details updated'
                || obj.action === 'Consultant pricing method updated (Hourly)' || obj.action === 'Client pricing method updated (Monthly)'
                || obj.action === 'Monthly Client Rate updated' || obj.action === 'Hourly Salary updated' || obj.action === 'Consultant pricing method updated (Monthly)'
                || obj.action === 'Client pricing method updated (Hourly)') {
                actionIcon = '<i style="color:#2aa7ff" class="bi bi-pencil-square"></i>';
                titleLabelClass = 'consultant-updated';
            } else if (obj.action === 'Consultant Deactivated') {
                actionIcon = '<i class="bi bi-x-lg red-label"></i>';
                titleLabelClass = 'consultant-deactivated';
            } else if (obj.action === 'Consultant Activated') {
                actionIcon = '<i class="bi bi-plus-lg green-label"></i>';
                titleLabelClass = 'consultant-activated';
            }
            var actionDate = new Date(obj.actionDate);
            var formattedDate = ('0' + (actionDate.getMonth() + 1)).slice(-2) + '/' +
                ('0' + actionDate.getDate()).slice(-2) + '/' +
                actionDate.getFullYear();
            var isExternalProject = document.getElementById('external-pt').checked;
            var clientRateLabel = '';
            if (isExternalProject) {
                clientRateLabel = `${obj.newValueDetail} Client Rate: <strong>$${obj.newValue}</strong>, `;
            }
            if (obj.action === 'Consultant Assigned First Time' && count < 3) {
                if (count === 0) {
                    firstRow += `<li>${actionIcon} <span class="history-title ${titleLabelClass}">${obj.action}</span> (${formattedDate}): ${clientRateLabel}`;
                } else if (count === 1) {
                    firstRow += `${obj.newValueDetail} Consultant Salary: <strong>$${obj.newValue}</strong>, `;
                } else if (count === 2) {
                    firstRow += `Position: <strong>${obj.newValueDetail}</strong>. Assigned by: ${obj.userActionedBy}.</li>`;
                    row += firstRow;
                }
            } else {
                if (obj.action !== "Consultant pricing method updated (Hourly)" && obj.action !== "Consultant pricing method updated (Monthly)"
                    && obj.action !== "Client pricing method updated (Monthly)" && obj.action !== "Client pricing method updated (Hourly)"
                    && obj.action !== "Position Details updated" && obj.action !== "Consultant Deactivated" && obj.action !== "Consultant Activated") {
                    row += `<li>${actionIcon} <span class="history-title ${titleLabelClass}">${obj.action}</span> (${formattedDate}): Old Value: <strong>$${obj.oldValue}</strong>, New value: <strong>$${obj.newValue}</strong>. Updated by: ${obj.userActionedBy}.</li>`;
                } else if (obj.action === "Position Details updated") {
                    row += `<li>${actionIcon} <span class="history-title ${titleLabelClass}">${obj.action}</span> (${formattedDate}): Old Value: <strong>${obj.oldValueDetail}</strong>, New value: <strong>${obj.newValueDetail}</strong>. Updated by: ${obj.userActionedBy}.</li>`;
                } else {
                    row += `<li>${actionIcon} <span class="history-title ${titleLabelClass}">${obj.action}</span> (${formattedDate}). Updated by: ${obj.userActionedBy}.</li>`;
                }
            }
            count++;
        });
        bodyList.innerHTML = row;
        hideSpinner();
        showModal(modalId);
    });
}


//HTTP REQUESTS
async function getSuccessManagerIdAndNameByClientId(clientId) {
    var url = "/AccountManagement/Clients/GetSuccessManagerIdAndNameByClientId?clientId=" + encodeURIComponent(clientId);
    try {
        let response = await fetch(url);
        if (response.ok) {
            return await response.json();
        } else {
            let errorData = await response.json();
            throw new Error('The request to the server failed!. More details: ' + errorData.error);
        }
    } catch (error) {
        console.error('Error fetching data:', error);
        return null;
    }
}

async function activateDeactivateConsultantFromProjectHttps(projectConsultantAssignedId, actionDate) {
    var url = "/AccountManagement/Projects/ActivateDeactivateConsultantFromProject";
    try {
        var token = $('[name="__RequestVerificationToken"]').val();
        var formData = new FormData();
        formData.append('projectConsultantAssignedId', projectConsultantAssignedId);
        formData.append('actionDate', actionDate);
        let response = await fetch(url, {
            method: 'POST',
            headers: {
                RequestVerificationToken: token
            },
            body: formData
        });
        if (response.ok) {
            return await response.json();
        } else {
            if (response.status === 404) {
                displayToasterError("Resource not found (404).");
                throw new Error('404 Not Found: The requested resource could not be found!');
            } else {
                let errorData = await response.json();
                displayToasterError(errorData.error || 'An unknown error occurred.');
                throw new Error('The request to the server failed!. More details: ' + errorData.error);
            }
        }
    } catch (error) {
        console.error('Error fetching data:', error);
        return null;
    }
}

async function getProjectConsultantHistoryHttps(projectConsultantAssignedId) {
    var url = "/AccountManagement/Projects/GetProjectConsultantAssignedHistoryById?projectConsultantAssignedId="
        + encodeURIComponent(projectConsultantAssignedId);
    try {
        let response = await fetch(url);
        if (response.ok) {
            return await response.json();
        } else {
            if (response.status === 404) {
                displayToasterError("Resource not found (404).");
                throw new Error('404 Not Found: The requested resource could not be found!');
            } else {
                let errorData = await response.json();
                displayToasterError(errorData.error || 'An unknown error occurred.');
                throw new Error('The request to the server failed!. More details: ' + errorData.error);
            }
            hideSpinner();
        }
    } catch (error) {
        displayToasterError('Error fetching data: ' + error);
        console.error('Error fetching data:', error);
        hideSpinner();
        return null;
    }
}


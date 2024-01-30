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
                listItem.innerHTML = '<strong>' + item.consultantName + '</strong>' + ' (' + item.email + ')';
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
    } else {
        document.getElementById('monthlyConsultantSalaryEl').style.display = 'none';
        document.getElementById('hourlyConsultantSalaryEl').style.display = 'block';
        document.getElementById('monthlySalary').value = null;
    }
}
//DISPLAY MODAL
async function displayAddUpdateConsultant(modalId, id) {
    inicializeSecondModalButtons(modalId);
    var modalTitle = document.getElementById('add-consultant-modal-title');
    modalTitle.textContent = id === null ? 'ADD CONSULTANT TO THE PROJECT' : 'EDIT CONSULTANT ASSIGNATION PROJECT';
    resetForm('form-add-update-consultant');
    createUpdateForm.find('[name="consultantIdFromSearch"]').val("");
    validateRatesInputs();
    showModal(modalId);

    if (id !== null) {
        createUpdateForm.find('[name="consultantIdFromSearch"]').val(id);

        var url = "/ProjectManagement/Projects/GetProjectDataById?projectId=" + encodeURIComponent(id);
    }
    //displaySpinner();
    //fetch(url)
    //    .then(response => {
    //        if (response.ok) {
    //            return response.json();
    //        } else {
    //            return response.json().then(errorData => {
    //                displayToasterError(errorData.error);
    //                hideModal(modalId);
    //                throw new Error('The request to the server failed!. More details: ' + errorData.detail);
    //            });
    //        }
    //    })
    //    .then(data => {
    //        createUpdateForm.find('[name="clientId"]').val(data.clientData.clientId);
    //        createUpdateForm.find('[name="clientName"]').val(data.clientData.name);
    //        createUpdateForm.find('[name="contact"]').val(data.clientData.contact);
    //        createUpdateForm.find('[name="contactOccupation"]').val(data.clientData.contactOccupation);
    //        createUpdateForm.find('[name="emails"]').val(data.clientData.emails);
    //        let adDate = new Date(data.clientData.admissionDate);
    //        createUpdateForm.find('[name="admissionDate"]').val(adDate.toISOString().split('T')[0]);
    //        createUpdateForm.find('[name="paymentCondition"]').val(data.clientData.paymentCondition);
    //        createUpdateForm.find('[name="latePaymentFee"]').val(Number(data.clientData.latePaymentFee * 100).toFixed(2));
    //        createUpdateForm.find('[name="clientClass"]').val(data.clientData.clientClass);
    //        createUpdateForm.find('[name="address"]').val(data.clientData.address);
    //        if (data.clientData.successManagerId !== null) {
    //            var newOption = document.createElement('option');
    //            newOption.value = data.clientData.successManagerId;
    //            newOption.text = data.clientData.successManager;
    //            newOption.selected = true;
    //            successManagerSelect.appendChild(newOption);
    //        } else {
    //            var nullOption = document.createElement('option');
    //            nullOption.value = null;
    //            nullOption.text = "-Select a user-";
    //            successManagerSelect.appendChild(nullOption);
    //        }
    //        var isActive = data.clientData.isActive === "S" ? true : false;
    //        createUpdateForm.find('[name="isActive"]').val(isActive);
    //        createUpdateForm.find('[name="isActive"]').prop('checked', isActive);
    //        createUpdateForm.find('[name="allowSentLatePaymentNotifications"]').val(data.clientData.allowSentLatePaymentNotifications);
    //        createUpdateForm.find('[name="allowSentLatePaymentNotifications"]').prop('checked', data.clientData.allowSentLatePaymentNotifications);
    //        if (data.clientData.additionalEmailsForNotifications !== null) {
    //            var emailsArray = data.clientData.additionalEmailsForNotifications.split(";");
    //            emailsArray = emailsArray.map(email => email.trim()).filter(email => email !== "");
    //            emailsArray.forEach(function (email) {
    //                addNewAdditionalEmailRow(email)
    //            });
    //        }
    //        showModal(modalId);
    //    })
    //    .finally(() => {
    //        hideSpinner();
    //    });
}
//ADD CONSULTANT TO PROJECT 
function addConsultantToProject(modalId) {
    var hourlyClientRateValue = createUpdateForm.find('[name="hourlyClientRate"]').val();
    var monthlyClientRateValue = createUpdateForm.find('[name="monthlyClientRate"]').val();
    var hourlyConsultantRateValue = createUpdateForm.find('[name="hourlySalary"]').val();
    var monthlyConsultantRateValue = createUpdateForm.find('[name="monthlySalary"]').val();
    var clientRateMethodRb = document.querySelector('input[name="client-rate-model"]:checked').value;
    var consultantRateMethodRb = document.querySelector('input[name="consultant-rate-model"]:checked').value;
    var positonDetailValue = createUpdateForm.find('[name="positionDetail"]').val();
    var modelState = true;
    if (createUpdateForm.find('[name="consultantIdFromSearch"]').val() === null
        || createUpdateForm.find('[name="consultantIdFromSearch"]').val() === '') {
        modelState = false;
        displayToasterWarning('You must search and select a Consultant.');
    }
    if (positonDetailValue.length === 0) {
        modelState = false;
        displayToasterWarning('The Position Description is required.');
    }

    if (Number(hourlyClientRateValue) === 0 && clientRateMethodRb === 'H') {
        modelState = false;
        displayToasterWarning('The Hourly Client Rate is required.');
    }
    if (Number(monthlyClientRateValue) === 0 && clientRateMethodRb === 'M') {
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

    if (modelState) {
        addConsultantToModalCreateUpdateProject(modalId);
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

//HTTP REQUESTS
async function getSuccessManagerIdAndNameByClientId(clientId) {
    var url = "/ProjectManagement/Clients/GetSuccessManagerIdAndNameByClientId?clientId=" + encodeURIComponent(clientId);
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
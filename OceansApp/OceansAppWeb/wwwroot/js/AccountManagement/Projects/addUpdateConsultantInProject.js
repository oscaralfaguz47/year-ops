var partnerSelect = document.getElementById("PartnerSelect");
let partnersArray = [];

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

function validateRatesInputs() {
    let clientRateMethod = document.querySelector('input[name="client-rate-model"]:checked').value;
    let consultantRateMethod = document.querySelector('input[name="consultant-rate-model"]:checked').value;
    let consultantPaymentModel = document.querySelector('input[name="consultant-payment-model"]:checked').value;
    let oceansPaymentSection = document.getElementById('oceans-payment-section');
    let oceansPaymentRadioSection = document.getElementById('oceans-payment-radio-section');
    let thirdPartySalaryInput = document.getElementById('thirdPartyConsultantSalaryEl');

    if (consultantPaymentModel === 'O' || consultantPaymentModel === 'Hy') {
        oceansPaymentSection.style.display = 'flex';
        oceansPaymentRadioSection.style.display = 'block';
    } else {
        oceansPaymentSection.style.display = 'none';
        oceansPaymentRadioSection.style.display = 'none';
    }
    if (consultantPaymentModel === 'T' || consultantPaymentModel === 'Hy') {
        thirdPartySalaryInput.style.display = 'flex';
    } else {
        thirdPartySalaryInput.style.display = 'none';
    }
    if (consultantPaymentModel === 'T') {
        document.getElementById('hourlySalary').value = null;
        document.getElementById('monthlySalary').value = null;
    } else if (consultantPaymentModel === 'O') {
        document.getElementById('thirdPartySalary').value = null;
        document.getElementById('PartnerSelect').value = null;
    }

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
    } else if (consultantRateMethod === 'H') {
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
    const modalTitle = document.getElementById('add-consultant-modal-title');
    modalTitle.textContent = id === null ? 'ADD CONSULTANT TO THE PROJECT' : 'EDIT CONSULTANT ASSIGNATION PROJECT';
    resetForm('form-add-update-consultant');
    createUpdateForm.find('[name="proConsAssignedId"]').val("");
    createUpdateForm.find('[name="consultantIdFromSearch"]').val("");
    createUpdateForm.find('[name="isDefaultProject"]').prop('disabled', false);
    validateRatesInputs();

    document.getElementById('search-input-cont').style.display = 'block';
    const clientRateSection = document.getElementById("client-rate-section");
    const clientRateInputs = document.getElementById("client-rate-inputs");
    if (document.getElementById('external-pt').checked) {
        clientRateSection.style.display = 'block';
        clientRateInputs.style.display = 'flex';
    } else {
        clientRateSection.style.display = 'none';
        clientRateInputs.style.display = 'none';
    }
    let positionIdSelect = createUpdateForm.find('[name="position"]');
    positionIdSelect.empty();
    let newOption = new Option('-First select a Consultant-', '');
    positionIdSelect.append(newOption);

    if (partnersArray.length === 0) {
        partnersArray = await getPartnersList();
    }
    populateSelect('PartnerSelect', partnersArray.partners, '-Select a partner-', null);

    if (id == null) {
        positionIdSelect.prop('disabled', true);
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
                let positionSelect = document.getElementById('positionSelect');
                fillPositionsSelect(positionSelect, data.consultantAssignation.consultantId, data.consultantAssignation.positionId);

                if (data.consultantAssignation.hourlyClientRate !== 0) document.getElementsByName('client-rate-model')[0].checked = true;
                if (data.consultantAssignation.monthlyClientRate !== 0) document.getElementsByName('client-rate-model')[1].checked = true;
                if (data.consultantAssignation.monthlySalary !== 0) document.getElementsByName('consultant-rate-model')[0].checked = true;
                if (data.consultantAssignation.hourlySalary !== 0) document.getElementsByName('consultant-rate-model')[1].checked = true;
                if (data.consultantAssignation.monthlySalaryThirdParty === 0 && (data.consultantAssignation.monthlySalary !== 0 ||
                    data.consultantAssignation.hourlySalary !== 0)) document.getElementsByName('consultant-payment-model')[0].checked = true;
                if (data.consultantAssignation.monthlySalaryThirdParty !== 0 && (data.consultantAssignation.monthlySalary !== 0 ||
                    data.consultantAssignation.hourlySalary !== 0)) document.getElementsByName('consultant-payment-model')[1].checked = true;
                if (data.consultantAssignation.monthlySalaryThirdParty !== 0 && (data.consultantAssignation.monthlySalary === 0 &&
                    data.consultantAssignation.hourlySalary === 0)) document.getElementsByName('consultant-payment-model')[2].checked = true;
                validateRatesInputs();
                createUpdateForm.find('[name="monthlyClientRate"]').val(data.consultantAssignation.monthlyClientRate);
                createUpdateForm.find('[name="hourlyClientRate"]').val(data.consultantAssignation.hourlyClientRate);
                createUpdateForm.find('[name="monthlySalary"]').val(data.consultantAssignation.monthlySalary);
                createUpdateForm.find('[name="hourlySalary"]').val(data.consultantAssignation.hourlySalary);
                createUpdateForm.find('[name="thirdPartySalary"]').val(data.consultantAssignation.monthlySalaryThirdParty);

                partnerSelect.value = data.consultantAssignation.partnerId;
                createUpdateForm.find('[name="isMonthlySalaryCalculatedPerHour"]').val(data.consultantAssignation.isMonthlySalaryCalculatedPerHour);
                createUpdateForm.find('[name="isMonthlySalaryCalculatedPerHour"]').prop('checked', data.consultantAssignation.isMonthlySalaryCalculatedPerHour);
                createUpdateForm.find('[name="accessToTrackingTool"]').val(data.consultantAssignation.accessToTrackingTool);
                createUpdateForm.find('[name="accessToTrackingTool"]').prop('checked', data.consultantAssignation.accessToTrackingTool);
                createUpdateForm.find('[name="isDefaultProject"]').val(data.consultantAssignation.isDefaultProject);
                createUpdateForm.find('[name="isDefaultProject"]').prop('checked', data.consultantAssignation.isDefaultProject);
                data.consultantAssignation.isDefaultProject ? createUpdateForm.find('[name="isDefaultProject"]').prop('disabled', true) : '';

                showModal(modalId);
            })
            .finally(() => {
                hideSpinner();
            });
    } else {
        showModal(modalId);
    }
}

// FILL POSITIONS LIST
async function fillPositionsSelect(selectElement, consultantId, selectedValue) {
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    getPositionsByConsultantIdList(consultantId)
        .then(data => {
            selectElement.innerHTML = '';
            selectElement.innerHTML = '<option value="">-Select a Position-</option>';
            data.positions.forEach(obj => {
                var option = new Option(obj.positionName, obj.consultantPositionId);
                selectElement.add(option);
                selectElement.disabled = false;
            });
            if (selectedValue !== null) {
                selectElement.value = selectedValue;
            }
        })
        .catch(error => {
            console.error('Error fetching:', error);
        });
}

//ADD CONSULTANT TO PROJECT 
function addConsultantToProject(modalId) {
    let projectConsultantAssignedValue = createUpdateForm.find('[name="proConsAssignedId"]').val();
    let hourlyClientRateValue = createUpdateForm.find('[name="hourlyClientRate"]').val();
    let monthlyClientRateValue = createUpdateForm.find('[name="monthlyClientRate"]').val();
    let hourlyConsultantRateValue = createUpdateForm.find('[name="hourlySalary"]').val();
    let monthlyConsultantRateValue = createUpdateForm.find('[name="monthlySalary"]').val();
    let thirdPartyConsultantSalaryValue = createUpdateForm.find('[name="thirdPartySalary"]').val();
    let clientRateMethodRb = document.querySelector('input[name="client-rate-model"]:checked').value;
    let consultantRateMethodRb = document.querySelector('input[name="consultant-rate-model"]:checked').value;
    let positionIdValue = createUpdateForm.find('[name="position"]').val();
    let actionDateValue = createUpdateForm.find('[name="actionDate"]').val();
    let isBillableValue = document.getElementById("IsBillable").value;
    let isMonthlySalaryCalculatedPerHourVal = createUpdateForm.find('[name="isMonthlySalaryCalculatedPerHour"]').prop('checked');
    let accessToTrackingToolVal = createUpdateForm.find('[name="accessToTrackingTool"]').prop('checked');
    let isDefaultProjectVal = createUpdateForm.find('[name="isDefaultProject"]').prop('checked');
    let consultantPaymentModel = document.querySelector('input[name="consultant-payment-model"]:checked').value;

    var modelState = true;
    if ((createUpdateForm.find('[name="consultantIdFromSearch"]').val() === null
        || createUpdateForm.find('[name="consultantIdFromSearch"]').val() === '') && projectConsultantAssignedValue === "") {
        modelState = false;
        displayToasterWarning('You must search and select a Consultant.');
    }

    if (positionIdValue === null || positionIdValue === '') {
        modelState = false;
        displayToasterWarning('The Position is required.');
    }

    if (isBillableValue === "true" && (Number(hourlyClientRateValue) === 0 && clientRateMethodRb === 'H')) {
        modelState = false;
        displayToasterWarning('The Hourly Client Rate is required.');
    }
    if (isBillableValue === "true" && (Number(monthlyClientRateValue) === 0 && clientRateMethodRb === 'M')) {
        modelState = false;
        displayToasterWarning('The Monthly Client Rate is required.');
    }
    if (Number(hourlyConsultantRateValue) === 0 && consultantRateMethodRb === 'H' && (consultantPaymentModel === 'Hy' || consultantPaymentModel === 'O')) {
        modelState = false;
        displayToasterWarning('The Hourly Consultant Salary is required.');
    }
    if (Number(monthlyConsultantRateValue) === 0 && consultantRateMethodRb === 'M' && (consultantPaymentModel === 'Hy' || consultantPaymentModel === 'O')) {
        modelState = false;
        displayToasterWarning('The Monthly Consultant Salary is required.');
    }
    if (Number(thirdPartyConsultantSalaryValue) === 0 && (consultantPaymentModel === 'T' || consultantPaymentModel === 'Hy')) {
        modelState = false;
        displayToasterWarning('Consultant Monthly Salary - Partner is required.');
    }
    if ((partnerSelect.value === 'null' || partnerSelect.value === '') && (consultantPaymentModel === 'T' || consultantPaymentModel === 'Hy')) {
        modelState = false;
        displayToasterWarning('The Partner is required.');
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
                MonthlySalaryThirdParty: Number(thirdPartyConsultantSalaryValue),
                PartnerId: partnerSelect.value === '' || partnerSelect.value === 'null' ? null : partnerSelect.value,
                PositionId: positionIdValue,
                ActionDate: actionDateValue ? actionDateValue.toString() : null,
                IsMonthlySalaryCalculatedPerHour: Boolean(isMonthlySalaryCalculatedPerHourVal),
                AccessToTrackingTool: Boolean(accessToTrackingToolVal),
                IsDefaultProject: Boolean(isDefaultProjectVal)
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
                .catch(error => {
                    validateSessionExpiration(error.message);
                })
                .finally(() => {
                    hideSpinner();
                })
        }
    }
}

let selectedIndexA = -1;
async function searchConsultantsBySearchText(searchTextInput, hiddenInputForId, consultantNameInput, consultantEmailInput, userCategoryName) {
    if (searchTextInput.value.length > 100) {
        searchTextInput.value = searchTextInput.value.slice(0, 100);
    } else {
        let resultsContainer = document.getElementById('consultant-search-results');
        resultsContainer.innerHTML = '';
        resultsContainer.innerHTML = `<div class="text-center"><div class="spinner-border" role="status">
        <span class="sr-only"></span>
        </div></div>`;
        let data = await getConsultantsBySearchText(searchTextInput.value);
        resultsContainer.innerHTML = '';
        resultsContainer.style.display = 'block';
        if (data.consultants.length > 0) {
            let resultList = document.createElement('ul');
            resultList.id = 'search-result-list'; // Assign an ID to the results list container
            for (let item of data.consultants) {
                let listItem = document.createElement('li');
                listItem.innerHTML = '<strong>' + item.consultantName + '</strong> ' + (item.userCategoryName === "Administrative" ? '<span style="color:gray">(' : '<span class="blue-label">(') + item.userCategoryName + ')</span>';
                listItem.onclick = function () {
                    document.getElementById(hiddenInputForId).value = item.consultantId;
                    document.getElementById(consultantNameInput).value = item.consultantName;
                    document.getElementById(consultantEmailInput).value = item.email;
                    hideConsultantResultsD();
                    let positionSelect = document.getElementById('positionSelect');
                    fillPositionsSelect(positionSelect, item.consultantId, null);
                };
                resultList.appendChild(listItem);
            }
            resultsContainer.appendChild(resultList);
        } else {
            resultsContainer.innerHTML = '<div class="red-label text-center">No results found</div>';
        }
    }
    document.addEventListener('keydown', keyboardNavigationC);
}

// Function to update the active item in the results list
function updateActiveItemA() {
    const listItems = document.querySelectorAll('#search-result-list li');
    // Removes the active class from all elements.
    listItems.forEach(item => {
        item.classList.remove('active');
    });
    // Adds the active class to the selected element.
    if (selectedIndexA >= 0 && selectedIndexA < listItems.length) {
        listItems[selectedIndexA].classList.add('active');
        listItems[selectedIndexA].scrollIntoView({ behavior: "smooth", block: "nearest" });
    }
}

function keyboardNavigationC(event) {
    const resultsContainer = document.getElementById('consultant-search-results');
    const listItems = document.querySelectorAll('#search-result-list li');
    if (resultsContainer.style.display !== 'none') {
        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                if (selectedIndexA < listItems.length - 1) {
                    selectedIndexA++;
                    updateActiveItemA();
                }
                break;
            case 'ArrowUp':
                event.preventDefault();
                if (selectedIndexA > 0) {
                    selectedIndexA--;
                    updateActiveItemA();
                }
                break;
            case 'Enter':
                event.preventDefault();
                if (selectedIndexA >= 0 && selectedIndexA < listItems.length) {
                    listItems[selectedIndexA].click();
                }
                break;
        }
    }
    if (event.key === 'Escape') {
        hideConsultantResultsD();
    }
}

function hideConsultantResultsD() {
    let resultsContainer = document.getElementById('consultant-search-results');
    resultsContainer.style.display = 'none';
    selectedIndexA = -1; // Reset the selected index
    document.getElementById('search-consultant-input').value = null;
}

// Add a listener for clicks outside the results container to close the results when clicked outside.
document.addEventListener('click', function (event) {
    const searchContainer = document.getElementById('consultants-search-cont');
    if (!searchContainer.contains(event.target)) {
        hideConsultantResultsD();
    }
});
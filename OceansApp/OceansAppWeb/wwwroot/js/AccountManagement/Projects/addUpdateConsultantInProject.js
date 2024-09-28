var partnerSelect = getElementById("PartnerSelect");
let partnersArray = [];

const lastActionDateMessage = getElementById('last-action-date-message');
const hourlySalaryInputCUCP = getElementById('hourlySalary');
const monthlySalaryInputCUCP = getElementById('monthlySalary');
const hourlyClientRateElCUCP = getElementById('hourlyClientRateEl');
const thirdPartySalaryInputCUCP = getElementById('thirdPartySalary');
const partnerSelectCUCP = getElementById('PartnerSelect');
const monthlyClientRateElCUCP = getElementById('monthlyClientRateEl');
const monthlyClientRateInputCUCP = getElementById('monthlyClientRate');
const hourlyClientRateInputCUCP = getElementById('hourlyClientRate');
const monthlyConsultantSalaryElCUCP = getElementById('monthlyConsultantSalaryEl');
const hourlyConsultantSalaryElCUCP = getElementById('hourlyConsultantSalaryEl');
const isMonthlySalaryCalculatedPerHourElCUCP = getElementById("isMonthlySalaryCalculatedPerHour");
const calculationMethodCheckboxCUCP = getElementById("calculationMethod");

function validateRatesInputs() {
    let clientRateMethod = document.querySelector('input[name="client-rate-model"]:checked');
    let consultantRateMethod = document.querySelector('input[name="consultant-rate-model"]:checked');
    let consultantPaymentModel = document.querySelector('input[name="consultant-payment-model"]:checked');
    let oceansPaymentSection = getElementById('oceans-payment-section');
    let oceansPaymentRadioSection = getElementById('oceans-payment-radio-section');
    let thirdPartySalaryInput = getElementById('thirdPartyConsultantSalaryEl');

    if (consultantPaymentModel.value === 'O' || consultantPaymentModel.value === 'Hy') {
        oceansPaymentSection.style.display = 'flex';
        oceansPaymentRadioSection.style.display = 'block';
    } else {
        oceansPaymentSection.style.display = 'none';
        oceansPaymentRadioSection.style.display = 'none';
    }
    if (consultantPaymentModel.value === 'T' || consultantPaymentModel.value === 'Hy') {
        thirdPartySalaryInput.style.display = 'block';
        benefitsArePaidByPartnerCheckboxCUCP.checked = true;
    } else {
        thirdPartySalaryInput.style.display = 'none';
        benefitsArePaidByPartnerCheckboxCUCP.checked = false;
    }
    if (consultantPaymentModel.value === 'T') {
        hourlySalaryInputCUCP.value = null;
        monthlySalaryInputCUCP.value = null;
        benefitsArePaidByPartnerCheckboxCUCP.checked = true;
    } else if (consultantPaymentModel.value === 'O') {
        thirdPartySalaryInputCUCP.value = null;
        partnerSelectCUCP.value = null;
        benefitsArePaidByPartnerCheckboxCUCP.checked = false;
    }

    if (clientRateMethod.value === 'H') {
        hourlyClientRateElCUCP.style.display = 'block';
        monthlyClientRateElCUCP.style.display = 'none';
        monthlyClientRateInputCUCP.value = null;
    } else {
        hourlyClientRateElCUCP.style.display = 'none';
        monthlyClientRateElCUCP.style.display = 'block';
        hourlyClientRateInputCUCP.value = null;
    }
    if (consultantRateMethod.value === 'M') {
        monthlyConsultantSalaryElCUCP.style.display = 'block';
        hourlyConsultantSalaryElCUCP.style.display = 'none';
        hourlySalaryInputCUCP.value = null;
        isMonthlySalaryCalculatedPerHourElCUCP.style.display = 'block';
        calculationMethodCheckboxCUCP.checked = true;
        accessToTrackingToolCheckboxCUCP.disabled = false;
        if (!accessToTrackingToolCheckboxCUCP.checked) {
            calculationMethodCheckboxCUCP.checked = false;
            calculationMethodCheckboxCUCP.disabled = true;
        } else {
            calculationMethodCheckboxCUCP.checked = true;
            calculationMethodCheckboxCUCP.disabled = false;
        }
    } else if (consultantRateMethod.value === 'H') {
        monthlyConsultantSalaryElCUCP.style.display = 'none';
        hourlyConsultantSalaryElCUCP.style.display = 'block';
        monthlySalaryInputCUCP.value = null;
        isMonthlySalaryCalculatedPerHourElCUCP.style.display = 'none';
        calculationMethodCheckboxCUCP.checked = true;
        accessToTrackingToolCheckboxCUCP.checked = true;
        accessToTrackingToolCheckboxCUCP.disabled = true;
    }
}
function hideShowMustPayHolidaysCheckbox(isDefault) {
    if (isDefault.checked) {
        holidaysMustBePaidElCUCP.style.display = 'block';
        holidaysMustBePaidCheckboxCUCP.checked = true;
    } else {
        holidaysMustBePaidElCUCP.style.display = 'none';
        holidaysMustBePaidCheckboxCUCP.checked = false;
    }

}
//DISPLAY MODAL
const createUpdateForm = getElementById('form-add-update-consultant');
const proConAssignedIdInputCUCP = createUpdateForm.querySelector('[name="proConsAssignedId"]');
const consultantIdInputCUCP = createUpdateForm.querySelector('[name="consultantIdFromSearch"]');
const isDefaultProjectCheckboxCUCP = createUpdateForm.querySelector('[name="isDefaultProject"]');
const searchInputContElCUCP = getElementById('search-input-cont');
const positionIdSelectCUCP = createUpdateForm.querySelector('[name="position"]');
const consultantNameInputCUCP = createUpdateForm.querySelector('[name="consultantNameInput"]');
const consultantEmailInputCUCP = getElementById('consultantEmailInput');
const isMonthlySalaryCalculatedPerHourCheckbox = createUpdateForm.querySelector('[name="isMonthlySalaryCalculatedPerHour"]');
const accessToTrackingToolCheckboxCUCP = createUpdateForm.querySelector('[name="accessToTrackingTool"]');
const holidaysMustBePaidElCUCP = getElementById('holidaysMustBePaidEl');
const holidaysMustBePaidCheckboxCUCP = getElementById('holidaysMustBePaid');
const benefitsArePaidByPartnerCheckboxCUCP = getElementById('PartnerPaysBenefits');

const clientRateSection = getElementById("client-rate-section");
const clientRateInputs = getElementById("client-rate-inputs");
const actionDate = getElementById('actionDate');

function disableActionDateDatePicker(date) {

    let dateTimeFormat = new Date(date);
    let dateFormatted = dateTimeFormat.toISOString().substr(0, 10);
    let todaysDate = new Date();
    let todaysDateFormated = todaysDate.toISOString().substr(0, 10);

    if (dateFormatted < todaysDateFormated) {
        actionDate.min = todaysDateFormated;
    } else {
        actionDate.min = dateFormatted;
    }
    function validateDate() {
        if (actionDate.value < actionDate.min) {
            actionDate.value = actionDate.min;
        }
    }
    actionDate.addEventListener('change', validateDate);
};
function enableAllDates() {
    const actionDate = getElementById('actionDate');
    actionDate.removeAttribute('min');
    actionDate.removeAttribute('max');

    if (typeof validateDate !== 'undefined' && actionDate.onchange) {
        actionDate.removeEventListener('change', validateDate);
    }
}
async function displayAddUpdateConsultant(modalId, id) {
    enableAllDates();
    inicializeSecondModalButtons(modalId);
    lastActionDateMessage.style.display = 'none';
    const modalTitle = getElementById('add-consultant-modal-title');
    modalTitle.textContent = id === null ? 'ADD CONSULTANT TO THE PROJECT' : 'EDIT CONSULTANT PARAMETERS';
    resetForm('form-add-update-consultant');
    proConAssignedIdInputCUCP.value = "";
    consultantIdInputCUCP.vaue = "";
    isDefaultProjectCheckboxCUCP.disabled = false;
    validateRatesInputs();

    searchInputContElCUCP.style.display = 'block';
    if (getElementById('external-pt').checked && isBillableInputCUP.checked) {
        clientRateSection.style.display = 'block';
        clientRateInputs.style.display = 'flex';
    } else {
        clientRateSection.style.display = 'none';
        clientRateInputs.style.display = 'none';
    }
    positionIdSelectCUCP.innerHTML = '<option>-First select a Consultant-</option>';

    if (partnersArray.length === 0) {
        partnersArray = await getPartnersList();
    }
    populateSelect('PartnerSelect', partnersArray.partners, '-Select a partner-', null);

    if (id == null) {
        positionIdSelectCUCP.disabled = true;
    }
    holidaysMustBePaidElCUCP.style.display = 'none';

    if (id !== null) {
        proConAssignedIdInputCUCP.value = id;
        searchInputContElCUCP.style.display = 'none';

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
                let creationDateDateFormat = new Date(data.consultantAssignation.creationDate);
                lastActionDateMessage.innerHTML = `<label>The Action Date for last update was 
                <strong>"${formatDateMmDdYyyy(data.consultantAssignation.actionDate)}"</strong>
                . All the displayed data belongs to the last one. The changes were applied on (${formatUtcToLocalMmDdYyyyTime(creationDateDateFormat)}).
                </label>`;
                disableActionDateDatePicker(data.consultantAssignation.actionDate);
                lastActionDateMessage.style.display = 'block';
                consultantIdInputCUCP.value = data.consultantAssignation.consultantId;
                consultantNameInputCUCP.value = data.consultantAssignation.consultantName;
                consultantEmailInputCUCP.value = data.consultantAssignation.email;

                fillPositionsSelect(positionIdSelectCUCP, data.consultantAssignation.consultantId, data.consultantAssignation.positionId);

                if (data.consultantAssignation.hourlyClientRate !== 0) document.getElementsByName('client-rate-model')[0].checked = true;
                if (data.consultantAssignation.monthlyClientRate !== 0) document.getElementsByName('client-rate-model')[1].checked = true;
                if (data.consultantAssignation.monthlySalary !== 0) document.getElementsByName('consultant-rate-model')[0].checked = true;
                if (data.consultantAssignation.hourlySalary !== 0) document.getElementsByName('consultant-rate-model')[1].checked = true;
                if (data.consultantAssignation.monthlySalaryPartner === 0 && (data.consultantAssignation.monthlySalary !== 0 ||
                    data.consultantAssignation.hourlySalary !== 0)) document.getElementsByName('consultant-payment-model')[0].checked = true;
                if (data.consultantAssignation.monthlySalaryPartner !== 0 && (data.consultantAssignation.monthlySalary !== 0 ||
                    data.consultantAssignation.hourlySalary !== 0)) document.getElementsByName('consultant-payment-model')[1].checked = true;
                if (data.consultantAssignation.monthlySalaryPartner !== 0 && (data.consultantAssignation.monthlySalary === 0 &&
                    data.consultantAssignation.hourlySalary === 0)) document.getElementsByName('consultant-payment-model')[2].checked = true;
                monthlyClientRateInputCUCP.value = data.consultantAssignation.monthlyClientRate;
                hourlyClientRateInputCUCP.value = data.consultantAssignation.hourlyClientRate;
                monthlySalaryInputCUCP.value = data.consultantAssignation.monthlySalary;
                hourlySalaryInputCUCP.value = data.consultantAssignation.hourlySalary;
                thirdPartySalaryInputCUCP.value = data.consultantAssignation.monthlySalaryPartner;
                partnerSelect.value = data.consultantAssignation.partnerId;
                accessToTrackingToolCheckboxCUCP.checked = data.consultantAssignation.accessToTrackingTool;
                isDefaultProjectCheckboxCUCP.checked = data.consultantAssignation.isDefaultProject;
                data.consultantAssignation.isDefaultProject ? isDefaultProjectCheckboxCUCP.disabled = true : isDefaultProjectCheckboxCUCP.disabled = false;
                data.consultantAssignation.isDefaultProject ? holidaysMustBePaidElCUCP.style.display = 'block' :
                    holidaysMustBePaidElCUCP.style.display = 'none';
                holidaysMustBePaidCheckboxCUCP.checked = data.consultantAssignation.holidaysMustBePaid;
                validateRatesInputs();
                benefitsArePaidByPartnerCheckboxCUCP.checked = data.consultantAssignation.partnerPaysBenefits;
                isMonthlySalaryCalculatedPerHourCheckbox.checked = data.consultantAssignation.isMonthlySalaryCalculatedPerHour;

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
async function addConsultantToProject(modalId) {
    let clientRateMethodRb = document.querySelector('input[name="client-rate-model"]:checked').value;
    let consultantRateMethodRb = document.querySelector('input[name="consultant-rate-model"]:checked').value;
    let actionDateValue = createUpdateForm.querySelector('[name="actionDate"]').value;
    let isBillableValue = getElementById("IsBillable").value;
    let consultantPaymentModel = document.querySelector('input[name="consultant-payment-model"]:checked').value;

    var modelState = true;
    if ((consultantIdInputCUCP.value === null || consultantIdInputCUCP.value === '') && proConAssignedIdInputCUCP.value === "") {
        modelState = false;
        displayToasterWarning('You must search and select a Consultant.');
    }

    if (positionIdSelectCUCP.value === null || positionIdSelectCUCP.value === '') {
        modelState = false;
        displayToasterWarning('The Position is required.');
    }

    if (isBillableValue === "true" && (Number(hourlyClientRateInputCUCP.value) === 0 && clientRateMethodRb === 'H')) {
        modelState = false;
        displayToasterWarning('The Hourly Client Rate is required.');
    }
    if (isBillableValue === "true" && (Number(monthlyClientRateInputCUCP.value) === 0 && clientRateMethodRb === 'M')) {
        modelState = false;
        displayToasterWarning('The Monthly Client Rate is required.');
    }
    if (Number(hourlySalaryInputCUCP.value) === 0 && consultantRateMethodRb === 'H' && (consultantPaymentModel === 'Hy' || consultantPaymentModel === 'O')) {
        modelState = false;
        displayToasterWarning('The Hourly Consultant Salary is required.');
    }
    if (Number(monthlySalaryInputCUCP.value) === 0 && consultantRateMethodRb === 'M' && (consultantPaymentModel === 'Hy' || consultantPaymentModel === 'O')) {
        modelState = false;
        displayToasterWarning('The Monthly Consultant Salary is required.');
    }
    if (Number(thirdPartySalaryInputCUCP.value) === 0 && (consultantPaymentModel === 'T' || consultantPaymentModel === 'Hy')) {
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
        displaySpinner();

        var token = $('[name="__RequestVerificationToken"]').val();
        var data = {
            HourlyClientRate: Number(hourlyClientRateInputCUCP.value),
            HourlySalary: Number(hourlySalaryInputCUCP.value),
            MonthlyClientRate: Number(monthlyClientRateInputCUCP.value),
            MonthlySalary: Number(monthlySalaryInputCUCP.value),
            MonthlySalaryPartner: Number(thirdPartySalaryInputCUCP.value),
            PartnerId: partnerSelect.value === '' || partnerSelect.value === 'null' ? null : partnerSelect.value,
            PositionId: positionIdSelectCUCP.value === '' ? null : positionIdSelectCUCP.value,
            ActionDate: actionDateValue ? actionDateValue.toString() : null,
            IsMonthlySalaryCalculatedPerHour: Boolean(isMonthlySalaryCalculatedPerHourCheckbox.checked),
            AccessToTrackingTool: Boolean(accessToTrackingToolCheckboxCUCP.checked),
            IsDefaultProject: Boolean(isDefaultProjectCheckboxCUCP.checked),
            IsAssigningFirstTime: proConAssignedIdInputCUCP.value === null || proConAssignedIdInputCUCP.value === '' ? true : false,
            ProjectId: projectIdInputCUP.value,
            ConsultantId: consultantIdInputCUCP.value === '' ? null : Number(consultantIdInputCUCP.value),
            PartnerPaysBenefits: Boolean(benefitsArePaidByPartnerCheckboxCUCP.checked),
            HolidaysMustBePaid: Boolean(holidaysMustBePaidCheckboxCUCP.checked)
        };

        try {
            const response = await fetch('/AccountManagement/Projects/AddUpdateConsultantInProjet', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    RequestVerificationToken: token
                },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (!response.ok) {
                if (result.messageType === "Validation Error") {
                    displayToasterWarningArray(result.errors);
                    throw new Error('Validation errors!');
                } else {
                    displayToasterError(result.error);
                    hideModal(modalId);
                    throw new Error('The request to the server failed!. More details: ' + result.detail);
                }
            } else {
                hideModal(modalId);
                if (proConAssignedIdInputCUCP.value === null || proConAssignedIdInputCUCP.value === '') {
                    await getListOfResults(false, false);
                    displayUpdateCreateProjectModal('modal-update-create-project', Number(projectIdInputCUP.value));
                }
                displayToasterSuccess(result.message);
            }
        } catch (error) {
            validateSessionExpiration(error.message);
        } finally {
            hideSpinner();
        }
    }
}


let selectedIndexA = -1;
let resultsContainer = getElementById('consultant-search-results');
async function searchConsultantsBySearchText(searchTextInput, hiddenInputForId, consultantNameInput, consultantEmailInput, userCategoryName) {
    if (searchTextInput.value.length > 100) {
        searchTextInput.value = searchTextInput.value.slice(0, 100);
    } else {
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
                    getElementById(hiddenInputForId).value = item.consultantId;
                    getElementById(consultantNameInput).value = item.consultantName;
                    getElementById(consultantEmailInput).value = item.email;
                    hideConsultantResultsD();
                    fillPositionsSelect(positionIdSelectCUCP, item.consultantId, null);
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
    resultsContainer.style.display = 'none';
    selectedIndexA = -1; // Reset the selected index
    getElementById('search-consultant-input').value = null;
}

// Add a listener for clicks outside the results container to close the results when clicked outside.
document.addEventListener('click', function (event) {
    const searchContainer = getElementById('consultants-search-cont');
    if (!searchContainer.contains(event.target)) {
        hideConsultantResultsD();
    }
});
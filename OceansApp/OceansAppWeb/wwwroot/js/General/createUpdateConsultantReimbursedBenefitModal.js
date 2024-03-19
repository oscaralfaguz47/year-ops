document.addEventListener("DOMContentLoaded", function () {
    var actionDate = document.getElementById('DateToBeReimbursed');
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

//CREATE / UPDATE REIMBURSEMENT
async function displayUpdateCreateReimbursementModal(modalId, id) {
    var modalTitle = document.getElementById('create-reimbursement-modal-title');
    modalTitle.textContent = "ADD NEW REIMBURSEMENT";
    var createUpdateForm = $('#form-create-update');
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    createUpdateForm.find('[name="reimburseBenefitId"]').val("");
    createUpdateForm.find('[name="consultantIdFromSearch"]').val("");
    const benefitSelect = createUpdateForm.find('[name="idBenefit"]')[0];
    const benefitCategorySelect = createUpdateForm.find('[name="benefitCategoryId"]')[0];
    benefitSelect.innerHTML = '<option value="">-Select a benefit-</option>';

    if (id !== null) {
        modalTitle.textContent = "UPDATE REIMBURSEMENT";
        var url = "/General/ConsultantReimbursedBenefits/GetBenefitReimbursementDataById?benefitReimbursementId=" + encodeURIComponent(id);
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
                console.log(data);
                createUpdateForm.find('[name="reimburseBenefitId"]').val(data.benefitReimbursementData.reimbursedBenefitId);
                createUpdateForm.find('[name="consultantIdFromSearch"]').val(data.benefitReimbursementData.consultantId);
                createUpdateForm.find('[name="consultantNameInput"]').val(data.benefitReimbursementData.consultantName);
                createUpdateForm.find('[name="consultantEmailInput"]').val(data.benefitReimbursementData.consultantEmail);
                createUpdateForm.find('[name="amountReimbursed"]').val(data.benefitReimbursementData.amountReimbursed);
                let dateToBeReimbursedDateFormat = new Date(data.benefitReimbursementData.dateToBeReimbursed);
                createUpdateForm.find('[name="dateToBeReimbursed"]').val(dateToBeReimbursedDateFormat.toISOString().split('T')[0]);
                createUpdateForm.find('[name="detail"]').val(data.benefitReimbursementData.detail);
                var benefitSelect = createUpdateForm.find('[name="idBenefit"]');
                var benefitCategorySelect = createUpdateForm.find('[name="benefitCategoryId"]');
                benefitSelect.html('<option value="' + data.benefitReimbursementData.benefitId + '">' + data.benefitReimbursementData.benefitName + '</option>');
                benefitSelect.val(data.benefitReimbursementData.benefitId);

                selectBenefit('BenefitSelect', data.benefitReimbursementData.benefitId, true, data.benefitReimbursementData.benefitCategoryId);
                benefitCategorySelect.val(data.benefitReimbursementData.benefitCategoryId);
                showModal(modalId);
            })
            .finally(() => {
                hideSpinner();
            });
    } else {
        benefitCategorySelect.disabled = true;
        benefitCategorySelect.innerHTML = '';
        benefitCategorySelect.innerHTML = '<option value>-First select a Benefit-</option>';
        showModal(modalId);
    }
}

//SELECT BENEFIT AND FILL BENEFIT CATEGORIES LIST
function selectBenefit(selectElementId, selectedValue, isEditing, selectedValueBenefitCategory) {
    if (selectedValue !== null) {
        var selectElement = document.getElementById(selectElementId);
        for (var i = 0; i < selectElement.options.length; i++) {
            if (selectElement.options[i].value === "" || selectElement.options[i].value === null) {
                selectElement.remove(i);
                break;
            }
        }
        fillBenefitCategoriesForSelect(selectedValue, isEditing, selectedValueBenefitCategory);
    }
}
function fillBenefitCategoriesForSelect(selectedValue, isEditing, selectedValueBenefitCategory) {
    var selectElement = document.getElementById("BenefitCategorySelect");
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    displaySpinner();
    getBenefitCategoriesList(selectedValue)
        .then(data => {
            selectElement.innerHTML = '';
            selectElement.innerHTML = '<option value>-Select a Category-</option>';
            selectElement.disabled = false;
            data.benefitCategories.forEach(obj => {
                selectElement.add(new Option(obj.text, obj.value));
            });
            if (isEditing) {
                selectElement.value = selectedValueBenefitCategory;
            }
            hideSpinner();
        })
        .catch(error => {
            hideSpinner();
            console.error('Error fetching roles:', error);
        });
}

//CREATE, UPDATE BENEFIT REIMBURESEMENT
async function createUpdateBenefitReimbursement(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var reimburseBenefitIdData = createUpdateForm.find('[name="reimburseBenefitId"]').val() || null;
    var consultantIdData = createUpdateForm.find('[name="consultantIdFromSearch"]').val() || null;
    var idBenefitData = createUpdateForm.find('[name="idBenefit"]').val() || null;
    var benefitCategoryIdData = createUpdateForm.find('[name="benefitCategoryId"]').val() || null;
    var amountReimbursedData = createUpdateForm.find('[name="amountReimbursed"]').val() || null;
    var dateToBeReimbursedData = createUpdateForm.find('[name="dateToBeReimbursed"]').val() || null;
    var detailData = createUpdateForm.find('[name="detail"]').val() || null;

    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        ReimbursedBenefitId: reimburseBenefitIdData,
        BenefitId: idBenefitData,
        Detail: detailData,
        ConsultantId: consultantIdData,
        AmountReimbursed: amountReimbursedData,
        DateToBeReimbursed: dateToBeReimbursedData,
        BenefitCategoryId: benefitCategoryIdData
    };
    console.log(data);
    fetch('/General/ConsultantReimbursedBenefits/CreateUpdateBenefitReimbursement', {
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
                        console.log(errorData.errors);
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
        });
}

// INPUT VALIDATIONS
document.getElementById('Detail').addEventListener('input', function (e) {
    if (this.value.length > 150) {
        this.value = this.value.slice(0, 150);
    }
});
document.addEventListener("DOMContentLoaded", function () {
    validateInputTypeNumber('AmountReimbursed');
});


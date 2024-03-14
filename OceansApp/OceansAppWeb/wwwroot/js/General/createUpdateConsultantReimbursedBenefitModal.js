//CREATE / UPDATE REIMBURSEMENT
async function displayUpdateCreateReimbursementModal(modalId, id) {
    var modalTitle = document.getElementById('create-reimbursement-modal-title');
    modalTitle.textContent = "ADD NEW REIMBURSEMENT";
    var createUpdateForm = $('#form-create-update');
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    createUpdateForm.find('[name="reimburseBenefitId"]').val("");
    const benefitSelect = createUpdateForm.find('[name="idBenefit"]')[0];
    benefitSelect.innerHTML = '<option value="">-Select a benefit-</option>';

    if (id !== null) {
        modalTitle.textContent = "UPDATE REIMBURSEMENT";
        var url = "/General/Consultants/GetConsultantDataById?consultantId=" + encodeURIComponent(id);
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
                createUpdateForm.find('[name="consultantId"]').val(data.consultantData.consultantId);
                createUpdateForm.find('[name="name"]').val(data.consultantData.name);
                createUpdateForm.find('[name="lastName"]').val(data.consultantData.lastName);
                createUpdateForm.find('[name="userName"]').val(data.consultantData.email);
                createUpdateForm.find('[name="userName"]').prop('disabled', true);
                createUpdateForm.find('[name="personalEmail"]').val(data.consultantData.personalEmail);
                createUpdateForm.find('[name="phoneNumber"]').val(data.consultantData.phoneNumber);
                createUpdateForm.find('[name="phone2"]').val(data.consultantData.phone2);
                data.consultantData.companyId !== null ? paymentMethodSelect.prop('disabled', false) : paymentMethodSelect.prop('disabled', true);
                companySelect.val(data.consultantData.companyId);
                selectCompany('CompanySelect', data.consultantData.companyId, true, data.consultantData.paymentMethodId);
                paymentMethodSelect.val(data.consultantData.paymentMethodId);
                selectCategory(data.consultantData.userCategoryName, data.consultantData.positions, true, data.consultantData.userRole);
                createUpdateForm.find('[name="userCategoryName"]').val(data.consultantData.userCategoryName);
                var countrySelect = createUpdateForm.find('[name="idCountry"]');
                countrySelect.html('<option value="' + data.consultantData.idCountry + '">' + data.consultantData.countryName + '</option>');
                createUpdateForm.find('[name="idCountry"]').val(data.consultantData.idCountry);
                createUpdateForm.find('[name="address"]').val(data.consultantData.address);
                createUpdateForm.find('[name="location"]').val(data.consultantData.location);
                showModal(modalId);
            })
            .finally(() => {
                hideSpinner();
            });
    } else {
        showModal(modalId);
    }
}
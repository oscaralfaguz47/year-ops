//CREATE / UPDATE CONSULTANT MODAL
async function displayUpdateCreateConsultantModal(modalId, id) {
    document.getElementById('create-consultant-modal-title').textContent = "ADD NEW CONSULTANT";
    var createUpdateForm = $('#form-create-update');
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    var projectsContainer = $("#projects-container");
    projectsContainer.empty();
    createUpdateForm.find('[name="consultantId"]').val("");
    document.getElementById("saved-consultant-message").style.display = "none";
    var projectsAssignedSection = document.getElementById("projects-assigned-section");
    projectsAssignedSection.style.display = "none";
    const countrySelect = createUpdateForm.find('[name="idCountry"]')[0];
    countrySelect.innerHTML = '<option value="">-Select a country-</option>';

    showModal(modalId);
    if (id !== null) {
        document.getElementById('create-Project-modal-title').textContent = "UPDATE PROJECT";
        var url = "/AccountManagement/Projects/GetProjectDataById?projectId=" + encodeURIComponent(id);
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
                createUpdateForm.find('[name="projectId"]').val(data.projectData.projectId);
                createUpdateForm.find('[name="projectName"]').val(data.projectData.name);
                createUpdateForm.find('[name="description"]').val(data.projectData.description);

                var newOptionClient = document.createElement('option');
                newOptionClient.value = data.projectData.clientId;
                newOptionClient.text = data.projectData.clientName;
                newOptionClient.selected = true;
                clientSelect.appendChild(newOptionClient);
                clientSelect.disabled = true;

                successManagerSelect.innerHTML = '';
                var newOptionSuccessManager = document.createElement('option');
                newOptionSuccessManager.value = data.projectData.successManagerId;
                newOptionSuccessManager.text = data.projectData.successManagerName;
                newOptionSuccessManager.selected = true;
                successManagerSelect.appendChild(newOptionSuccessManager);
                successManagerSelect.disabled = false;

                let startDateDateFormat = new Date(data.projectData.startDate);
                createUpdateForm.find('[name="startDate"]').val(startDateDateFormat.toISOString().split('T')[0]);

                createUpdateForm.find('[name="isActive"]').val(data.projectData.isActive);
                createUpdateForm.find('[name="isActive"]').prop('checked', data.projectData.isActive);
                createUpdateForm.find('[name="isBillable"]').val(data.projectData.isBillable);
                createUpdateForm.find('[name="isBillable"]').prop('checked', data.projectData.isBillable);
                createUpdateForm.find('[name="clientHasTrackingTool"]').val(data.projectData.clientHasTrackingTool);
                createUpdateForm.find('[name="clientHasTrackingTool"]').prop('checked', data.projectData.clientHasTrackingTool);
                data.projectData.assignedConsultants.forEach(function (item, index, arr) {
                    addNewConsultantRow(item.consultantName, item.projectConsultantAssignedId, item.consultantId, item.positionDetail,
                        item.hourlyClientRate, item.monthlyClientRate, item.hourlySalary, item.monthlySalary, item.isActive)
                });
                showModal(modalId);
            })
            .finally(() => {
                hideSpinner();
            });
    }
}

//CreateUpdate Consultant METHOD
async function createUpdateConsultant(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var consultantIdData = createUpdateForm.find('[name="consultantId"]').val() || null;
    var consultantNameData = createUpdateForm.find('[name="name"]').val();
    var consultantLastNameData = createUpdateForm.find('[name="lastName"]').val();
    var emailData = createUpdateForm.find('[name="userName"]').val();
    var userCategoryNameData = createUpdateForm.find('[name="userCategoryName"]').val();
    var idCountryData = createUpdateForm.find('[name="idCountry"]').val();
    var phoneNumberData = createUpdateForm.find('[name="phoneNumber"]').val() || null;
    var phone2Data = createUpdateForm.find('[name="phone2"]').val() || null;
    var addressData = createUpdateForm.find('[name="address"]').val() || null;
    var personalEmailData = createUpdateForm.find('[name="personalEmail"]').val() || null;
    var locationData = createUpdateForm.find('[name="location"]').val() || null;
    var userRoleData = createUpdateForm.find('[name="userRole"]').val();

    var token = $('[name="__RequestVerificationToken"]').val();
    var data = {
        ConsultantId: consultantIdData,
        Name: consultantNameData,
        LastName: consultantLastNameData,
        Email: emailData,
        UserCategoryName: userCategoryNameData,
        IdCountry: idCountryData,
        PhoneNumber: phoneNumberData,
        Phone2: phone2Data,
        Address: addressData,
        PersonalEmail: personalEmailData,
        Location: locationData,
        UserRole: userRoleData
    };
    console.log(data);
    fetch('/General/Consultants/CreateUpdateConsultant', {
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
            document.getElementById("projects-assigned-section").style.display = "block";
            inicializeModalButtons(modalId);
            displayToasterSuccess(data.message);
            console.log(data.consultantId);
            if (data.consultantId > 0) {
                document.getElementById("saved-consultant-message").style.display = "block";
                createUpdateForm.find('[name="consultantId"]').val(data.consultantId);
            } else {
                hideModal(modalId);
            }
            getListOfResults(false, false);
        });
}
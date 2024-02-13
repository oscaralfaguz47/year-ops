//CREATE / UPDATE CONSULTANT
async function displayUpdateConsultantModal(modalId, id) {
    document.getElementById('create-consultant-modal-title').textContent = "CREATE NEW CONSULTANT";
    var createUpdateForm = $('#form-create-update');
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    var projectsContainer = $("#projects-container");
    projectsContainer.empty();
    createUpdateForm.find('[name="consultantId"]').val("");

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

$(document).ready(function () {
    getListOfResults(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/AccountManagement/Projects/GetProjectsList?model=" + encodeURIComponent(queryString);

    fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    displayToasterErrorArray(errorData.errors);
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .then(data => {
            var tbody = $(".global-table-container table tbody");
            var noResultsMessage = $(".no-results");
            noResultsMessage.empty();
            tbody.empty();
            var count = 1;
            data.projectsList.forEach(function (obj) {
                var startDate = new Date(obj.startDate);
                var formattedDate = ('0' + (startDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + startDate.getDate()).slice(-2) + '/' +
                    startDate.getFullYear();
                var row = `<tr class="hover-group">
                  <td>
                  <i onclick="displayUpdateModal('modal-update-create-project', ${obj.projectId})" class='bi bi-pencil-square table-icon edit-table-icon' title="Edit"></i>
                      ${obj.name}
                  </td>
                  <td>${obj.clientName}</td>
                  <td>${formattedDate}</td>
                  <td style="text-align:center"><label class="switch">
                    <input onchange="activateDeactivateProject(this, ${obj.projectId}, '${obj.name}', ${obj.isActive})" value="${obj.isActive}" ${obj.isActive ? 'checked' : ''} type="checkbox">
                    <span class="slider round"></span>
                    </label>
                  </td>
                  <td>${obj.successManagerName === null ? "" : obj.successManagerName}</td>
                  <td><div class="assigned-consultants-div" id="conAssigned${count}"></div></td>
                  <td class="tracking-tool-td">${obj.clientHasTrackingTool ? '<i class="bi bi-check green-label"></i>' : '<i class="bi bi-x red-label"></i>'}</td>
                  <td class="tracking-tool-td">${obj.isBillable ? '<i class="bi bi-check green-label"></i>' : '<i class="bi bi-x red-label"></i>'}</td>
                  <td>${obj.description === null ? "" : obj.description}</td>
              </tr>`;
                tbody.append(row);
                addConsultantIcons(obj.numConsultantsAssigned, "conAssigned" + count);
                count++;
            });

            if (data.projectsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
            };
            updatePagination(data.paginationFilters.paginationWithoutFilters.pagination);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            hideSpinner();
        });
}
function addConsultantIcons(num, tdId) {
    const tdElement = document.getElementById(tdId);
    tdElement.innerHTML = "";
    for (let i = 0; i < Math.min(num, 3); i++) {
        tdElement.innerHTML += '<i style="z-index:' + i + '" class="bi bi-person-fill"></i>';
        // tdElement.innerHTML += '<img src="https://ca.slack-edge.com/TJV63SXV5-U047NF10QH5-b853bea57b67-72" style="z-index:' + i + ';width:35px; border-radius:50%; margin-right: -7px;border: solid 2px #fff;">';
    }
    if (num > 3) {
        tdElement.innerHTML += '<i class="more-consultants-span">+' + (num - 3) + '</i> ';
    }
    if (num === 0) {
        tdElement.innerHTML += '<i style="font-size:21px; margin-right:5px;" class="bi bi-person-x-fill red-label"></i><span class="red-label"> No assigned consultants.</span>';
    }
}

//CREATE / UPDATE PROJECT
async function displayUpdateModal(modalId, id) {
    document.getElementById('create-Project-modal-title').textContent = "CREATE NEW PROJECT";
    var createUpdateForm = $('#form-create-update');
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    var consultantsContainer = $("#consultants-container");
    consultantsContainer.empty();
    createUpdateForm.find('[name="projectId"]').val("");
    var projectTypeInputsCont = document.getElementById("project-type-inputs-cont");
    projectTypeInputsCont.style.display = 'block';
    var projectTypeLabel = document.getElementById("saved-project-type-label");
    projectTypeLabel.style.display = 'none';
    const clientSelect = createUpdateForm.find('[name="client"]')[0];
    var clientSelectCont = document.getElementById("client-select-cont");
    clientSelectCont.style.display = 'block';
    clientSelect.innerHTML = '<option value="null">-Select a client-</option>';
    clientSelect.disabled = false;
    const successManagerSelect = createUpdateForm.find('[name="successManager"]')[0];
    successManagerSelect.innerHTML = '<option value="null">-Select a user-</option>';
    successManagerSelect.disabled = true;
    var billableTrackingToolCont = document.getElementById("billable-tracking-tool-cont");
    billableTrackingToolCont.style.display = 'block';
    var consultantsAssignedSection = document.getElementById("consultants-assigned-section");
    consultantsAssignedSection.style.display = 'none';
    document.getElementById("saved-project-message").style.display = "none";
    var billableInput = document.getElementById("IsBillable");
    billableInput.disabled = false;
    showModal(modalId);
    if (id !== null) {
        billableInput.disabled = true;
        document.getElementById('create-Project-modal-title').textContent = "UPDATE PROJECT";
        var url = "/AccountManagement/Projects/GetProjectDataById?projectId=" + encodeURIComponent(id);
        displaySpinner();
        projectTypeInputsCont.style.display = 'none';
        projectTypeLabel.style.display = 'block';
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
                consultantsAssignedSection.style.display = 'block';
                createUpdateForm.find('[name="projectId"]').val(data.projectData.projectId);
                createUpdateForm.find('[name="projectName"]').val(data.projectData.name);
                createUpdateForm.find('[name="description"]').val(data.projectData.description);
                if (data.projectData.clientName === "Oceans Code Experts") {
                    clientSelectCont.style.display = 'none';
                    billableTrackingToolCont.style.display = 'none';
                    document.getElementById("saved-project-type-span").textContent = '"Administrative Internal"';
                    document.getElementById('internal-pt').checked = true;
                    document.getElementById('external-pt').checked = false;
                } else {
                    document.getElementById("saved-project-type-span").textContent = '"Client External"';
                    document.getElementById('external-pt').checked = true;
                    document.getElementById('internal-pt').checked = false;
                }
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
                        item.hourlyClientRate, item.monthlyClientRate, item.hourlySalary, item.monthlySalary, item.statusAction, null,
                        null, data.allowedManageAdminConsultants, item.userCategoryName, data.projectData.projectId, item.MonthlySalaryThirdParty);
                });
                showModal(modalId);
            })
            .catch(error => {
                validateSessionExpiration(error.message);
            })
            .finally(() => {
                hideSpinner();
            });
    }
}
function validateProjectType() {
    var externalProjectType = document.querySelector('input[name="projectTypeRb"]:checked').value;
    var clientSelectContainer = document.getElementById("client-select-cont");
    var clientSelect = document.getElementById("ClientSelect");
    var successManagerSelect = document.getElementById("successManagerIdSelect");
    var billableTrackingToolCont = document.getElementById("billable-tracking-tool-cont");

    if (externalProjectType === 'E') {
        clientSelectContainer.style.display = 'block';
        clientSelect.value = null;
        billableTrackingToolCont.style.display = 'block';
    } else {
        clientSelectContainer.style.display = 'none';
        successManagerSelect.disabled = false;
        billableTrackingToolCont.style.display = 'none';
    }
}
//CreateUpdate Project
async function createUpdateProject(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var projectIdData = createUpdateForm.find('[name="projectId"]').val() || null;
    var projectNameData = createUpdateForm.find('[name="projectName"]').val();
    var projectDetailData = createUpdateForm.find('[name="description"]').val();
    var clientIdData = createUpdateForm.find('[name="client"]').val();
    var startDateData = createUpdateForm.find('[name="startDate"]').val() || null;
    var successManagerData = createUpdateForm.find('[name="successManager"]').val();
    var isActiveData = createUpdateForm.find('[name="isActive"]').prop('checked');
    var isBillableData = createUpdateForm.find('[name="isBillable"]').prop('checked');
    var clientHasTrackingToolData = createUpdateForm.find('[name="clientHasTrackingTool"]').prop('checked');
    var consultantsElements = document.querySelectorAll(".consultantRow");
    var projectTypeValue = document.querySelector('input[name="projectTypeRb"]:checked').value;
    var billableInput = document.getElementById("IsBillable");
    var consultantsData = [];
    consultantsData = Array.from(consultantsElements).map(function (fila) {
        var projectConsultantAssignedId = fila.querySelector('[name="projectConsultantAssignedId"]').value ? fila.querySelector('[name="projectConsultantAssignedId"]').value : null;
        var consultantId = fila.querySelector('[name="consultantIdCreateProject"]').value;
        var positionDetail = fila.querySelector('[name="positionDetailCreateProject"]').value;
        var hourlyClientRateCreateProject = fila.querySelector('[name="hourlyClientRateCreateProject"]').value;
        var monthlyClientRateCreateProject = fila.querySelector('[name="monthlyClientRateCreateProject"]').value;
        var hourlySalaryCreateProject = fila.querySelector('[name="hourlySalaryCreateProject"]').value;
        var monthlySalaryCreateProject = fila.querySelector('[name="monthlySalaryCreateProject"]').value;
        var monthlySalaryThirdPartyCreateProject = fila.querySelector('[name="monthlySalaryThirdPartyCreateProject"]').value;
        var actionDateCreateProject = fila.querySelector('[name="actionDateCreateProject"]').value;
        var mothlySalaryCalculatedPerHourInput = fila.querySelector('input[name="monthlySalaryCalculatedPerHourCreateProject"]');
        return {
            ProjectConsultantAssignedId: projectConsultantAssignedId,
            ConsultantId: Number(consultantId),
            HourlyClientRate: Number(hourlyClientRateCreateProject),
            HourlySalary: Number(hourlySalaryCreateProject),
            MonthlyClientRate: Number(monthlyClientRateCreateProject),
            MonthlySalary: Number(monthlySalaryCreateProject),
            MonthlySalaryThirdParty: Number(monthlySalaryThirdPartyCreateProject),
            IsMonthlySalaryCalculatedPerHour: Boolean(mothlySalaryCalculatedPerHourInput.value),
            PositionDetail: positionDetail,
            ActionDate: actionDateCreateProject ? actionDateCreateProject.toString() : null
        };
    });

    var token = $('[name="__RequestVerificationToken"]').val();
    var data = {
        ProjectId: projectIdData,
        Name: projectNameData,
        Description: projectDetailData,
        ClientId: Number(clientIdData),
        StartDate: startDateData ? startDateData.toString() : null,
        SuccessManagerId: Number(successManagerData),
        IsActive: Boolean(isActiveData),
        IsBillable: Boolean(isBillableData),
        ClientHasTrackingTool: Boolean(clientHasTrackingToolData),
        AssignedConsultants: consultantsData,
        ProjectType: projectTypeValue
    };
    fetch('/AccountManagement/Projects/CreateUpdateProject', {
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
            document.getElementById("consultants-assigned-section").style.display = "block";
            billableInput.disabled = true;
            inicializeModalButtons(modalId);
            displayToasterSuccess(data.message);
            createUpdateForm.find('[name="client"]').prop('disabled', true);
            if (data.projectId > 0) {
                billableInput.value = isBillableData;
                document.getElementById("saved-project-type-label").style.display = 'block';
                if (projectTypeValue === "I") {
                    document.getElementById("saved-project-type-span").textContent = '"Administrative Internal"';
                } else {
                    document.getElementById("saved-project-type-span").textContent = '"Client External"';
                }
                document.getElementById("project-type-inputs-cont").style.display = "none";
                document.getElementById("saved-project-message").style.display = "block";
                createUpdateForm.find('[name="projectId"]').val(data.projectId);
            } else {
                hideModal(modalId);
            }
            getListOfResults(false, false);
        }).catch(error => {
            validateSessionExpiration(error.message);
        });
}
function fillClientsSelectForCreateProjectModal(selectElement, firstOption) {
    fillClientsSelectForFilters(selectElement, firstOption);
    selectElement.onchange = function () {
        displaySpinner();
        getSuccessManagerIdAndNameByClientId(selectElement.value)
            .then(data => {
                var successManagerSelect = document.getElementById('successManagerIdSelect');
                if (data !== null) {
                    successManagerSelect.innerHTML = '<option selected value="' + data.successManager.userId + '">' + data.successManager.userName + '</option>';
                } else {
                    successManagerSelect.innerHTML = '<option selected value="null">-Select a user-</option>';
                }
                hideSpinner();
            })
            .catch(error => {
                validateSessionExpiration(error.message);
                console.error('Error fetching data:', error);
            });
        document.getElementById('successManagerIdSelect').disabled = false;
    };
}

//ADD / UPDATE CONSULTANT

function addConsultantToModalCreateUpdateProject(modalId) {
    var createUpdateConsultantForm = $('#form-add-update-consultant');
    var consultantProjectAssignedId = createUpdateConsultantForm.find('[name="proConsAssignedId"]').val();
    var consultantIdValue = createUpdateConsultantForm.find('[name="consultantIdFromSearch"]').val();
    var consultantNameValue = createUpdateConsultantForm.find('[name="consultantNameInput"]').val();
    var positionDetailValue = createUpdateConsultantForm.find('[name="positionDetail"]').val();
    var hourlyClientRateValue = createUpdateConsultantForm.find('[name="hourlyClientRate"]').val();
    var monthlyClientRateValue = createUpdateConsultantForm.find('[name="monthlyClientRate"]').val();
    var hourlyConsultantRateValue = createUpdateConsultantForm.find('[name="hourlySalary"]').val();
    var monthlyConsultantRateValue = createUpdateConsultantForm.find('[name="monthlySalary"]').val();
    var monthlyConsultantThirdPartyValue = createUpdateConsultantForm.find('[name="thirdPartySalary"]').val();
    var actionDateValue = createUpdateConsultantForm.find('[name="actionDate"]').val();
    var monthlySalaryCalculatedPerHourValue = createUpdateConsultantForm.find('[name="isMonthlySalaryCalculatedPerHour"]').prop('checked');

    if (consultantProjectAssignedId === "") {
        addNewConsultantRow(consultantNameValue, consultantProjectAssignedId, consultantIdValue, positionDetailValue,
            hourlyClientRateValue, monthlyClientRateValue, hourlyConsultantRateValue, monthlyConsultantRateValue, null,
            actionDateValue, monthlySalaryCalculatedPerHourValue, null, null, null, monthlyConsultantThirdPartyValue)
    } else {
        document.getElementById('positionDetail-' + consultantProjectAssignedId).value = positionDetailValue;
        document.getElementById('hourlyClientRate-' + consultantProjectAssignedId).value = hourlyClientRateValue;
        document.getElementById('monthlyClientRate-' + consultantProjectAssignedId).value = monthlyClientRateValue;
        document.getElementById('hourlySalary-' + consultantProjectAssignedId).value = hourlyConsultantRateValue;
        document.getElementById('monthlySalary-' + consultantProjectAssignedId).value = monthlyConsultantRateValue;
    }
    hideModal(modalId);
}
function addNewConsultantRow(consultantName, consProjAssId, consultantId, positionDetail, hourlyClientRate, monthlyClientRate,
    hourlyConsultantSalary, monthlyConsultantSalary, statusAction, actionDate, monthlySalaryCalculatedPerHour, allowedMAdminConsultants, userCategoryName, projectId, monthlySalaryThirdParty) {
    // Create new row
    var row = document.createElement("div");
    row.className = "consultantRow";
    let spanToInnerToConsultant = `<strong>${consultantName}</strong>`;

    if (consProjAssId !== '') {
        var actionStatusSpan = '';
        var activeInactiveBtn = '';
        let userCategorySpanColor = userCategoryName === 'Consultant' ? '#2196F3' : 'gray';
        if (statusAction !== null) {
            const todayDate = new Date();
            const localDate = new Date(todayDate.getFullYear(), todayDate.getMonth(), todayDate.getDate());

            let statusAndDate = statusAction.split("; ");
            let statusDate = new Date(statusAndDate[0]);
            let statusText = statusAndDate[1];
            let ableToActivateOrInactivate = false;
            let isActive = false;
            let statusLabel = '';
            let statusClass = 'red-label';
            if (statusText === 'Consultant Activated') {
                statusLabel = 'Activated';
                statusClass = 'green-label';
            } else {
                statusLabel = 'Deactivated';
            }
            if (statusDate.toISOString().split('T')[0] > localDate.toISOString().split('T')[0]) {
                actionStatusSpan = '<label style="font-size:13px" id="a-i-label-' + consProjAssId + '"><span class="' + statusClass + '">Will be <strong>' + statusLabel + '</strong> on ' + statusDate.toISOString().split('T')[0] + '</span></label>';
                spanToInnerToConsultant = `<strong>${consultantName}</strong><span style="font-size: 12px; color:${userCategorySpanColor}"> (${userCategoryName})</span> ${actionStatusSpan}`;
            } else if (statusDate.toISOString().split('T')[0] < localDate.toISOString().split('T')[0]) {
                ableToActivateOrInactivate = true;
                if (statusText === 'Consultant Activated') {
                    isActive = true;
                }
                actionStatusSpan = '<label style="font-size:13px" id="a-i-label-' + consProjAssId + '"><span class="' + statusClass + '"><strong>' + statusLabel + '</strong> on ' + statusDate.toISOString().split('T')[0] + '</span></label>';
                spanToInnerToConsultant = `<strong>${consultantName}</strong><span style="font-size: 12px; color:${userCategorySpanColor}"> (${userCategoryName})</span> ${actionStatusSpan}`;
            } else if (statusDate.toISOString().split('T')[0] === localDate.toISOString().split('T')[0]) {
                if (statusText === 'Consultant Activated') {
                    isActive = true;
                    actionStatusSpan = '<label style="font-size:13px" id="a-i-label-' + consProjAssId + '"><span class="' + statusClass + '"><strong>' + statusLabel + '</strong> today</span></label>';
                } else {
                    actionStatusSpan = '<label style="font-size:13px" id="a-i-label-' + consProjAssId + '"><span class="' + statusClass + '"><strong>' + statusLabel + '</strong> today</span></label>';
                }
                ableToActivateOrInactivate = false;
                spanToInnerToConsultant = `<strong>${consultantName}</strong><span style="font-size: 12px; color:${userCategorySpanColor}"> (${userCategoryName})</span> ${actionStatusSpan}`;
            }
            activeInactiveBtn = ableToActivateOrInactivate ? `<li id="activate-deactivate-li-${consProjAssId}" onclick="activateDeactivateConFromProject(${consProjAssId}, '${consultantName}', ${isActive}, ${projectId})">${isActive ? '<i class="bi bi-x-lg red-label"></i>' : '<i class="bi bi-plus-lg green-label"></i>'}${isActive ? 'Deactivate from Project' : 'Activate in the Project'}</li>` : '';
        } else {
            activeInactiveBtn = `<li id="activate-deactivate-li-${consProjAssId}" onclick="activateDeactivateConFromProject(${consProjAssId}, '${consultantName}', true, ${projectId})"><i class="bi bi-x-lg red-label"></i>Deactivate from Project</li>`;
            spanToInnerToConsultant += `<span style="font-size: 12px; color:${userCategorySpanColor}"> (${userCategoryName})</span>` + ` <label style="font-size:13px"><span class="green-label"><strong>(Active)</strong></span></label>`;
        }
        var dotsIcon = document.createElement("i");
        var editConsultantParametersBtn = '';
        var viewHistoryBtn = '';
        if (allowedMAdminConsultants || userCategoryName === 'Consultant') {
            editConsultantParametersBtn = `<li onclick="displayAddUpdateConsultant('modal-add-consultant', ${consProjAssId})"><i class="bi bi-pencil-square"></i> Edit Consultant parameters</li>`;
            viewHistoryBtn = `<li onclick="getProjectConsultantHistory(${consProjAssId}, 'modal-consultant-history')"><i class="bi bi-clock-history"></i> View History</li>`;
        }
        if (activeInactiveBtn !== '' || editConsultantParametersBtn !== '' || viewHistoryBtn !== '') {
            dotsIcon.innerHTML = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${consProjAssId}', 'menuIcon-${consProjAssId}')" class="bi bi-three-dots-vertical" id="menuIcon-${consProjAssId}"></i>
                         <div class="menu-options" id="menuOptions-${consProjAssId}">
                           <ul>
                               ${activeInactiveBtn}
                               ${editConsultantParametersBtn}
                               ${viewHistoryBtn}
                           </ul>
                         </div>
                         `;
        } else {
            dotsIcon.innerHTML = `<li style="color:transparent" class="bi bi-info-circle"></li>`;
        }
        row.appendChild(dotsIcon);
    }

    var profileIcon = document.createElement("i");
    profileIcon.innerHTML = '<i class="bi bi-person-circle consultant-icon"></i>';
    row.appendChild(profileIcon);

    var projectConsultantAssignetIdInput = document.createElement("input");
    projectConsultantAssignetIdInput.value = consProjAssId;
    projectConsultantAssignetIdInput.type = "hidden";
    projectConsultantAssignetIdInput.name = "projectConsultantAssignedId";
    row.appendChild(projectConsultantAssignetIdInput);

    var consultantIdInput = document.createElement("input");
    consultantIdInput.value = consultantId;
    consultantIdInput.name = "consultantIdCreateProject";
    consultantIdInput.type = "hidden";
    row.appendChild(consultantIdInput);

    var spanElement = document.createElement("span");
    spanElement.innerHTML = spanToInnerToConsultant;

    row.appendChild(spanElement);

    var positionDetailInput = document.createElement("input");
    positionDetailInput.value = positionDetail;
    positionDetailInput.id = `positionDetail-${consProjAssId}`;
    positionDetailInput.name = "positionDetailCreateProject";
    positionDetailInput.type = "hidden";
    row.appendChild(positionDetailInput);

    var hourlyClientRateInput = document.createElement("input");
    hourlyClientRateInput.value = hourlyClientRate;
    hourlyClientRateInput.id = `hourlyClientRate-${consProjAssId}`;
    hourlyClientRateInput.name = "hourlyClientRateCreateProject";
    hourlyClientRateInput.type = "hidden";
    row.appendChild(hourlyClientRateInput);

    var monthlyClientRateInput = document.createElement("input");
    monthlyClientRateInput.value = monthlyClientRate;
    monthlyClientRateInput.id = `monthlyClientRate-${consProjAssId}`;
    monthlyClientRateInput.name = "monthlyClientRateCreateProject";
    monthlyClientRateInput.type = "hidden";
    row.appendChild(monthlyClientRateInput);

    var hourlyConsultantSalaryInput = document.createElement("input");
    hourlyConsultantSalaryInput.value = hourlyConsultantSalary;
    hourlyConsultantSalaryInput.id = `hourlySalary-${consProjAssId}`;
    hourlyConsultantSalaryInput.name = "hourlySalaryCreateProject";
    hourlyConsultantSalaryInput.type = "hidden";
    row.appendChild(hourlyConsultantSalaryInput);

    var monthlyConsultantSalaryInput = document.createElement("input");
    monthlyConsultantSalaryInput.value = monthlyConsultantSalary;
    monthlyConsultantSalaryInput.id = `monthlySalary-${consProjAssId}`;
    monthlyConsultantSalaryInput.name = "monthlySalaryCreateProject";
    monthlyConsultantSalaryInput.type = "hidden";
    row.appendChild(monthlyConsultantSalaryInput);

    var monthlyConsultantSalaryThirdPartyInput = document.createElement("input");
    monthlyConsultantSalaryThirdPartyInput.value = monthlySalaryThirdParty;
    monthlyConsultantSalaryThirdPartyInput.id = `monthlySalaryThirdParty-${consProjAssId}`;
    monthlyConsultantSalaryThirdPartyInput.name = "monthlySalaryThirdPartyCreateProject";
    monthlyConsultantSalaryThirdPartyInput.type = "hidden";
    row.appendChild(monthlyConsultantSalaryThirdPartyInput);

    var monthlySalaryCalculatedPerHourInput = document.createElement("input");
    monthlySalaryCalculatedPerHourInput.value = monthlySalaryCalculatedPerHour;
    monthlySalaryCalculatedPerHourInput.id = `monthlySalaryCalculatedPerHour-${consProjAssId}`;
    monthlySalaryCalculatedPerHourInput.name = "monthlySalaryCalculatedPerHourCreateProject";
    monthlySalaryCalculatedPerHourInput.type = "hidden";
    row.appendChild(monthlySalaryCalculatedPerHourInput);

    var actionDateInput = document.createElement("input");
    actionDateInput.value = actionDate === undefined ? null : actionDate;
    actionDateInput.name = "actionDateCreateProject";
    actionDateInput.type = "hidden";
    row.appendChild(actionDateInput);

    // Create delete button
    if (consProjAssId === '' || consProjAssId === null) {
        var btnDelete = document.createElement("button");
        btnDelete.innerHTML = '<i class="bi bi-trash3"></i>';
        btnDelete.className = "btn-delete";
        btnDelete.onclick = function () {
            this.parentElement.remove();
        };
        row.appendChild(btnDelete);
    }

    document.getElementById("consultants-container").appendChild(row);
}

async function getSuccessManagers(thisElement) {
    const successManagerSelect = thisElement;
    const selectedValue = successManagerSelect.value;

    if (successManagerSelect.options.length > 1) {
        if (selectedValue) {
            successManagerSelect.value = selectedValue;
        }
        return;
    }

    successManagerSelect.innerHTML += '<option value="loading">Loading options… (⏳)</option>';

    try {
        const response = await fetch("/General/ConsultantDetails/GetSuccessManagers");
        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
        }
        const data = await response.json();
        successManagerSelect.innerHTML = '<option value="null">-Select a user-</option>';

        data.successManagers.forEach(obj => {
            const option = new Option(obj.userName, obj.userId);
            successManagerSelect.add(option);
        });
        if (selectedValue) {
            successManagerSelect.value = selectedValue;
        }

    } catch (error) {
            validateSessionExpiration(error.message);
        console.error('Error fetching success managers:', error);
    } finally {
        const loadingOption = successManagerSelect.querySelector('option[value="loading"]');
        if (loadingOption) {
            loadingOption.remove();
        }
    }
}
// Activate / Deactivate consultant
async function activateDeactivateConFromProject(projectConsultantAssignedId, name, status, projectId) {
    try {
        const data = await activateDeactivateConsultantFromProject(projectConsultantAssignedId, name, status);
        if (data) {
            displayUpdateModal('modal-update-create-project', projectId);
        }
    } catch (error) {
        console.error("Error: ", error);
    }
}
// Activate / Deactivate project
async function activateDeactivateProject(inputElement, projectId, name, status) {
    var title = status ? "Deactivate Project" : "Activate Project";
    var textAction = status ? "Deactivate" : "Activate";
    Swal.fire({
        title: title,
        text: 'Are you sure you want to ' + textAction + ' the "' + name + '"?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, do it!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            displaySpinner();
            var token = $('[name="__RequestVerificationToken"]').val();
            var formData = new FormData();
            formData.append('projectId', projectId);
            fetch("/AccountManagement/Projects/ActivateDeactivateProject"
                , {
                    method: 'POST',
                    headers: {
                        RequestVerificationToken: token
                    },
                    body: formData
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        toastr.success(data.message);
                        getListOfResults(false, false);
                    } else {
                        inputElement.checked = status;
                        displayToasterError(data.error);
                        console.error('There has been a problem with the fetch operation:', data.detail);
                    }
                })
                .catch(error => {
                    validateSessionExpiration(error.message);
                })
                .finally(() => {
                    hideSpinner();
                });
        } else {
            inputElement.checked = status;
        }
    });
}


//Pagination and Filters
function paginationSubmit(firstTime, filters) {
    getListOfResults(firstTime, filters);
}
function recolectDataFromForm(filters) {
    {
        var searchText = $('#search-input').val();
        const activeInactiveRadioElement = document.querySelector('.active-inactive-rg input[type="radio"]:checked');
        var activeInactiveValue = null;
        if (activeInactiveRadioElement !== null) {
            activeInactiveValue = Boolean(document.querySelector('input[name="active-inactive"]:checked').value === 'true');
        }
        var datesValMessageSpan = document.getElementById("dates-val-message");
        datesValMessageSpan.textContent = "";
        var startDateValue = document.getElementById("startDate").value || null;
        var endDateValue = document.getElementById("endDate").value || null;
        if (startDateValue === null || endDateValue === null) {
            startDateValue = null;
            endDateValue = null;
        } else if (startDateValue !== null & endDateValue !== null) {
            if (startDateValue > endDateValue) {
                datesValMessageSpan.textContent = "Date From should be less than the Date Until";
            }
        }
        var clientIdValue = Number(document.getElementById("clientFilter").value) || null;
        var successManagerValue = Number(document.getElementById("succesManagerFilter").value) || null;

        var filtersData = {
            SearchText: searchText,
            IsActive: activeInactiveValue,
            StartDate: startDateValue,
            EndDate: endDateValue,
            ClientId: clientIdValue,
            SuccessManagerId: successManagerValue
        };
        var inputFieldToOrder = document.getElementsByName('fieldToOrder')[0];
        var inputDirectionOrder = document.getElementsByName('directionOrder')[0];
        var orderByData = {
            FieldToOrder: inputFieldToOrder.value,
            DirectionOrder: inputDirectionOrder.value
        }
        var paginationData = returnCurrentPaginationValues();
        var paginationWithoutFilters = {
            Pagination: paginationData,
            RequestFromFilters: filters,
            OrderBy: orderByData
        }

        return {
            Filters: filtersData,
            PaginationWithoutFilters: paginationWithoutFilters
        };
    }
}
function updatePagination(paginationData) {
    updatePaginationValues(paginationData);
}

function enterInSearch(event) {
    paginationSubmit(false, true);
}

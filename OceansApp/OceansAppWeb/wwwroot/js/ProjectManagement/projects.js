$(document).ready(function () {
    getListOfResults(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/ProjectManagement/Projects/GetProjectsList?model=" + encodeURIComponent(queryString);

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
                var row = `<tr>
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
        }).finally(() => {
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

    const clientSelect = createUpdateForm.find('[name="client"]')[0];
    clientSelect.innerHTML = '<option value="null">-Select a client-</option>';
    clientSelect.disabled = false;
    const successManagerSelect = createUpdateForm.find('[name="successManager"]')[0];
    successManagerSelect.innerHTML = '<option value="null">-Select a user-</option>';
    successManagerSelect.disabled = true;
    showModal(modalId);
    if (id !== null) {
        document.getElementById('create-Project-modal-title').textContent = "UPDATE PROJECT";
        var url = "/ProjectManagement/Projects/GetProjectDataById?projectId=" + encodeURIComponent(id);
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
//CreateUpdate Project
async function createUpdateProject(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var projectIdData = createUpdateForm.find('[name="projectId"]').val() || null;
    var projectNameData = createUpdateForm.find('[name="projectName"]').val();
    var projectDetailData = createUpdateForm.find('[name="description"]').val();
    var clientIdData = createUpdateForm.find('[name="client"]').val();
    var startDateData = createUpdateForm.find('[name="startDate"]').val();
    var successManagerData = createUpdateForm.find('[name="successManager"]').val();
    var isActiveData = createUpdateForm.find('[name="isActive"]').prop('checked');
    var isBillableData = createUpdateForm.find('[name="isBillable"]').prop('checked');
    var clientHasTrackingToolData = createUpdateForm.find('[name="clientHasTrackingTool"]').prop('checked');
    var consultantsElements = document.querySelectorAll(".consultantRow");
    var consultantsData = [];
    consultantsData = Array.from(consultantsElements).map(function (fila) {
        var projectConsultantAssignedId = fila.querySelector('[name="projectConsultantAssignedId"]').value ? fila.querySelector('[name="projectConsultantAssignedId"]').value : null;
        var consultantId = fila.querySelector('[name="consultantIdCreateProject"]').value;
        var positionDetail = fila.querySelector('[name="positionDetailCreateProject"]').value;
        var hourlyClientRateCreateProject = fila.querySelector('[name="hourlyClientRateCreateProject"]').value;
        var monthlyClientRateCreateProject = fila.querySelector('[name="monthlyClientRateCreateProject"]').value;
        var hourlySalaryCreateProject = fila.querySelector('[name="hourlySalaryCreateProject"]').value;
        var monthlySalaryCreateProject = fila.querySelector('[name="monthlySalaryCreateProject"]').value;
        var actionDateCreateProject = fila.querySelector('[name="actionDateCreateProject"]').value;
        return {
            ProjectConsultantAssignedId: projectConsultantAssignedId,
            ConsultantId: Number(consultantId),
            HourlyClientRate: Number(hourlyClientRateCreateProject),
            HourlySalary: Number(hourlySalaryCreateProject),
            MonthlyClientRate: Number(monthlyClientRateCreateProject),
            MonthlySalary: Number(monthlySalaryCreateProject),
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
        AssignedConsultants: consultantsData
    };
    fetch('/ProjectManagement/Projects/CreateUpdateProject', {
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
            hideModal(modalId);
            createUpdateForm[0].reset();
            displayToasterSuccess(data.message);
            getListOfResults(false, false);
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
    var actionDateValue = createUpdateConsultantForm.find('[name="actionDate"]').val();

    if (consultantProjectAssignedId === "") {
        addNewConsultantRow(consultantNameValue, consultantProjectAssignedId, consultantIdValue, positionDetailValue,
            hourlyClientRateValue, monthlyClientRateValue, hourlyConsultantRateValue, monthlyConsultantRateValue, null, actionDateValue)
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
    hourlyConsultantSalary, monthlyConsultantSalary, isActive, actionDate) {
    // Create new row
    var row = document.createElement("div");
    row.className = "consultantRow";
    if (consProjAssId !== '') {
        var dotsIcon = document.createElement("i");
        dotsIcon.innerHTML = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${consProjAssId}', 'menuIcon-${consProjAssId}')" class="bi bi-three-dots-vertical" id="menuIcon-${consProjAssId}"></i>
                         <div class="menu-options" id="menuOptions-${consProjAssId}">
                           <ul>
                             <li id="activate-deactivate-li-${consProjAssId}" onclick="activateDeactivateConFromProject(${consProjAssId}, '${consultantName}', ${isActive})">${isActive ? '<i class="bi bi-x-lg red-label"></i>' : '<i class="bi bi-plus-lg green-label"></i>'}${isActive ? ' Deactivate from Project' : ' Activate in the Project'}</li>
                             <li onclick="displayAddUpdateConsultant('modal-add-consultant', ${consProjAssId})"><i class="bi bi-pencil-square"></i> Edit Consultant parameters</li>
                              <li onclick="getProjectConsultantHistory(${consProjAssId}, 'modal-consultant-history')"><i class="bi bi-clock-history"></i> View History</li>
                           </ul>
                         </div>
                         `;
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
    if (isActive !== null) {
        var isActiveSpan = isActive ? '<label id="a-i-label-' + consProjAssId + '"><span class="green-label">(Active)</span></label>' : '<label id="a-i-label-' + consProjAssId + '"><span class="red-label">(Inactive)</span></label>';
        spanElement.innerHTML = consultantName + ' ' + isActiveSpan + '';
    } else {
        spanElement.textContent = consultantName;
    }
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
        console.error('Error fetching success managers:', error);
    } finally {
        const loadingOption = successManagerSelect.querySelector('option[value="loading"]');
        if (loadingOption) {
            loadingOption.remove();
        }
    }
}
// Activate / Deactivate consultant
async function activateDeactivateConFromProject(projectConsultantAssignedId, name, status) {
    try {
        const data = await activateDeactivateConsultantFromProject(projectConsultantAssignedId, name, status);
        if (data) {
            var updatedStatus = status ? false : true;
            var activateInactivateBtn = document.getElementById('activate-deactivate-li-' + projectConsultantAssignedId);
            var activateInactivateLabel = document.getElementById('a-i-label-' + projectConsultantAssignedId);

            var iTag = activateInactivateBtn.querySelector('i');
            if (iTag) {
                iTag.parentNode.removeChild(iTag);
            }
            activateInactivateBtn.innerHTML = updatedStatus ? `
                             <i class="bi bi-x-lg red-label"></i>${updatedStatus ? ' Deactivate from Project' : ' Activate in the Project'}` :
                `<i class="bi bi-plus-lg green-label"></i>${updatedStatus ? ' Deactivate from Project' : ' Activate in the Project'}`;
            activateInactivateBtn.setAttribute('onclick', `activateDeactivateConFromProject(${projectConsultantAssignedId}, '${name}', ${updatedStatus})`);
            activateInactivateLabel.innerHTML = '';
            activateInactivateLabel.innerHTML = updatedStatus ? `<span class="green-label">(Active)</span>` :
                `<span class="red-label"> (Inactive)</span>`;
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
            fetch("/ProjectManagement/Projects/ActivateDeactivateProject"
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

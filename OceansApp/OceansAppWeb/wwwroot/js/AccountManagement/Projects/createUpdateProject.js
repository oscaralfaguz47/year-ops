//Create update project Variables
const createUpdateProjectForm = getElementById('form-create-update');
const projectIdInputCUP = createUpdateProjectForm.querySelector('[name="projectId"]')
const projectNameInputCUP = createUpdateProjectForm.querySelector('[name="projectName"]');
const descriptionInputCUP = createUpdateProjectForm.querySelector('[name="description"]');
const startDateInputCUP = createUpdateProjectForm.querySelector('[name="startDate"]');
const isActiveInputCUP = createUpdateProjectForm.querySelector('[name="isActive"]');
const isBillableInputCUP = createUpdateProjectForm.querySelector('[name="isBillable"]');
const clientHasTrackingToolInputCUP = createUpdateProjectForm.querySelector('[name="clientHasTrackingTool"]');
const clientSelecteCUP = createUpdateProjectForm.querySelector('[name="client"]');
const successManagerSelectCUP = createUpdateProjectForm.querySelector('[name="successManager"]');

const savedDisplayMessage = getElementById("saved-project-message");
const savedProjectTypeSpan = getElementById("saved-project-type-span");
const projectTypeLabel = getElementById("saved-project-type-label");
const projectTypeInputsCont = getElementById("project-type-inputs-cont");
const billableTrackingToolCont = getElementById("billable-tracking-tool-cont");
const clientSelectCont = getElementById("client-select-cont");

//CREATE / UPDATE PROJECT

async function displayUpdateCreateProjectModal(modalId, id) {
    const modalTitleCreateUpdateProject = getElementById('create-update-project-modal-title');
    modalTitleCreateUpdateProject.textContent = "CREATE NEW PROJECT";
    inicializeModalButtons(modalId);
    resetForm('form-create-update');
    if (activeClientsArray.length === 0) {
        displaySpinner();
        activeClientsArray = await getActiveClientsList();
        hideSpinner();
    }
    populateSelect('ClientSelect', activeClientsArray.clients, '-Select a client-', null);

    if (successManagersArray.length === 0) {
        displaySpinner();
        successManagersArray = await getSuccessManagersList();
        hideSpinner();
    }
    populateSelect('successManagerIdSelect', successManagersArray.successManagers, '-Select a success manager-', null);

    const consultantsContainer = $("#consultants-container");
    consultantsContainer.empty();
    projectTypeInputsCont.style.display = 'block';
    projectTypeLabel.style.display = 'none';
    clientSelectCont.style.display = 'block';
    clientSelectCreateUpdate.disabled = false;
    successManagerSelectCreateUpdate.disabled = true;
    billableTrackingToolCont.style.display = 'block';
    const consultantsAssignedSection = getElementById("consultants-assigned-section");
    consultantsAssignedSection.style.display = 'none';
    savedDisplayMessage.style.display = "none";
    const billableInput = getElementById("IsBillable");
    clientHasTrackingToolInput.disabled = false;
    billableInput.disabled = false;
    projectIdInputCUP.value = "";
    if (id !== null) {
        projectIdInputCUP.value = id;
        billableInput.disabled = true;
        clientHasTrackingToolInput.disabled = true;
        modalTitleCreateUpdateProject.textContent = "UPDATE PROJECT";
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
                projectNameInputCUP.value = data.projectData.name;
                descriptionInputCUP.value = data.projectData.description;
                if (data.projectData.clientName === "Oceans Code Experts") {
                    clientSelectCont.style.display = 'none';
                    billableTrackingToolCont.style.display = 'none';
                    savedProjectTypeSpan.textContent = '"Administrative Internal"';
                    internalClientRb.checked = true;
                    externalClientRb.checked = false;
                } else {
                    savedProjectTypeSpan.textContent = '"Client External"';
                    externalClientRb.checked = true;
                    internalClientRb.checked = false;
                }
                clientSelectCreateUpdate.value = data.projectData.clientId;
                clientSelectCreateUpdate.disabled = true;

                successManagerSelectCreateUpdate.value = data.projectData.successManagerId;
                successManagerSelectCreateUpdate.disabled = false;

                let startDateDateFormat = new Date(data.projectData.startDate);
                startDateInputCUP.value = startDateDateFormat.toISOString().split('T')[0];

                isActiveInputCUP.value = data.projectData.isActive;
                isActiveInputCUP.checked = data.projectData.isActive;
                isBillableInputCUP.value = data.projectData.isBillable;
                isBillableInputCUP.checked = data.projectData.isBillable;
                clientHasTrackingToolInputCUP.value = data.projectData.clientHasTrackingTool;
                clientHasTrackingToolInputCUP.checked = data.projectData.clientHasTrackingTool;

                const assignedConsultants = JSON.parse(data.projectData.assignedConsultants);
                assignedConsultants.forEach(function (item, index, arr) {
                    addNewConsultantRow(item.ConsultantName, item.ProjectConsultantAssignedId, item.IsActive,
                        item.UserCategory, item.BeforeOrAfterStatusActionDate, data.allowedManageAdminConsultants,
                        item.FutureStatus, item.FutureStatusDate);
                });
                showModal(modalId);
            })
            .catch(error => {
                validateSessionExpiration(error.message);
            })
            .finally(() => {
                hideSpinner();
            });
    } else {
        showModal(modalId);
    }
}
function addNewConsultantRow(consultantName, consProjAssId, statusAction, userCategoryName, actionDate, allowedMAdminConsultants,
    futureStatus, futureStatusDate) {
    // Create new row
    var row = document.createElement("div");
    row.className = "consultantRow";
    let spanToInnerToConsultant = `<strong>${consultantName}</strong>`;

    let actionStatusSpan = '';
    let activeInactiveBtn = '';
    let userCategorySpanColor = userCategoryName === 'Consultant' ? '#2196F3' : 'gray';
    function converStringToDate(stringDate) {
        const [year, month, day] = stringDate.split('-').map(Number);
        return new Date(year, month - 1, day, 0, 0, 0);
    }
    const futureStatusDateString = futureStatusDate !== undefined ? converStringToDate(futureStatusDate) : null;

    let futureStatusLabel = '';
    let currentStatusLabel = '';
    let currentStatusClass = 'red-label';
    let futureStatusClass = 'red-label';

    if (statusAction) {
        currentStatusLabel = 'Activated';
        currentStatusClass = 'green-label';
    } else {
        statusLabel = 'Deactivated';
        currentStatusLabel = 'Deactivated';
    }

    if (futureStatus !== undefined && futureStatus) {
        futureStatusLabel = 'Activated';
        futureStatusClass = 'green-label';
        if ((statusAction && futureStatus === undefined) || (futureStatus !== undefined && futureStatus && !statusAction)) {

        }
    } else {
        futureStatusLabel = 'Deactivated';
    }
    let currentStatusElement = `<label style="font-size:13px" class="${currentStatusClass}"><strong> ${statusAction ? 'Assigned First Time' : 'Deactivated' }</strong></label>`;
    if ((statusAction && futureStatus === undefined) || (futureStatus !== undefined && futureStatus && !statusAction ||
       (futureStatus !== undefined && !futureStatus && statusAction))) {
        currentStatusElement = `<label style="font-size:13px" class="${currentStatusClass}"><strong>${currentStatusLabel}</strong></label>`;
    }
    actionStatusSpan =
        currentStatusElement + (futureStatus !== undefined ?
        `<label style="font-size:13px" id="a-i-label-${consProjAssId}">
        <span class="${futureStatusClass}">&nbsp;(Will be <strong>${futureStatusLabel}</strong> on ${futureStatusDateString.toISOString().split('T')[0]})</span>
    </label>` : ``);

    spanToInnerToConsultant = `<strong>${consultantName}</strong><span style="font-size: 12px; color:${userCategorySpanColor}"> (${userCategoryName})</span> ${actionStatusSpan}`;

    futureStatus === undefined ? activeInactiveBtn = `
<li id="activate-deactivate-li-${consProjAssId}" 
    onclick="activateDeactivateConFromProject(${consProjAssId}, '${consultantName}', ${statusAction}, ${projectIdInputCUP.value})">
    ${statusAction ? '<i class="bi bi-x-lg red-label"></i>Deactivate from Project' : '<i class="bi bi-plus-lg green-label"></i>Activate in the Project'}
</li>` : activeInactiveBtn = ``;

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

    var profileIcon = document.createElement("i");
    profileIcon.innerHTML = '<i class="bi bi-person-circle consultant-icon"></i>';
    row.appendChild(profileIcon);

    var projectConsultantAssignetIdInput = document.createElement("input");
    projectConsultantAssignetIdInput.value = consProjAssId;
    projectConsultantAssignetIdInput.type = "hidden";
    projectConsultantAssignetIdInput.name = "projectConsultantAssignedId";
    row.appendChild(projectConsultantAssignetIdInput);

    var spanElement = document.createElement("span");
    spanElement.innerHTML = spanToInnerToConsultant;

    row.appendChild(spanElement);

    document.getElementById("consultants-container").appendChild(row);
}
//CreateUpdate Project
async function createUpdateProject(modalId) {
    waitingForPostMethod();
    const projectTypeCheckedInputCUP = createUpdateProjectForm.querySelector('input[name="projectTypeRb"]:checked');
    var token = $('[name="__RequestVerificationToken"]').val();
    var data = {
        ProjectId: projectIdInputCUP.value || null,
        Name: projectNameInputCUP.value,
        Description: descriptionInputCUP.value,
        ClientId: clientSelecteCUP.value === '' ? null : Number(clientSelecteCUP.value),
        StartDate: startDateInputCUP.value ? startDateInputCUP.value.toString() : null,
        SuccessManagerId: successManagerSelectCUP.value === '' ? null : Number(successManagerSelectCUP.value),
        IsActive: Boolean(isActiveInputCUP.checked),
        IsBillable: Boolean(isBillableInputCUP.checked),
        ClientHasTrackingTool: Boolean(clientHasTrackingToolInputCUP.checked),
        ProjectType: projectTypeCheckedInputCUP.value
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
            isBillableInputCUP.disabled = true;
            inicializeModalButtons(modalId);
            displayToasterSuccess(data.message);
            clientSelecteCUP.disabled = true;
            if (data.projectId > 0) {
                isBillableInputCUP.value = Boolean(isBillableInputCUP.checked);
                projectTypeLabel.style.display = 'block';
                if (projectTypeCheckedInputCUP.value === "I") {
                    savedProjectTypeSpan.textContent = '"Administrative Internal"';
                } else {
                    savedProjectTypeSpan.textContent = '"Client External"';
                }
                projectTypeInputsCont.style.display = "none";
                savedDisplayMessage.style.display = "block";
                projectIdInputCUP.value = data.projectId;
                clientHasTrackingToolInput.disabled = true;
            } else {
                hideModal(modalId);
            }
            getListOfResults(false, false);
        }).catch(error => {
            validateSessionExpiration(error.message);
        });
}

function validateProjectType() {
    const projectTypeCheckedInputCUP = createUpdateProjectForm.querySelector('input[name="projectTypeRb"]:checked');
    if (projectTypeCheckedInputCUP.value === 'E') {
        clientSelectCont.style.display = 'block';
        clientSelecteCUP.value = null;
        billableTrackingToolCont.style.display = 'block';
        isBillableInputCUP.checked = true;
    } else {
        clientSelectCont.style.display = 'none';
        successManagerSelectCUP.disabled = false;
        billableTrackingToolCont.style.display = 'none';
        isBillableInputCUP.checked = false;
    }
}

async function selectSuccessManagerByClientId(selectElement) {
    selectElement.onchange = async function () {
        displaySpinner();

        try {
            let selectedOptionText = selectElement.options[selectElement.selectedIndex].text;
            if (selectedOptionText === "Oceans Code Experts") {
                internalClientRb.checked = true;
                validateProjectType();
            } else {
                externalClientRb.disabled = false;
            }
            if (selectElement.value !== '' && selectElement.value !== 'null') {
                const data = await getSuccessManagerIdAndNameByClientId(Number(selectElement.value));
                if (data && data.successManager) {
                    successManagerSelectCreateUpdate.value = data.successManager.userId;
                } else {
                    successManagerSelectCreateUpdate.value = null;
                }
            } else {
                successManagerSelectCreateUpdate.value = null;
            }
            successManagerSelectCreateUpdate.disabled = false;
        } catch (error) {
            console.error("Error fetching success manager data:", error);
        } finally {
            hideSpinner();
        }
    };
}

//Activate and deactivate Consultant from project
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
async function activateDeactivateConsultantFromProject(projectConsultantAssignedId, name, status) {
    const title = status ? "Deactivate Consultant" : "Activate Consultant";
    const textAction = status ? "Deactivate" : "Activate";
    const textSpan = status ? "Deactivated" : "Activated";
    const validationMessage = status ? "The Deactivation Date is required." : "The Activation Date is required.";
    const statusToChange = status ? false : true;

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
            },
            didOpen: () => {
                const today = new Date();
                const localDate = new Date(today.getTime() - (today.getTimezoneOffset() * 60000)).toISOString().split('T')[0];
                document.getElementById('swal-input-action-date').setAttribute('min', localDate);
                document.getElementById('swal-input-action-date').onkeydown = (e) => {
                    e.preventDefault();
                };
            }
        })

        if (result.isConfirmed) {
            displaySpinner();
            var actionDate = document.getElementById('swal-input-action-date').value;
            const data = await activateDeactivateConsultantFromProjectHttps(projectConsultantAssignedId, actionDate, statusToChange);
            toastr.success(data.message);
            displayUpdateCreateProjectModal('modal-update-create-project', Number(projectIdInputCUP.value));
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
async function activateDeactivateConsultantFromProjectHttps(projectConsultantAssignedId, actionDate, status) {
    var url = "/AccountManagement/Projects/ActivateDeactivateConsultantFromProject";
    try {
        var token = $('[name="__RequestVerificationToken"]').val();
        var formData = new FormData();
        formData.append('projectConsultantAssignedId', projectConsultantAssignedId);
        formData.append('actionDate', actionDate);
        formData.append('statusToChange', status);
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
        validateSessionExpiration(error.message);
        console.error('Error fetching data:', error);
        return null;
    }
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
        validateSessionExpiration(error.message);
        console.error('Error fetching data:', error);
        return null;
    }
}
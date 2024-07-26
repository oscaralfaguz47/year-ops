let rightSidebarFiltersIsDiplayed = false;
let successManagersArray = [];
let allActiveInactiveClientsArray = [];
let activeClientsArray = [];
let successManagerSelectFilters = null;
let activeClientsSelectFilters = null;
let activeInactiveRadioElement = null;
const successManagerSelectCreateUpdate = document.getElementById('successManagerIdSelect');
const clientSelectCreateUpdate = document.getElementById('ClientSelect');
const clientHasTrackingToolInput = document.getElementById("ClientHasTrackingTool");

$(document).ready(function () {
    getListOfResults(true, false);
    selectSuccessManagerByClientId(clientSelectCreateUpdate);
});

let externalClientRb = document.getElementById('external-pt');
let internalClientRb = document.getElementById('internal-pt');

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
                  <i onclick="displayUpdateCreateProjectModal('modal-update-create-project', ${obj.projectId})" class='bi bi-pencil-square table-icon edit-table-icon' title="Edit"></i>
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

//MORE FILTERS
async function displayMoreFiltersProjects() {
    if (!rightSidebarFiltersIsDiplayed) {
        displaySpinner();
        let rightSidebarContainer = document.getElementById('right-sidebar-container');
        rightSidebarContainer.innerHTML = `
        <div class="header-btns-container">
         <button class="clear-btn" onclick="clearFilters('filters-form')"><img class="filter-icon" src="/icons/Shared/clear.svg">Clear filters </button>
        </div>
        <div class="scroll-container">
          <form id="filters-form">
          <div class="select-container">
             <label>Client</label>
             <select onchange="paginationSubmit(false, true)" id="ClientIdFilters" class="form-select">
             </select>
           </div>
           <div class="select-container">
             <label>Success Manager</label>
             <select onchange="paginationSubmit(false, true)" id="SuccessManagerIdFilters" class="form-select">
             </select>
           </div>
           <div class="radio-buttons-container">
            <div class="radio-group active-inactive-rg">
             <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" name="active-inactive" type="radio" value="true" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; Active
             </label>
             <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" name="active-inactive" type="radio" value="false" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; Inactive
             </label>
            </div>
           </div>
             <div>
              <div class="start-end-date-container">
                  <label>Start Date</label>
                  <div class="datepickers-container">
                    <div>
                        <label for="startDate">Date From</label>
                        <input onchange="paginationSubmit(false, true)" class="form-control" type="date" id="startDate" />
                    </div>
                    <div>
                        <label for="endDate">Date Until</label>
                        <input onchange="paginationSubmit(false, true)" class="form-control" type="date" id="endDate" />
                    </div>
                  </div>
                  <label id="dates-val-message" class="validation-message"></label>
              </div>
           </div>
          </form>
        <div>`;

        successManagerSelectFilters = document.getElementById('SuccessManagerIdFilters');
        if (successManagersArray.length === 0) {
            successManagersArray = await getSuccessManagersList();
        }
        populateSelect('SuccessManagerIdFilters', successManagersArray.successManagers, 'All success managers', null);
        activeClientsSelectFilters = document.getElementById('ClientIdFilters');
        if (allActiveInactiveClientsArray.length === 0) {
            allActiveInactiveClientsArray = await getClientsList();
        }
        populateSelect('ClientIdFilters', allActiveInactiveClientsArray.clients, 'All clients', null);

        activeInactiveRadioElement = document.querySelector('.active-inactive-rg');
        rightSidebarFiltersIsDiplayed = true;
    }
    hideSpinner();
    openRightSidebar();
}
function clearFilters(formId) {
    resetFormElements(formId);
    getListOfResults(false, true);
}
function addConsultantIcons(num, tdId) {
    const tdElement = document.getElementById(tdId);
    tdElement.innerHTML = "";
    for (let i = 0; i < Math.min(num, 3); i++) {
        tdElement.innerHTML += `<i style="z-index:${i}" class="bi bi-person-fill"></i>`;
        // tdElement.innerHTML += '<img src="https://ca.slack-edge.com/TJV63SXV5-U047NF10QH5-b853bea57b67-72" style="z-index:' + i + ';width:35px; border-radius:50%; margin-right: -7px;border: solid 2px #fff;">';
    }
    if (num > 3) {
        tdElement.innerHTML += '<i class="more-consultants-span">+' + (num - 3) + '</i> ';
    }
    if (num === 0) {
        tdElement.innerHTML += '<i style="font-size:21px; margin-right:5px;" class="bi bi-person-x-fill red-label"></i><span class="red-label"> No assigned consultants.</span>';
    }
}

//ADD / UPDATE CONSULTANT
function addConsultantToModalCreateUpdateProject(modalId) {
    var createUpdateConsultantForm = $('#form-add-update-consultant');
    var consultantProjectAssignedId = createUpdateConsultantForm.find('[name="proConsAssignedId"]').val();
    var consultantIdValue = createUpdateConsultantForm.find('[name="consultantIdFromSearch"]').val();
    var consultantNameValue = createUpdateConsultantForm.find('[name="consultantNameInput"]').val();
    var positionIdValue = createUpdateConsultantForm.find('[name="position"]').val();
    var hourlyClientRateValue = createUpdateConsultantForm.find('[name="hourlyClientRate"]').val();
    var monthlyClientRateValue = createUpdateConsultantForm.find('[name="monthlyClientRate"]').val();
    var hourlyConsultantRateValue = createUpdateConsultantForm.find('[name="hourlySalary"]').val();
    var monthlyConsultantRateValue = createUpdateConsultantForm.find('[name="monthlySalary"]').val();
    var monthlyConsultantThirdPartyValue = createUpdateConsultantForm.find('[name="thirdPartySalary"]').val();
    var partnerIdValue = createUpdateConsultantForm.find('[name="idPartner"]').val();
    var accessToTrackingToolValue = createUpdateConsultantForm.find('[name="accessToTrackingTool"]').prop('checked');
    var isDefaultProjectValue = createUpdateConsultantForm.find('[name="isDefaultProject"]').prop('checked');
    var actionDateValue = createUpdateConsultantForm.find('[name="actionDate"]').val();
    var monthlySalaryCalculatedPerHourValue = createUpdateConsultantForm.find('[name="isMonthlySalaryCalculatedPerHour"]').prop('checked');

    if (consultantProjectAssignedId === "") {
        addNewConsultantRow(consultantNameValue, consultantProjectAssignedId, consultantIdValue, positionIdValue,
            hourlyClientRateValue, monthlyClientRateValue, hourlyConsultantRateValue, monthlyConsultantRateValue, null,
            actionDateValue, monthlySalaryCalculatedPerHourValue, null, null, null, monthlyConsultantThirdPartyValue,
            accessToTrackingToolValue, isDefaultProjectValue, partnerIdValue)
    } else {
        document.getElementById('positionDetail-' + consultantProjectAssignedId).value = positionIdValue;
        document.getElementById('hourlyClientRate-' + consultantProjectAssignedId).value = hourlyClientRateValue;
        document.getElementById('monthlyClientRate-' + consultantProjectAssignedId).value = monthlyClientRateValue;
        document.getElementById('hourlySalary-' + consultantProjectAssignedId).value = hourlyConsultantRateValue;
        document.getElementById('monthlySalary-' + consultantProjectAssignedId).value = monthlyConsultantRateValue;
    }
    hideModal(modalId);
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
        var activeInactiveValue = activeInactiveRadioElement?.querySelector('input[type="radio"]:checked')?.value || null;
        var datesValMessageSpan = document.getElementById("dates-val-message");
        var datesValMessageSpan = document.getElementById("dates-val-message");
        if (datesValMessageSpan !== null) {
            datesValMessageSpan.textContent = "";
        }
        var startDateValue = document.getElementById("startDate") === null ? null : document.getElementById("startDate").value || null;
        var endDateValue = document.getElementById("endDate") === null ? null : document.getElementById("endDate").value || null;
        if (startDateValue === null || endDateValue === null) {
            startDateValue = null;
            endDateValue = null;
        } else if (startDateValue !== null & endDateValue !== null) {
            if (startDateValue > endDateValue) {
                datesValMessageSpan.textContent = "Date From must be less than Date Until";
            }
        }

        var filtersData = {
            SearchText: searchText,
            IsActive: activeInactiveValue === null ? null : activeInactiveValue === 'true' ? true : false,
            StartDate: startDateValue,
            EndDate: endDateValue,
            ClientId: activeClientsSelectFilters === null ? null : activeClientsSelectFilters.value === '' ? null : Number(activeClientsSelectFilters.value),
            SuccessManagerId: successManagerSelectFilters === null ? null : successManagerSelectFilters.value === '' ? null : Number(successManagerSelectFilters.value)
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

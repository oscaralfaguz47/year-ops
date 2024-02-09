$(document).ready(function () {
    getListOfResults(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/General/Consultants/GetConsultantsList?model=" + encodeURIComponent(queryString);

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
            console.log(data);
            data.consultantsList.forEach(function (obj) {
                var row = `<tr>
                  <td>
                  <i onclick="displayUpdateModal('modal-update-create-project', ${obj.consultantId})" class='bi bi-pencil-square table-icon edit-table-icon' title="Edit"></i>
                      ${obj.consultantName}
                  </td>
                  <td>${obj.internalEmail}</td>
                  <td>${obj.personalEmail === null ? "" : obj.personalEmail}</td>
                  <td>${obj.country}</td>
                  <td>${obj.userCategory}</td>
                  <td>${obj.consultantPositions === null ? "" : obj.consultantPositions}</td>
                  <td>${obj.consultantProjects === null ? "" : obj.consultantProjects}</td>
                  <td>${obj.isActive ? '<span class="green-label">Active</span>' : '<span class="red-label">Inactive</span>'}</td>
                  <td>${obj.phoneNumber === null ? "" : obj.phoneNumber}</td>
                  <td>${obj.phone2 === null ? "" : obj.phone2}</td>
                  <td>${obj.address === null ? "" : obj.address}</td>
                  <td>${obj.location === null ? "" : obj.location}</td>
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
            });

            if (data.consultantsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
            };
            updatePagination(data.paginationFilters.paginationWithoutFilters.pagination);
        }).finally(() => {
            hideSpinner();
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

        var filtersData = {
            SearchText: searchText,
            IsActive: activeInactiveValue
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
$(document).ready(function () {
    getListOfResults(true, false);
});

async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = recolectDataFromForm(filters, firstTime);
    var queryString = JSON.stringify(formData);
    var url = "/AdminCenter/ConsultantPositions/GetConsultantPositionsList?model=" + encodeURIComponent(queryString);

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
            var tableRows = $(".global-table-container table");
            var noResultsMessage = $(".no-results");
            tableRows.css("display", "block");
            noResultsMessage.empty();
            tbody.empty();
            data.positionsList.forEach(function (obj) {

                var editBtn = `<li onclick="displayUpdateCreatePositionModal('modal-update-create-position', ${obj.consultantPositionId})""><i class="bi bi-pencil-square"></i> Edit</li>`;
                var menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.consultantPositionId}', 'menuIcon-${obj.consultantPositionId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.consultantPositionId}"></i>
                              <div class="menu-options" id="menuOptions-${obj.consultantPositionId}">
                               <ul>
                                 ${editBtn}
                               </ul>
                              </div>`;

                var row = `<tr class="hover-group">
                  <td>
                      ${menuBtn}
                      ${obj.positionName}
                  </td>
                  <td>${obj.isPositionAdministrative ? 'Administrative':'Consultant'}</td>
                  <td>${obj.movementTypeName !== null ? obj.movementTypeName : ''}</td>
                  <td>${obj.companyId !== null ? obj.companyId : ''}</td>
                  <td>${obj.costCenterCode !== null ? obj.costCenterCode : ''}</td>
                  <td>${obj.costCenterName !== null ? obj.costCenterName : ''}</td>
                  <td>${obj.accountingAccountCode !== null ? obj.accountingAccountCode : ''}</td>
                  <td>${obj.accountingAccountName !== null ? obj.accountingAccountName : ''}</td>
              </tr>`;
                tbody.append(row);
            });

            if (data.positionsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
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


//Pagination and Filters
function paginationSubmit(firstTime, filters) {
    getListOfResults(firstTime, filters);
}
function recolectDataFromForm(filters, firstTime) {
    {
        var searchText = $('#search-input').val();

        var filtersData = {
            SearchText: searchText
        };
        var inputFieldToOrder = document.getElementsByName('fieldToOrder')[0];
        var inputDirectionOrder = document.getElementsByName('directionOrder')[0];
        var orderByData = {
            FieldToOrder: inputFieldToOrder.value,
            DirectionOrder: inputDirectionOrder.value
        }
        var paginationData = returnCurrentPaginationValues();
        if (firstTime) {
            paginationData.PageSize = 50;
        }
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
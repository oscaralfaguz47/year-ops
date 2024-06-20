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
            console.log(data);
            var tbody = $(".global-table-container table tbody");
            var tableRows = $(".global-table-container table");
            var noResultsMessage = $(".no-results");
            tableRows.css("display", "block");
            noResultsMessage.empty();
            tbody.empty();

            let previousName = null;
            let nameCount = 0;
            let rows = [];
            let startIndex = 0;
            let groupName = 0;

            data.positionsList.forEach(function (obj, index) {
                var editBtn = `<li onclick="displayUpdateCreatePositionModal('modal-update-create-position', ${obj.consultantPositionId}, ${false})"><i class="bi bi-pencil-square"></i> Edit Position</li>`;
                var cloneBtn = `<li onclick="displayUpdateCreatePositionModal('modal-update-create-position', ${obj.consultantPositionId}, ${true})"><i class="fa-regular fa-clone"></i> Clone Position</li>`;
                var menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.consultantPositionId}', 'menuIcon-${obj.consultantPositionId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.consultantPositionId}"></i>
                              <div class="menu-options" id="menuOptions-${obj.consultantPositionId}">
                               <ul>
                                 ${editBtn} ${cloneBtn}
                               </ul>
                              </div>`;

                if (obj.positionName !== previousName) {
                    if (previousName !== null) {
                        rows[startIndex] = rows[startIndex].replace('rowspan="1"', `rowspan="${nameCount}"`);
                    }
                    startIndex = rows.length;
                    nameCount = 1;
                    groupName++;
                    rows.push(`<tr class="hover-group-${groupName}">
                      <td class="first-cell" rowspan="1">
                          ${menuBtn}
                          ${obj.positionName}
                      </td>
                      <td>${obj.isPositionAdministrative ? 'Administrative' : 'Consultant'}</td>
                      <td>${obj.movementTypeName !== null ? obj.movementTypeName : ''}</td>
                      <td>${obj.companyId !== null ? obj.companyId : ''}</td>
                      <td>${obj.costCenterCode !== null ? obj.costCenterCode : ''}</td>
                      <td>${obj.costCenterName !== null ? obj.costCenterName : ''}</td>
                      <td>${obj.accountingAccountCode !== null ? obj.accountingAccountCode : ''}</td>
                      <td>${obj.accountingAccountName !== null ? obj.accountingAccountName : ''}</td>
                  </tr>`);
                } else {
                    nameCount++;
                    rows.push(`<tr class="hover-group-${groupName}">
                      <td>${obj.isPositionAdministrative ? 'Administrative' : 'Consultant'}</td>
                      <td>${obj.movementTypeName !== null ? obj.movementTypeName : ''}</td>
                      <td>${obj.companyId !== null ? obj.companyId : ''}</td>
                      <td>${obj.costCenterCode !== null ? obj.costCenterCode : ''}</td>
                      <td>${obj.costCenterName !== null ? obj.costCenterName : ''}</td>
                      <td>${obj.accountingAccountCode !== null ? obj.accountingAccountCode : ''}</td>
                      <td>${obj.accountingAccountName !== null ? obj.accountingAccountName : ''}</td>
                  </tr>`);
                }
                previousName = obj.positionName;

                if (index === data.positionsList.length - 1) {
                    rows[startIndex] = rows[startIndex].replace('rowspan="1"', `rowspan="${nameCount}"`);
                }
            });

            tbody.html('');
            rows.forEach(row => {
                tbody.append(row);
            });

            // Handle hover to change combined cell background color
            $('[class^="hover-group"]').hover(
                function () { // Mouse-in function
                    var groupClass = $(this).attr('class').match(/hover-group-\d+/)[0];
                    $('.' + groupClass + ' .first-cell').css('background-color', 'rgb(155, 168, 184, 0.2)');
                },
                function () { // Mouse exit function
                    var groupClass = $(this).attr('class').match(/hover-group-\d+/)[0];
                    $('.' + groupClass + ' .first-cell').css('background-color', '');
                }
            );

            if (data.positionsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
            }
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
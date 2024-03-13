$(document).ready(function () {
    getListOfResults(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/General/ConsultantReimbursedBenefits/GetConsultantReimbursedBenefitsList?model=" + encodeURIComponent(queryString);

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
            noResultsMessage.empty();
            tableRows.css("display", "block");
            tbody.empty();
            data.reimbursedBenefitsList.forEach(function (obj) {

                var row = `<tr>
                  <td>
                        <i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.reimbursedBenefitId}', 'menuIcon-${obj.reimbursedBenefitId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.reimbursedBenefitId}"></i>
                          <div class="menu-options" id="menuOptions-${obj.reimbursedBenefitId}">
                           <ul>
                             <li onclick="displayUpdateCreateConsultantModal('modal-update-create-consultant', ${obj.reimbursedBenefitId})""><i class="bi bi-pencil-square"></i> Edit Reimbursement</li>
                           </ul>
                         </div>
                      ${obj.consultantName}
                  </td>
                  <td>${obj.benefitName}</td>
                  <td>${obj.amountReimbursed}</td>
                  <td>${obj.detail === null ? "" : obj.detail}</td>
                  <td>${obj.dateToBeReimbursed}</td>
                  <td class="shared-table-td">${obj.benefitPaid ? '<span class="green-label">Paid</span>' : '<span class="red-label">Unpaid</span>'}</td>
                  <td>${obj.userCreatedBy}</td>
                  <td>${obj.creationDate}</td>
                  <td>${obj.userLastUpdatedBy === null ? "Unedited" : obj.userLastUpdatedBy}</td>
                  <td>${obj.lastUpdateDate === null ? "Unedited" : obj.lastUpdateDate}</td>
              </tr>`;
                tbody.append(row);
            });

            if (data.reimbursedBenefitsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
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

        var filtersData = {
            SearchText: searchText
        };
        console.log(filtersData);
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

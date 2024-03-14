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
                var reimbursedDate = new Date(obj.dateToBeReimbursed);
                var reimbursedformattedDate = ('0' + (reimbursedDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + reimbursedDate.getDate()).slice(-2) + '/' +
                    reimbursedDate.getFullYear();

                var creationDate = new Date(obj.creationDate);
                var creationformattedDate = ('0' + (creationDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + creationDate.getDate()).slice(-2) + '/' +
                    creationDate.getFullYear();

                var updateDate = new Date(obj.lastUpdateDate);
                var updateformattedDate = ('0' + (updateDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + updateDate.getDate()).slice(-2) + '/' +
                    updateDate.getFullYear();

                var deleteBtn = ``;
                var editBtn = ``;
                var menuBtn = `<i title="You are not able to edit or delete, it is already paid" style="cursor:pointer; color: var(--clr-blueLight);" class="bi bi-exclamation-circle"></i> `;
                if (!obj.benefitPaid) {
                    deleteBtn = `<li onclick="deleteReimbursement(${obj.reimbursedBenefitId})""><i class="bi bi-trash3 red-label"></i> Delete</li>`;
                    editBtn = `<li onclick="displayUpdateCreateConsultantModal('modal-update-create-consultant', ${obj.reimbursedBenefitId})""><i class="bi bi-pencil-square"></i> Edit Reimbursement</li>`;
                    menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.reimbursedBenefitId}', 'menuIcon-${obj.reimbursedBenefitId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.reimbursedBenefitId}"></i>
                              <div class="menu-options" id="menuOptions-${obj.reimbursedBenefitId}">
                               <ul>
                                 ${editBtn}
                                 ${deleteBtn}
                               </ul>
                              </div>`;
                }

                var row = `<tr>
                  <td>
                      ${menuBtn}
                      ${obj.consultantName}
                  </td>
                  <td>${obj.benefitName}</td>
                  <td>$${obj.amountReimbursed}</td>
                  <td>${reimbursedformattedDate}</td>
                  <td>${obj.benefitPaid ? '<div><span class="green-label cel-status"><i class="bi bi-emoji-smile"></i> Paid</span>' : '<span class="red-label cel-status"><i class="bi bi-emoji-frown"></i> Unpaid</span>'}</div></td>
                  <td>${obj.detail === null ? "" : obj.detail}</td>
                  <td>${obj.userCreatedBy}</td>
                  <td>${creationformattedDate}</td>
                  <td>${obj.userLastUpdatedBy === null ? "Not updated" : obj.userLastUpdatedBy}</td>
                  <td>${obj.lastUpdateDate === null ? "Not updated" : updateformattedDate}</td>
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

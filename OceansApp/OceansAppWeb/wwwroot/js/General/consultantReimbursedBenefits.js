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

                var rejectBtn = ``;
                var editBtn = ``;
                var menuBtn = `<i title="You are not able to edit it, it is status ${obj.transactionStatusName}" style="cursor:pointer; color: var(--clr-blueLight);" class="bi bi-exclamation-circle"></i> `;
                if (obj.transactionStatusName !== "Rejected" && obj.transactionStatusName === "Approved") {
                    rejectBtn = `<li onclick="rejectBenefitReimbursement(${obj.reimbursedBenefitId}, '${obj.consultantName}')""><i class="red-label bi bi-x-lg"></i> Reject</li>`;
                    editBtn = `<li onclick="displayUpdateCreateReimbursementModal('modal-update-create-reimbursement', ${obj.reimbursedBenefitId})""><i class="bi bi-pencil-square"></i> Edit</li>`;
                    menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.reimbursedBenefitId}', 'menuIcon-${obj.reimbursedBenefitId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.reimbursedBenefitId}"></i>
                              <div class="menu-options" id="menuOptions-${obj.reimbursedBenefitId}">
                               <ul>
                                 ${editBtn}
                                 ${rejectBtn}
                               </ul>
                              </div>`;
                }

                var row = `<tr class="hover-group">
                  <td>
                      ${menuBtn}
                      ${obj.consultantName}
                  </td>
                  <td>${obj.benefitName}</td>
                  <td>${obj.benefitCategoryName}</td>
                  <td>${obj.detail === null ? "" : obj.detail}</td>
                  <td>$${obj.amountReimbursed}</td>
                  <td>${reimbursedformattedDate}</td>
                  <td>${getStatusLabel(obj.transactionStatusName)}</td>
                  <td>${obj.userCreatedBy}</td>
                  <td>${formatUtcToLocalMmDdYyyyTime(obj.creationDate)}</td>
                  <td>${obj.userLastUpdatedBy === null ? "Not updated" : obj.userLastUpdatedBy}</td>
                  <td>${obj.lastUpdateDate === null ? "Not updated" : formatUtcToLocalMmDdYyyyTime(obj.lastUpdateDate) }</td>
              </tr>`;
                tbody.append(row);
            });

            if (data.reimbursedBenefitsList.length === 0) {
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

// DELETE BENEFIT REIMBURSEMENT
async function rejectBenefitReimbursement(benefitReimbursementId, consultantName) {
    Swal.fire({
        title: "Reject Reimbursement",
        text: 'Are you sure you want to reject the Reimbursement for ' + consultantName + '?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, Delete!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            displaySpinner();
            var token = $('[name="__RequestVerificationToken"]').val();
            var formData = new FormData();
            formData.append('benefitReimbursementId', benefitReimbursementId);
            fetch("/General/ConsultantReimbursedBenefits/RejectBenefitReimbursement"
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
                    } else {
                        displayToasterError(data.error);
                        console.error('There has been a problem with the fetch operation:', data.detail);
                    }
                    getListOfResults(false, false);
                })
                .catch(error => {
                    validateSessionExpiration(error.message);
                })
                .finally(() => {
                    hideSpinner();
                });
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

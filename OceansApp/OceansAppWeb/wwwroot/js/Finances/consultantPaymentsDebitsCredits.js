$(document).ready(function () {
    getListOfResults(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/Finances/ConsultantPaymentsDebitsCredits/GetConsultantPaymentsDebitsCreditsList?model=" + encodeURIComponent(queryString);

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
            data.paymentsDebitsCreditsList.forEach(function (obj) {
                var actionDate = new Date(obj.actionDateWithinFortnight);
                var actionformattedDate = ('0' + (actionDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + actionDate.getDate()).slice(-2) + '/' +
                    actionDate.getFullYear();

                var creationDate = new Date(obj.creationDate);
                var creationformattedDate = ('0' + (creationDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + creationDate.getDate()).slice(-2) + '/' +
                    creationDate.getFullYear();

                var updateDate = new Date(obj.lastUpdateDate);
                var updateformattedDate = ('0' + (updateDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + updateDate.getDate()).slice(-2) + '/' +
                    updateDate.getFullYear();

                var statusLabel = ``;
                if (obj.transactionStatusName === 'Rejected') {
                    statusLabel = `<span class="cel-status red-label"><i class="bi bi-x"></i>${obj.transactionStatusName}</span>`;
                } else if (obj.transactionStatusName === 'Approved') {
                    statusLabel = `<span class="cel-status gray-lable"><i class="bi bi-check"></i>${obj.transactionStatusName}</span>`;
                } else if (obj.transactionStatusName === 'Approved and sent') {
                    statusLabel = `<span class="cel-status blueLight-lable"><i class="bi bi-check-all"></i>${obj.transactionStatusName}</span>`;
                } else if (obj.transactionStatusName === 'Paid') {
                    statusLabel = `<span class="cel-status green-label"><i class="bi bi-credit-card-2-back"></i>${obj.transactionStatusName}</span>`;
                }

                var rejectBtn = ``;
                var editBtn = ``;
                var menuBtn = `<i title="You are not able to edit, it already has status: ${obj.transactionStatusName}" style="cursor:pointer; color: var(--clr-blueLight);" class="bi bi-exclamation-circle"></i> `;
                if (obj.transactionStatusName !== "Rejected" && (obj.transactionStatusName === "Approved" || obj.transactionStatusName === "Waiting to be approved")) {
                    rejectBtn = `<li onclick="rejectDebitCredit(${obj.consultantPaymentDebitsCreditsId}, '${obj.consultantName}')""><i class="bi bi-x"></i> Reject</li>`;
                    editBtn = `<li onclick="displayUpdateCreateDebitCreditModal('modal-update-create-debit-credit', ${obj.consultantPaymentDebitsCreditsId})""><i class="bi bi-pencil-square"></i> Edit</li>`;
                    menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.consultantPaymentDebitsCreditsId}', 'menuIcon-${obj.consultantPaymentDebitsCreditsId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.consultantPaymentDebitsCreditsId}"></i>
                              <div class="menu-options" id="menuOptions-${obj.consultantPaymentDebitsCreditsId}">
                               <ul>
                                 ${editBtn}
                                 ${rejectBtn}
                               </ul>
                              </div>`;
                }

                var row = `<tr>
                  <td>
                      ${menuBtn}
                      ${obj.consultantName}
                  </td>
                  <td>${obj.detail}</td>
                  <td>${obj.costCenterName}</td>
                  <td>${obj.accountingAccountName}</td>
                  <td>${obj.quantity}</td>
                  <td>$${obj.amount}</td>
                  <td>$${(obj.quantity * obj.amount)}</td>
                  <td>${actionformattedDate}</td>
                  <td>${obj.transactionTypeName}</td>
                  <td>${statusLabel}</td>
                  <td>${obj.userCreatedBy}</td>
                  <td>${creationformattedDate}</td>
                  <td>${obj.lastUpdatedBy === null ? "Not updated" : obj.lastUpdatedBy}</td>
                  <td>${obj.lastUpdateDate === null ? "Not updated" : updateformattedDate}</td>
              </tr>`;
                tbody.append(row);
            });

            if (data.paymentsDebitsCreditsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
            };
            updatePagination(data.paginationFilters.paginationWithoutFilters.pagination);
        }).finally(() => {
            hideSpinner();
        });
}

// REJECT DEBIT CREDIT
async function rejectDebitCredit(paymentDebitCreditId, consultantName, transactionType) {
    Swal.fire({
        title: "Reject " + transactionType,
        text: 'Are you sure you want to reject de ' + transactionType + ' for ' + consultantName + '?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, Reject!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            displaySpinner();
            var token = $('[name="__RequestVerificationToken"]').val();
            var formData = new FormData();
            formData.append('paymentDebitCreditId', paymentDebitCreditId);
            fetch("/Finances/ConsultantPaymentsDebitsCredits/RejectDebitCredit"
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

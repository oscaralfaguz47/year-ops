$(document).ready(function () {
    getListOfResults(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/Finances/PaymentSheets/GetConsultantsToPayList?model=" + encodeURIComponent(queryString);

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
            console.log(data);
            data.consultantsToPayList.forEach(function (obj) {
                var submissionDate = new Date(obj.submissionDate);
                var submissionformattedDate = ('0' + (submissionDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + submissionDate.getDate()).slice(-2) + '/' +
                    submissionDate.getFullYear();

                var lastSubmissionDate = new Date(obj.lastSubmissionDate);
                var lastSubmissionformattedDate = ('0' + (lastSubmissionDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + lastSubmissionDate.getDate()).slice(-2) + '/' +
                    lastSubmissionDate.getFullYear();

                var statusLabel = ``;
                if (obj.transactionStatusName === 'Rejected') {
                    statusLabel = `<span class="cel-status red-label"><i class="bi bi-x"></i>${obj.transactionStatusName}</span>`;
                } else if (obj.transactionStatusName === 'Approved') {
                    statusLabel = `<span class="cel-status"><i class="bi bi-check"></i>${obj.transactionStatusName}</span>`;
                } else if (obj.transactionStatusName === 'Sent to be paid') {
                    statusLabel = `<span class="cel-status blueLight-lable"><i class="bi bi-send-check"></i>${obj.transactionStatusName}</span>`;
                } else if (obj.transactionStatusName === 'Paid') {
                    statusLabel = `<span class="cel-status paid-label"><i class="bi bi-credit-card-2-back"></i>${obj.transactionStatusName}</span>`;
                } else if (obj.transactionStatusName === 'Waiting to be approved') {
                    statusLabel = `<span class="cel-status gray-lable"><i class="bi bi-hourglass-split"></i>${obj.transactionStatusName}</span>`;
                } else if (obj.transactionStatusName === 'Accounted - Accounts Payable') {
                    statusLabel = `<span class="cel-status orange-label"><i class="bi bi-journal-bookmark-fill"></i>${obj.transactionStatusName}</span>`;
                } else if (obj.transactionStatusName === 'Done') {
                    statusLabel = `<span class="cel-status green-label"><i class="bi bi-check-circle-fill"></i>${obj.transactionStatusName}</span>`;
                }

              //  var row = `<tr>
              //    <td>
              //        ${menuBtn}
              //        ${obj.consultantName}
              //    </td>
              //    <td>${obj.durationMinutes.toFixed(2)} minutes</td>
              //    <td>${((1 / 60) * obj.durationMinutes).toFixed(2)} hours</td>
              //    <td>${actionformattedDate}</td>
              //    <td>${statusLabel}</td>
              //    <td>${obj.createdBy}</td>
              //    <td>${creationformattedDate}</td>
              //    <td>${obj.lastUpdatedBy === null ? "Not updated" : obj.lastUpdatedBy}</td>
              //    <td>${obj.lastUpdateDate === null ? "Not updated" : updateformattedDate}</td>
              //</tr>`;
              //  tbody.append(row);
            });

            if (data.consultantsToPayList.length === 0) {
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
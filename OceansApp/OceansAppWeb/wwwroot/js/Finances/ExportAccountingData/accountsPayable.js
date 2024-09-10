async function getJournalAccountsPayableList(firstTime, filters) {
    displaySpinner();
    var formData = recolectDataFromForm(filters, firstTime);
    var queryString = JSON.stringify(formData);
    var url = "/Finances/ExportAccountingData/GetJournalAccountsPayableList?model=" + encodeURIComponent(queryString);

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
            var tbody = $(".accounts-payable-table-cont table tbody");
            var tableRows = $(".accounts-payable-table-cont table");
            var noResultsMessage = $("a-p-no-results");
            noResultsMessage.empty();
            tbody.empty();
            data.journalAccountsPayableList.forEach(function (obj, index) {
                var accountingDate = new Date(obj.accountingDate);
                var accountingDateformattedDate = ('0' + (accountingDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + accountingDate.getDate()).slice(-2) + '/' +
                    accountingDate.getFullYear();

                var startPeriodDate = new Date(obj.startDatePeriod);
                var startPeriodDateformattedDate = ('0' + (startPeriodDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + startPeriodDate.getDate()).slice(-2) + '/' +
                    startPeriodDate.getFullYear();

                var endPeriodDate = new Date(obj.endDatePeriod);
                var endPeriodDateformattedDate = ('0' + (endPeriodDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + endPeriodDate.getDate()).slice(-2) + '/' +
                    endPeriodDate.getFullYear();


                var row = `<tr class="hover-group">
                  <td>${obj.seatNumber}</td>
                  <td>${obj.companyName}</td>
                  <td>${accountingDateformattedDate}</td>
                  <td>${startPeriodDateformattedDate}</td>
                  <td>${endPeriodDateformattedDate}</td>
                  <td>${obj.transactionStatusName}</td>
                  <td></td>
              </tr>`;
                tbody.append(row);
            });

            if (data.journalAccountsPayableList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
            } else {
                tableRows.css("display", "block");
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
    getJournalAccountsPayableList(firstTime, filters);
}
function recolectDataFromForm(filters) {
    {
        var filtersData = {
            CompanyId: null,
            TransactionStatusId: null
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

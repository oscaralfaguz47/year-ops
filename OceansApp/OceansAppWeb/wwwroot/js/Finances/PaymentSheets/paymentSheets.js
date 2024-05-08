let dateToInput = document.getElementById('dateToInput');
let dateFromInput = document.getElementById('dateFromInput');
let paymentPeriodSelect = document.getElementById('paymentPeriod');
$(document).ready(function () {
    let currentDateNoChange = new Date();
    paymentPeriod = 1;
    calculatePeriod(currentDateNoChange, paymentPeriod);
});

function changePaymentPeriod() {
    let selectedDate = new Date(dateToInput.value);
    paymentPeriod = Number(paymentPeriodSelect.value);
    calculatePeriod(selectedDate, paymentPeriod);
}

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = recolectDataFromForm(filters, firstTime);
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
            tbody.empty();
            data.consultantsToPayList.forEach(function (obj) {
                var submissionDate = new Date(obj.submissionDate);
                var submissionformattedDate = ('0' + (submissionDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + submissionDate.getDate()).slice(-2) + '/' +
                    submissionDate.getFullYear();

                var lastSubmissionDate = new Date(obj.lastSubmissionDate);
                var lastSubmissionformattedDate = ('0' + (lastSubmissionDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + lastSubmissionDate.getDate()).slice(-2) + '/' +
                    lastSubmissionDate.getFullYear();

                var row = `<tr>
                  <td>
                      ${obj.consultantName}
                  </td>
                  <td>${obj.projectName}</td>
                  <td>${obj.lastSubmissionDate === null ? "No re-submitted" : lastSubmissionformattedDate}</td>
                  <td>${obj.submissionDate === null ? "Not submitted yet" : submissionformattedDate}</td>
                  <td>${getStatusLabel(obj.transactionStatusName)}</td>
                  <td></td>
              </tr>`;
                tbody.append(row);
            });

            if (data.consultantsToPayList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
            } else {
                tableRows.css("display", "block");
            };
            updatePagination(data.paginationFilters.paginationWithoutFilters.pagination);
        }).finally(() => {
            hideSpinner();
        });
}

//Navitate between dates
function navitateBetweenDates(startDate, endDate, button) {
    getListOfResults(false, true).then(() => {
        if (button) button.disabled = false;
    });
}

//Pagination and Filters
function paginationSubmit(firstTime, filters) {
    getListOfResults(firstTime, filters);
}
function recolectDataFromForm(filters, firstTime) {
    {
        var searchText = $('#search-input').val();
        let startDateData = new Date(dateFromInput.value).toISOString();
        let endDateData = new Date(dateToInput.value).toISOString();

        var filtersData = {
            SearchText: searchText,
            StartDate: startDateData,
            EndDate: endDateData,
            PaymentPeriod: Number(paymentPeriodSelect.value)
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
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

            let previousName = null;
            let nameCount = 0;
            let rows = [];
            let startIndex = 0;
            let groupName = 0;
            console.log(data);
            data.consultantsToPayList.forEach(function (obj, index) {
                let actionsBtns = '<div class="no-actions-div">No actions needed</div>';
                var submissionformattedDate = "Not submitted yet";
                let makePaymentBtn = '';
                let setAsAccountsPayableBtn = '';
                let menuBtn = '';

                if (obj.submissionDate !== null) {
                    submissionformattedDate = formatUtcToLocalMmDdYyyyTime(obj.submissionDate);
                }

                var lastSubmissionformattedDate = 'No re-submitted';

                if (obj.lastSubmissionDate !== null) {
                    lastSubmissionformattedDate = formatUtcToLocalMmDdYyyyTime(obj.lastSubmissionDate);
                }
                if (obj.transactionStatusName === 'Approved' && obj.submissionId === null) {
                    lastSubmissionformattedDate = 'No Submission is needed';
                    submissionformattedDate = 'No Submission is needed';
                }

                if (obj.transactionStatusName === 'Waiting to be approved') {
                    actionsBtns = `<div class="action-btns-box"><button onclick="displayReviewForApprovalModal('modal-review-for-approval', ${obj.submissionId})" class="review-btn">Review for approval</button></div>`;
                }

                if (obj.numApprovedSubmissions === obj.numProjectsIsActive) {
                    makePaymentBtn = `<li>Make Payment</li>`;
                    sendPaymentDetails = `<li>Send payment details</li>`;
                    menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.consultantId}', 'menuIcon-${obj.consultantId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.consultantId}"></i>
                              <div class="menu-options" id="menuOptions-${obj.consultantId}">
                               <ul>
                                 ${makePaymentBtn + setAsAccountsPayableBtn}
                               </ul>
                              </div>`;
                }

                if (obj.consultantName !== previousName) {
                    if (previousName !== null) {
                        rows[startIndex] = rows[startIndex].replace('rowspan="1"', `rowspan="${nameCount}"`);
                    }
                    startIndex = rows.length;
                    nameCount = 1;
                    groupName++;
                    rows.push(`<tr class="hover-group-${groupName}">
                <td class="first-cell" rowspan="1">${menuBtn}${obj.consultantName}</td>
                <td>${obj.projectName}</td>
                <td>${lastSubmissionformattedDate}</td>
                <td>${submissionformattedDate}</td>
                <td>${getStatusLabel(obj.transactionStatusName)}</td>
                <td>${actionsBtns}</td>
            </tr>`);
                } else {
                    nameCount++;
                    rows.push(`<tr class="hover-group-${groupName}">
                <td>${obj.projectName}</td>
                <td>${lastSubmissionformattedDate}</td>
                <td>${submissionformattedDate}</td>
                <td>${getStatusLabel(obj.transactionStatusName)}</td>
                <td>${actionsBtns}</td>
            </tr>`);
                }
                previousName = obj.consultantName;

                if (index === data.consultantsToPayList.length - 1) {
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



            if (data.consultantsToPayList.length === 0) {
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
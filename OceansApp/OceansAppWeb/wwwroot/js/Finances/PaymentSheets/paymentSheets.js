let dateToInput = document.getElementById('dateToInput');
let dateFromInput = document.getElementById('dateFromInput');
let paymentPeriodSelect = document.getElementById('paymentPeriod');
let statusSelectFilters = null;
let paymentStatusSelectFilters = null;
let projectSelectFilters = null;
let rightSidebarFiltersIsDiplayed = false;

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
            var tbody = $(".cl-payment-sheets-table table tbody");
            var tableRows = $(".cl-payment-sheets-table table");
            var noResultsMessage = $(".cl-payment-sheets-table .no-results");
            noResultsMessage.empty();
            tbody.empty();

            let previousName = null;
            let nameCount = 0;
            let rows = [];
            let startIndex = 0;
            let groupName = 0;
            data.consultantsToPayList.forEach(function (obj, index) {
                let actionsBtns = '<div class="no-actions-div">No actions needed</div>';
                var submissionformattedDate = "Not submitted yet";
                let reviewForPaymentBtn = '';
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
                    actionsBtns = `<div class="action-btns-box status-actions"><button onclick="displayReviewForApprovalModal('modal-review-for-approval', ${obj.submissionId})" class="review-btn">Review for approval</button></div>`;
                }
                if (obj.transactionStatusName === 'Approved' && obj.submissionId !== null) {
                    actionsBtns = `<div class="action-btns-box status-actions"><button onclick="displayApproveRejectConfirmation('Reject', 'PaymentSheets', ${obj.submissionId})" class="reject-approvement-btn"><img src="/icons/Shared/circle-x-mark.svg"> Reject Approvement</button></div>`;
                }
                if ((obj.transactionStatusName === 'Rejected' && obj.paymentStatus !== 'Paid') || obj.transactionStatusName === 'Pending' ||
                    (obj.transactionStatusName === 'Approved' && obj.submissionId === null)) {
                    actionsBtns = `<div class="action-btns-box status-actions"><button onclick="removeProjectInPeriod('${obj.projectName}', ${obj.projectId}, ${obj.consultantId})" class="remove-for-period-btn"><img src="/icons/Shared/trash.svg">Remove for this period</button></div>`;
                }
                if (obj.numApprovedSubmissions === obj.numProjectsIsActive) {
                    reviewForPaymentBtn = `<li onclick="displayReviewForPaymentModal('modal-review-for-payment', ${obj.consultantId})">Review for payment</li>`;
                    sendPaymentDetails = `<li>Send payment details</li>`;
                    menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.consultantId}', 'menuIcon-${obj.consultantId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.consultantId}"></i>
                  <div class="menu-options" id="menuOptions-${obj.consultantId}">
                   <ul>
                     ${reviewForPaymentBtn + setAsAccountsPayableBtn}
                   </ul>
                  </div>`;
                }

                if (obj.consultantName !== previousName) {
                    if (previousName !== null) {
                        rows[startIndex] = rows[startIndex].replace('rowspan="1"', `rowspan="${nameCount}"`);
                        // Aquí agregamos el rowspan a la nueva columna de paymentStatus
                        rows[startIndex] = rows[startIndex].replace('rowspan="1"', `rowspan="${nameCount}"`);
                    }
                    startIndex = rows.length;
                    nameCount = 1;
                    groupName++;
                    rows.push(`<tr class="hover-group-${groupName}">
    <td class="first-cell" rowspan="1">${menuBtn}${obj.consultantName}</td>
    <td class="first-cell" rowspan="1">${getStatusLabel(obj.paymentStatus)}</td> <!-- Aplicar misma clase -->
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
                    // Agregar rowspan final para paymentStatus
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

//Reject Approvement
let submissionIdToReject = null;
async function rejectApprovement() {
    let commentInputValue = null;
    let confirmModal = 'modal-approve-reject-submission';
    let reviewModal = 'modal-review-for-approval';

    let commentInput = document.getElementById('comment-input');
    if (commentInput.value === '' || commentInput.value === null) {
        document.getElementById('val-mess-message').style.display = 'block';
        return;
    }
    commentInputValue = commentInput.value;

    displaySpinner();
    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        SubmissionId: Number(submissionIdToReject),
        Body: commentInputValue,
        TransactionStatus: 'Rejected'
    };

    try {
        const response = await fetch('/Finances/PaymentSheets/RejectApproveSubmission', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: token
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
                        return acc.concat(current);
                    }, []);
                    displayToasterWarningArray(allErrors);
                    break;
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }

        const dataFromApi = await response.json();
        hideModal(confirmModal);
        hideModal(reviewModal);
        getListOfResults(false, true);
        displayToasterSuccess(dataFromApi.message);
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Something went wrong, more details: ' + err);
        hideSpinner();
        return null;
    }
}

//Remove Project in Period 
async function removeProjectInPeriod(projectName, projectId, consultantId) {
    const confirmation = await Swal.fire({
        title: "Remove project in Period",
        text: `You are going to remove the ${projectName} from the consultant for this period. Do you want to continue?`,
        icon: 'warning',
        showCancelButton: true,
        cancelButtonText: 'Cancel',
        cancelButtonColor: '#9ba8b8',
        confirmButtonColor: '#eeb30f',
        confirmButtonText: 'Yes, Remove it!'
    });

    if (!confirmation.isConfirmed) {
        return;
    }
    displaySpinner();
    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        ProjectId: projectId,
        ConsultantId: consultantId,
        StartPeriodDate: dateFromInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2'),
        EndPeriodDate: dateToInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2')
    };
    try {
        const response = await fetch('/Finances/PaymentSheets/RemoveProjectConsultantInPeriod', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: token
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
                        return acc.concat(current);
                    }, []);
                    displayToasterWarningArray(allErrors);
                    break;
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }

        const dataFromApi = await response.json();
        getListOfResults(false, true);
        displayToasterSuccess(dataFromApi.message);
        hideSpinner();
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Something went wrong, more details: ' + err);
        hideSpinner();
        return null;
    }
}
//Send Submission Reminders
async function sendSubmissionReminders() {
    const confirmation = await Swal.fire({
        title: "Send Submission Reminders",
        text: `Are you sure you want to remind every consultant that is pending submission?`,
        icon: 'warning',
        showCancelButton: true,
        cancelButtonText: 'Cancel',
        cancelButtonColor: '#9ba8b8',
        confirmButtonColor: '#eeb30f',
        confirmButtonText: 'Yes, Send Reminders!'
    });

    if (!confirmation.isConfirmed) {
        return;
    }
    displaySpinner();
    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        StartDate: dateFromInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2'),
        EndDate: dateToInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2'),
        PaymentPeriod: Number(paymentPeriodSelect.value)
    };
    
    try {
        const response = await fetch('/Finances/PaymentSheets/SendSubmissionReminders', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: token
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
                        return acc.concat(current);
                    }, []);
                    displayToasterWarningArray(allErrors);
                    break;
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }

        const dataFromApi = await response.json();
        displayToasterSuccess(dataFromApi.message);
        hideSpinner();
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Something went wrong, more details: ' + err);
        hideSpinner();
        return null;
    }
}
//Navitate between dates
function navitateBetweenDates(startDate, endDate, buttons) {
    getListOfResults(false, true).then(() => {
        if (buttons) {
            buttons.forEach(btn => {
                if (btn) btn.disabled = false;
            });
        }
    });
}

//More filters
async function displayMoreFiltersPaymentSheet() {
    if (!rightSidebarFiltersIsDiplayed) {
        displaySpinner();
        let rightSidebarContainer = document.getElementById('right-sidebar-container');
        rightSidebarContainer.innerHTML = `
        <div class="header-btns-container">
         <button class="clear-btn" onclick="clearFilters('filters-form')"><img class="filter-icon" src="/icons/Shared/clear.svg">Clear filters </button>
        </div>
        <div class="scroll-container">
          <form id="filters-form">
            <div class="select-container">
                <label>Payment Status</label>
                <select onchange="getListOfResults(false, true)" id="paymentStatusSelectFilters" class="form-select">
                    <option value="">All statuses</option>
                    <option value="Paid">Paid</option>
                    <option value="Pending">Pending</option>
                    <option value="Sent to be paid">Sent to be paid</option>
                    <option value="Updated - Pending Review">Updated - Pending Review</option>
                </select>
            </div>
             <div class="select-container">
                <label>Submission Status</label>
                <select onchange="getListOfResults(false, true)" id="statusSelectFilters" class="form-select">
                    <option value="">All statuses</option>
                    <option value="Approved">Approved</option>
                    <option value="Pending">Pending</option>
                    <option value="Rejected">Rejected</option>
                    <option value="Waiting to be approved">Waiting to be approved</option>
                </select>
            </div>
            <div class="select-container">
                <label>Project</label>
                <select onchange="getListOfResults(false, true)" id="projectSelectFilters" class="form-select">
                </select>
            </div>
          </form>
        <div>`;

        statusSelectFilters = document.getElementById('statusSelectFilters');
        paymentStatusSelectFilters = document.getElementById('paymentStatusSelectFilters');
        projectSelectFilters = document.getElementById('projectSelectFilters');

        try {
            const data = await getActiveProjectsList();
            populateSelect('projectSelectFilters', data.projects, 'All projects', null);

            openRightSidebar();
            rightSidebarFiltersIsDiplayed = true;
        } catch (error) {
            console.error(error);
            throw error;
        } finally {
            hideSpinner();
        }
    }
    openRightSidebar();
}
function clearFilters(formId) {
    resetFormElements(formId);
    getListOfResults(false, true);
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
            PaymentPeriod: Number(paymentPeriodSelect.value),
            TransactionStatusName: statusSelectFilters === null ? null : statusSelectFilters.value === '' ? null : statusSelectFilters.value,
            AccountsPayableStatusName: paymentStatusSelectFilters === null ? null : paymentStatusSelectFilters.value === '' ? null : paymentStatusSelectFilters.value,
            ProjectId: projectSelectFilters === null ? null : projectSelectFilters.value === '' ? null : Number(projectSelectFilters.value)
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
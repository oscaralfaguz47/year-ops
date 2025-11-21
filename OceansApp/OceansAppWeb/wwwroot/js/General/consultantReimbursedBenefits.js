let rightSidebarFiltersIsDiplayed = false;
let benefitsArray = [];
let transactionStatusesArray = [];
let transactionStatusSelectFilters = null;
let benefitSelectFilters = null;

const mainPaginationContainerSelector = '.principal-header-container .pagination-container';

$(document).ready(function () {
    setGeneralItemActive();
    getListOfResults(true, false);
});

// ===============================
// Get list (main table)
// ===============================
async function getListOfResults(firstTime, fromFilters) {
    displaySpinner();

    const formData = firstTime ? {} : recolectDataFromFormReimbursements(fromFilters);
    const queryString = JSON.stringify(formData);
    const url = "/General/ConsultantReimbursedBenefits/GetConsultantReimbursedBenefitsList?model=" + encodeURIComponent(queryString);

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
            const tbody = $(".global-table-container table tbody");
            const tableRows = $(".global-table-container table");
            const noResultsMessage = $(".no-results");

            noResultsMessage.empty();
            tableRows.css("display", "block");
            tbody.empty();

            data.reimbursedBenefitsList.forEach(function (obj) {
                const reimbursedDate = new Date(obj.dateToBeReimbursed);
                const reimbursedformattedDate = ('0' + (reimbursedDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + reimbursedDate.getDate()).slice(-2) + '/' +
                    reimbursedDate.getFullYear();

                let rejectBtn = ``;
                let editBtn = ``;
                let menuBtn = `<i title="You are not able to edit it, it is status ${obj.transactionStatusName}" style="cursor:pointer; color: var(--clr-blueLight);" class="bi bi-exclamation-circle"></i> `;

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

                const row = `<tr class="hover-group">
                  <td>
                      ${menuBtn}
                      ${obj.consultantName}
                  </td>
                  <td>${obj.benefitName}</td>
                  <td>${obj.benefitCategoryName}</td>
                  <td>${obj.detail === null ? "" : obj.detail}</td>
                  <td>$${obj.amountReimbursed.toLocaleString('en-US')}</td>
                  <td>${reimbursedformattedDate}</td>
                  <td>${getStatusLabel(obj.transactionStatusName)}</td>
                  <td>${obj.userCreatedBy}</td>
                  <td>${formatUtcToLocalMmDdYyyyTime(obj.creationDate)}</td>
                  <td>${obj.userLastUpdatedBy === null ? "Not updated" : obj.userLastUpdatedBy}</td>
                  <td>${obj.lastUpdateDate === null ? "Not updated" : formatUtcToLocalMmDdYyyyTime(obj.lastUpdateDate)}</td>
              </tr>`;
                tbody.append(row);
            });

            if (data.reimbursedBenefitsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
            }

            updatePaginationReimbursements(data.paginationFilters.paginationWithoutFilters.pagination);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            hideSpinner();
        });
}

// ===============================
// Pagination + filters (main)
// ===============================
function recolectDataFromFormReimbursements(fromFilters) {
    const searchText = $('#search-input').val();

    const filtersData = {
        SearchText: searchText,
        TransactionStatusId: transactionStatusSelectFilters === null ? null : transactionStatusSelectFilters.value === '' ? null : Number(transactionStatusSelectFilters.value),
        BenefitId: benefitSelectFilters === null ? null : benefitSelectFilters.value === '' ? null : Number(benefitSelectFilters.value)
    };

    const paginationContainer = document.querySelector(mainPaginationContainerSelector);

    const inputFieldToOrder = paginationContainer.querySelector('input[name="fieldToOrder"]');
    const inputDirectionOrder = paginationContainer.querySelector('input[name="directionOrder"]');

    const orderByData = {
        FieldToOrder: inputFieldToOrder.value,
        DirectionOrder: inputDirectionOrder.value
    };

    const paginationData = returnCurrentPaginationValues(paginationContainer);

    const paginationWithoutFilters = {
        Pagination: paginationData,
        RequestFromFilters: fromFilters,
        OrderBy: orderByData
    };

    return {
        Filters: filtersData,
        PaginationWithoutFilters: paginationWithoutFilters
    };
}

function updatePaginationReimbursements(paginationData) {
    updatePaginationValues(
        paginationData,
        document.querySelector(mainPaginationContainerSelector)
    );
}

// Search input (main)
function enterInSearchReimbursements(event) {
    const container = document.querySelector(mainPaginationContainerSelector);
    paginationSubmit(true, false, container);
}


// ===============================
// MORE FILTERS (main)
// ===============================
async function displayMoreFiltersReimbursements() {
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
             <label>Status</label>
             <select onchange="onChangeFiltersReimbursements()" id="TransactionStatusIdFilters" class="form-select">
             </select>
           </div>
           <div class="select-container">
             <label>Benefit</label>
             <select onchange="onChangeFiltersReimbursements()" id="BenefitIdFilters" class="form-select">
             </select>
           </div>
          </form>
        <div>`;

        transactionStatusSelectFilters = document.getElementById('TransactionStatusIdFilters');
        if (transactionStatusesArray.length === 0) {
            transactionStatusesArray = await getTransactionStatusesList();
        }
        populateSelect('TransactionStatusIdFilters', transactionStatusesArray.statuses, 'All statuses', null);

        benefitSelectFilters = document.getElementById('BenefitIdFilters');
        if (benefitsArray.length === 0) {
            benefitsArray = await getBenefitsList();
        }
        populateSelect('BenefitIdFilters', benefitsArray.benefits, 'All benefits', null);
        rightSidebarFiltersIsDiplayed = true;
    }
    hideSpinner();
    openRightSidebar();
}

// Filters select onchange
function onChangeFiltersReimbursements() {
    const container = document.querySelector(mainPaginationContainerSelector);
    paginationSubmit(true, false, container);
}
function clearFilters(formId) {
    resetFormElements(formId);
    getListOfResults(false, true);
}


// ===============================
// Global hook for ALL paginators
// ===============================

window.handlePaginationSubmit = function (args) {
    const ctx = args.paginationContext;
    const container = ctx.container;

    const isBenefitsModalPaginator = !!container.closest('#modal-consultants_benefits-balance');

    if (isBenefitsModalPaginator) {
        // Modal paginator
        getListOfBenefitsBalance(false, args.reloadTable);
    } else {
        // Main paginator
        getListOfResults(false, args.reloadTable);
    }
};




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
// ===============================
// Modal: Consultants & Benefits Balance
// ===============================

const modalPaginationContainerSelector = '#modal-consultants_benefits-balance .pagination-container';
const searchTextBenefitsBalance = $('#search-input-benefit-balance');
let rightSidebarFiltersIsDiplayedBenefitsBalance = false;

// Open modal and initial load
async function displayConsultantAndBenefitsBalanceModal(modalId) {
    showModal(modalId);
    searchTextBenefitsBalance.val(null);
    getListOfBenefitsBalance(true, false);
}

async function getListOfBenefitsBalance(firstTime, fromFilters) {
    displaySpinner();

    const formData = firstTime ? {} : recolectDataFromFormBenefitsBalance(fromFilters);

    const queryString = JSON.stringify(formData);
    const url = "/General/ConsultantReimbursedBenefits/GetConsultantsAndBenefitsBalanceList?model=" + encodeURIComponent(queryString);

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
            const tbody = $("#modal-consultants_benefits-balance .global-table-container table tbody");
            const tableRows = $("#modal-consultants_benefits-balance .global-table-container table");
            const noResultsMessage = $("#modal-consultants_benefits-balance .no-results");

            noResultsMessage.empty();
            tableRows.css("display", "block");
            tbody.empty();
            console.log(data.resultsList);
            data.resultsList.forEach(function (obj) {
                const row = `<tr class="hover-group">
                    <td>${obj.consultantName} ${obj.isActive ? '<span style="color: var(--clr-blueLight);">(Active)</span>' : '<span style="color: red;">(Inactive)</span>'}</td>
                    <td>${obj.benefitName} ($${obj.amountBase.toLocaleString('en-US') })</td>
                    <td style="color:${Number(obj.balanceAmount) === 0 ? 'red' : 'var(--clr-blueLight)'}">${obj.isActive ? '$' + obj.balanceAmount.toLocaleString('en-US') : '$' + obj.balanceAmount.toLocaleString('en-US') + '<span style="color:red"> (Unusable)<span>'}</td>
                </tr>`;
                tbody.append(row);
            });

            if (data.resultsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
            }

            updatePaginationBenefitsBalance(
                data.paginationFilters.paginationWithoutFilters.pagination
            );
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            hideSpinner();
        });
}

// ===============================
// Pagination + filters for modal
// ===============================
function recolectDataFromFormBenefitsBalance(fromFilters) {

    const filtersData = {
        SearchText: searchTextBenefitsBalance.val()
    };

    const paginationContainer = document.querySelector(modalPaginationContainerSelector);

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

function updatePaginationBenefitsBalance(paginationData) {
    updatePaginationValues(
        paginationData,
        document.querySelector(modalPaginationContainerSelector)
    );
}

function enterInSearchConsultantsAndBenefitsBalance(event) {
    const container = document.querySelector(modalPaginationContainerSelector);
    paginationSubmit(true, false, container);
}


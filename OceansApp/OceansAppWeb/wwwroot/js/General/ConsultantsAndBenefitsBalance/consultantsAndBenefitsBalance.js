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
            const list = data.resultsList || [];

            // If there are no results, show "NO RECORDS FOUND"
            if (list.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
            } else {
                // We assume the list is already sorted by consultantName
                let i = 0;

                while (i < list.length) {
                    const current = list[i];
                    const currentName = current.consultantName;
                    const currentIsActive = current.isActive;

                    // Calculate how many rows belong to this consultantName group
                    let groupSize = 1;
                    while (
                        i + groupSize < list.length &&
                        list[i + groupSize].consultantName === currentName
                    ) {
                        groupSize++;
                    }

                    // Build the rowspan cell content (consultant name + status)
                    const consultantStatusHtml = currentIsActive
                        ? `<span style="color: var(--clr-blueLight);">(Active)</span>`
                        : `<span style="color: red;">(Inactive)</span>`;

                    const consultantCellHtml =
                        `${currentName} ${consultantStatusHtml}`;

                    // Render all rows for this consultant
                    for (let j = 0; j < groupSize; j++) {
                        const item = list[i + j];

                        const benefitText =
                            `${item.benefitName} ($${Number(item.amountBase).toLocaleString('en-US')}) ${item.consultantAndBenefitId === null ? '' : `<a class="view-history-link" onclick="getListOfBenefitHistory(${item.consultantAndBenefitId}, '${item.benefitName}', '${currentName.split(" ")[0]}')" >👀 View History</a>`
}`;

                        const isZeroBalance = Number(item.balanceAmount) === 0;
                        const balanceColor = isZeroBalance ? 'red' : 'var(--clr-blueLight)';

                        const balanceContent = item.benefitName === 'Bonusly' ? '<span style="color:gray">No limit</span>' + (!item.isActive ? ' <span style="color:red">(Unusable)</span>' : '') : item.isActive
                            ? '$' + Number(item.balanceAmount).toLocaleString('en-US')
                            : '$' + Number(item.balanceAmount).toLocaleString('en-US') +
                            ' <span style="color:red">(Unusable)</span>';

                        let row = `<tr class="hover-group">`;

                        // Only add the first column once per group, with rowspan
                        if (j === 0) {
                            row += `<td rowspan="${groupSize}">${consultantCellHtml}</td>`;
                        }

                        row += `
                            <td>${benefitText}</td>
                            <td style="color:${balanceColor}">${balanceContent}</td>
                        </tr>`;

                        tbody.append(row);
                    }

                    // Move to the next group
                    i += groupSize;
                }
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

const descriptionConfirmReset = getElementById('description-confirm-reset');
const wordConfirmReset = getElementById('word-confirm-reset');
const validationMessageConfirm = getElementById('word-val-message');
function displayConfirmationToResetBalance() {
    descriptionConfirmReset.value = null;
    wordConfirmReset.value = null;
    validationMessageConfirm.style.display = 'none';

    const url = "/General/ConsultantReimbursedBenefits/GetDataToResetAllBenefits";
    displaySpinner();
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
            const alertMessage = getElementById('reset-alert-message');
            alertMessage.innerHTML = `<label><strong>${data.currentUser.toUpperCase()}, BE CAREFUL!!! </strong><span>😳</span></label><label>By clicking <strong>“Reset All”</strong>, all consultant benefit balances will be reset to their configured yearly amounts.
Before proceeding, make sure there are no pending or upcoming redemptions, as this action cannot be undone. </label>
                    <label>
                        Feel free to click “Reset All” only if you are 100% sure about this action.<span>😉</span></label>`;
            showModal('modal-confirm-reset');
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            hideSpinner();
        });
}
async function resetAllConsultantsBalance() {
    if (wordConfirmReset.value === 'RESET') {
        displaySpinner();
        var token = $('[name="__RequestVerificationToken"]').val();
        var formData = new FormData();
        formData.append('description', descriptionConfirmReset.value);
        fetch("/General/ConsultantReimbursedBenefits/ResetAllConsultantsBenefitsBalance"
            , {
                method: 'POST',
                headers: {
                    RequestVerificationToken: token
                },
                body: formData
            })
            .then(response => {
                if (response.ok) {
                    return response.json();
                } else {
                    return response.json().then(errorData => {
                        if (errorData.messageType === "Validation Error") {
                            displayToasterWarningArray(errorData.errors);
                            throw new Error('Validation errors!');
                        } else {
                            displayToasterError(errorData.error);
                            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                        }
                    });
                }
            })
            .then(data => {
                if (data.success) {
                    toastr.success(data.message);
                    hideModal('modal-confirm-reset');
                    getListOfBenefitsBalance(false, false);
                } else {
                    displayToasterError(data.error);
                    console.error('There has been a problem with the fetch operation:', data.detail);
                }
            })
            .catch(error => {
                validateSessionExpiration(error.message);
            })
            .finally(() => {
                hideSpinner();
            });
    } else {
        if (wordConfirmReset.value === null || wordConfirmReset.value === '') {
            validationMessageConfirm.textContent = 'The Word confirmation is required.';
        } else {
            validationMessageConfirm.textContent = 'The word does not match the required.';
        }
        validationMessageConfirm.style.display = 'block';
    }

}

async function getListOfBenefitHistory(consultantBenefitId, benefitName, consultantName) {
    displaySpinner();
    const modalTitle = getElementById('benefit-history-title');
    modalTitle.textContent = `History '${benefitName}' for ${consultantName}`;
    const url = "/General/ConsultantReimbursedBenefits/GetConsultantsAndBenefitsHistory?consultantAndBenefitId=" + encodeURIComponent(consultantBenefitId);

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
            const tbody = $("#modal-benefits-history .global-table-container table tbody");
            const tableRows = $("#modal-benefits-history .global-table-container table");
            const noResultsMessage = $("#modal-benefits-history .no-results");

            noResultsMessage.empty();
            tableRows.css("display", "block");
            tbody.empty();
            const list = data.historyList || [];

            // If there are no results, show "NO RECORDS FOUND"
            if (list.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
            } else {
                list.forEach(function (obj) {
                    const row = `<tr class="hover-group">
                  <td>${obj.benefitCategory === null ? 'THE BENEFIT WAS RESET 🔄' : obj.benefitCategory}</td>
                   <td class="${obj.newValue - obj.oldValue > 0 ? 'green-label' : 'red-label'}">${obj.newValue - obj.oldValue > 0 ? '+$' : '-$'}${formatNumber(Math.abs(obj.newValue - obj.oldValue))}</td>
                  <td class="${obj.oldValue >= obj.newValue ? 'green-label' : 'red-label'}">$${obj.oldValue.toLocaleString('en-US')}</td>
                  <td class="${obj.oldValue >= obj.newValue ? 'red-label' : 'green-label'}">$${obj.newValue.toLocaleString('en-US')}</td>
                  <td>${obj.reimbursementDetail === null ? '' : obj.reimbursementDetail}</td>
                  <td>${obj.notes === null ? '' : obj.notes}</td>
                  <td>${obj.userCreatedBy}</td>
                  <td>${formatUtcToLocalMmDdYyyyTime(obj.creationDate)}</td>
              </tr>`;
                    tbody.append(row);
                });
            }
            showModal('modal-benefits-history');
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            hideSpinner();
        });
}

function formatNumber(num) {
    return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(num ?? 0);
}
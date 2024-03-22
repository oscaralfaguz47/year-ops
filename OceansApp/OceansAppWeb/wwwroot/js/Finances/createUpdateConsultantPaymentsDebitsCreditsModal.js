//HTML ELEMENTS
var costCenterSelect = document.getElementById("CostCenterSelect");
var accountingAccountSelect = document.getElementById("AccountingAccountSelect");

//OTHER VARIBLES
let selectedCompany = '';

document.addEventListener("DOMContentLoaded", function () {
    var actionDate = document.getElementById('ActionDateWithinFortnight');
    var today = new Date();
    var todayFormatted = today.toISOString().substr(0, 10);
    actionDate.min = todayFormatted;
    function validateDate() {
        if (actionDate.value < actionDate.min) {
            actionDate.value = actionDate.min;
        }
    }
    actionDate.addEventListener('change', validateDate);
});

//CREATE / UPDATE DEBIR OR CREDIT
async function displayUpdateCreateDebitCreditModal(modalId, id) {
    var modalTitle = document.getElementById('create-debit-credit-modal-title');
    modalTitle.textContent = "ADD NEW DEBIT/CREDIT";
    var createUpdateForm = $('#form-create-update');
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    createUpdateForm.find('[name="consultantPaymentDebitsCreditsId"]').val("");
    createUpdateForm.find('[name="consultantIdFromSearch"]').val("");
    selectedCompany = '';

    if (id !== null) {
        modalTitle.textContent = "UPDATE DEBIT/CREDIT";
        var url = "/Finances/ConsultantPaymentsDebitsCredits/GetDebitCreditDataById?consultantPaymentDebitsCreditsId=" + encodeURIComponent(id);
        displaySpinner();
        fetch(url)
            .then(response => {
                if (response.ok) {
                    return response.json();
                } else {
                    return response.json().then(errorData => {
                        displayToasterError(errorData.error);
                        hideModal(modalId);
                        getListOfResults(false, false);
                        throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                    });
                }
            })
            .then(data => {
                createUpdateForm.find('[name="consultantPaymentDebitsCreditsId"]').val(data.debitCreditData.consultantPaymentDebitsCreditsId);
                createUpdateForm.find('[name="consultantIdFromSearch"]').val(data.debitCreditData.consultantId);
                createUpdateForm.find('[name="consultantNameInput"]').val(data.debitCreditData.consultantName);
                createUpdateForm.find('[name="consultantEmailInput"]').val(data.debitCreditData.consultantEmail);
                selectedCompany = data.debitCreditData.consultantCompanyId;
                selectConsultant(data.debitCreditData.consultantCompanyId, true, data.debitCreditData.costCenterId, data.debitCreditData.accountingAccountId);
                createUpdateForm.find('[name="quantity"]').val(data.debitCreditData.quantity);
                createUpdateForm.find('[name="amount"]').val(data.debitCreditData.amount);
                updateTotalAmount();
                let actionDateFormat = new Date(data.debitCreditData.actionDateWithinFortnight);
                createUpdateForm.find('[name="actionDateWithinFortnight"]').val(actionDateFormat.toISOString().split('T')[0]);
                createUpdateForm.find('[name="detail"]').val(data.debitCreditData.detail);

                var radioButton = document.querySelector(`input[name="transaction-type"][value="${data.debitCreditData.transactionTypeName}"]`);
                if (radioButton) radioButton.checked = true;

                showModal(modalId);
            })
            .finally(() => {
                hideSpinner();
            });
    } else {
        costCenterSelect.disabled = true;
        costCenterSelect.innerHTML = '<option value>-First select a Consultant-</option>';
        accountingAccountSelect.disabled = true;
        accountingAccountSelect.innerHTML = '<option value>-First select a Cost Center-</option>';
        showModal(modalId);
    }
}

//SELECT CONSULTANT AND FILL COSTS CENTERS LIST
function selectConsultant(selectedValue, isEditing, selectedValueCostCenter, selectedValueAccountingAccount) {
    fillCostsCentersForSelect(selectedValue, isEditing, selectedValueCostCenter, selectedValueAccountingAccount);
}
function fillCostsCentersForSelect(selectedValue, isEditing, selectedValueCostCenter, selectedValueAccountingAccount) {
    var selectElement = costCenterSelect;
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    displaySpinner();
    getCostsCentersWhereCompanyList(selectedValue)
        .then(data => {
            selectElement.innerHTML = '';
            selectElement.innerHTML = '<option value>-Select a Cost Center-</option>';
            selectElement.disabled = false;
            data.costsCenters.forEach(obj => {
                var costCenterCode = '';
                var selectValue = null;
                obj.acceptData === 'S' ? costCenterCode = '(' + obj.costCenterCode + ')' : costCenterCode = '';
                obj.acceptData === 'S' ? selectValue = obj.costCenterId : selectValue = null;
                var option = new Option(obj.description + ' ' + costCenterCode, selectValue);
                if (obj.acceptData === 'N') {
                    option.className = 'option-no-accept-data';
                    option.disabled = true;
                }
                selectElement.add(option);
            });
            if (isEditing) {
                selectElement.value = selectedValueCostCenter;
                selectCostCenter('CostCenterSelect', selectedValueCostCenter, true, selectedValueAccountingAccount)
            }
            hideSpinner();
        })
        .catch(error => {
            hideSpinner();
            console.error('Error fetching roles:', error);
        });
}

//SELECT COST CENTER AND FILL ACCOUNTING ACOUNTS LIST
function selectCostCenter(selectElementId, selectedValue, isEditing, selectedValueAccountingAccount) {
    if (selectedValue !== null) {
        var selectElement = document.getElementById(selectElementId);
        for (var i = 0; i < selectElement.options.length; i++) {
            if (selectElement.options[i].value === "" || selectElement.options[i].value === null) {
                selectElement.remove(i);
                break;
            }
        }
        fillAccountingAccountsForSelect(selectedValue, isEditing, selectedValueAccountingAccount);
    }
}
function fillAccountingAccountsForSelect(selectedValue, isEditing, selectedValueAccountingAccount) {
    var selectElement = accountingAccountSelect;
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    displaySpinner();
    getAccountingAccountsWhereCostCenterList(selectedValue)
        .then(data => {
            selectElement.innerHTML = '';
            selectElement.innerHTML = '<option value>-Select an Account-</option>';
            selectElement.disabled = false;
            data.accountingAccounts.forEach(obj => {
                var accountCode = '';
                var selectValue = null;
                obj.acceptData === 'S' ? accountCode = '(' + obj.accountingAccountCode + ')' : accountCode = '';
                obj.acceptData === 'S' ? selectValue = obj.accountingAccountId : selectValue = null;
                var option = new Option(obj.description + ' ' + accountCode, selectValue);
                if (obj.acceptData === 'N') {
                    option.className = 'option-no-accept-data';
                    option.disabled = true;
                }
                selectElement.add(option);
            });
            if (isEditing) {
                selectElement.value = selectedValueAccountingAccount;
            }
            hideSpinner();
        })
        .catch(error => {
            hideSpinner();
            console.error('Error fetching roles:', error);
        });
}

//CREATE, UPDATE DEBIT CREDIT
async function createUpdateDebitCredit(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var consultantPaymentDebitsCreditsIdData = createUpdateForm.find('[name="consultantPaymentDebitsCreditsId"]').val() || null;
    var consultantIdData = createUpdateForm.find('[name="consultantIdFromSearch"]').val() || null;
    var costCenterIdData = createUpdateForm.find('[name="costCenterId"]').val() || null;
    var accountingAccountIdData = createUpdateForm.find('[name="AccountingAccountId"]').val() || null;
    var quantityData = createUpdateForm.find('[name="quantity"]').val() || null;
    var amountData = createUpdateForm.find('[name="amount"]').val() || null;
    var actionDateData = createUpdateForm.find('[name="actionDateWithinFortnight"]').val() || null;
    var detailData = createUpdateForm.find('[name="detail"]').val() || null;
    var transactionTypeData = document.querySelector('.transaction-type-rg input[type="radio"]:checked')?.value || null;

    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        ConsultantPaymentDebitsCreditsId: consultantPaymentDebitsCreditsIdData,
        Detail: detailData,
        CostCenterId: costCenterIdData,
        ConsultantId: consultantIdData,
        AccountingAccountId: accountingAccountIdData,
        Quantity: quantityData,
        Amount: amountData,
        ActionDateWithinFortnight: actionDateData,
        TransactionTypeName: transactionTypeData
    };
    console.log(data);
    fetch('/Finances/ConsultantPaymentsDebitsCredits/CreateUpdateDebitCredit', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            RequestVerificationToken: token
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    if (errorData.messageType === "Validation Error") {
                        console.log(errorData.errors);
                        displayToasterWarningArray(errorData.errors);
                        inicializeModalButtons(modalId);
                        throw new Error('Validation errors!');
                    } else {
                        displayToasterError(errorData.error);
                        hideModal(modalId);
                        throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                    }
                });
            }
        })
        .then(data => {
            inicializeModalButtons(modalId);
            displayToasterSuccess(data.message);
            hideModal(modalId);
            getListOfResults(false, false);
        });
}

// INPUT VALIDATIONS
document.getElementById('Detail').addEventListener('input', function (e) {
    if (this.value.length > 150) {
        this.value = this.value.slice(0, 150);
    }
});
document.addEventListener("DOMContentLoaded", function () {
    validateInputTypeNumber('Amount');
});
document.addEventListener("DOMContentLoaded", function () {
    validateInputTypeNumber('QuantityInput');
});

let selectedIndexOp = -1;

async function searchAllActiveConsultantsBySearchTextFromDebitsCredits(searchTextInput, hiddenInputForId, consultantNameInput, consultantEmailInput, userCategoryName) {
    if (searchTextInput.value.length > 100) {
        searchTextInput.value = searchTextInput.value.slice(0, 100);
    } else {
        let resultsContainer = document.getElementById('consultant-search-results');
        resultsContainer.innerHTML = '';
        resultsContainer.innerHTML = `<div class="text-center"><div class="spinner-border" role="status">
        <span class="sr-only"></span>
        </div></div>`;
        let data = await getAllActiveConsultantsBySearchText(searchTextInput.value, userCategoryName);
        resultsContainer.innerHTML = '';
        resultsContainer.style.display = 'block';
        if (data.consultants.length > 0) {
            let resultList = document.createElement('ul');
            resultList.id = 'search-result-list'; // Assign an ID to the results list container
            for (let item of data.consultants) {
                let listItem = document.createElement('li');
                listItem.innerHTML = '<strong>' + item.consultantName + '</strong> ' + (item.userCategoryName === "Administrative" ? '<span class="green-label">(' : '<span class="blue-label">(') + item.userCategoryName + ')</span>';
                listItem.onclick = function () {
                    document.getElementById(hiddenInputForId).value = item.consultantId;
                    document.getElementById(consultantNameInput).value = item.consultantName;
                    document.getElementById(consultantEmailInput).value = item.email;
                    hideConsultantResultsDiv();
                    if (selectedCompany !== item.companyId) {
                        selectConsultant(item.companyId, false, null);
                        accountingAccountSelect.disabled = true;
                        accountingAccountSelect.innerHTML = '<option value>-First select a Cost Center-</option>';
                    }
                    selectedCompany = item.companyId;
                };
                resultList.appendChild(listItem);
            }
            resultsContainer.appendChild(resultList);
        } else {
            resultsContainer.innerHTML = '<div class="red-label text-center">No results found</div>';
        }
    }
    document.addEventListener('keydown', keyboardNavigationOpt);
}

// Function to update the active item in the results list
function updateActiveItemS() {
    const listItems = document.querySelectorAll('#search-result-list li');
    // Removes the active class from all elements.
    listItems.forEach(item => {
        item.classList.remove('active');
    });
    // Adds the active class to the selected element.
    if (selectedIndexOp >= 0 && selectedIndexOp < listItems.length) {
        listItems[selectedIndexOp].classList.add('active');
        listItems[selectedIndexOp].scrollIntoView({ behavior: "smooth", block: "nearest" });
    }
}

function keyboardNavigationOpt(event) {
    const resultsContainer = document.getElementById('consultant-search-results');
    const listItems = document.querySelectorAll('#search-result-list li');
    if (resultsContainer.style.display !== 'none') {
        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                if (selectedIndexOp < listItems.length - 1) {
                    selectedIndexOp++;
                    updateActiveItemS();
                }
                break;
            case 'ArrowUp':
                event.preventDefault();
                if (selectedIndexOp > 0) {
                    selectedIndexOp--;
                    updateActiveItemS();
                }
                break;
            case 'Enter':
                event.preventDefault();
                if (selectedIndexOp >= 0 && selectedIndexOp < listItems.length) {
                    listItems[selectedIndexOp].click();
                }
                break;
        }
    }
    if (event.key === 'Escape') {
        hideConsultantResultsDiv();
    }
}

function hideConsultantResultsDiv() {
    let resultsContainer = document.getElementById('consultant-search-results');
    resultsContainer.style.display = 'none';
    selectedIndexOp = -1; // Reset the selected index
    document.getElementById('search-consultant-input').value = null;
}

// Add a listener for clicks outside the results container to close the results when clicked outside.
document.addEventListener('click', function (event) {
    const searchContainer = document.getElementById('consultants-search-cont');
    if (!searchContainer.contains(event.target)) {
        hideConsultantResultsDiv();
    }
});

//UPDATE TOTAL AMOUNT INUT
function updateTotalAmount() {
    var quantity = parseFloat(document.getElementById('QuantityInput').value);
    var amount = parseFloat(document.getElementById('Amount').value);

    var total = quantity * amount || 0;

    document.getElementById('TotalAmount').value = total.toFixed(2);
}

document.getElementById('QuantityInput').addEventListener('input', updateTotalAmount);
document.getElementById('Amount').addEventListener('input', updateTotalAmount);

updateTotalAmount();
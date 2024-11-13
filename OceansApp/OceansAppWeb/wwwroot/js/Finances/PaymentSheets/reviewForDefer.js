const movementsContainerRD = getElementById('movements-container');
const actionDateRD = getElementById('ActionDateRD');
let costCentersArray = [];

function updateArray(index, field, newValue) {
    let entry = valuesArray.find(item => item.index === index);

    if (!entry) {
        entry = { index }; // Create a new entry if it doesn't exist
        valuesArray.push(entry);
    }

    entry[field] = newValue; // Update or add the field value
}
async function displayReviewForDeferModal(modalId, consultantId) {
    let url = "/Finances/PaymentSheets/GetDifferenceToDeferNextPeriod?consultantId=" + encodeURIComponent(consultantId)
        + "&startDate=" + encodeURIComponent(dateFromInput.value)
        + "&endDate=" + encodeURIComponent(dateToInput.value);

    displaySpinner();
    try {
        const response = await fetch(url);

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            hideModal(modalId);
            return null;
        }

        const dataFromApi = await response.json();
        console.log(dataFromApi);
        if (costCentersArray.length === 0) {
            costCentersArray = await getCostsCentersWhereCompanyList(dataFromApi.data.companyId);
        }

        movementsContainerRD.innerHTML = '';
        let actionDateFormat = new Date(dataFromApi.data.actionDate);
        actionDateRD.value = actionDateFormat.toISOString().split('T')[0];
        disableActionDateDatePicker(dataFromApi.data.actionDate);

        dataFromApi.data.listOfMovementsToDefer.forEach(function (obj, index) {
            let isNotDebitOrCredit = obj.accountingAccountName !== null || obj.costCenterName !== null;
            let divRow = document.createElement('div');
            divRow.className = 'movement-row';

            // Template HTML creation
            let createdElement = `
            <div>
              <input type="hidden" id="mo-id-${index}" value="${obj.id}">
              <h5 class="${obj.transactionTypeName === 'Credit' ? 'credit-background' : 'debit-background'}">${obj.transactionTypeName === 'Credit' ? '<img class="icon-debit-credit" src="/icons/Shared/square-plus.svg">' : '<img class="icon-debit-credit" src="/icons/Shared/square-minus.svg">'}${obj.transactionTypeName}</h5>
            </div>
            <div class="numbers-cont">
              <div>
                 <label>Quantity</label>
                 <input class="form-control" type="number" id="quantity-${index}" disabled value="${obj.quantity.toFixed(2)}">
              </div>
              <div>
                 <label>Unit Price</label>
                 <input class="form-control" type="number" id="unitPrice-${index}" disabled value="${obj.amount.toFixed(2)}">
              </div>
              <div>
                <label>Total Amount</label>
                <input class="form-control" type="number" id="totalAmount-${index}" disabled value="${(obj.quantity * obj.amount).toFixed(2)}"> 
              </div>
            </div>
            <div class="selects-cont">
              <div class="select-cont">
                <label>Cost Center</label>
                <select id="sel-${index}" ${isNotDebitOrCredit ? 'disabled' : ''} class="form-select">
                ${isNotDebitOrCredit ? `<option value="${obj.costCenterId}">${obj.accountingAccountName}</option>` : ``}
                <select>
              </div>
              <div class="select-cont">
                <label>Accounting Account</label>
                <select id="aa-${index}" ${isNotDebitOrCredit ? 'disabled' : ''} class="form-select">
                ${isNotDebitOrCredit ? `<option value="${obj.accountingAccountId}">${obj.accountingAccountName}</option>` : ``}
                <select>
              </div>
            </div>
            <div class="text-cont">
              <label>Description</label>
              <input class="form-control" id="description-${index}" type="text" value="${obj.detail}">
            </div>
            `;

            divRow.innerHTML = createdElement;
            movementsContainerRD.appendChild(divRow);

            if (obj.costCenterName == null) {
                let selectElCostCenter = document.getElementById(`sel-${index}`);
                let selectAccountingAccount = document.getElementById(`aa-${index}`);
                selectAccountingAccount.innerHTML = '<option value>-Select a Cost Center-</option>';
                selectAccountingAccount.disabled = true;
                selectElCostCenter.innerHTML = '<option value>-Select a Cost Center-</option>';
                selectElCostCenter.disabled = false;

                costCentersArray.costsCenters.forEach(costCenter => {
                    var costCenterCode = '';
                    var selectValue = null;
                    costCenter.acceptData === 'S' ? costCenterCode = '(' + costCenter.costCenterCode + ')' : costCenterCode = '';
                    costCenter.acceptData === 'S' ? selectValue = costCenter.costCenterId : selectValue = null;
                    var option = new Option(costCenter.description + ' ' + costCenterCode, selectValue);
                    if (costCenter.acceptData === 'N') {
                        option.className = 'option-no-accept-data';
                        option.disabled = true;
                    }
                    selectElCostCenter.add(option);
                });

                selectElCostCenter.addEventListener('change', async function () {
                    await fillAccountingAccountsForSelect(this.value, selectAccountingAccount);
                    selectAccountingAccount.disabled = false;
                });
            }
        });

        hideSpinner();
        showModal(modalId);
    }
    catch (err) {
        hideSpinner();
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err.message);
        displayToasterError('Something went wrong, more details: ' + err);
    }
}

function disableActionDateDatePicker(date) {

    let dateTimeFormat = new Date(date);
    let dateFormatted = dateTimeFormat.toISOString().substr(0, 10);
    let todaysDate = new Date();
    let todaysDateFormated = todaysDate.toISOString().substr(0, 10);

    if (dateFormatted < todaysDateFormated) {
        actionDateRD.min = todaysDateFormated;
    } else {
        actionDateRD.min = dateFormatted;
    }
    function validateDate() {
        if (actionDateRD.value < actionDateRD.min) {
            actionDateRD.value = actionDateRD.min;
        }
    }
    actionDateRD.addEventListener('change', validateDate);
};

//SELECT COST CENTER AND FILL ACCOUNTING ACOUNTS LIST

async function fillAccountingAccountsForSelect(selectedValue, selectEl) {
    var selectElement = selectEl;
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    displaySpinner();
    data = await getAccountingAccountsWhereCostCenterList(selectedValue);

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
    hideSpinner();
}

async function DeferDebitsCredits(modalId) {
    let valuesArray = [];
    document.querySelectorAll('.movement-row').forEach(function (row, index) {
        let rowData = {
            Id: Number(getElementById(`mo-id-${index}`).value),
            CostCenterId: getElementById(`sel-${index}`) ? getElementById(`sel-${index}`).value === '' ? null : Number(getElementById(`sel-${index}`).value) : null,
            AccountingAccountId: getElementById(`aa-${index}`) ? document.getElementById(`aa-${index}`).value === '' ? null : Number(document.getElementById(`aa-${index}`).value) : null,
            Description: getElementById(`description-${index}`).value === '' ? null : getElementById(`description-${index}`).value
        };
        valuesArray.push(rowData);
    });
    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        ConsultantId: consultantIdInputMP.value === '' ? null : Number(consultantIdInputMP.value),
        StartDate: dateFromInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2'),
        EndDate: dateToInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2'),
        ActionDate: actionDateRD.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2'),
        MovementsList: valuesArray
    };
    console.log(data);

    displaySpinner();
    try {
        const response = await fetch('/Finances/PaymentSheets/DeferDebitCredit', {
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
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }
        hideModal(modalId);
        hideModal('modal-review-for-payment');
        const dataFromApi = await response.json();
        displayToasterSuccess(dataFromApi.message);
        hideSpinner();
        await getListOfResults(false, true);
        return dataFromApi;

    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        hideSpinner();
        return null;
    }
}
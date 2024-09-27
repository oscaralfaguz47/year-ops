const movementsContainerRD = getElementById('movements-container');
const actionDateRD = getElementById('ActionDateRD');
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
        movementsContainerRD.innerHTML = '';
        console.log(dataFromApi);
        let actionDateFormat = new Date(dataFromApi.data.actionDate);
        actionDateRD.value = actionDateFormat.toISOString().split('T')[0];

        dataFromApi.data.listOfMovementsToDefer.forEach(function (obj, index) {
            let isNotDebitOrCredit = obj.accountingAccountName !== null || obj.costCenterName !== null ? true : false;
            let divRow = document.createElement('div');
            divRow.className = 'movement-row';
            let createdElement = `
            <div>
              <h5 class="${obj.transactionTypeName === 'Credit' ? 'credit-background' : 'debit-background'}">${obj.transactionTypeName === 'Credit' ? '<img class="icon-debit-credit" src="/icons/Shared/square-plus.svg">'
                    : '<img class="icon-debit-credit" src="/icons/Shared/square-minus.svg">'}${obj.transactionTypeName}</h5>
            </div>
            <div class="numbers-cont">
              <div>
                 <label>Quantity</label>
                 <input class="form-control" type="number" disabled value="${obj.quantity.toFixed(2)}">
              </div>
              <div>
                 <label>Unit Price</label>
                 <input class="form-control" type="number" disabled value="${obj.amount.toFixed(2)}">
              </div>
              <div>
                <label>Total Amount</label>
                <input class="form-control" type="number" disabled value="${(obj.quantity * obj.amount).toFixed(2)}"> 
              </div>
            </div>
            <div class="selects-cont">
              <div class="select-cont">
                <label for="CostCenterRD">Cost Center</label>
                <select ${isNotDebitOrCredit ? 'disabled' : ''} class="form-select">
                ${isNotDebitOrCredit ? `<option>${obj.accountingAccountName}</option>` : ``}
                <select>
              </div>
              <div class="select-cont">
                <label for="CostCenterRD">Accounting Account</label>
                <select ${isNotDebitOrCredit ? 'disabled': ''} class="form-select">
                ${isNotDebitOrCredit ? `<option>${obj.accountingAccountName}</option>` : ``}
                <select>
              </div>
            </div>
            <div class="text-cont">
              <label>Description</label>
              <input class="form-control" type="text" value="${obj.detail}">
            </div>
            `;
            divRow.innerHTML = createdElement;
            movementsContainerRD.appendChild(divRow);
        });
       
        hideSpinner();
        showModal(modalId);
        return dataFromApi;
    }
    catch (err) {
        hideSpinner();
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err.message);
        displayToasterError('Something went wrong, more details: ' + err);
        return null;
    }
}
async function getAccountingAccountsWhereCostCenterList(costCenterId) {
    var url = "/Finances/AccountingAccounts/GetAccountingAccountsListWhereCostCenterId?costCenterId=" + encodeURIComponent(costCenterId);
    return fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    displayToasterError(errorData.error);
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .catch(error => {
                validateSessionExpiration(error.message);
            displayToasterError("Network error or unable to reach the server.");
            throw new Error('Network error or unable to reach the server. More details: ' + error.message);
        });
}


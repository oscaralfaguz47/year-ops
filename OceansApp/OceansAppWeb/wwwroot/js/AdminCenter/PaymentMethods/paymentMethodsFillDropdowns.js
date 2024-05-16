
async function getPaymentMethodsWhereCompanyList(companyId) {
    var url = "/AdminCenter/PaymentMethods/GetPaymentMethodsListWhereCompany?companyId=" + encodeURIComponent(companyId);
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
            displayToasterError(error.message);
        });
}
async function getPartnersList() {
    var url = "/AccountManagement/Partners/GetAllPartnersListForSelect";
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
            displayToasterError("Internet connection failed");
            throw new Error('Network error or unable to reach the server. More details: ' + error.message);
        });
}

function fillPartnersSelectForFilters(selectElement, firstOption) {
    if (selectElement.length > 1) {
        return;
    }
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    getPartnersList()
        .then(data => {
            selectElement.innerHTML = '<option value="">-' + firstOption + '-</option>';
            data.partners.forEach(obj => {
                selectElement.add(new Option(obj.text, obj.value));
            });
        })
        .catch(error => {
            console.error('Error fetching partners:', error);
        });
}
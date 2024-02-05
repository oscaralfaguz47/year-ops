async function getClientsList() {
    var url = "/AccountManagement/Clients/GetAllClientsListForSelect";
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
        });
}
function fillClientsSelectForFilters(selectElement, firstOption) {
    if (selectElement.length > 1) {
        return;
    }
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    getClientsList()
        .then(data => {
            selectElement.innerHTML = '<option value="null">-' + firstOption + '-</option>';
            data.clients.forEach(obj => {
                selectElement.add(new Option(obj.text, obj.value));
            });
        })
        .catch(error => {
            console.error('Error fetching clients:', error);
        });
}
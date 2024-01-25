async function getSuccessManagersList() {
    var url = "/General/ConsultantDetails/GetSuccessManagers";
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
function fillSuccessManagersSelectForFilters(selectElement) {
    if (selectElement.length > 1) {
        return;
    }
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    getSuccessManagersList()
        .then(data => {
            selectElement.innerHTML = '<option value="">-All Success Managers-</option>';
            data.successManagers.forEach(obj => {
                selectElement.add(new Option(obj.userName, obj.userId));
            });
        })
        .catch(error => {
            console.error('Error fetching success managers:', error);
        });
}
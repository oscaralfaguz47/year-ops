
async function getRolesList() {
    var url = "/AdminCenter/UserRolesPermissions/GetAllRolesListForSelect";
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
            displayToasterError(error.message);
        });
}

function fillRolesSelectForFilters(selectElement, firstOption) {
    if (selectElement.length > 1) {
        return;
    }
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    getRolesList()
        .then(data => {
            selectElement.innerHTML = '<option value="">-' + firstOption + '-</option>';
            data.roles.forEach(obj => {
                selectElement.add(new Option(obj.name, obj.value));
            });
        })
        .catch(error => {
            console.error('Error fetching roles:', error);
        });
}
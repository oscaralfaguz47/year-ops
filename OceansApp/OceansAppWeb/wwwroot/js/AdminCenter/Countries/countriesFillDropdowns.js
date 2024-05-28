
async function getCountriesList() {
    var url = "/AdminCenter/Countries/GetAllCountriesListForSelect";
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

function fillCountriesSelectForFilters(selectElement, firstOption) {
    if (selectElement.length > 1) {
        return;
    }
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    getCountriesList()
        .then(data => {
            selectElement.innerHTML = '<option value="">-' + firstOption + '-</option>';
            data.countries.forEach(obj => {
                selectElement.add(new Option(obj.name, obj.value));
            });
        })
        .catch(error => {
            console.error('Error fetching countries:', error);
        });
}
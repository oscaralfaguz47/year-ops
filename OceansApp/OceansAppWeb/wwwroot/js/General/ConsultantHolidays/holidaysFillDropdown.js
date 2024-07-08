async function getHolidaysList() {
    var url = "/General/ConsultantHolidays/GetHolidaysListForSelect";
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

function fillHolidaysSelect(selectElement, firstOption) {
    let previousValue = selectElement.value;
    if (selectElement.length > 1) {
        return;
    }
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    getHolidaysList()
        .then(data => {
            selectElement.innerHTML = '<option value="">-' + firstOption + '-</option>';
            data.holidays.forEach(obj => {
                selectElement.add(new Option(obj.text, obj.value));
            });
            selectElement.value = previousValue;
        })
        .catch(error => {
            console.error('Error fetching holidays:', error);
        });
}
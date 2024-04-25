async function getHolidaysList(year) {
    var url = "/General/ConsultantHolidays/GetHolidaysListForSelectByYear?year=" + year;
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
            displayToasterError("Internet connection failed");
            throw new Error('Network error or unable to reach the server. More details: ' + error.message);
        });
}

function fillHolidaysSelect(selectElement, firstOption, parameter, selectedOption) {
    console.log("VALUE: " + selectedOption);
    selectElement.innerHTML = '';
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    getHolidaysList(parameter)
        .then(data => {
            console.log(parameter);
            selectElement.innerHTML = '<option value="">-' + firstOption + '-</option>';
            data.holidays.forEach(obj => {
                selectElement.add(new Option(obj.text, obj.value));
            });
            selectElement.value = selectedOption !== null ? selectedOption : '';
        })
        .catch(error => {
            console.error('Error fetching holidays:', error);
        });
}
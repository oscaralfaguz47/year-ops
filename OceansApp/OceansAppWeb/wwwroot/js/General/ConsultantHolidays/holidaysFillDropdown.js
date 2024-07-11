async function getHolidaysList() {
    var url = "/General/ConsultantHolidays/GetHolidaysListForSelect";
    try {
        const response = await fetch(url);
        if (response.ok) {
            return await response.json();
        } else {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
        }
    } catch (error) {
        validateSessionExpiration(error.message);
        displayToasterError("Internet connection failed");
        throw new Error('Network error or unable to reach the server. More details: ' + error.message);
    }
}

async function fillHolidaysSelect(selectElement, firstOption) {
    if (selectElement.dataset.loaded === 'true') {
        return;
    }
    selectElement.dataset.loaded = 'true';

    let previousValue = selectElement.value;
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    try {
        const data = await getHolidaysList();
        console.log(data);
        selectElement.innerHTML = '<option value="">-' + firstOption + '-</option>';
        data.holidays.forEach(obj => {
            selectElement.add(new Option(obj.text, obj.value));
        });
        selectElement.value = previousValue;
    } catch (error) {
        console.error('Error fetching holidays:', error);
    }
}
document.addEventListener('DOMContentLoaded', function () {
    const selectElement = document.getElementById('HolidaysSelect');
    selectElement.addEventListener('click', function () {
        fillHolidaysSelect(this, 'Select a Holiday');
    });
});
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
    let previousValue = selectElement.value;
    if (selectElement.length > 1) {
        return;
    }

    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    try {
        const data = await getHolidaysList();
        console.log(data);
        selectElement.innerHTML = '<option value="">-' + firstOption + '-</option>';
        data.holidays.forEach(obj => {
            selectElement.add(new Option(obj.text, obj.value));
        });

        selectElement.value = previousValue;
        reopenSelect(selectElement);
    } catch (error) {
        console.error('Error fetching holidays:', error);
    }
}
function reopenSelect(selectElement) {
    // Crear un nuevo evento de clic
    const clickEvent = new MouseEvent('click', {
        view: window,
        bubbles: true,
        cancelable: true
    });

    // Reabrir el select
    selectElement.dispatchEvent(clickEvent);
}
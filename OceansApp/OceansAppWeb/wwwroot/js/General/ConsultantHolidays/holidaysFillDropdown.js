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

function fillHolidaysSelect(selectElement, firstOption) {
    let previousValue = selectElement.value;
    if (selectElement.length > 1) {
        return;
    }

    // Mostrar "Loading options" inmediatamente
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    selectElement.value = 'loading';

    getHolidaysList()
        .then(data => {
            const tempSelect = document.createElement('select');
            tempSelect.innerHTML = '<option value="">-' + firstOption + '-</option>';
            data.holidays.forEach(obj => {
                tempSelect.add(new Option(obj.text, obj.value));
            });

            // Reemplazar el contenido del select original con el nuevo
            selectElement.innerHTML = tempSelect.innerHTML;
            selectElement.value = previousValue;

            // Forzar reapertura del select
            setTimeout(() => {
                // Crear y disparar un evento 'click' personalizado para forzar la reapertura
                selectElement.focus();
                selectElement.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true, view: window }));
            }, 100); // Añadir un pequeño retraso para asegurar que el DOM se haya actualizado
        })
        .catch(error => {
            console.error('Error fetching holidays:', error);
        });
}
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

    // Añadir la opción de "Loading..."
    const loadingOption = new Option("Loading options… (⏳)", "loading", true, true);
    selectElement.add(loadingOption);

    getHolidaysList()
        .then(data => {
            // Crear un documento fragmento para añadir las opciones
            const fragment = document.createDocumentFragment();

            // Añadir la primera opción
            fragment.appendChild(new Option('-' + firstOption + '-', '', true, true));

            // Añadir las opciones obtenidas del backend
            data.holidays.forEach(obj => {
                fragment.appendChild(new Option(obj.text, obj.value));
            });

            // Limpiar el select y añadir las nuevas opciones
            selectElement.innerHTML = '';
            selectElement.appendChild(fragment);
            selectElement.value = previousValue;

            // Forzar la actualización del DOM y abrir el menú desplegable
            openSelectMenu(selectElement);
        })
        .catch(error => {
            console.error('Error fetching holidays:', error);
        });
}

function openSelectMenu(selectElement) {
    // Guardar el índice seleccionado actual
    const selectedIndex = selectElement.selectedIndex;

    // Crear y disparar un evento de clic para abrir el menú
    const event = new MouseEvent('mousedown', {
        view: window,
        bubbles: true,
        cancelable: true,
        clientX: 0,
        clientY: 0
    });
    selectElement.dispatchEvent(event);

    // Restaurar el índice seleccionado después de abrir el menú
    selectElement.selectedIndex = selectedIndex;
}




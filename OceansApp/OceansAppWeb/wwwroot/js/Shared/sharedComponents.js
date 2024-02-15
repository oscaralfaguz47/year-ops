function displayMenuListFromMenuIcon(menuIconsElementId, menuIconId) {
    // Ocultar todos los menús excepto el actual
    document.querySelectorAll('.menu-options.show').forEach(function (menu) {
        if (menu.id !== menuIconsElementId) {
            menu.classList.remove('show');
        }
    });

    var menu = document.getElementById(menuIconsElementId);
    menu.classList.toggle('show');
    event.stopPropagation();

    // Asegurar que el clic en una opción del menú también oculte el menú
    attachHideMenuEvent(menu);
}

function attachHideMenuEvent(menu) {
    menu.querySelectorAll('li').forEach(function (menuItem) {
        menuItem.addEventListener('click', function () {
            menu.classList.remove('show');
            // Opcional: Detener la propagación para evitar que se active cualquier otro manejador de eventos 'click'
            // event.stopPropagation();
        });
    });
}

document.addEventListener('click', function (event) {
    var menus = document.querySelectorAll('.menu-options.show');
    menus.forEach(function (menu) {
        var iconId = menu.id.replace('menuOptions-', 'menuIcon-');
        var icon = document.getElementById(iconId);

        if (!menu.contains(event.target) && (!icon || !icon.contains(event.target))) {
            menu.classList.remove('show');
        }
    });
});

document.addEventListener('keydown', function (event) {
    if (event.key === "Escape") {
        var menus = document.querySelectorAll('.menu-options.show');
        menus.forEach(function (menu) {
            menu.classList.remove('show');
        });
    }
});

// Dropdown with checkboxes
document.addEventListener('DOMContentLoaded', function () {
    const options = ['Javascript', 'HTML', 'CSS', 'C#', 'PHP', 'Angular', '.Net Core', 'Golang'];
    const optionsContainer = document.getElementById('optionsContainer');
    const dropdownContent = document.querySelector('.dropdown-content');
    const selectedCount = document.getElementById('selectedCount');

    // Generar checkboxes
    options.forEach(option => {
        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        checkbox.id = option;
        checkbox.value = option;

        const label = document.createElement('label');
        label.htmlFor = option;
        label.appendChild(document.createTextNode(option));

        const div = document.createElement('div');
        div.appendChild(checkbox);
        div.appendChild(label);
        optionsContainer.appendChild(div);
    });

    // Mostrar/ocultar dropdown
    document.querySelector('.dropbtn').addEventListener('click', function (event) {
        dropdownContent.style.display = 'block';
        event.stopPropagation(); // Previene el cierre cuando se hace clic en el botón
    });

    // Cerrar dropdown al hacer clic fuera
    document.addEventListener('click', function () {
        dropdownContent.style.display = 'none';
    });

    // Prevenir el cierre del dropdown al hacer clic en el mismo
    dropdownContent.addEventListener('click', function (event) {
        event.stopPropagation();
    });

    // Cerrar dropdown con ESC
    document.addEventListener('keydown', function (e) {
        if (e.key === "Escape") {
            dropdownContent.style.display = 'none';
        }
    });

    // Filtrar opciones
    document.getElementById('filterInput').addEventListener('keyup', function (e) {
        const text = e.target.value.toLowerCase();
        const divs = optionsContainer.querySelectorAll('div');

        divs.forEach(div => {
            const label = div.querySelector('label');
            if (label.textContent.toLowerCase().indexOf(text) > -1) {
                div.style.display = '';
            } else {
                div.style.display = 'none';
            }
        });
    });

    // Escuchar cambios en los checkboxes y mostrar resultados seleccionados
    optionsContainer.addEventListener('change', function (e) {
        if (e.target.type === 'checkbox') {
            updateSelectedCount();
        }
    });

    function updateSelectedCount() {
        const checkboxes = optionsContainer.querySelectorAll('input[type="checkbox"]');
        const selectedOptions = [];

        checkboxes.forEach(checkbox => {
            if (checkbox.checked) {
                selectedOptions.push({ value: checkbox.value, selected: checkbox.checked });
            }
        });

        selectedCount.textContent = `Selected Positions: ${selectedOptions.length}`;

        // Mostrar las opciones seleccionadas en la consola como un array de objetos
        console.log("Opciones seleccionadas:", selectedOptions);
    }
});


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



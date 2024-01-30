function displayMenuListFromMenuIcon(menuIconsElementId, menuIconId) {
    var menu = document.getElementById(menuIconsElementId);
    menu.classList.toggle('show');
    event.stopPropagation();
    var menu = document.getElementById(menuIconsElementId);
    var icon = document.getElementById(menuIconId);

    if (!menu.contains(event.target) && !icon.contains(event.target)) {
        menu.classList.remove('show');
    }
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

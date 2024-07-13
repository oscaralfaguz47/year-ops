/* FILTERS SIDEBAR */
function openNav() {
    const sidebar = document.getElementById("filtersSidebar");
    sidebar.style.width = "300px";
    setTimeout(() => {
        document.addEventListener('click', outsideClickListener);
        document.addEventListener('keydown', escKeyListener);
    }, 100);
}

function closeNav() {
    const sidebar = document.getElementById("filtersSidebar");
    sidebar.style.width = "0";
    document.removeEventListener('click', outsideClickListener);
    document.removeEventListener('keydown', escKeyListener);
}

function outsideClickListener(event) {
    const sidebar = document.getElementById("filtersSidebar");
    const openBtn = document.getElementById("openSidebarBtn");
    if (!sidebar.contains(event.target) && event.target !== openBtn) {
        closeNav();
    }
}

function escKeyListener(event) {
    if (event.key === 'Escape') {
        closeNav();
    }
}
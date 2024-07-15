/* FILTERS SIDEBAR */
const sidebar = document.getElementById("right-sidebar");
function openRightSidebar() {
    sidebar.style.width = "300px";
    setTimeout(() => {
        document.addEventListener('click', outsideClickListener);
        document.addEventListener('keydown', escKeyListener);
    }, 100);
}

function closeRightSidebar() {
    sidebar.style.width = "0";
    document.removeEventListener('click', outsideClickListener);
    document.removeEventListener('keydown', escKeyListener);
}

function outsideClickListener(event) {
    const openBtn = document.getElementById("openSidebarBtn");
    if (!sidebar.contains(event.target) && event.target !== openBtn) {
        closeRightSidebar();
    }
}

function escKeyListener(event) {
    if (event.key === 'Escape') {
        closeRightSidebar();
    }
}
function resetFormElements(formId) {
    var createUpdateForm = $('#' + formId);
    createUpdateForm[0].reset();
}
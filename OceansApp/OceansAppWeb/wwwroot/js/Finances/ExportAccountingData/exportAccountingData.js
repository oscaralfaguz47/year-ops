let bookEntriesPartial = null;
let accountsPayablePartial = null;
document.addEventListener("DOMContentLoaded", function () {
    const tabs = document.querySelectorAll(".tab");
    const panels = document.querySelectorAll(".tab-panel");

    tabs.forEach(tab => {
        tab.addEventListener("click", function () {
            const target = this.dataset.tab;

            // Remover la clase 'active' de todos los tabs y paneles
            tabs.forEach(t => t.classList.remove("active"));
            panels.forEach(p => p.classList.remove("active"));

            // Agregar la clase 'active' al tab y panel seleccionados
            this.classList.add("active");
            document.getElementById(target).classList.add("active");
        });
    });
});

$(document).ready(function () {
   getJournalAccountsPayableList(true, false);
});
function paginationSubmit(firstTime, filters) {
    if (bookEntriesPartial === null) {
        getJournalAccountsPayableList(firstTime, filters);
    } else {
        getBookEntriesList(firstTime, filters);
    }
}

document.addEventListener("DOMContentLoaded", function () {
    setFinancesItemActive();
    const tabs = document.querySelectorAll(".tab");
    const panels = document.querySelectorAll(".tab-panel");

    tabs.forEach(tab => {
        tab.addEventListener("click", function () {
            const target = this.dataset.tab;

            tabs.forEach(t => t.classList.remove("active"));
            panels.forEach(p => p.classList.remove("active"));

            this.classList.add("active");
            document.getElementById(target).classList.add("active");
        });
    });
});

$(document).ready(function () {
   getJournalAccountsPayableList(true, false);
});

function cleanValue(value) {
    if (typeof value === 'string') {
        return value.trim();
    }
    return value;
}
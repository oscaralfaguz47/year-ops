function loadingISpinner() {
    return `<div class="widget-spinner-cont"><div class="spinner"></div>
            <div>
            </div></div>`;
}
function cardErrorInfo(text, onClick) {
    return `<div class="card-error">
   <div>
    <div><img src="/icons/Shared/warning.svg"></div>
    <span>${text}</span>
    <div><button onclick="${onClick}">Retry</button></div>
   </div>
    </div>`;
}
const dateDifferenceDiv = getElementById('timeInCompany');
const startDate = new Date(dateDifferenceDiv.getAttribute('data-start-date'));

const currentDate = new Date();

let years = currentDate.getFullYear() - startDate.getFullYear();
let months = currentDate.getMonth() - startDate.getMonth();
let days = currentDate.getDate() - startDate.getDate();

if (months < 0 || (months === 0 && days < 0)) {
    years--;
    months += 12;
}
if (days < 0) {
    const lastMonth = new Date(currentDate.getFullYear(), currentDate.getMonth(), 0);
    days += lastMonth.getDate();
    months--;
}

dateDifferenceDiv.innerHTML += `<p>You have been with us for <span>${years} years</span>, <span>${months} months</span>, and <span>${days} days</span>!</p>`;

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
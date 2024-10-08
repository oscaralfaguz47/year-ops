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

function isValidDate(dateString) {
    const regex = /^\d{4}-\d{2}-\d{2}$/;

    if (dateString.match(regex) === null) {
        return false;
    }

    const dateN = new Date(dateString);
    const timestamp = dateN.getTime();

    if (typeof timestamp !== 'number' || Number.isNaN(timestamp)) {
        return false;
    }

    return dateN.toISOString().startsWith(dateString);
}

function limitDigitsAndDecimals(inputElement) {
    let inputValue = inputElement.value;

    inputElement.addEventListener('keydown', function (e) {
        if (e.key === 'ArrowUp' || e.key === 'ArrowDown' || e.key === 'e' || e.key === 'E') {
            e.preventDefault();
        }
    });

    let validPattern = /^(\d{0,16})(\.\d{0,2})?$/;

    if (!validPattern.test(inputValue) && inputValue !== '') {
        inputElement.value = inputElement.dataset.lastValid || '';
    } else {
        inputElement.dataset.lastValid = inputValue;
    }
}


document.addEventListener('keydown', function (e) {
    if (e.target.classList.contains('integer-input')) {
        const key = e.key;
        const isNumberKey = /^\d$/.test(key);
        const allowedKeys = ['Backspace', 'ArrowLeft', 'ArrowRight', 'Delete', 'Tab'];

        if (!isNumberKey && !allowedKeys.includes(key)) {
            e.preventDefault();
        }
    }
});

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

function validateRequiredInput(element, message) {
    let isValid = true;

    // Check if the error message element exists; if not, create it
    let errorMessage = document.querySelector(`#${element.id}-error`);
    if (!errorMessage) {
        // Create error message element
        errorMessage = document.createElement('div');
        errorMessage.id = `${element.id}-error`;
        errorMessage.classList.add('error-message'); // Apply CSS class for styling

        // Set styles dynamically
        errorMessage.style.position = 'absolute';
        errorMessage.style.color = '#fa2615';
        errorMessage.style.fontSize = '12px';
        errorMessage.style.left = '0';        // Align to the left of the input
        errorMessage.style.whiteSpace = 'nowrap';

        // Append the error message after the input element
        element.parentNode.style.position = 'relative'; // Ensure parent is positioned relative for absolute positioning
        element.parentNode.appendChild(errorMessage);
    }

    // Hide previous error message
    errorMessage.style.display = 'none';

    // Validate based on the type of element
    if (element) {
        switch (element.type) {
            case 'checkbox':
            case 'radio':
                if (!element.checked) {
                    isValid = false;
                }
                break;
            case 'select-one':
            case 'select-multiple':
                if (!element.value || element.value === '') {
                    isValid = false;
                }
                break;
            default:
                if (!element.value.trim()) {
                    isValid = false;
                }
        }

        // Show error message if validation fails
        if (!isValid) {
            element.style.border = '2px solid #fa2615'; // Change border to red

            // Display the error message
            errorMessage.textContent = message;
            errorMessage.style.display = 'block'; // Show the error message
        } else {
            element.style.border = ''; // Reset border if valid
        }

        // Add event listener to clear validation when the user types or changes value
        element.addEventListener('input', clearValidation);
        element.addEventListener('change', clearValidation);
    } else {
        console.error('Element not found.');
        isValid = false;
    }

    return isValid; // Return true if valid, false otherwise
}

function clearValidation(event) {
    const element = event.target;

    // Remove red border and error message when the input is no longer empty
    if (element.value.trim() || (element.type === 'checkbox' || element.type === 'radio') && element.checked) {
        element.style.border = ''; // Reset border to default
        const errorMessage = document.querySelector(`#${element.id}-error`);
        if (errorMessage) {
            errorMessage.style.display = 'none'; // Hide the error message
        }

        // Remove event listeners after clearing the validation
        element.removeEventListener('input', clearValidation);
        element.removeEventListener('change', clearValidation);
    }
}

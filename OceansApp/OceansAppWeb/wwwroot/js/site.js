// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function displaySpinner() {
    const spinner = document.getElementById("loading-spinner");
    spinner.style.display = "block";
}
function hideSpinner() {
    const spinner = document.getElementById("loading-spinner");
    spinner.style.display = "none";
}
function redirectToCalculatorIndex() {
    location.href = '/Finances/Calculator';
}
function enableAuthenticatorStyles() {
    //var elem = document.getElementById("qrCode").getElementsByTagName('img');;
    elem.style.backgroundColor = 'red';
}

document.addEventListener("DOMContentLoaded", function () {
    const forms = document.querySelectorAll("form");

    forms.forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();

            if (form.checkValidity()) {
                const spinner = document.getElementById("loading-spinner");
                spinner.style.display = "block";
                form.submit();
            } else {
                // Si deseas mostrar un mensaje personalizado cuando el formulario no es válido, puedes hacerlo aquí
            }
        });
    });
});

// COPY TEXT TO CLIPBOARD
function copyToClipboard(text, messageSuccess) {
    // Create a temporary input element
    const tempInput = document.createElement('textarea');
    tempInput.value = text;
    document.body.appendChild(tempInput);

    // Select the text inside the input element
    tempInput.select();

    try {
        // Execute the "copy" command
        document.execCommand('copy');
        displayToasterSuccess(messageSuccess);
    } catch (err) {
        console.error('Unable to copy text to clipboard:', err);
        toastr.error('Algo salió mal, reporta este issue a soporte.');
    } finally {
        // Remove the temporary input element
        document.body.removeChild(tempInput);
    }
}

function displayToasterSuccess(text) {
    toastr.success(text);
}
function displayToasterError(text) {
    toastr.error(text);
}
function displayToasterErrorArray(errorsArray) {
    errorsArray.forEach(function (error) {
        toastr.error(error);
    });
}
function displayToasterInformation(message){
    toastr.info(message);
}
function displayToasterWarningArray(messagesArray) {
    messagesArray.forEach(function (message) {
        toastr.warning(message);
    });
}
function displayToasterWarning(message) {
    toastr.warning(message);
}

//FILTERS
function hideShowFilters() {
    var moreFiltersDiv = document.getElementById("more-filters");
    var hideShowFiltersBtn = document.getElementById("show-hide-filters-btn");
    if (moreFiltersDiv.classList.contains('hide')) {
        moreFiltersDiv.classList.remove('hide')
        moreFiltersDiv.classList.add('show')
        hideShowFiltersBtn.innerHTML = '<i class="bi bi-funnel-fill"></i> Hide Filters';
    } else {
        moreFiltersDiv.classList.add('hide')
        moreFiltersDiv.classList.remove('show')
        hideShowFiltersBtn.innerHTML = '<i class="bi bi-funnel"></i> Show Filters';
    }
}

//HIDE MODALS
function hideModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.style.display = "none";
}
function showModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.style.display = "block";
    modal.scrollTop = 0;
}

function waitingForPostMethod() {
    var spinnerSaving = $('#spinner-saving');
    spinnerSaving.show();
    var btnSaving = $('#btn-saving');
    btnSaving.prop('disabled', true);
    var btnCancel = document.getElementById('btn-cancel');
    btnCancel.onclick = null;
    btnCancel.style.cursor = 'not-allowed';
    var spanInsideBtnSaving = $('#btn-saving span');
    spanInsideBtnSaving.text('Wait...');
}
function inicializeModalButtons(modalId) {
    var spanInsideBtnSaving = $('#btn-saving span');
    spanInsideBtnSaving.text('Save');
    var spinnerSaving = $('#spinner-saving');
    spinnerSaving.hide();
    var btnSaving = $('#btn-saving');
    btnSaving.prop('disabled', false);
    var originalClickHandler = function () {
        hideModal(modalId);
    };
    var btnCancel = document.getElementById('btn-cancel');
    btnCancel.onclick = originalClickHandler;
    btnCancel.style.cursor = 'pointer';
}
function resetForm(formId) {
    var createUpdateForm = $('#' + formId);
    createUpdateForm[0].reset();
}

//FORMAT DATE
function formatDateWeekDayMonthDaySuffix(date) {
    var date = new Date(date);

    var months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    var monthName = months[date.getMonth()];

    var weekDays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    var weekDayName = weekDays[date.getDay()];

    var day = date.getDate();
    var daySuffix;

    if (day > 3 && day < 21) {
        daySuffix = 'th';
    } else {
        switch (day % 10) {
            case 1: daySuffix = "st"; break;
            case 2: daySuffix = "nd"; break;
            case 3: daySuffix = "rd"; break;
            default: daySuffix = "th";
        }
    }

    var formattedDate = weekDayName + ", " + monthName + " " + day + daySuffix;
    return formattedDate;
}
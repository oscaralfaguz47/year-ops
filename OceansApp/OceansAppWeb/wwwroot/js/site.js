// Utility functions
let getElementById = id => document.getElementById(id);
window.onload = function () {
    var currentYear = new Date().getFullYear();
    var copyrightYearElement = document.querySelector("#copyright-year");
    if (copyrightYearElement) {
        copyrightYearElement.innerHTML = currentYear;
    }
}

function copyToClipboard(inputId) {
    const inputElement = document.getElementById(inputId);

    if (inputElement) {
        const tempTextarea = document.createElement('textarea');
        tempTextarea.value = inputElement.value;
        document.body.appendChild(tempTextarea);

        // Select and copy the text
        tempTextarea.select();
        tempTextarea.setSelectionRange(0, 99999);
        document.execCommand('copy');

        document.body.removeChild(tempTextarea);

        displayToasterSuccess('Token copied to clipboard!');
    } else {
        alert('Element not found!');
    }
}



function validateSessionExpiration(message, statusCode) {
    if (statusCode === undefined || statusCode === 501) {
        if (message.toString().includes('Unexpected token')) {
            window.location.href = "/SessionEnded";
        }
    } else {
        if (statusCode !== undefined) {
            displayToasterError(message);
        }
    }
}
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
                
            }
        });
    });
});

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

    const btnXCloseModal = document.getElementById('close-modal-x-btn');
    if (btnXCloseModal) {
        btnXCloseModal.onclick = null;
        btnXCloseModal.style.cursor = 'not-allowed';
    }
}
function enableModalButtons(submitBtns, otherBtns, spinnersClass) {
    otherBtns.forEach(btnId => {
        let btnToEnable = document.getElementById(btnId);
        btnToEnable.disabled = false;
        btnToEnable.style.cursor = 'pointer';
    });

    submitBtns.forEach(submitBtn => {
        let btnToEnable = document.getElementById(submitBtn.id);
        btnToEnable.disabled = false;
        btnToEnable.style.cursor = 'pointer';
        const spanInsideBtnSaving = btnToEnable.querySelector('span');
        spanInsideBtnSaving.textContent = submitBtn.text === null || submitBtn.text === undefined ? 'Save' : submitBtn.text;
    });
    const spinnersSaving = document.querySelectorAll('.' + spinnersClass);
    spinnersSaving.forEach(function (element) {
        element.style.display = 'none';
    });
}
function disableButtonsWaitingForPostMethod(submitBtnId, otherBtns, spinnerClass) {
    otherBtns.forEach(btnId => {
        let btnToDisable = document.getElementById(btnId);
        btnToDisable.disabled = true;
        btnToDisable.style.cursor = 'not-allowed';
    });
    const submitBtn = document.getElementById(submitBtnId);
    submitBtn.disabled = true;

    const spinnerSaving = submitBtn.querySelectorAll('.' + spinnerClass);
    spinnerSaving.forEach(function (element) {
        element.style.display = 'block';
    });

    var spanInsideBtnSaving = submitBtn.querySelector('span');
    spanInsideBtnSaving.textContent = 'Wait...';
}
function inicializeModalButtons(modalId, confirmBtnText) {
    const spanInsideBtnSaving = $('#btn-saving span');
    spanInsideBtnSaving.text(confirmBtnText === null || confirmBtnText === undefined ? 'Save' : confirmBtnText);
    const spinnerSaving = $('#spinner-saving');
    spinnerSaving.hide();
    const btnSaving = $('#btn-saving');
    btnSaving.prop('disabled', false);
    let originalClickHandler = function () {
        hideModal(modalId);
    };
    const btnCancel = document.getElementById('btn-cancel');
    btnCancel.onclick = originalClickHandler;
    btnCancel.style.cursor = 'pointer';

    const btnXCloseModal = document.getElementById('close-modal-x-btn');
    if (btnXCloseModal) {
        btnXCloseModal.onclick = originalClickHandler;
        btnXCloseModal.style.cursor = 'pointer';
    }
}
function inicializeSecondModalButtons(modalId) {
    var spanInsideBtnSaving = $('#second-btn-saving span');
    spanInsideBtnSaving.text('Save');
    var spinnerSaving = $('#second-spinner-saving');
    spinnerSaving.hide();
    var btnSaving = $('#second-btn-saving');
    btnSaving.prop('disabled', false);
    var originalClickHandler = function () {
        hideModal(modalId);
    };
    var btnCancel = document.getElementById('second-btn-cancel');
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
function initializeTooltips() {
    const tooltipTargets = document.querySelectorAll('.tooltip-target');

    tooltipTargets.forEach(target => {
        target.removeEventListener('mouseenter', showTooltip);
        target.removeEventListener('mousemove', positionTooltip);
        target.removeEventListener('mouseleave', hideTooltip);

        target.addEventListener('mouseenter', showTooltip);
        target.addEventListener('mousemove', positionTooltip);
        target.addEventListener('mouseleave', hideTooltip);
    });

    function showTooltip(event) {
        const tooltipText = event.currentTarget.getAttribute('data-tooltip');
        let tooltip = document.querySelector('.tooltip');

        if (!tooltip) {
            tooltip = document.createElement('div');
            tooltip.classList.add('tooltip');
            document.body.appendChild(tooltip);
        }

        tooltip.innerHTML = tooltipText;
        tooltip.style.opacity = 1;
        tooltip.style.pointerEvents = 'auto';

        positionTooltip(event);
    }

    function positionTooltip(event) {
        const tooltip = document.querySelector('.tooltip');
        const offset = 2;

        let x = event.clientX + offset;
        let y = event.clientY + offset;

        const tooltipRect = tooltip.getBoundingClientRect();

        const isOutOfBounds = (
            x + tooltipRect.width > window.innerWidth ||
            y + tooltipRect.height > window.innerHeight ||
            x < 0 ||
            y < 0
        );

        if (isOutOfBounds) {
            x = (window.innerWidth - tooltipRect.width) / 2;
            y = (window.innerHeight - tooltipRect.height) / 2;
        }

        tooltip.style.left = `${x}px`;
        tooltip.style.top = `${y}px`;
    }

    function hideTooltip() {
        const tooltip = document.querySelector('.tooltip');
        if (tooltip) {
            tooltip.style.opacity = 0;
            tooltip.style.pointerEvents = 'none';
        }
    }
}


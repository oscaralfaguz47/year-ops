// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

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

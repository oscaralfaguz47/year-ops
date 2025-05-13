function displayCreateNewDocumentsModal(modalId){
    showModal(modalId);
}

function initGlobalToggles() {
    const toggles = document.querySelectorAll('.global-toggle-container');

    toggles.forEach(toggle => {
        const debitOption = toggle.querySelector('.global-toggle-debit');
        const creditOption = toggle.querySelector('.global-toggle-credit');

        toggle.addEventListener('click', () => {
            toggle.classList.toggle('active');

            if (toggle.classList.contains('active')) {
                debitOption.classList.remove('active');
                creditOption.classList.add('active');
            } else {
                creditOption.classList.remove('active');
                debitOption.classList.add('active');
            }
        });
    });
}

// Inicializa todos los toggles cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', initGlobalToggles);
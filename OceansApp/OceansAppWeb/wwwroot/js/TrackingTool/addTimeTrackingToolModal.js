let modalTitle = document.getElementById('create-update-time-modal-title');
function displayCreateUpdateTime(modalId, selectedDate, timeFrom, timeTo) {
    modalTitle.textContent = selectedDate;
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    showModal(modalId);
}

window.showProductClientCompanyAccountingConfigModal = function ({
    modalId,
    product,
    targetLine,
    onSave
}) {
    const modal = document.getElementById(modalId);
    const messageEl = modal.querySelector('.product-client-config-message');
    const taxInput = modal.querySelector('.product-client-config-tax-input');
    const hiddenProductIdInput = modal.querySelector('.product-client-config-hidden-product-id');
    const saveBtn = modal.querySelector('.product-client-config-save-btn');
    const cancelBtn = modal.querySelector('.product-client-config-cancel-btn');

    messageEl.textContent = `This client doesn't have an accounting config for ${product.productName}.`;
    taxInput.value = product.taxPercentage || 0;
    hiddenProductIdInput.value = product.productId;

    showModal(modalId);

    saveBtn.onclick = () => {
        const updatedTax = parseFloat(taxInput.value);
        hideModal(modalId);
        product.taxPercentage = updatedTax;

        if (typeof onSave === 'function') {
            onSave(product, targetLine);
        }
    };

    cancelBtn.onclick = () => {
        hideModal(modalId);

        if (targetLine) {
            const searchInput = targetLine.querySelector('.invoice-body-product-search');
            const resultsBox = targetLine.querySelector('.invoice-body-search-results');
            searchInput.value = '';
            resultsBox.innerHTML = '';
            resultsBox.style.display = 'none';
            targetLine.classList.remove('search-active');
        }
    };
};

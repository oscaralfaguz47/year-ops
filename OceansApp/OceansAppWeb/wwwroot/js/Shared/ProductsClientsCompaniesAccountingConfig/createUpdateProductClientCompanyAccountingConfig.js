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
    const cancelBtns = modal.querySelectorAll('.product-client-config-cancel-btn');

    messageEl.textContent = `This client doesn't have an accounting config for ${product.productName}.`;
    taxInput.value = product.taxPercentage || 0;
    hiddenProductIdInput.value = product.productId;

    showModal(modalId);

    // ✅ Dispatch custom event immediately after opening the modal
    const event = new CustomEvent('showProductClientCompanyAccountingConfigModal:open', {
        detail: { targetLine }
    });
    window.dispatchEvent(event);

    saveBtn.onclick = () => {
        const updatedTax = parseFloat(taxInput.value);
        hideModal(modalId);
        product.taxPercentage = updatedTax;

        if (typeof onSave === 'function') {
            onSave(product, targetLine);
        }
    };

    cancelBtns.forEach(btn => {
        btn.onclick = () => {
            hideModal(modalId);
        };
    });
};

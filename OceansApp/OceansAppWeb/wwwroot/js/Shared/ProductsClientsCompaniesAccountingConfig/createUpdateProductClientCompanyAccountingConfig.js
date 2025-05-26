window.showProductClientCompanyAccountingConfigModal = function ({
    modalId,
    product,
    clientId,
    clientName,
    targetLine,
    onSave
}) {
    const modal = document.getElementById(modalId);
    const messageEl = modal.querySelector('.product-client-config-message');
    const taxInput = modal.querySelector('.product-client-config-tax-input');
    const hiddenProductIdInput = modal.querySelector('.product-client-config-hidden-product-id');
    const saveBtn = modal.querySelector('.product-client-config-save-btn');
    const cancelBtns = modal.querySelectorAll('.product-client-config-cancel-btn');
    const searchClientContainerProdConfig = modal.querySelector('.search-input-container');
    const SelectedClientHiddenInputProdConfig = modal.querySelector('.selected-entity-id');
    const SelectedClientNameProdConfig = modal.querySelector('.selected-entity-display');

    taxInput.value = product.taxPercentage || 0;
    hiddenProductIdInput.value = product.productId;
    if (Number(clientId) > 0) {
        searchClientContainerProdConfig.style.display = 'none';
        SelectedClientHiddenInputProdConfig.value = clientId;
        SelectedClientNameProdConfig.value = clientName;
        SelectedClientNameProdConfig.style.display = 'block';
    } else {
        searchClientContainerProdConfig.style.display = 'block';
        SelectedClientNameProdConfig.style.display = 'none';
    }
    if (modalId === 'product-client-config-modal') {
        messageEl.textContent = `This client doesn't have an accounting config for product: '${product.productName}', please create it.`;
    } else {
        if (Number(clientId) > 0) {
            messageEl.textContent = `You are editing the Accounting Config for product: '${product.productName}'.`;
        } else {
            messageEl.textContent = `You are going to create a new Accounting Configuration.`;
        }
    }

    showModal(modalId);

    // ✅ Dispatch custom event immediately after opening the modal
    const event = new CustomEvent('showProductClientCompanyAccountingConfigModal:open', {
        detail: { targetLine }
    });
    window.dispatchEvent(event);

    saveBtn.onclick = () => {
        const updatedTax = parseFloat(taxInput.value);
        hideModal(modalId);
        console.log("ClientName: " + clientName);
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

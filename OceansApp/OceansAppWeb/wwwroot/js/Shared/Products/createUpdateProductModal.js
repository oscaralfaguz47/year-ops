window.showNewProductModal = function ({ modalId, title, name, alias, detail, onSaveCallback }) {
    const modal = document.getElementById(modalId);
    const titleEl = modal.querySelector('.new-product-modal-title');
    const nameInput = modal.querySelector('.new-product-name');
    const aliasInput = modal.querySelector('.new-product-alias');
    const detailInput = modal.querySelector('.new-product-detail');
    const saveBtn = modal.querySelector('.new-product-save-btn');
    const cancelBtn = modal.querySelector('.new-product-cancel-btn');

    titleEl.textContent = title || 'Create Product';
    nameInput.value = name || '';
    aliasInput.value = alias || '';
    detailInput.value = detail || '';

    modal.style.display = 'flex';

    saveBtn.onclick = async () => {
        const body = {
            ProductName: nameInput.value.trim(),
            Alias: aliasInput.value.trim(),
            Detail: detailInput.value.trim()
        };

        displaySpinner();
        var token = $('[name="__RequestVerificationToken"]').val();
        try {
            const response = await fetch('/AdminCenter/Products/CreateUpdateProduct', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    RequestVerificationToken: token
                },
                body: JSON.stringify(body)
            });

            const data = await response.json();

            modal.style.display = 'none';
            nameInput.value = '';
            aliasInput.value = '';
            detailInput.value = '';

            if (onSaveCallback) onSaveCallback(data);
            hideSpinner();
        } catch (err) {
            hideSpinner();
            validateSessionExpiration(err.message);
            console.error('Error creating product:', err);
        }
    };

    cancelBtn.onclick = () => {
        modal.style.display = 'none';
        nameInput.value = '';
        aliasInput.value = '';
        detailInput.value = '';
    };
};

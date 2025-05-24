function showValidationMessage(element) {
    element.style.display = 'block';
}

function hideValidationMessage(element) {
    element.style.display = 'none';
}

window.showNewProductModal = function ({ modalId, title, name, alias, detail, onSaveCallback }) {
    const modal = document.getElementById(modalId);
    const titleEl = modal.querySelector('.new-product-modal-title');
    const nameInput = modal.querySelector('.new-product-name');
    const aliasInput = modal.querySelector('.new-product-alias');
    const detailInput = modal.querySelector('.new-product-detail');
    const saveBtn = modal.querySelector('.new-product-save-btn');
    const cancelButtons = modal.querySelectorAll('.new-product-cancel-btn');

    const nameValidation = modal.querySelector('.product-name-validation');
    const aliasValidation = modal.querySelector('.product-alias-validation');

    titleEl.textContent = title || 'Create Product';
    nameInput.value = name || '';
    aliasInput.value = alias || '';
    detailInput.value = detail || '';

    hideValidationMessage(nameValidation);
    hideValidationMessage(aliasValidation);

    showModal(modalId);

    // Hide validation message on input
    nameInput.addEventListener('input', () => hideValidationMessage(nameValidation));
    aliasInput.addEventListener('input', () => hideValidationMessage(aliasValidation));

    saveBtn.onclick = async () => {
        const trimmedName = nameInput.value.trim();
        const trimmedAlias = aliasInput.value.trim();
        const trimmedDetail = detailInput.value.trim();

        let valid = true;

        if (!trimmedName) {
            showValidationMessage(nameValidation);
            valid = false;
        }

        if (!trimmedAlias) {
            showValidationMessage(aliasValidation);
            valid = false;
        }

        if (!valid) return;

        const body = {
            ProductName: trimmedName,
            Alias: trimmedAlias,
            Detail: trimmedDetail
        };

        displaySpinner();
        const token = $('[name="__RequestVerificationToken"]').val();

        try {
            const response = await fetch('/AdminCenter/Products/CreateUpdateProduct', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    RequestVerificationToken: token
                },
                body: JSON.stringify(body)
            });

            if (!response.ok) {
                const errorData = await response.json();

                if (errorData.messageType === "Validation Error") {
                    displayToasterWarningArray(errorData.errors);
                } else {
                    displayToasterError(errorData.error);
                }

                hideSpinner();
                throw new Error('Server returned a validation or general error');
            }

            const data = await response.json();

            hideModal(modalId);
            nameInput.value = '';
            aliasInput.value = '';
            detailInput.value = '';

            hideValidationMessage(nameValidation);
            hideValidationMessage(aliasValidation);

            if (onSaveCallback) onSaveCallback(data);
            hideSpinner();
        } catch (err) {
            hideSpinner();
            validateSessionExpiration(err.message);
            console.error('Error creating product:', err);
        }
    };

    cancelButtons.forEach(btn => {
        btn.onclick = () => {
            hideModal(modalId);
            nameInput.value = '';
            aliasInput.value = '';
            detailInput.value = '';

            hideValidationMessage(nameValidation);
            hideValidationMessage(aliasValidation);
        };
    });
};

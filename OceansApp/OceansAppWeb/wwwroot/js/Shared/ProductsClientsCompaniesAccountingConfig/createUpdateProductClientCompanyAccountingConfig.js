window.showProductClientCompanyAccountingConfigModal = async function ({
    modalId,
    product,
    clientId,
    clientName,
    targetLine,
    onSave,
    movementTypeId
}) {
    displaySpinner();
    const modal = document.getElementById(modalId);
    const messageEl = modal.querySelector('.product-client-config-message');
    const companySelectProdConfig = modal.querySelector('.companyId');
    const movementTypeSelectProdConfig = modal.querySelector('.movementTypeId');
    const costCenterSalesSelectProdConfig = modal.querySelector('.costCenterSalesId');
    const costCenterSalesReturnsSelectProdConfig = modal.querySelector('.costCenterSalesReturnId');
    const costCenterSalesDiscountsSelectProdConfig = modal.querySelector('.costCenterSalesDiscountId');
    const costCenterSalesTaxSelectProdConfig = modal.querySelector('.costCenterSalesTaxId');

    const accountingAccountSalesSelectProdConfig = modal.querySelector('.accountingAccountSalesId');
    const accountingAccountSalesReturnsSelectProdConfig = modal.querySelector('.accountingAccountSalesReturnId');
    const accountingAccountSalesDiscountsSelectProdConfig = modal.querySelector('.accountingAccountSalesDiscountId');
    const accountingAccountSalesTaxSelectProdConfig = modal.querySelector('.accountingAccountSalesTaxId');

    accountingAccountSalesSelectProdConfig.innerHTML = `<option value>-Select a Cost Center-<option>`;
    accountingAccountSalesReturnsSelectProdConfig.innerHTML = `<option value>-Select a Cost Center-<option>`;
    accountingAccountSalesDiscountsSelectProdConfig.innerHTML = `<option value>-Select a Cost Center-<option>`;
    accountingAccountSalesTaxSelectProdConfig.innerHTML = `<option value>-Select a Cost Center-<option>`;

    accountingAccountSalesSelectProdConfig.disabled = true;
    accountingAccountSalesReturnsSelectProdConfig.disabled = true;
    accountingAccountSalesDiscountsSelectProdConfig.disabled = true;
    accountingAccountSalesTaxSelectProdConfig.disabled = true;


    const taxInput = modal.querySelector('.product-client-config-tax-input');
    const hiddenProductIdInput = modal.querySelector('.product-client-config-hidden-product-id');
    const saveBtn = modal.querySelector('.product-client-config-save-btn');
    const cancelBtns = modal.querySelectorAll('.product-client-config-cancel-btn');
    const searchClientContainerProdConfig = modal.querySelector('.search-input-container');
    const SelectedClientHiddenInputProdConfig = modal.querySelector('.selected-entity-id');
    const SelectedClientNameProdConfig = modal.querySelector('.selected-entity-display');
    let costCentersArray = [];

    const clientCompany = await getCompanyIdByClient(clientId);

    var companiesList = await getCompaniesList();
    companySelectProdConfig.innerHTML = '';
    populateSelectByElement(companySelectProdConfig, companiesList.companies, '-Select a Company-', null);
    companySelectProdConfig.value = clientCompany.companyId;


    const movementTypesList = await getMovementTypesList();
    movementTypeSelectProdConfig.innerHTML = '';
    populateSelectByElement(movementTypeSelectProdConfig, movementTypesList.movementTypes, 'None', null);
    if (movementTypeId !== null) {
        movementTypeSelectProdConfig.value = movementTypeId;
        movementTypeSelectProdConfig.disabled = true;
    } else {
        movementTypeSelectProdConfig.disabled = false;
    }

    costCentersArray = await getCostsCentersWhereCompanyList(clientCompany.companyId);

    populateCostCentersSelects(costCenterSalesSelectProdConfig, costCentersArray.costsCenters);
    populateCostCentersSelects(costCenterSalesReturnsSelectProdConfig, costCentersArray.costsCenters);
    populateCostCentersSelects(costCenterSalesDiscountsSelectProdConfig, costCentersArray.costsCenters);
    populateCostCentersSelects(costCenterSalesTaxSelectProdConfig, costCentersArray.costsCenters);


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
        companySelectProdConfig.disabled = true;
    } else {
        messageEl.textContent = Number(clientId) > 0
            ? `You are editing the Accounting Config for product: '${product.productName}'.`
            : `You are going to create a new Accounting Configuration.`;
    }
    document.querySelectorAll('.validation-message').forEach(el => {
        el.style.display = 'none';
    });
    validateIfTaxIsGreaterThanCero();
    showModal(modalId);
    hideSpinner();

    const event = new CustomEvent('showProductClientCompanyAccountingConfigModal:open', {
        detail: { targetLine }
    });
    window.dispatchEvent(event);

    saveBtn.onclick = async () => {
        const isFormValid = validateRequiredFields();
        if (!isFormValid) return;

        const success = await createUpdateProductClientCompanyAccountingConfig('product-client-config-modal');
        if (success) {
            const updatedTax = parseFloat(taxInput.value);
            product.taxPercentage = updatedTax;
            if (typeof onSave === 'function') onSave(product, targetLine);
        }
    };


    taxInput.addEventListener('input', () => {
        validateIfTaxIsGreaterThanCero();
    });

    function validateIfTaxIsGreaterThanCero() {
        const taxValue = Number(taxInput.value);

        if (taxValue > 0) {
            costCenterSalesTaxSelectProdConfig.disabled = false;
        } else {
            costCenterSalesTaxSelectProdConfig.disabled = true;
            accountingAccountSalesTaxSelectProdConfig.disabled = true;

            // Reset selects
            costCenterSalesTaxSelectProdConfig.value = '';
            accountingAccountSalesTaxSelectProdConfig.value = '';
            accountingAccountSalesTaxSelectProdConfig.innerHTML = '<option value>-Select a Cost Center-</option>';
            populateCostCentersSelects(costCenterSalesTaxSelectProdConfig, costCentersArray.costsCenters);

            const costCenterMsg = costCenterSalesTaxSelectProdConfig.parentElement.parentElement.querySelector('.validation-message');
            const accountMsg = accountingAccountSalesTaxSelectProdConfig.parentElement.parentElement.querySelector('.validation-message');
            if (costCenterMsg) costCenterMsg.style.display = 'none';
            if (accountMsg) accountMsg.style.display = 'none';
        }
    }


    async function createUpdateProductClientCompanyAccountingConfig(modalId) {
        displaySpinner();
        const token = $('[name="__RequestVerificationToken"]').val();

        const body = {
            ProductId: product.productId,
            ClientId: clientId,
            MovementTypeId: movementTypeSelectProdConfig.value === 'null' ? null : Number(movementTypeSelectProdConfig.value),
            CostCenterIdSales: costCenterSalesSelectProdConfig.value === '' ? null : costCenterSalesSelectProdConfig.value,
            CostCenterIdSalesDiscount: costCenterSalesDiscountsSelectProdConfig.value === '' ? null : costCenterSalesDiscountsSelectProdConfig.value,
            CostCenterIdSalesReturn: costCenterSalesReturnsSelectProdConfig.value === '' ? null : costCenterSalesReturnsSelectProdConfig.value,
            CostCenterIdSalesTax: costCenterSalesTaxSelectProdConfig.value === '' ? null : costCenterSalesTaxSelectProdConfig.value,
            AccountingAccountIdSales: accountingAccountSalesSelectProdConfig.value === '' ? null : accountingAccountSalesSelectProdConfig.value,
            AccountingAccountIdSalesDiscount: accountingAccountSalesDiscountsSelectProdConfig.value === '' ? null : accountingAccountSalesDiscountsSelectProdConfig.value,
            AccountingAccountIdSalesReturn: accountingAccountSalesReturnsSelectProdConfig.value === '' ? null : accountingAccountSalesReturnsSelectProdConfig.value,
            AccountingAccountIdSalesTax: accountingAccountSalesTaxSelectProdConfig.value === '' ? null : accountingAccountSalesTaxSelectProdConfig.value,
            TaxPercentage: taxInput.value === '' ? null : Number(taxInput.value),
            IsCreating: true
        };
        console.log(body);
        try {
            const response = await fetch('/AdminCenter/ProductsClientsCompaniesAccountingConfigForBilling/CreateUpdateProductClientCompanyAccountingConfig', {
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
                return false;
            }

            const data = await response.json();

            hideModal(modalId);
            toastr.success(data.message);
            hideSpinner();

            return true;
        } catch (err) {
            hideSpinner();
            validateSessionExpiration(err.message);
            console.error('Error creating product:', err);
            return false;
        }
    }


    cancelBtns.forEach(btn => {
        btn.onclick = () => {
            hideModal(modalId);
        };
    });

    costCenterSalesSelectProdConfig.onchange = () => {
        selectCostCenter(costCenterSalesSelectProdConfig.value, accountingAccountSalesSelectProdConfig);
    };
    costCenterSalesReturnsSelectProdConfig.onchange = () => {
        selectCostCenter(costCenterSalesReturnsSelectProdConfig.value, accountingAccountSalesReturnsSelectProdConfig);
    };
    costCenterSalesDiscountsSelectProdConfig.onchange = () => {
        selectCostCenter(costCenterSalesDiscountsSelectProdConfig.value, accountingAccountSalesDiscountsSelectProdConfig);
    };
    costCenterSalesTaxSelectProdConfig.onchange = () => {
        selectCostCenter(costCenterSalesTaxSelectProdConfig.value, accountingAccountSalesTaxSelectProdConfig);
    };


    function populateCostCentersSelects(selectElement, costCentersList) {
        selectElement.innerHTML = '<option value="">-Select a Cost Center-</option>';

        costCentersList.forEach(obj => {
            const costCenterCode = obj.acceptData === 'S' ? `(${obj.costCenterCode})` : '';
            const selectValue = obj.acceptData === 'S' ? obj.costCenterId : null;
            const option = new Option(`${obj.description} ${costCenterCode}`, selectValue);

            if (obj.acceptData === 'N') {
                option.className = 'option-no-accept-data';
                option.disabled = true;
            }

            selectElement.add(option);
        });
    }
    async function selectCostCenter(selectedValue, accountingAccountSelect) {
        if (!selectedValue || selectedValue === 'null') {
            accountingAccountSelect.innerHTML = '<option value="">-Select a Cost Center-</option>';
            accountingAccountSelect.disabled = true;
            return;
        }
        try {
            displaySpinner();
            accountingAccountSelect.innerHTML = '<option value="">Loading options… (⏳)</option>';
            const data = await getAccountingAccountsWhereCostCenterList(selectedValue);

            accountingAccountSelect.innerHTML = '<option value="">-Select an Account-</option>';
            accountingAccountSelect.disabled = false;

            data.accountingAccounts.forEach(obj => {
                const accountCode = obj.acceptData === 'S' ? `(${obj.accountingAccountCode})` : '';
                const selectValue = obj.acceptData === 'S' ? obj.accountingAccountId : null;
                const option = new Option(`${obj.description} ${accountCode}`, selectValue);

                if (obj.acceptData === 'N') {
                    option.className = 'option-no-accept-data';
                    option.disabled = true;
                }

                accountingAccountSelect.add(option);
            });

        } catch (error) {
            console.error('Error loading accounting accounts:', error);
        } finally {
            hideSpinner();
        }
    }

    //Validation messages
    [
        costCenterSalesSelectProdConfig,
        accountingAccountSalesSelectProdConfig,
        costCenterSalesReturnsSelectProdConfig,
        accountingAccountSalesReturnsSelectProdConfig,
        costCenterSalesDiscountsSelectProdConfig,
        accountingAccountSalesDiscountsSelectProdConfig,
        costCenterSalesTaxSelectProdConfig,
        accountingAccountSalesTaxSelectProdConfig
    ].forEach(select => {
        select.addEventListener('change', () => {
            const msg = select.parentElement.parentElement.querySelector('.validation-message');
            if (select.value) msg.style.display = 'none';
        });
    });
    function validateRequiredFields() {
        let isValid = true;

        function validateField(selectElement) {
            const messageEl = selectElement.parentElement.parentElement.querySelector('.validation-message');
            if (!selectElement.value || selectElement.value === 'null') {
                messageEl.style.display = 'block';
                isValid = false;
            } else {
                messageEl.style.display = 'none';
            }
        }

        validateField(costCenterSalesSelectProdConfig);
        validateField(accountingAccountSalesSelectProdConfig);
        validateField(costCenterSalesReturnsSelectProdConfig);
        validateField(accountingAccountSalesReturnsSelectProdConfig);
        validateField(costCenterSalesDiscountsSelectProdConfig);
        validateField(accountingAccountSalesDiscountsSelectProdConfig);

        if (Number(taxInput.value) > 0) {
            validateField(costCenterSalesTaxSelectProdConfig);
            validateField(accountingAccountSalesTaxSelectProdConfig);
        }

        return isValid;
    }

};

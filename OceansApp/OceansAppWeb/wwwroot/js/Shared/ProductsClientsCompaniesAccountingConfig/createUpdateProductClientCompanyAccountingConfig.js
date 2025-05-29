window.showProductClientCompanyAccountingConfigModal = async function ({
    modalId,
    product,
    clientId,
    clientName,
    targetLine,
    onSave
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

    accountingAccountSalesSelectProdConfig.innerHTML = `<option>-Select a Cost Center-<option>`;
    accountingAccountSalesReturnsSelectProdConfig.innerHTML = `<option>-Select a Cost Center-<option>`;
    accountingAccountSalesDiscountsSelectProdConfig.innerHTML = `<option>-Select a Cost Center-<option>`;
    accountingAccountSalesTaxSelectProdConfig.innerHTML = `<option>-Select a Cost Center-<option>`;

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

    showModal(modalId);
    hideSpinner();

    const event = new CustomEvent('showProductClientCompanyAccountingConfigModal:open', {
        detail: { targetLine }
    });
    window.dispatchEvent(event);

    saveBtn.onclick = () => {
        const updatedTax = parseFloat(taxInput.value);
        hideModal(modalId);
        product.taxPercentage = updatedTax;
        if (typeof onSave === 'function') onSave(product, targetLine);
    };

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
};

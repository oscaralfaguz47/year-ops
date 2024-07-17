let createUpdateForm = $('#form-create-update');
let positionIdInput = createUpdateForm.find('[name="positionId"]');
let positionNameInput = createUpdateForm.find('[name="positionName"]');
let companiesArray = [];
let costCentersByCompanyArray = [];


//DISPLAY CREATE / UPDATE POSITION
async function displayUpdateCreatePositionModal(modalId, id, isCloning) {
    let modalTitle = document.getElementById('create-position-modal-title');
    id === null && !isCloning ? modalTitle.textContent = 'CREATE NEW POSITION' : id !== null && !isCloning
        ? modalTitle.textContent = 'EDIT POSITION' : modalTitle.textContent = 'CLONE POSITION';
    inicializeModalButtons(modalId);
    resetForm('form-create-update');
    positionIdInput.val("");
    const formElements = document.getElementById('form-elements');
    formElements.innerHTML = '';

    let url = id !== null ? `/AdminCenter/ConsultantPositions/GetPositionDataById?positionId=${encodeURIComponent(id)}`
        : `/AdminCenter/ConsultantPositions/GetPositionDataById`;

    displaySpinner();
    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError("Something went wrong: " + errorData.error);
            hideModal(modalId);
            getListOfResults(false, false);
            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
        }
        const data = await response.json();

        if (!isCloning) {
            positionIdInput.val(id);
        }
        positionNameInput.val(data.positionConfigData.positionName === null ? '' : data.positionConfigData.positionName + (isCloning ? ' - (CLONE)' : ''));
        let currentCompanyName = "";

        if (id !== null) {
            document.querySelector(`input[name="positionTypeCb"][value="${data.positionConfigData.isAdministrative}"]`).checked = true;
        }
        if (companiesArray.length === 0) {
            let companyId = '';
            data.positionConfigData.positionConfiguration.forEach(function (item) {
                if (companyId !== item.companyId) {
                    companiesArray.push(item.companyId);
                }
                companyId = item.companyId;
            });
        }
        if (costCentersByCompanyArray.length === 0) {
            for (const companyItem of companiesArray) {
                const costCentersByCompany = await getCostsCentersWhereCompanyList(companyItem);
                for (const costCenter of costCentersByCompany.costsCenters) {
                    costCentersByCompanyArray.push({
                        companyId: companyItem,
                        costCenterId: costCenter.costCenterId,
                        costCenterCode: costCenter.costCenterCode,
                        description: costCenter.description,
                        acceptData: costCenter.acceptData
                    });
                }
            }
        }
        
        for (const item of data.positionConfigData.positionConfiguration) {
            if (item.companyName !== currentCompanyName) {
                currentCompanyName = item.companyName;

                const companyLabel = document.createElement('label');
                companyLabel.textContent = `${currentCompanyName}`;
                companyLabel.className = 'company-label';
                formElements.appendChild(companyLabel);
                formElements.appendChild(document.createElement('br'));
            }

            const row = document.createElement('div');
            row.className = 'movement-row';

            const movementLabel = document.createElement('span');
            movementLabel.className = 'movement-label';
            movementLabel.textContent = `${item.movementTypeName}`;
            row.appendChild(movementLabel);

            // Hidden inputs
            const idConfigInput = document.createElement('input');
            idConfigInput.type = 'hidden';
            idConfigInput.className = 'idConfigInput';
            idConfigInput.value = item.id;

            const companyInput = document.createElement('input');
            companyInput.type = 'hidden';
            companyInput.className = 'companyInput';
            companyInput.value = item.companyId;

            const movementTypeInput = document.createElement('input');
            movementTypeInput.type = 'hidden';
            movementTypeInput.className = 'movementTypeInput';
            movementTypeInput.value = item.movementTypeId;



            const costCenterSelect = document.createElement('select');
            costCenterSelect.className = 'form-select position-selects costCenterInput';
            const costCenterOption = document.createElement('option');
            costCenterOption.value = item.costCenterId;
            costCenterOption.textContent = item.costCenterName;

            fillCostCentersSelect(costCenterSelect, costCentersByCompanyArray.filter(x => x.companyId === item.companyId));

            row.appendChild(idConfigInput);
            row.appendChild(companyInput);
            row.appendChild(movementTypeInput);
            row.appendChild(costCenterSelect);

            const accountingAccountSelect = document.createElement('select');
            accountingAccountSelect.className = 'form-select position-selects accountingAccountInput';
            const accountingAccountOption = document.createElement('option');
            accountingAccountOption.value = item.accountingAccountId;
            accountingAccountOption.textContent = item.accountingAccountName;
            row.appendChild(accountingAccountSelect);

            if (id === null) {
                accountingAccountSelect.disabled = true;
                const accountingAccountOptionByDefault = document.createElement('option');
                accountingAccountOptionByDefault.textContent = '-First Select a Cost Center-';
                accountingAccountOptionByDefault.value = '';
                accountingAccountSelect.value = '';
                accountingAccountSelect.appendChild(accountingAccountOptionByDefault);
            } else {
                costCenterSelect.value = item.costCenterId;
                if (item.costCenterId !== null) {
                    await fillAccountingAccountsSelect(accountingAccountSelect, Number(costCenterSelect.value));
                    accountingAccountSelect.value = item.accountingAccountId;
                }
            }

            costCenterSelect.addEventListener('change', function () {
                fillAccountingAccountsSelect(accountingAccountSelect, Number(costCenterSelect.value));
            });

            formElements.appendChild(row);
        }
        showModal(modalId);
    } catch (error) {
        validateSessionExpiration(error.message);
    } finally {
        hideSpinner();
    }
}


function fillCostCentersSelect(selectElement, data) {
    if (selectElement.length > 1) {
        return;
    }
    selectElement.innerHTML = '<option value="">-Select a cost center-</option>';
    for (const item of data) {
        var costCenterCode = '';
        var selectValue = null;
        item.acceptData === 'S' ? costCenterCode = '(' + item.costCenterCode + ')' : costCenterCode = '';
        item.acceptData === 'S' ? selectValue = item.costCenterId : selectValue = null;
        var option = new Option(item.description + ' ' + costCenterCode, selectValue);
        if (item.acceptData === 'N') {
            option.className = 'option-no-accept-data';
            option.disabled = true;
        }
        selectElement.add(option);
    }
}
async function fillAccountingAccountsSelect(selectElement, selectedValue) {
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    const data = await getAccountingAccountsWhereCostCenterList(selectedValue);
    selectElement.innerHTML = '';
    selectElement.innerHTML = '<option value="">-Select an Account-</option>';
    selectElement.disabled = false;
    data.accountingAccounts.forEach(obj => {
        var accountCode = '';
        var selectValue = null;
        obj.acceptData === 'S' ? accountCode = '(' + obj.accountingAccountCode + ')' : accountCode = '';
        obj.acceptData === 'S' ? selectValue = obj.accountingAccountId : selectValue = null;
        var option = new Option(obj.description + ' ' + accountCode, selectValue);
        if (obj.acceptData === 'N') {
            option.className = 'option-no-accept-data';
            option.disabled = true;
        }
        selectElement.add(option);
    });
}

// CREATE - UPDATE POSITION POST METHOD
async function createUpdatePosition(modalId) {
    waitingForPostMethod();
    let token = $('[name="__RequestVerificationToken"]').val();
    let positionConfigElements = document.querySelectorAll(".movement-row");
    let positionId = createUpdateForm.find('[name="positionId"]').val() === '' ? null : Number(createUpdateForm.find('[name="positionId"]').val());
    let positionName = createUpdateForm.find('[name="positionName"]').val();
    let positionTypeElement = document.querySelector('input[name="positionTypeCb"]:checked');
    let positionType = positionTypeElement ? positionTypeElement.value === "true" : null;
    let positionConfigurationData = [];
    positionConfigurationData = Array.from(positionConfigElements).map(function (fila) {
        let configId = fila.querySelector(".idConfigInput").value === '' ? null : Number(fila.querySelector(".idConfigInput").value);
        let companyId = fila.querySelector(".companyInput").value;
        let costCenterId = fila.querySelector(".costCenterInput").value === '' ? null : Number(fila.querySelector(".costCenterInput").value);
        let accountingAccountId = fila.querySelector(".accountingAccountInput").value === '' ? null : Number(fila.querySelector(".accountingAccountInput").value);
        let movementTypeId = fila.querySelector(".movementTypeInput").value === '' ? null : Number(fila.querySelector(".movementTypeInput").value);
        return {
            Id: configId, CompanyId: companyId, CostCenterId: costCenterId, AccountingAccountId: accountingAccountId,
            MovementTypeId: movementTypeId
        };
    });
    let data = {
        PositionId: positionId,
        PositionName: positionName,
        IsAdministrative: positionType,
        PositionConfiguration: positionConfigurationData
    };
    try {
        const response = await fetch('/AdminCenter/ConsultantPositions/CreateUpdateConsultantPosition', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: token
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
                        return acc.concat(current);
                    }, []);
                    displayToasterWarningArray(allErrors);
                    break;
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            inicializeModalButtons(modalId);
            return null;
        }

        const dataFromApi = await response.json();
        hideModal(modalId);
        displayToasterSuccess(dataFromApi.message);
        getListOfResults(false, false);
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        inicializeModalButtons(modalId);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        return null; // Return null to signify an error that prevented a successful fetch
    }
}
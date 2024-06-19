let createUpdateForm = $('#form-create-update');
let positionIdInput = createUpdateForm.find('[name="positionId"]');
let positionNameInput = createUpdateForm.find('[name="positionName"]');

//DISPLAY CREATE / UPDATE POSITION
async function displayUpdateCreatePositionModal(modalId, id) {
    let modalTitle = document.getElementById('create-position-modal-title');
    id === null ? modalTitle.textContent = 'CREATE NEW POSITION' : modalTitle.textContent = 'EDIT POSITION';
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    positionIdInput.val("");
    const formElements = document.getElementById('form-elements');
    formElements.innerHTML = '';

    let url = id !== null ? `/AdminCenter/ConsultantPositions/GetPositionDataById?positionId=${encodeURIComponent(id)}` : `/AdminCenter/ConsultantPositions/GetPositionDataById`;

    displaySpinner();
    fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    displayToasterError("Something went wrong: " + errorData.error);
                    hideModal(modalId);
                    getListOfResults(false, false);
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .then(data => {
            positionIdInput.val(id);
            positionNameInput.val(data.positionConfigData.positionName);
            let currentCompanyName = "";
            showModal(modalId);
            data.positionConfigData.positionConfiguration.forEach(function(item) {
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

                //Hidden inputs
                const idConfigInput = document.createElement('input');
                idConfigInput.type = 'hidden';
                idConfigInput.className = 'idConfigInput';
                idConfigInput.value = item.id;

                const companyInput = document.createElement('input');
                companyInput.type = 'hidden';
                companyInput.className = 'companyInput';
                companyInput.value = item.companyId;

                const costCenterInput = document.createElement('input');
                costCenterInput.type = 'hidden';
                costCenterInput.className = 'costCenterInput';
                costCenterInput.value = item.costCenterId;

                const accountingAccountInput = document.createElement('input');
                accountingAccountInput.type = 'hidden';
                accountingAccountInput.className = 'accountingAccountInput';
                accountingAccountInput.value = item.accountingAccountId;

                const movementTypeInput = document.createElement('input');
                movementTypeInput.type = 'hidden';
                movementTypeInput.className = 'movementTypeInput';
                movementTypeInput.value = item.movementTypeId;

                const costCenterSelect = document.createElement('select');
                costCenterSelect.className = 'form-select position-selects';
                const costCenterOption = document.createElement('option');
                costCenterOption.value = item.costCenterId;
                costCenterOption.textContent = item.costCenterName;
                costCenterSelect.addEventListener('click', function () {
                    fillCostCentersSelect(this, item.companyId, id === null ? false : true);
                });
                row.appendChild(idConfigInput);
                row.appendChild(companyInput);
                row.appendChild(costCenterInput);
                row.appendChild(accountingAccountInput);
                row.appendChild(movementTypeInput);
                row.appendChild(costCenterSelect);

                const accountingAccountSelect = document.createElement('select');
                accountingAccountSelect.addEventListener('click', function () {
                    fillAccountingAccountsSelect(accountingAccountSelect, costCenterSelect.value, true, false);
                });
                accountingAccountSelect.className = 'form-select position-selects';
                const accountingAccountOption = document.createElement('option');
                accountingAccountOption.value = item.accountingAccountId;
                accountingAccountOption.textContent = item.accountingAccountName;
                row.appendChild(accountingAccountSelect);

                if (id === null) {
                    accountingAccountSelect.disabled = true;
                    const costCenterOptionByDefault = document.createElement('option');
                    costCenterOptionByDefault.textContent = '-Select a Cost Center-';
                    costCenterOptionByDefault.value = '';
                    costCenterSelect.value = '';
                    costCenterSelect.appendChild(costCenterOptionByDefault);
                    const accountingAccountOptionByDefault = document.createElement('option');
                    accountingAccountOptionByDefault.textContent = '-First Select a Cost Center-';
                    accountingAccountOptionByDefault.value = '';
                    accountingAccountSelect.value = '';
                    accountingAccountSelect.appendChild(accountingAccountOptionByDefault);
                } else {
                    costCenterSelect.appendChild(costCenterOption);
                    accountingAccountSelect.appendChild(accountingAccountOption);
                }
                function removeEmptyOptions(selectElement) {
                    for (let i = selectElement.options.length - 1; i >= 0; i--) {
                        if (selectElement.options[i].value === "" || selectElement.options[i].value === null) {
                            selectElement.remove(i);
                        }
                    }
                }
                costCenterSelect.addEventListener('change', function () {
                    removeEmptyOptions(costCenterSelect);
                    fillAccountingAccountsSelect(accountingAccountSelect, costCenterSelect.value, false, true);
                });

                formElements.appendChild(row);
            });
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            hideSpinner();
        });
}

function fillCostCentersSelect(selectElement, companyId, isEditing) {
    let previousValue = selectElement.value;
    if (selectElement.length > 1) {
        return;
    }
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    getCostsCentersWhereCompanyList(companyId)
        .then(data => {
            selectElement.innerHTML = '';
            selectElement.innerHTML = '<option value>-Select a Cost Center-</option>';
            selectElement.disabled = false;
            data.costsCenters.forEach(obj => {
                var costCenterCode = '';
                var selectValue = null;
                obj.acceptData === 'S' ? costCenterCode = '(' + obj.costCenterCode + ')' : costCenterCode = '';
                obj.acceptData === 'S' ? selectValue = obj.costCenterId : selectValue = null;
                var option = new Option(obj.description + ' ' + costCenterCode, selectValue);
                if (obj.acceptData === 'N') {
                    option.className = 'option-no-accept-data';
                    option.disabled = true;
                }
                selectElement.add(option);
            });
            if (isEditing) {
                selectElement.value = previousValue;
            }
        })
        .catch(error => {
            console.error('Error fetching:', error);
        });
}
function fillAccountingAccountsSelect(selectElement, selectedValue, isEditing, isFromCostCenterSelect) {
    let previousValue = selectElement.value;
    if (selectElement.length > 1 && isEditing) {
        return;
    }
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    getAccountingAccountsWhereCostCenterList(selectedValue)
        .then(data => {
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
            if (!isFromCostCenterSelect) {
                selectElement.value = previousValue;
            }
        })
        .catch(error => {
            console.error('Error fetching:', error);
        });
}

// CREATE - UPDATE POSITION POST METHOD
async function createUpdatePosition() {
    waitingForPostMethod();
    var token = $('[name="__RequestVerificationToken"]').val();
    var positionConfigElements = document.querySelectorAll(".movement-row");
    var positionName = createUpdateForm.find('[name="positionName"]').val();
    var positionConfigurationData = [];
    positionConfigurationData = Array.from(positionConfigElements).map(function (fila) {
        var configId = fila.querySelector(".idConfigInput").value;
        var companyId = fila.querySelector(".companyInput").value;
        var costCenterId = fila.querySelector(".costCenterInput").value;
        var accountingAccountId = fila.querySelector(".accountingAccountInput").value;
        var movementTypeId = fila.querySelector(".movementTypeInput").value;
        return {
            Id: configId, CompanyId: companyId, CostCenterId: costCenterId, AccountingAccountId: accountingAccountId,
            MovementTypeId: movementTypeId
        };
    });
    var data = {
        PositionName: positionName,
        PositionConfiguration: positionConfigurationData
    };

    //try {
    //    const response = await fetch('/TrackingTool/ReportingMyTime/CreateUpdateTimeEntryTrackingTool', {
    //        method: 'POST',
    //        headers: {
    //            'Content-Type': 'application/json',
    //            RequestVerificationToken: token
    //        },
    //        body: JSON.stringify(data)
    //    });

    //    if (!response.ok) {
    //        const errorData = await response.json();
    //        switch (errorData.messageType) {
    //            case "Validation Error":
    //                const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
    //                    return acc.concat(current);
    //                }, []);
    //                displayToasterWarningArray(allErrors);
    //                break;
    //            case "Not Found":
    //                displayToasterError('Resource not found: ' + errorData.detail);
    //                break;
    //            default:
    //                button.style.display = 'block';
    //                displayToasterError('An unexpected error occurred: ' + errorData.error);
    //        }
    //        spinnerLabel.style.display = 'none';
    //        checkSavedIcon.style.display = 'none';
    //        button.style.display = 'block';
    //        return null;
    //    }

    //    const dataFromApi = await response.json();
    //    movementIdInput.value = dataFromApi.movementId;
    //    spinnerLabel.style.display = 'none';
    //    checkSavedIcon.style.display = 'block';
    //    return dataFromApi;
    //} catch (err) {
    //    validateSessionExpiration(err.message);
    //    console.error('Network or fetch error:', err);
    //    displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
    //    spinnerLabel.style.display = 'none';
    //    checkSavedIcon.style.display = 'none';
    //    button.style.display = 'block';
    //    return null; // Return null to signify an error that prevented a successful fetch
    //}
}
let createUpdateForm = $('#form-create-update');
//CREATE / UPDATE POSITION
async function displayUpdateCreatePositionModal(modalId, id) {
    let modalTitle = document.getElementById('create-position-modal-title');
    modalTitle.textContent = "CREATE NEW POSITION";
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    createUpdateForm.find('[name="positionId"]').val("");
    const formElements = document.getElementById('form-elements');
    formElements.innerHTML = '';

    if (id !== null) {
        modalTitle.textContent = "EDIT POSITION";
        createUpdateForm.find('[name="positionId"]').val(id);
    }
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
            console.log(data);
            let currentCompanyName = "";
            showModal(modalId);
            data.positionConfigData.positionConfiguration.forEach(function(item) {
                if (item.companyName !== currentCompanyName) {
                    currentCompanyName = item.companyName;

                    const companyLabel = document.createElement('label');
                    companyLabel.textContent = `${currentCompanyName}`;
                    formElements.appendChild(companyLabel);
                    formElements.appendChild(document.createElement('br'));
                }

                const row = document.createElement('div');
                row.style.display = 'flex';
                row.style.marginBottom = '10px';

                const movementLabel = document.createElement('label');
                movementLabel.textContent = `${item.movementTypeName}`;
                row.appendChild(movementLabel);

                const costCenterSelect = document.createElement('select');
                costCenterSelect.className = 'form-select';
                const costCenterOption = document.createElement('option');
                costCenterOption.value = item.costCenterId;
                costCenterOption.textContent = item.costCenterName;
                costCenterSelect.appendChild(costCenterOption);
                row.appendChild(costCenterSelect);

                const accountingAccountSelect = document.createElement('select');
                accountingAccountSelect.className = 'form-select';
                const accountingAccountOption = document.createElement('option');
                accountingAccountOption.value = item.accountingAccountId;
                accountingAccountOption.textContent = item.accountingAccountName;
                accountingAccountSelect.appendChild(accountingAccountOption);
                row.appendChild(accountingAccountSelect);

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
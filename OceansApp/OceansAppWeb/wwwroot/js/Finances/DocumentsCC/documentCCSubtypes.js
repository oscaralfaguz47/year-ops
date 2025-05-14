const cancelBtn = getElementById('cancel-btn-create-subtype');
const createSubtypeContainer = getElementById('create-subtype-container');
const createBtnContainer = getElementById('create-subtype-out-btn-cont');
const createBtn = createBtnContainer.querySelector('button');
let companiesArray = [];
let documentTypesArray = [];
const descriptionInputCreateSubtype = getElementById('description');
const companySelectCreateSubtype = getElementById('companyId');
const documentTypeSelectCreateSubtype = getElementById('documentTypeId');
const costCenterSelectCreateSubtype = getElementById('costCenterId');
const accountingAccountSelectCreateSubtype = getElementById('accountingAccountId');
const documentSubtypeIdInputCreateSubtype = getElementById('documentCCSubtypeId');
const subtypesListContainer = getElementById('subtypes-list-container');



//BTN Actions
cancelBtn.addEventListener('click', () => {
    hideCreateSubtypeSection();
});

function hideCreateSubtypeSection() {
    createSubtypeContainer.style.display = 'none';
    createBtnContainer.style.display = 'block';
    documentTypeSelectCreateSubtype.value = null;
    companySelectCreateSubtype.value = null;
    costCenterSelectCreateSubtype.innerHTML = '<option value>-Select a Company-</option>';
    accountingAccountSelectCreateSubtype.innerHTML = '<option value>-Select a Cost Center-</option>';
    descriptionInputCreateSubtype.value = null;
    documentSubtypeIdInputCreateSubtype.value = null;
}

createBtn.addEventListener('click', async () => {
    await showCreateUpdateDocSubtypeForm();
});

async function showCreateUpdateDocSubtypeForm() {
    displaySpinner();
    documentSubtypeIdInputCreateSubtype.value = null;
    costCenterSelectCreateSubtype.disabled = true;
    accountingAccountSelectCreateSubtype.disabled = true;
    if (documentTypesArray.length === 0) {
        documentTypesArray = await getDocumentTypesList();
    }
    populateSelect('documentTypeId', documentTypesArray.documentTypes, '-Select a Doc Type-', null);

    if (companiesArray.length === 0) {
        companiesArray = await getCompaniesList();
    }

    populateSelect('companyId', companiesArray.companies, '-Select a company-', null);

    createSubtypeContainer.style.display = 'block';
    createBtnContainer.style.display = 'none';
    documentTypeSelectCreateSubtype.disabled = false;
    companySelectCreateSubtype.disabled = false;

    hideSpinner();
}
async function displayUpdateDocumentSubtypeForm(id) {
    try {
        await showCreateUpdateDocSubtypeForm();
        displaySpinner();

        const url = `/Finances/DocumentCCSubtypes/GetDocumentSubtypeDataById?docSubtypeId=${encodeURIComponent(id)}`;
        const response = await fetch(url);

        // Validate server response
        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError(errorData.error || 'An unexpected error occurred.');
            throw new Error(`The request to the server failed! More details: ${errorData.detail || 'No details available.'}`);
        }

        // Process document data
        const data = await response.json();
        documentSubtypeIdInputCreateSubtype.value = id;
        descriptionInputCreateSubtype.value = data.documentSubtypeData.description;
        documentTypeSelectCreateSubtype.value = data.documentSubtypeData.documentTypeId;
        companySelectCreateSubtype.value = data.documentSubtypeData.companyId;

        // Wait for cost centers and accounting accounts to be loaded
        await selectCompany(data.documentSubtypeData.companyId);
        await selectCostCenter(data.documentSubtypeData.costCenterId);

        costCenterSelectCreateSubtype.value = data.documentSubtypeData.costCenterId;
        accountingAccountSelectCreateSubtype.value = data.documentSubtypeData.accountingAccountId;
        documentTypeSelectCreateSubtype.disabled = true;
        companySelectCreateSubtype.disabled = true;

    } catch (error) {
        validateSessionExpiration(error.message);
        console.error('Error fetching document subtype data:', error);
    } finally {
        hideSpinner();
    }
}

async function selectCompany(companyId) {
    try {
        displaySpinner();
        const costCenterList = await getCostsCentersWhereCompanyList(companyId);

        if (companyId !== null && companyId !== 'null' && companyId !== '') {
            costCenterSelectCreateSubtype.innerHTML = '<option value="">-Select a Cost Center-</option>';
            costCenterSelectCreateSubtype.disabled = false;

            costCenterList.costsCenters.forEach(obj => {
                const costCenterCode = obj.acceptData === 'S' ? `(${obj.costCenterCode})` : '';
                const selectValue = obj.acceptData === 'S' ? obj.costCenterId : null;
                const option = new Option(`${obj.description} ${costCenterCode}`, selectValue);

                if (obj.acceptData === 'N') {
                    option.className = 'option-no-accept-data';
                    option.disabled = true;
                }

                costCenterSelectCreateSubtype.add(option);
            });
        } else {
            costCenterSelectCreateSubtype.innerHTML = '<option value="">-Select a Company-</option>';
            costCenterSelectCreateSubtype.disabled = true;
        }

        accountingAccountSelectCreateSubtype.disabled = true;
        accountingAccountSelectCreateSubtype.innerHTML = '<option value="">-Select a Cost Center-</option>';

    } catch (error) {
        console.error('Error loading cost centers:', error);
    } finally {
        hideSpinner();
    }
}

async function selectCostCenter(selectedValue) {
    if (!selectedValue || selectedValue === 'null') {
        accountingAccountSelectCreateSubtype.innerHTML = '<option value="">-Select a Cost Center-</option>';
        accountingAccountSelectCreateSubtype.disabled = true;
        return;
    }
    try {
        displaySpinner();
        accountingAccountSelectCreateSubtype.innerHTML = '<option value="">Loading options… (⏳)</option>';
        const data = await getAccountingAccountsWhereCostCenterList(selectedValue);

        accountingAccountSelectCreateSubtype.innerHTML = '<option value="">-Select an Account-</option>';
        accountingAccountSelectCreateSubtype.disabled = false;

        data.accountingAccounts.forEach(obj => {
            const accountCode = obj.acceptData === 'S' ? `(${obj.accountingAccountCode})` : '';
            const selectValue = obj.acceptData === 'S' ? obj.accountingAccountId : null;
            const option = new Option(`${obj.description} ${accountCode}`, selectValue);

            if (obj.acceptData === 'N') {
                option.className = 'option-no-accept-data';
                option.disabled = true;
            }

            accountingAccountSelectCreateSubtype.add(option);
        });

    } catch (error) {
        console.error('Error loading accounting accounts:', error);
    } finally {
        hideSpinner();
    }
}


function displayDocumentSubtypesModal(modalId) {
    hideCreateSubtypeSection();
    getDocumentSubtypesList(modalId);
}

async function getDocumentSubtypesList(modalId) {
    displaySpinner();
    var url = "/Finances/DocumentCCSubtypes/GetDocumentCCSubtypesList";

    fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    displayToasterErrorArray(errorData.errors);
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .then(data => {
            showDocumentSubtypesInList(data);
            showModal(modalId);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            hideSpinner();
        });
}
function showDocumentSubtypesInList(subtypesListData) {
    var tbody = $("#subtypes-list-container .global-table-container table tbody");
    var tableRows = $("#subtypes-list-container .global-table-container table");
    var noResultsMessage = $("#subtypes-list-container .no-results");
    noResultsMessage.empty();
    tableRows.css("display", "block");
    tbody.empty();
    subtypesListData.forEach(function (docSubtype) {

        var editBtn = `<li onclick="displayUpdateDocumentSubtypeForm(${docSubtype.documentCCSubtypeId})""><i class="bi bi-pencil-square"></i> Edit</li>`;

        var menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${docSubtype.documentCCSubtypeId}', 'menuIcon-${docSubtype.documentCCSubtypeId}')" class="bi bi-three-dots-vertical" id="menuIcon-${docSubtype.documentCCSubtypeId}"></i>
                              <div class="menu-options" id="menuOptions-${docSubtype.documentCCSubtypeId}">
                               <ul>
                                 ${editBtn}
                               </ul>
                              </div>`;


        const row = `
        <tr class="hover-group">
            <td> ${menuBtn}
               ${docSubtype.description}
            </td>
            <td>${docSubtype.documentType}</td>
            <td>${docSubtype.costCenter}</td>
            <td>${docSubtype.accountingAccount}</td>
             <td>${docSubtype.company}</td>
        </tr>
    `;

        tbody.append(row);
    });

    if (subtypesListData.length === 0) {
        noResultsMessage.text("NO RECORDS FOUND");
        tableRows.css("display", "none");
    };
}
async function createUpdateDocumentSubtype() {
    displaySpinner();

    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        DocumentCCSubtypeId: documentSubtypeIdInputCreateSubtype.value === 'null' || documentSubtypeIdInputCreateSubtype.value === '' ? null : documentSubtypeIdInputCreateSubtype.value,
        DocumentTypeId: documentTypeSelectCreateSubtype.value === 'null' ? null : documentTypeSelectCreateSubtype.value,
        Description: descriptionInputCreateSubtype.value,
        CompanyId: companySelectCreateSubtype.value === 'null' ? null : companySelectCreateSubtype.value,
        CostCenterId: costCenterSelectCreateSubtype.value === 'null' || costCenterSelectCreateSubtype.value === '' ? null : costCenterSelectCreateSubtype.value,
        AccountingAccountId: accountingAccountSelectCreateSubtype.value === 'null' || accountingAccountSelectCreateSubtype.value === '' ? null : accountingAccountSelectCreateSubtype.value
    };
    console.log(data);
    try {
        const response = await fetch('/Finances/DocumentCCSubtypes/CreateUpdateDocumentSubtype', {
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
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }

        const dataFromApi = await response.json();
        displayToasterSuccess(dataFromApi.message);
        getDocumentSubtypesList('modal-document-subtypes');
        hideCreateSubtypeSection();
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        hideSpinner();
        return null;
    }
}

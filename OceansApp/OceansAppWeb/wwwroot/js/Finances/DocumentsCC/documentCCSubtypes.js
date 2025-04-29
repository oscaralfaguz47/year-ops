const cancelBtn = getElementById('cancel-btn-create-subtype');
const createSubtypeContainer = getElementById('create-subtype-container');
const createBtnContainer = getElementById('create-subtype-out-btn-cont');
const createBtn = createBtnContainer.querySelector('button');
let companiesArray = [];
const descriptionInputCreateSubtype = getElementById('description');
const documentTypeSelectCreateSubtype = getElementById('documentTypeId');
const costCenterSelectCreateSubtype = getElementById('costCenterId');
const accountingAccountSelectCreateSubtype = getElementById('accountingAccountId');


//BTN Actions
cancelBtn.addEventListener('click', () => {
    hideCreateSubtypeSection();
});

function hideCreateSubtypeSection() {
    createSubtypeContainer.style.display = 'none';
    createBtnContainer.style.display = 'block';
}

createBtn.addEventListener('click', async () => {
    displaySpinner();
    costCenterSelectCreateSubtype.disabled = true;
    costCenterSelectCreateSubtype.append(new Option('-Select a Company', null)); 
    accountingAccountSelectCreateSubtype.disabled = true;
    accountingAccountSelectCreateSubtype.append(new Option('-Select a Cost Center', null)); 
    if (companiesArray.length === 0) {
        companiesArray = await getCompaniesList();
    }

    populateSelect('companyId', companiesArray.companies, '-Select a company-', null);

    createSubtypeContainer.style.display = 'block';
    createBtnContainer.style.display = 'none';

    hideSpinner();
});



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
           
            console.log(data);
            showModal(modalId);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            hideSpinner();
        });
}

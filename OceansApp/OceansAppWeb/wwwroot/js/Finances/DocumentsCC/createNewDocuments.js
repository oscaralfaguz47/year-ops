const searchClientElementContainer = getElementById('search-client-element');
const headerContainerCreateDoc = document.querySelector('.create-doc-header-cont');
const moreFormElementsContainer = getElementById('more-form-elements-container');
const selectedClientInputCreateDoc = searchClientElementContainer.querySelector('.client-selected-input');
const transactionTypeInputCreateDoc = document.querySelector('#transaction-Type-toogle .global-toggle-hidden-input');
const documentNumberInputCreateDoc = getElementById('documentNumberCreateDoc');
const clientIdInputCreateDoc = searchClientElementContainer.querySelector('#search-client-element .clientId');
const documentTypeSelectCreateDoc = getElementById('documentTypeIdCreateDoc');
const subtypeSelectCreateDoc = getElementById('subTypeIdCreateDoc');




function resetToggle() {
    moreFormElementsContainer.style.display = 'none';
    headerContainerCreateDoc.style.display = 'none';
    const transactionTypeToggleContainer = getElementById('transaction-Type-toogle');
    const debitTobbleOpt = document.querySelector('#transaction-Type-toogle .global-toggle-opt1');
    const creditTobbleOpt = document.querySelector('#transaction-Type-toogle .global-toggle-opt2');
    transactionTypeToggleContainer.classList.remove('active');
    debitTobbleOpt.classList.add('active');
    creditTobbleOpt.classList.remove('active');
}
async function displayCreateNewDocumentsModal(modalId) {
    createDocForm.reset();
    resetToggle();
    initClientIdInputEventListener();
    selectedClientInputCreateDoc.style.display = 'none';
    clientIdInputCreateDoc.value = null;
    showModal(modalId);
}

//Load global toogle
document.addEventListener('DOMContentLoaded', initGlobalToggles);


document.addEventListener('DOMContentLoaded', () => {

    if (transactionTypeInputCreateDoc) {
        transactionTypeInputCreateDoc.addEventListener('input', async (event) => {
            //execute debit or credit changes
            await fillDocumentTypeDropdown(transactionTypeInputCreateDoc.value);
            if (Number(transactionTypeInputCreateDoc.value) === 2) {
                documentNumberInputCreateDoc.value = null;
                documentNumberInputCreateDoc.placeholder = 'Enter a Doc Number';
                documentNumberInputCreateDoc.disabled = false;
            } else {
                documentNumberInputCreateDoc.disabled = true;
                documentNumberInputCreateDoc.value = 'Select a Doc Type';
            }
        });
    }
});

async function initClientIdInputEventListener() {
    if (!clientIdInputCreateDoc) {
        console.error("clientIdInputCreateDoc not found");
        return;
    }

    // Only add the observer if it has not been added before
    if (!clientIdInputCreateDoc.hasAttribute('data-observer-initialized')) {
        const observer = new MutationObserver(async () => {
            try {
                if (clientIdInputCreateDoc.value !== '' && clientIdInputCreateDoc.value !== null) {
                    await handleClientSelection();
                }
            } catch (error) {
                console.error("Error handling client selection:", error);
            }
        });

        observer.observe(clientIdInputCreateDoc, {
            attributes: true,
            attributeFilter: ['value']
        });

        // Mark as initialized
        clientIdInputCreateDoc.setAttribute('data-observer-initialized', 'true');
    }
}


async function handleClientSelection() {
    try {
        let debitCredit = transactionTypeInputCreateDoc.value === null ? 1 : transactionTypeInputCreateDoc.value;

        await fillDocumentTypeDropdown(debitCredit);
        moreFormElementsContainer.style.display = 'block';
        headerContainerCreateDoc.style.display = 'flex';
    } catch (error) {
        console.error("Error in handleClientSelection:", error);
    }
}


async function fillDocumentTypeDropdown(transactionTypeId) {
    if (Number(transactionTypeId) === 1) {
        documentNumberInputCreateDoc.value = 'Select a Doc Type';
    }
    subtypeSelectCreateDoc.disabled = true;
    subtypeSelectCreateDoc.innerHTML = `<option>-Select a Doc Type-</option>`;
    try {
        displaySpinner();
        const documentTypesList = await getDocumentTypesListByTransactionType(transactionTypeId);

        documentTypeSelectCreateDoc.innerHTML = '';
        populateSelect('documentTypeIdCreateDoc', documentTypesList.documentTypes, '-Select a Doc Type-', null);

    } catch (error) {
        console.error('Error loading cost centers:', error);
    } finally {
        hideSpinner();
    }
}

documentTypeSelectCreateDoc.addEventListener('change', async (event) => {
    //execute debit or credit changes

    subtypeSelectCreateDoc.disabled = false;
});
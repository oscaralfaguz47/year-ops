const searchClientElementContainer = getElementById('search-client-element');
const moreFormElementsContainer = getElementById('more-form-elements-container');
const selectedClientInputCreateDoc = searchClientElementContainer.querySelector('.client-selected-input');
const transactionTypeInputCreateDoc = document.querySelector('#transaction-Type-toogle .global-toggle-hidden-input');
const documentNumberInputCreateDoc = getElementById('documentNumberCreateDoc');
const clientIdInputCreateDoc = searchClientElementContainer.querySelector('#search-client-element .clientId');
const documentTypeSelectCreateDoc = getElementById('documentTypeIdCreateDoc');




function resetToggle() {
    moreFormElementsContainer.style.display = 'none';
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
    await fillDocumentTypeDropdown(1);
    showModal(modalId);
}

//Load global toogle
document.addEventListener('DOMContentLoaded', initGlobalToggles);


document.addEventListener('DOMContentLoaded', () => {

    if (transactionTypeInputCreateDoc) {
        transactionTypeInputCreateDoc.addEventListener('input', async (event) => {
            //execute debit or credit changes

            await fillDocumentTypeDropdown(transactionTypeInputCreateDoc.value);
        });
    }
});

function initClientIdInputEventListener() {
    if (!clientIdInputCreateDoc) {
        console.error("clientIdInputCreateDoc not found");
        return;
    }

    // Only add the observer if it has not been added before
    if (!clientIdInputCreateDoc.hasAttribute('data-observer-initialized')) {
        const observer = new MutationObserver(() => {
            if (clientIdInputCreateDoc.value !== '' && clientIdInputCreateDoc.value !== null) {
                //Execute all the code after selecting the client

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



async function fillDocumentTypeDropdown(transactionTypeId) {
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
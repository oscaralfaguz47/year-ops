const searchClientElementContainer = getElementById('search-client-element');
const searchConsultantElementContainer = getElementById('search-consultant-element');
const headerContainerCreateDoc = document.querySelector('.create-doc-header-cont');
const moreFormElementsContainer = getElementById('more-form-elements-container');
const selectedClientInputCreateDoc = searchClientElementContainer.querySelector('.selected-entity-display');
const selectedConsultantInputCreateDoc = searchConsultantElementContainer.querySelector('.selected-entity-display');
const clientConsultantRBCreateDoc = document.querySelector('#client-consultant-toogle .global-toggle-hidden-input');
const transactionTypeInputCreateDoc = document.querySelector('#transaction-Type-toogle .global-toggle-hidden-input');
const documentNumberInputCreateDoc = getElementById('documentNumberCreateDoc');
const clientIdInputCreateDoc = searchClientElementContainer.querySelector('.selected-entity-id');
const consultantIdInputCreateDoc = searchConsultantElementContainer.querySelector('.selected-entity-id');
const documentTypeSelectCreateDoc = getElementById('documentTypeIdCreateDoc');
const subtypeSelectCreateDoc = getElementById('subTypeIdCreateDoc');
const descriptionInputCreateDoc = getElementById('applicationDescriptionCreateDoc');
const documentBody = document.querySelector('.document-body');
const descriptionContainerInputElement = getElementById('description-container');
const invoiceBodyContainer = getElementById('invoice-body-container');


function resetClientToggle() {
    moreFormElementsContainer.style.display = 'none';
    headerContainerCreateDoc.style.display = 'none';
    const transactionTypeToggleContainer = getElementById('transaction-Type-toogle');
    const debitTobbleOpt = document.querySelector('#client-consultant-toogle .global-toggle-opt1');
    const creditTobbleOpt = document.querySelector('#client-consultant-toogle .global-toggle-opt2');
    transactionTypeToggleContainer.classList.remove('active');
    debitTobbleOpt.classList.add('active');
    creditTobbleOpt.classList.remove('active');
}
function resetTransactionTypeToggle() {
    if (Number(transactionTypeInputCreateDoc.value) === 1 ) {
        moreFormElementsContainer.style.display = 'none';
        headerContainerCreateDoc.style.display = 'none';
        const transactionTypeToggleContainer = getElementById('transaction-Type-toogle');
        const debitTobbleOpt = document.querySelector('#transaction-Type-toogle .global-toggle-opt1');
        const creditTobbleOpt = document.querySelector('#transaction-Type-toogle .global-toggle-opt2');
        transactionTypeToggleContainer.classList.remove('active');
        debitTobbleOpt.classList.add('active');
        creditTobbleOpt.classList.remove('active');
    }

}
async function displayCreateNewDocumentsModal(modalId) {
    createDocForm.reset();
    initClientIdInputEventListener();
    selectedClientInputCreateDoc.style.display = 'none';
    selectedConsultantInputCreateDoc.style.display = 'none';
    searchConsultantElementContainer.style.display = 'none';
    showHideHtmlElements(invoiceBodyContainer, 'none');
    showHideHtmlElements(documentBody, 'none');
    searchClientElementContainer.style.display = 'block';
    hideShowDescriptionInput('hide');
    descriptionInputCreateDoc.placeholder = 'Enter the document description';
    clientIdInputCreateDoc.value = null;
    initGlobalToggles();
    resetClientToggle();
    showModal(modalId);
}


//client consultant Radio button change
if (clientConsultantRBCreateDoc) {
    clientConsultantRBCreateDoc.addEventListener('input', async (event) => {
        if (Number(clientConsultantRBCreateDoc.value) === 2) {
            searchConsultantElementContainer.style.display = 'block';
            searchClientElementContainer.style.display = 'none';
            selectedConsultantInputCreateDoc.style.display = 'none';
            initConsultantIdInputEventListener();
        } else {
            searchClientElementContainer.style.display = 'block';
            searchConsultantElementContainer.style.display = 'none';
            selectedClientInputCreateDoc.style.display = 'none';

        }
        clientIdInputCreateDoc.value = null;
        consultantIdInputCreateDoc.value = null;
        showHideFormToCreate('hide');
        showHideHtmlElements(documentBody, 'none');
    });
}

document.addEventListener('DOMContentLoaded', () => {

    if (transactionTypeInputCreateDoc) {
        transactionTypeInputCreateDoc.addEventListener('input', async (event) => {
            //execute debit or credit changes
            await fillDocumentTypeDropdownHtmlElement();
            showHideHtmlElements(invoiceBodyContainer, 'none');
            showHideHtmlElements(documentBody, 'none');
        });
    }
});

async function fillDocumentTypeDropdownHtmlElement() {
    await fillDocumentTypeDropdown(transactionTypeInputCreateDoc.value);
    if (Number(transactionTypeInputCreateDoc.value) === 2) {
        documentNumberInputCreateDoc.value = null;
        documentNumberInputCreateDoc.placeholder = 'Enter a Doc Number';
        documentNumberInputCreateDoc.disabled = false;
    } else {
        documentNumberInputCreateDoc.disabled = true;
        documentNumberInputCreateDoc.value = 'Select a Doc Type';
    }
}
function showHideFormToCreate(showHide) {
    moreFormElementsContainer.style.display = showHide === 'show' ? 'block' : 'none';
    headerContainerCreateDoc.style.display = showHide === 'show' ? 'flex' : 'none';
}
async function initConsultantIdInputEventListener() {
    if (!consultantIdInputCreateDoc) {
        console.error("clientIdInputCreateDoc not found");
        return;
    }

    // Only add the observer if it has not been added before
    if (!consultantIdInputCreateDoc.hasAttribute('data-observer-initialized')) {
        const observer = new MutationObserver(async () => {
            try {
                if (consultantIdInputCreateDoc.value !== '' && consultantIdInputCreateDoc.value !== null) {
                    await handleConsultantSelection();
                }
            } catch (error) {
                console.error("Error handling client selection:", error);
            }
        });

        observer.observe(consultantIdInputCreateDoc, {
            attributes: true,
            attributeFilter: ['value']
        });

        // Mark as initialized
        consultantIdInputCreateDoc.setAttribute('data-observer-initialized', 'true');
    }
}
async function handleConsultantSelection() {
    try {
        let debitCredit = transactionTypeInputCreateDoc.value === null ? 1 : transactionTypeInputCreateDoc.value;
        await fillDocumentTypeDropdown(debitCredit);
        initGlobalToggles();
        resetTransactionTypeToggle();
        showHideFormToCreate('show');
    } catch (error) {
        console.error("Error in handleConsultantSelection:", error);
    }
}
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
        initGlobalToggles();
        resetTransactionTypeToggle();
        showHideFormToCreate('show');
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
    displaySpinner();
    let isCredit = Number(transactionTypeInputCreateDoc.value) === 1 ? true : false;
    let isClient = Number(clientConsultantRBCreateDoc.value) === 1 ? true : false;
    let clientConsultantIdValue = clientIdInputCreateDoc.value === null || clientIdInputCreateDoc.value === '' ? consultantIdInputCreateDoc.value : clientIdInputCreateDoc.value;

    if (documentTypeSelectCreateDoc.value !== "null") {
        let data = await getSubtypesAndDocConsecutiveNumber(documentTypeSelectCreateDoc.value, clientConsultantIdValue, isClient, isCredit);

        subtypeSelectCreateDoc.innerHTML = '<option>-Select a Subtype-</option>';

        if (data.subtypesListAndConsecutiveNumber.subtypesList.length > 0) {

            populateSelect('subTypeIdCreateDoc', data.subtypesListAndConsecutiveNumber.subtypesList, '-Select a Subtype-', null);
        }
        if (isCredit) {
            documentNumberInputCreateDoc.value = data.subtypesListAndConsecutiveNumber.docConsecutiveNumber;
        } else {
            documentNumberInputCreateDoc.value = null;
        }

        subtypeSelectCreateDoc.disabled = false;
        showHideHtmlElements(documentBody, 'block');
    } else {
        subtypeSelectCreateDoc.disabled = true;
        subtypeSelectCreateDoc.innerHTML = '<option>-Select a Doc Type-</option>';
        showHideHtmlElements(documentBody, 'none');
    }

    documentTypeCases(documentTypeSelectCreateDoc.value);

    hideSpinner();
});

function hideShowDescriptionInput(hideShow, descValue) {
    descriptionContainerInputElement.style.display = hideShow === 'show' ? 'block' : 'none';
    descriptionInputCreateDoc.value = descValue;
}

function showHideHtmlElements(htmlElement, showHide) {
    htmlElement.style.display = showHide;
}



//DISPLAY DEBITS
function docTypeIsNull() {
    hideShowDescriptionInput('hide', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}
function displayInvoice() {
    hideShowDescriptionInput('hide', null);
    showHideHtmlElements(invoiceBodyContainer, 'block');
}
function displayCurrentInterest() {
    hideShowDescriptionInput('show', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}
function displayLatePaymentFee() {
    hideShowDescriptionInput('show', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}
function displayBillOfExchange() {
    hideShowDescriptionInput('show', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}
function displayDebitNote() {
    hideShowDescriptionInput('show', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}
function displayOtherDebit() {
    hideShowDescriptionInput('show', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}
function displayPromissoryNote() {
    hideShowDescriptionInput('show', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}

//DISPLAY CREDITS
function displayDeposit() {
    hideShowDescriptionInput('show', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}
function displayCreditNote() {
    hideShowDescriptionInput('show', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}
function displayCreditNote() {
    hideShowDescriptionInput('show', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}
function displayOtherCredit() {
    hideShowDescriptionInput('show', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}
function displayTransfer() {
    hideShowDescriptionInput('show', null);
    showHideHtmlElements(invoiceBodyContainer, 'none');
}

//DOCUMENT TYPES CASES
function documentTypeCases(documentType) {
    switch (documentType) {
        case 'FAC':
            return displayInvoice();
        case 'I/C':
            return displayCurrentInterest();
        case 'INT':
            return displayLatePaymentFee();
        case 'L/C':
            return displayBillOfExchange();
        case 'N/D':
            return displayDebitNote();
        case 'O/D':
            return displayOtherDebit();
        case 'PAG':
            return displayPromissoryNote();
        case 'DEP':
            return displayDeposit();
        case 'N/C':
            return displayCreditNote();
        case 'O/C':
            return displayOtherCredit();
        case 'TEF':
            return displayTransfer();
        default:
            return docTypeIsNull();
    }
}

//ADD LINES TO THE DOCUMENT
const container = document.getElementById('linesContainer');
const addLineBtn = document.getElementById('addLineBtn');
const summarySubtotal = document.getElementById('summary-subtotal');
const summaryDiscount = document.getElementById('summary-discount');
const summaryTax = document.getElementById('summary-tax');
const summaryTotal = document.getElementById('summary-total');
const emptyMessage = container.querySelector('.empty-message');
let lineCount = 0;

function createLine() {
    lineCount++;
    const line = document.createElement('div');
    line.className = 'document-line';
    line.draggable = true;

    line.innerHTML = `
        <span class="line-number">${lineCount}</span>
        <input type="text" placeholder="Description" class="product-description column" />
        <input type="number" min="0" value="0" class="quantity column" />
        <input type="number" min="0" value="0" class="unit-price column" />
        <input type="number" min="0" value="0" class="discount column" />
        <input type="text" class="tax column" readonly />
        <input type="text" class="subtotal column" readonly />
        <button class="delete-line column">X</button>
      `;

    line.querySelectorAll('input').forEach(input => {
        input.addEventListener('input', () => {
            updateLineCalculations(line);
            updateSummary();
        });
    });

    const deleteBtn = line.querySelector('.delete-line');
    deleteBtn.addEventListener('click', () => {
        line.remove();
        updateLineNumbers();
        updateSummary();
        checkEmpty();
    });

    line.addEventListener('dragstart', handleDragStart);
    line.addEventListener('dragover', handleDragOver);
    line.addEventListener('drop', handleDrop);

    container.appendChild(line);
    emptyMessage.style.display = 'none';
}

function updateLineCalculations(line) {
    const qty = parseFloat(line.querySelector('.quantity').value) || 0;
    const price = parseFloat(line.querySelector('.unit-price').value) || 0;
    const discount = parseFloat(line.querySelector('.discount').value) || 0;
    const base = (qty * price) - discount;
    const tax = base * 0.13;
    line.querySelector('.tax').value = tax.toFixed(2);
    line.querySelector('.subtotal').value = base.toFixed(2);
}

function updateLineNumbers() {
    const lines = container.querySelectorAll('.document-line');
    lines.forEach((line, index) => {
        line.querySelector('.line-number').textContent = index + 1;
    });
}

function updateSummary() {
    let subtotal = 0;
    let totalDiscount = 0;
    let totalTax = 0;
    container.querySelectorAll('.document-line').forEach(line => {
        const qty = parseFloat(line.querySelector('.quantity').value) || 0;
        const price = parseFloat(line.querySelector('.unit-price').value) || 0;
        const discount = parseFloat(line.querySelector('.discount').value) || 0;
        const base = (qty * price) - discount;
        const tax = base * 0.13;
        subtotal += base;
        totalDiscount += discount;
        totalTax += tax;
    });
    summarySubtotal.textContent = subtotal.toFixed(2);
    summaryDiscount.textContent = totalDiscount.toFixed(2);
    summaryTax.textContent = totalTax.toFixed(2);
    summaryTotal.textContent = (subtotal + totalTax).toFixed(2);
}

function checkEmpty() {
    if (container.querySelectorAll('.document-line').length === 0) {
        emptyMessage.style.display = 'block';
        lineCount = 0;
    }
}

let dragged;
function handleDragStart(e) {
    dragged = this;
    e.dataTransfer.effectAllowed = 'move';
}

function handleDragOver(e) {
    e.preventDefault();
    const draggingOver = this;
    if (draggingOver !== dragged) {
        const bounding = draggingOver.getBoundingClientRect();
        const offset = bounding.y + bounding.height / 2;
        if (e.clientY - offset > 0) {
            draggingOver.after(dragged);
        } else {
            draggingOver.before(dragged);
        }
        updateLineNumbers();
        updateSummary();
    }
}

function handleDrop(e) {
    e.preventDefault();
}

addLineBtn.addEventListener('click', () => createLine());
createLine();
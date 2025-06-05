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
const searchBillableHoursBtn = document.getElementById('search-bill-hours-btn');


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
    if (Number(transactionTypeInputCreateDoc.value) === 1) {
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
        showHideHtmlElements(documentBody, 'none');
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
        showHideHtmlElements(documentBody, 'none');
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
    container.querySelectorAll('.document-line').forEach(line => line.remove());
    lineCount = 0;
    createLine();
    updateSummary();
    document.querySelectorAll('.validation-message').forEach(el => {
        el.style.display = 'none';
    });

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
//SEARCH FOR BILLABLE HOURS
searchBillableHoursBtn.onclick = async () => {
    const startInput = document.getElementById('startDateSearchBillHours');
    const endInput = document.getElementById('endDateSearchBillHours');

    const startMsg = startInput.nextElementSibling;
    const endMsg = endInput.nextElementSibling;

    const startDateValue = startInput.value;
    const endDateValue = endInput.value;

    let isValid = true;

    // Reset validation messages
    startMsg.style.display = 'none';
    endMsg.style.display = 'none';

    // Helper to validate date string
    const isValidDateStrict = (dateStr) => {
        const date = new Date(dateStr);
        return (
            /^\d{4}-\d{2}-\d{2}$/.test(dateStr) &&
            !isNaN(date.getTime()) &&
            dateStr === date.toISOString().slice(0, 10)
        );
    };

    // Required validation
    if (!startDateValue) {
        startMsg.textContent = 'This field is required.';
        startMsg.style.display = 'block';
        isValid = false;
    } else if (!isValidDateStrict(startDateValue)) {
        startMsg.textContent = 'Not valid date.';
        startMsg.style.display = 'block';
        isValid = false;
    }

    if (!endDateValue) {
        endMsg.textContent = 'This field is required.';
        endMsg.style.display = 'block';
        isValid = false;
    } else if (!isValidDateStrict(endDateValue)) {
        endMsg.textContent = 'Not valid date.';
        endMsg.style.display = 'block';
        isValid = false;
    }

    // Date logic validation
    if (isValid && new Date(startDateValue) > new Date(endDateValue)) {
        startMsg.textContent = "Start date couldn't be greater than End Date.";
        startMsg.style.display = 'block';
        isValid = false;
    }

    if (!isValid) return;

    const dataFromApi = await searchForBillableHours(startInput.value, endInput.value, clientIdInputCreateDoc.value);
    let isThereProductsToConfigure = false;

    for (const obj of dataFromApi.billableHours) {
        if (obj.productIdConfigured === null) {
            isThereProductsToConfigure = true;
            let productObject = {
                productId: obj.productIdToConfigure,
                productName: obj.productNameToConfigure,
                taxPercentage: 0
            };
            window.showProductClientCompanyAccountingConfigModal({
                modalId: 'product-client-config-modal',
                product: productObject,
                clientId: clientIdInputCreateDoc.value,
                clientName: selectedClientInputCreateDoc.value,
                targetLine: null,
                onSave: null,
                movementTypeId: obj.movementTypeId
            });
            break;
        }
    }

    if (!isThereProductsToConfigure) {
        clearLines();
        for (const obj of dataFromApi.billableHours) {
            if (obj.productIdConfigured !== null) {
                // Create new line
                const newLine = createLine();

                // Prepare the product
                const product = {
                    productId: obj.productIdConfigured,
                    productName: obj.productDescription,
                    productCode: obj.productCodeConfigured,
                    taxPercentage: obj.taxPercentage || 0
                };

                // Aplicar el producto
                applyProduct(product, newLine);

                newLine.querySelector('.quantity').value = obj.totalHours;
                newLine.querySelector('.unit-price').value = obj.unitPrice;
                newLine.dataset.taxPercentage = obj.taxPercentage || 0;

                updateLineCalculations(newLine);
                updateSummary();
            }
        }
    }
};
async function searchForBillableHours(startDate, endDate, clientId) {
    displaySpinner();
    var url = "/Finances/DocumentsCC/GetBillableHoursByClient?clientId=" + encodeURIComponent(clientId)
        + "&startDate=" + encodeURIComponent(startDate)
        + "&endDate=" + encodeURIComponent(endDate);
    try {
        const response = await fetch(url);

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }

        const dataFromApi = await response.json();
        hideSpinner();
        return dataFromApi;
    }
    catch (err) {
        hideSpinner();
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err.message);
        displayToasterError('Something went wrong, more details: ' + err);
        return null;
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
let currentProductLine = null;

function clearLines() {
    container.querySelectorAll('.document-line').forEach(line => {
        line.remove();
    });

    lineCount = 0;

    updateSummary();

    if (emptyMessage) {
        emptyMessage.style.display = 'block';
    }

    dragged = null;

    container.querySelectorAll('.validation-message').forEach(msg => {
        msg.style.display = 'none';
        msg.textContent = '';
    });
}




function createLine() {
    lineCount++;
    const line = document.createElement('div');
    line.className = 'document-line';
    line.style.position = 'relative';

    line.innerHTML = `
    <div class="invoice-body-left-section">
      <span class="line-number">${lineCount}</span>
      <div class="invoice-body-search-container">
        <input type="text" class="invoice-body-product-search" placeholder="🔍" />
        <div class="invoice-body-search-results"></div>
      </div>
    </div>
    <div class="invoice-body-description-wrapper">
      <label class="invoice-body-product-code-label"></label>
      <input type="text" disabled placeholder="Search and select a Product" class="product-description column" />
      <input type="hidden" class="hidden-product-id" />
    </div>
    <input type="number" min="0" value="0" class="quantity column" />
    <input type="number" min="0" value="0" class="unit-price column" />
    <input type="number" min="0" value="0" class="discount column" />
    <input type="text" class="tax column" readonly />
    <input type="text" class="subtotal column" readonly />
    <button class="delete-line column">
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" width="20">
        <path fill="#ed143d" d="M135.2 17.7L128 32 32 32C14.3 32 0 46.3 0 64S14.3 96 32 96l384 0c17.7 0 32-14.3 32-32s-14.3-32-32-32l-96 0-7.2-14.3C307.4 6.8 296.3 0 284.2 0L163.8 0c-12.1 0-23.2 6.8-28.6 17.7zM416 128L32 128 53.2 467c1.6 25.3 22.6 45 47.9 45l245.8 0c25.3 0 46.3-19.7 47.9-45L416 128z"/>
      </svg>
    </button>
  `;

    setupSearchHandlers(line);

    const handle = line.querySelector('.line-number');
    handle.classList.add('drag-handle');
    handle.draggable = true;

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

    const searchInput = line.querySelector('.invoice-body-product-search');
    searchInput.addEventListener('focus', () => {
        line.classList.add('search-active');
    });
    searchInput.addEventListener('blur', () => {
        setTimeout(() => line.classList.remove('search-active'), 200);
    });

    handle.addEventListener('dragstart', handleDragStart);
    line.addEventListener('dragover', handleDragOver);
    line.addEventListener('drop', handleDrop);

    container.appendChild(line);
    emptyMessage.style.display = 'none';
    updateLineNumbers();

    const productDescriptionInput = line.querySelector('.product-description');
    productDescriptionInput.addEventListener('dragstart', e => {
        e.preventDefault(); // Prevent dragging text from the disabled input
    });
    const label = line.querySelector('.invoice-body-description-wrapper');
    label.setAttribute('draggable', 'false');
    label.addEventListener('dragstart', e => e.preventDefault());

    deleteBtn.setAttribute('draggable', 'false');
    return line;
}


function ensureSearchListVisibility(searchInput, resultsBox) {
    const line = searchInput.closest('.document-line');

    resultsBox.style.position = 'absolute';
    resultsBox.style.top = `${searchInput.offsetTop + searchInput.offsetHeight + 6}px`;
    resultsBox.style.left = `${searchInput.offsetLeft}px`;
    resultsBox.style.width = '280px';
    resultsBox.style.zIndex = '1000';
    resultsBox.style.background = '#fff';
    resultsBox.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';
    resultsBox.style.border = '1px solid #ccc';
    resultsBox.style.borderRadius = '4px';
    resultsBox.style.maxHeight = '200px';
    resultsBox.style.overflowY = 'auto';

    container.querySelectorAll('.document-line').forEach(l => {
        l.style.zIndex = l === line ? '1000' : '1';
    });
}

function repositionAllSearchDropdowns() {
    const searchInputs = container.querySelectorAll('.invoice-body-product-search');
    searchInputs.forEach(input => {
        const line = input.closest('.document-line');
        if (line && line.classList.contains('search-active')) {
            const resultsBox = line.querySelector('.invoice-body-search-results');
            if (resultsBox && resultsBox.style.display === 'block') {
                ensureSearchListVisibility(input, resultsBox);
            }
        }
    });
}

function updateLineCalculations(line) {
    const qty = parseFloat(line.querySelector('.quantity').value) || 0;
    const price = parseFloat(line.querySelector('.unit-price').value) || 0;
    const discount = parseFloat(line.querySelector('.discount').value) || 0;
    const taxPercentage = parseFloat(line.dataset.taxPercentage) || 0;
    const base = (qty * price) - discount;
    const tax = base * (taxPercentage / 100);
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
    let subtotalBruto = 0;
    let totalDiscount = 0;
    let totalTax = 0;

    container.querySelectorAll('.document-line').forEach(line => {
        const qty = parseFloat(line.querySelector('.quantity').value) || 0;
        const price = parseFloat(line.querySelector('.unit-price').value) || 0;
        const discount = parseFloat(line.querySelector('.discount').value) || 0;
        const taxPercentage = parseFloat(line.dataset.taxPercentage) || 0;

        const lineSubtotal = qty * price;
        const lineTax = (lineSubtotal - discount) * (taxPercentage / 100);

        subtotalBruto += lineSubtotal;
        totalDiscount += discount;
        totalTax += lineTax;
    });

    const total = subtotalBruto - totalDiscount + totalTax;

    summarySubtotal.textContent = formatCurrency(subtotalBruto);
    summaryDiscount.textContent = formatCurrency(totalDiscount);
    summaryTax.textContent = formatCurrency(totalTax);
    summaryTotal.textContent = formatCurrency(total);
}

function formatCurrency(value) {
    return `$${value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function checkEmpty() {
    if (container.querySelectorAll('.document-line').length === 0) {
        emptyMessage.style.display = 'block';
        lineCount = 0;
    }
}

let dragged;
function handleDragStart(e) {
    const line = e.target.closest('.document-line');
    if (!line) return;

    dragged = line;
    e.dataTransfer.effectAllowed = 'move';

    const emptyImg = new Image();
    emptyImg.src = '';
    e.dataTransfer.setDragImage(emptyImg, 0, 0);
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
    if (!dragged) return;
}


addLineBtn.addEventListener('click', createLine);

function setupSearchHandlers(line) {
    const searchInput = line.querySelector('.invoice-body-product-search');
    const resultsBox = line.querySelector('.invoice-body-search-results');
    let activeIndex = -1;
    let results = [];

    searchInput.addEventListener('input', async () => {
        const query = searchInput.value.trim().toLowerCase();
        line.classList.toggle('search-active', query.length > 0);
        if (query.length < 2) {
            resultsBox.innerHTML = '';
            resultsBox.style.display = 'none';
            return;
        }

        try {
            const clientId = clientIdInputCreateDoc.value;
            const response = await fetch(`/AdminCenter/Products/SearchProjectsByTextWithAccountingConfigStatus?searchText=${encodeURIComponent(query)}&clientId=${encodeURIComponent(clientId)}`);
            const data = await response.json();
            results = data.productsList || [];
        } catch (error) {
            console.error('Error fetching products:', error);
            results = [];
        }

        if (results.length === 0) {
            resultsBox.innerHTML = `
                <div class="no-results">
                    No results found
                    <button class="add-new-product-btn btn-primary-submit">Create New Product</button>
                </div>
            `;

            const addBtn = resultsBox.querySelector('.add-new-product-btn');
            addBtn.addEventListener('click', () => {
                const value = searchInput.value.trim();
                if (typeof window.showNewProductModal === 'function') {
                    window.showNewProductModal({
                        modalId: 'new-product-modal',
                        title: 'Create New Product',
                        name: value,
                        alias: value,
                        detail: '',
                        onSaveCallback: (createdProduct) => {
                            window.showProductClientCompanyAccountingConfigModal({
                                modalId: 'product-client-config-modal',
                                product: createdProduct.genericObject,
                                clientId: clientIdInputCreateDoc.value,
                                clientName: selectedClientInputCreateDoc.value,
                                targetLine: line,
                                onSave: (configuredProduct, lineToUse) => applyProduct(configuredProduct, lineToUse),
                                movementTypeId: null
                            });
                        }
                    });
                }
                resetSearchUI();
            });
        } else {
            resultsBox.innerHTML = results.map((p, i) =>
                `<div class="result-item" data-index="${i}">${p.productName}</div>`
            ).join('');
        }

        activeIndex = -1;
        resultsBox.style.display = 'block';
        ensureSearchListVisibility(searchInput, resultsBox);
    });

    searchInput.addEventListener('keydown', e => {
        const items = resultsBox.querySelectorAll('.result-item');
        if (e.key === 'ArrowDown') {
            activeIndex = (activeIndex + 1) % items.length;
        } else if (e.key === 'ArrowUp') {
            activeIndex = (activeIndex - 1 + items.length) % items.length;
        } else if (e.key === 'Enter' && items[activeIndex]) {
            items[activeIndex].click();
            e.preventDefault();
        } else if (e.key === 'Escape') {
            resetSearchUI();
        }
        items.forEach((el, idx) => el.classList.toggle('active', idx === activeIndex));
    });

    resultsBox.addEventListener('click', e => {
        const index = e.target.closest('.result-item')?.dataset?.index;
        if (index !== undefined) {
            const product = results[index];
            if (!product.clientHasAccountingConfig) {
                currentProductLine = line;
                window.showProductClientCompanyAccountingConfigModal({
                    modalId: 'product-client-config-modal',
                    product,
                    clientId: clientIdInputCreateDoc.value,
                    clientName: selectedClientInputCreateDoc.value,
                    targetLine: line,
                    onSave: (configuredProduct, lineToUse) => applyProduct(configuredProduct, lineToUse),
                    movementTypeId: null
                });
            } else {
                applyProduct(product, line);
            }
        }
    });


    function resetSearchUI() {
        line.classList.remove('search-active');
        searchInput.value = '';
        resultsBox.innerHTML = '';
        resultsBox.style.display = 'none';
    }

    document.addEventListener('click', e => {
        if (!line.contains(e.target)) resetSearchUI();
    });
}
function applyProduct(product, targetLine) {
    const searchInput = targetLine.querySelector('.invoice-body-product-search');
    const resultsBox = targetLine.querySelector('.invoice-body-search-results');
    const descriptionInput = targetLine.querySelector('.product-description');
    const productCodeLabel = targetLine.querySelector('.invoice-body-product-code-label');
    const hiddenProductId = targetLine.querySelector('.hidden-product-id');

    searchInput.value = '';
    resultsBox.innerHTML = '';
    resultsBox.style.display = 'none';
    targetLine.classList.remove('search-active');

    descriptionInput.value = product.productName;
    descriptionInput.disabled = false;
    productCodeLabel.textContent = product.productCode;
    hiddenProductId.value = product.productId;
    targetLine.dataset.taxPercentage = product.taxPercentage;

    updateLineCalculations(targetLine);
    updateSummary();
}

// Hide search UI when product config modal is opened
window.addEventListener('showProductClientCompanyAccountingConfigModal:open', (e) => {
    const line = e.detail?.targetLine;
    if (!line) return;

    const searchInput = line.querySelector('.invoice-body-product-search');
    const resultsBox = line.querySelector('.invoice-body-search-results');

    searchInput.value = '';
    resultsBox.innerHTML = '';
    resultsBox.style.display = 'none';
    line.classList.remove('search-active');
});


container.addEventListener('dragstart', function (e) {
    const isBadTarget =
        ['INPUT', 'TEXTAREA', 'SELECT', 'LABEL'].includes(e.target.tagName);

    if (isBadTarget && e.target.closest('.document-line')) {
        e.preventDefault();
    }
});

//CREATE DOCUMENT

function logAllLines() {
    const lines = container.querySelectorAll('.document-line');
    const result = [];

    lines.forEach((line, index) => {
        const lineNumber = index + 1;
        const description = line.querySelector('.product-description')?.value || '';
        const quantity = parseFloat(line.querySelector('.quantity')?.value) || 0;
        const unitPrice = parseFloat(line.querySelector('.unit-price')?.value) || 0;
        const discount = parseFloat(line.querySelector('.discount')?.value) || 0;
        const tax = parseFloat(line.querySelector('.tax')?.value) || 0;

        result.push({
            lineNumber,
            description,
            quantity,
            unitPrice,
            discount,
            tax
        });
    });

    console.log(result);
}
// ===============================
// Pagination reusable helpers
// ===============================

// Resolve pagination context (container + related elements)
function getPaginationContext(paginationContainer) {
    let container = null;

    if (paginationContainer instanceof HTMLElement) {
        // If an inner element is passed (button/select), go up to closest pagination container
        container = paginationContainer.closest('.pagination-container');
    } else if (typeof paginationContainer === 'string') {
        // If a selector string is passed, use querySelector
        container = document.querySelector(paginationContainer);
    }

    // Fallback: first paginator on page (only used if caller forgets to pass a context)
    if (!container) {
        container = document.querySelector('.pagination-container');
    }

    if (!container) {
        return {
            container: null,
            pageIndexInput: null,
            totalResultsInput: null,
            isLastPageInput: null,
            pageSizeChangedInput: null,
            itemsPerPageSelect: null,
            labelPagInfo: null,
            labelTotalResults: null,
            leftButtons: [],
            rightButtons: []
        };
    }

    return {
        container: container,
        // Everything scoped to THIS container
        pageIndexInput: container.querySelector('.page-index-input'),
        totalResultsInput: container.querySelector('.total-results-input'),
        isLastPageInput: container.querySelector('.is-last-page-input'),
        pageSizeChangedInput: container.querySelector('.page-size-changed-input'),
        itemsPerPageSelect: container.querySelector('.items-per-page-select'),
        labelPagInfo: container.querySelector('.label-pag-info'),
        labelTotalResults: container.querySelector('.label-total-results'),
        leftButtons: container.querySelectorAll('.left-pag-buttons'),
        rightButtons: container.querySelectorAll('.right-pag-buttons')
    };
}

// Update pagination UI values for a specific paginator
function updatePaginationValues(paginationData, paginationContainer) {
    const ctx = getPaginationContext(paginationContainer);

    if (!ctx.pageIndexInput || !ctx.totalResultsInput || !ctx.itemsPerPageSelect) {
        return;
    }

    ctx.pageIndexInput.value = paginationData.pageIndex;
    ctx.totalResultsInput.value = paginationData.totalResults;
    ctx.isLastPageInput.value = paginationData.isLastPage;

    const select = ctx.itemsPerPageSelect;
    select.innerHTML = "";
    paginationData.itemsPerPageList.forEach(function (optionItem) {
        const option = document.createElement("option");
        option.value = optionItem.value;
        option.text = optionItem.text;
        if (parseInt(optionItem.value, 10) === paginationData.pageSize) {
            option.selected = true;
        }
        select.appendChild(option);
    });

    disableButtons(
        paginationData.pageIndex,
        paginationData.isLastPage,
        paginationData.totalResults,
        paginationData.pageSize,
        ctx.container
    );

    if (ctx.labelTotalResults) {
        ctx.labelTotalResults.textContent = "Total Results: " + paginationData.totalResults;
    }
}

// Global flag kept for backward compatibility
var changedPageSize = false;

// Mark page size as changed for a specific pagination instance
function changePageSizeValue(paginationContainerOrElement) {
    const ctx = getPaginationContext(paginationContainerOrElement);
    changedPageSize = true; // legacy global flag
    if (ctx.pageSizeChangedInput) {
        ctx.pageSizeChangedInput.value = "true";
    }
}

// Check if page size changed for a given paginator
function pageSizeChanged(paginationContainerOrElement) {
    const ctx = getPaginationContext(paginationContainerOrElement);
    if (!ctx.pageSizeChangedInput) {
        // Fallback to global behavior
        return changedPageSize;
    }
    return ctx.pageSizeChangedInput.value === "true";
}

// Return current pagination values (PageSize, PageIndex) for a specific paginator
function returnCurrentPaginationValues(paginationContainer) {
    const ctx = getPaginationContext(paginationContainer);
    if (!ctx.pageIndexInput) {
        return { PageSize: 0, PageIndex: 1 };
    }

    const currentPageIndex = parseInt(ctx.pageIndexInput.value || "1", 10);

    if (pageSizeChanged(paginationContainer)) {
        return {
            PageSize: parseInt(ctx.itemsPerPageSelect.value, 10),
            PageIndex: currentPageIndex
        };
    } else {
        return {
            PageSize: 0,
            PageIndex: currentPageIndex
        };
    }
}

// Enable/disable navigation buttons and update pagination info label
function disableButtons(pageIndex, isLastPage, totalResults, pageSize, paginationContainer) {
    const ctx = getPaginationContext(paginationContainer);

    if (ctx.labelPagInfo && pageSize > 0) {
        ctx.labelPagInfo.textContent = pageIndex + " of " + Math.ceil(totalResults / pageSize);
    }

    ctx.leftButtons.forEach(function (button) {
        button.disabled = (pageIndex === 1);
    });

    ctx.rightButtons.forEach(function (button) {
        button.disabled = (
            isLastPage ||
            totalResults === 0 ||
            (Math.ceil((parseInt(totalResults) / parseInt(pageSize))) === pageIndex)
        );
    });
}

// Handle paginator navigation buttons (first, prev, next, last)
function paginationButtonActions(action, paginationContainerOrElement) {
    const ctx = getPaginationContext(paginationContainerOrElement);

    if (!ctx.pageIndexInput || !ctx.totalResultsInput || !ctx.itemsPerPageSelect) {
        return;
    }

    let pageIndex = parseInt(ctx.pageIndexInput.value || "1", 10);
    const totalResults = parseInt(ctx.totalResultsInput.value || "0", 10);
    const isLastPage = ctx.isLastPageInput.value === "true" || ctx.isLastPageInput.value === "1";
    const pageSize = parseInt(ctx.itemsPerPageSelect.value || "1", 10);

    let currentValue = pageIndex;

    if (action === 'increment') {
        currentValue++;
    } else if (action === 'decrement') {
        currentValue--;
    } else if (action === 'firstPage') {
        currentValue = 1;
    } else if (action === 'lastPage') {
        const newValue = Math.max(1, Math.ceil(totalResults / pageSize));
        currentValue = newValue;
    }

    if (currentValue < 1) {
        currentValue = 1;
    }

    ctx.pageIndexInput.value = currentValue;

    disableButtons(
        currentValue,
        isLastPage,
        totalResults,
        pageSize,
        ctx.container
    );
}

// ===============================
// Sorting (orderByTableList)
// ===============================

// Afecta SOLO a la tabla donde se hizo click y al paginator correcto
function orderByTableList(columnName, value, sortArrow, thElement) {
    const th = thElement || document.getElementsByName(value)[0];
    if (!th) return;

    // Current table
    const table = th.closest('table');
    if (!table) return;

    // Associated paginator (main or modal)
    let paginationContainerSelector;
    if (th.closest('#modal-consultants_benefits-balance')) {
        paginationContainerSelector = '#modal-consultants_benefits-balance .pagination-container';
    } else {
        paginationContainerSelector = '.principal-header-container .pagination-container';
    }

    const paginationContainer = document.querySelector(paginationContainerSelector);
    if (!paginationContainer) return;

    // Sort inputs ONLY from this paginator
    const inputFieldToOrder = paginationContainer.querySelector('input[name="fieldToOrder"]');
    const inputDirectionOrder = paginationContainer.querySelector('input[name="directionOrder"]');

    // Clear arrows ONLY from this table
    const arrowSpan = th.querySelector('.sort-arrow');
    const sortArrows = table.querySelectorAll('.sort-arrow');
    sortArrows.forEach(function (arrow) {
        arrow.innerHTML = "";
    });

    switch (th.getAttribute("data-sort")) {
        case "ANY":
            th.setAttribute("data-sort", "DESC");
            if (inputFieldToOrder) inputFieldToOrder.value = columnName;
            if (arrowSpan) arrowSpan.innerHTML = '<i class="bi bi-arrow-down"></i>';
            break;
        case "DESC":
            th.setAttribute("data-sort", "ASC");
            if (inputFieldToOrder) inputFieldToOrder.value = columnName;
            if (arrowSpan) arrowSpan.innerHTML = '<i class="bi bi-arrow-up"></i>';
            break;
        default:
            th.setAttribute("data-sort", "ANY");
            if (inputFieldToOrder) inputFieldToOrder.value = "ANY";
            if (arrowSpan) arrowSpan.innerHTML = "";
    }

    if (inputDirectionOrder) {
        inputDirectionOrder.value = th.getAttribute("data-sort");
    }

    // Sort change is like a filter → go back to page 1 and RequestFromFilters = true
    paginationSubmit(true, false, paginationContainer);
}

// ===============================
// Generic pagination submit
// ===============================

// reloadTable === RequestFromFilters (true = viene de filtros/orden/search)
function paginationSubmit(reloadTable, keepPage, paginationContainerOrElement) {
    const ctx = getPaginationContext(paginationContainerOrElement);

    if (!ctx.pageIndexInput) {
        return;
    }

    // If we don't want to keep current page, reset to first page
    if (!keepPage) {
        ctx.pageIndexInput.value = "1";
    }

    // Custom handler (per page) defined by your screens
    if (typeof window.handlePaginationSubmit === 'function') {
        window.handlePaginationSubmit({
            reloadTable: reloadTable,   // will be mapped to RequestFromFilters
            keepPage: keepPage,
            paginationContext: ctx
        });
    }
}

// ===============================
// Optional: load pagination via AJAX
// ===============================
async function getPaginationComponent() {
    const url = `/Pagination/GetPagination`;
    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorData = await response.text();
            displayToasterError("Error loading pagination component");
            throw new Error(`The request to the server failed! More details: ${errorData}`);
        }

        const htmlContent = await response.text();
        return htmlContent;
    } catch (error) {
        validateSessionExpiration(error.message);
        displayToasterError("Internet connection failed");
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}

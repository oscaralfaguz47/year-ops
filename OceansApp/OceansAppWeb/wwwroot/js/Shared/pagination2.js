
function updatePaginationValuesP2(paginationData) {
    $('[name="PageIndexP2"]').val(paginationData.pageIndex);
    document.getElementById("total-results-inputP2").value = paginationData.totalResults;
    document.getElementById("is-last-page-inputP2").value = paginationData.isLastPage;
    let select = document.getElementById("items-per-pageP2");
    select.innerHTML = "";
    paginationData.itemsPerPageList.forEach(function (opcion) {
        let option = document.createElement("option");
        option.value = opcion.value;
        option.text = opcion.text;
        if (parseInt(opcion.value, 10) === paginationData.pageSize) {
            option.selected = true;
        }
        select.appendChild(option);
    });
    disableButtonsP2(paginationData.pageIndex, paginationData.isLastPage, paginationData.totalResults, paginationData.pageSize);
    document.getElementById("label-total-resultsP2").textContent = "Total Results: " + paginationData.totalResults;
}
var changedPageSizeP2 = false;
function changePageSizeValueP2() {
    changedPageSizeP2 = true;
}
function pageSizeChangedP2() {
    var pageSizeCurrentValue = document.getElementById("page-size-changedP2").value;
    if (pageSizeCurrentValue) {
        return true;
    } else {
        return false;
    }
}
function returnCurrentPaginationValuesP2() {
    if (changedPageSizeP2) {
        return {
            PageSize: parseInt($('#items-per-pageP2').val()),
            PageIndex: parseInt($('[name="PageIndexP2"]').val())
        }
    } else {
        return {
            PageSize: 0,
            PageIndex: parseInt($('[name="PageIndexP2"]').val())
        }
    }
}
function disableButtonsP2(pageIndex, isLastPage, totalResults, pageSize) {
    document.getElementById("label-pag-infoP2").textContent = pageIndex + " of " + Math.ceil(totalResults / pageSize);
    document.querySelectorAll('.left-pag-buttonsP2').forEach(function (boton) {
        boton.disabled = (pageIndex === 1);
    });
    document.querySelectorAll('.right-pag-buttonsP2').forEach(function (boton) {
        boton.disabled = (isLastPage || totalResults === 0 || ((Math.ceil((parseInt(totalResults) / parseInt(pageSize)))) === pageIndex));
    });
}
function paginationButtonActionsP2(action) {
    var pageIndex = parseInt($('[name="PageIndexP2"]').val());
    var totalResults = parseInt(document.getElementById("total-results-inputP2").value);
    var isLastPage = document.getElementById("is-last-page-inputP2").value;
    var currentValue = parseInt(pageIndex);
    var pageSize = select = document.getElementById("items-per-pageP2").value;

    if (action === 'increment') {
        currentValue++;
    } else if (action === 'decrement') {
        currentValue--;
    } else if (action === 'firstPage') {
        currentValue = 1;
    } else if (action === 'lastPage') {
        var newValue = Math.ceil((parseInt(totalResults) / parseInt(pageSize)));
        currentValue = newValue;
    }
    $('[name="PageIndexP2"]').val(currentValue);
    disableButtonsP2(currentValue, isLastPage, totalResults, pageSize);
}
function orderByTableListP2(columnName, value, sortArrow) {
    var th = document.getElementsByName(value)[0];
    var inputFieldToOrder = document.getElementsByName('fieldToOrderP2')[0];
    var inputDirectionOrder = document.getElementsByName('directionOrderP2')[0];
    var arrowSpan = document.getElementById(sortArrow);
    var sortArrows = document.querySelectorAll('.sort-arrow');
    sortArrows.forEach(function (arrow) {
        arrow.innerHTML = "";
    });
    switch (th.getAttribute("data-sort")) {
        case "ANY":
            th.setAttribute("data-sort", "DESC");
            inputFieldToOrder.value = columnName;
            arrowSpan.innerHTML = '<i class="bi bi-arrow-down"></i>';
            break;
        case "DESC":
            th.setAttribute("data-sort", "ASC");
            inputFieldToOrder.value = columnName;
            arrowSpan.innerHTML = '<i class="bi bi-arrow-up"></i>';
            break;
        default:
            th.setAttribute("data-sort", "ANY");
            inputFieldToOrder.value = "ANY";
            arrowSpan.innerHTML = "";
    }
    inputDirectionOrder.value = th.getAttribute("data-sort");
    paginationSubmitP2(false, false);
}

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


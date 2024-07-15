
function updatePaginationValues(paginationData) {
    $('[name="PageIndex"]').val(paginationData.pageIndex);
    document.getElementById("total-results-input").value = paginationData.totalResults;
    document.getElementById("is-last-page-input").value = paginationData.isLastPage;
    let select = document.getElementById("items-per-page");
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
    disableButtons(paginationData.pageIndex, paginationData.isLastPage, paginationData.totalResults, paginationData.pageSize);
    document.getElementById("label-total-results").textContent = "Total Results: " + paginationData.totalResults;
}
var changedPageSize = false;
function changePageSizeValue() {
    changedPageSize = true;
}
function pageSizeChanged() {
    var pageSizeCurrentValue = document.getElementById("page-size-changed").value;
    if (pageSizeCurrentValue) {
        return true;
    } else {
        return false;
    }
}
function returnCurrentPaginationValues() {
    if (changedPageSize) {
        return {
            PageSize: parseInt($('#items-per-page').val()),
            PageIndex: parseInt($('[name="PageIndex"]').val())
        }
    } else {
        return {
            PageSize: 0,
            PageIndex: parseInt($('[name="PageIndex"]').val())
        }
    }
}
function disableButtons(pageIndex, isLastPage, totalResults, pageSize) {
    document.getElementById("label-pag-info").textContent = pageIndex + " of " + Math.ceil(totalResults / pageSize);
    document.querySelectorAll('.left-pag-buttons').forEach(function (boton) {
        boton.disabled = (pageIndex === 1);
    });
    document.querySelectorAll('.right-pag-buttons').forEach(function (boton) {
        boton.disabled = (isLastPage || totalResults === 0 || ((Math.ceil((parseInt(totalResults) / parseInt(pageSize)))) === pageIndex));
    });
}
function paginationButtonActions(action) {
    var pageIndex = parseInt($('[name="PageIndex"]').val());
    var totalResults = parseInt(document.getElementById("total-results-input").value);
    var isLastPage = document.getElementById("is-last-page-input").value;
    var currentValue = parseInt(pageIndex);
    var pageSize = select = document.getElementById("items-per-page").value;

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
    $('[name="PageIndex"]').val(currentValue);
    disableButtons(currentValue, isLastPage, totalResults, pageSize);
}
function orderByTableList(columnName, value, sortArrow) {
    var th = document.getElementsByName(value)[0];
    var inputFieldToOrder = document.getElementsByName('fieldToOrder')[0];
    var inputDirectionOrder = document.getElementsByName('directionOrder')[0];
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
    paginationSubmit(false, false);
}
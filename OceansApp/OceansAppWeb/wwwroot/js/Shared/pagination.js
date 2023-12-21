
function updatePaginationValues(paginationData) {
    $('[name="PageIndex"]').val(paginationData.PageIndex);
    document.getElementById("total-results-input").value = paginationData.TotalResults;
    document.getElementById("is-last-page-input").value = paginationData.IsLastPage;
    let select = document.getElementById("items-per-page");
    select.innerHTML = "";
    paginationData.ItemsPerPageList.forEach(function (opcion) {
        let option = document.createElement("option");
        option.value = opcion.Value;
        option.text = opcion.Name;
        select.appendChild(option);
    });
    disableButtons(paginationData.PageIndex, paginationData.IsLastPage, paginationData.TotalResults, paginationData.PageSize);
    document.getElementById("label-total-results").textContent = "Total Resultados: " + paginationData.TotalResults;
}
function disableButtons(pageIndex, isLastPage, totalResults, pageSize) {
    document.getElementById("label-pag-info").textContent = pageIndex + " de " + Math.ceil(totalResults / pageSize);
    document.querySelectorAll('.left-pag-buttons').forEach(function (boton) {
        boton.disabled = (pageIndex === 1);
    });
    document.querySelectorAll('.right-pag-buttons').forEach(function (boton) {
        boton.disabled = (isLastPage);
        boton.disabled = ((Math.ceil((parseInt(totalResults) / parseInt(pageSize)))) === pageIndex)
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

function enterInSearch(event) {
    if (event.keyCode === 13 || event.which === 13) {
        //submitFiltersForm();
        alert("test");
    }
}
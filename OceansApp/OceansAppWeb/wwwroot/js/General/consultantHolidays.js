$(document).ready(function () {
    getDataForFiltersList();
    getHolidaysList(true, false);
});

function getDataForFiltersList() {
    var url = "/General/ConsultantHolidays/GetUniqueYears";
    $.ajax({
        type: "GET",
        url: url,
        success: function (data) {
            let select = document.getElementById("year");
            data.forEach(function (yearValue) {
                let option = document.createElement("option");
                option.value = yearValue;
                option.text = yearValue;
                select.appendChild(option);
            });
        },
        error: function (error) {
            displayToasterError("More error details: " + error.responseJSON.detail);
            displayToasterError(error.responseJSON.errors + " Ponte en contacto con el administrador para solucionar el problema");
        }
    });
}

function paginationSubmit(firstTime, filters) {
    getHolidaysList(firstTime, filters);
}
function recolectDataFromForm(filters) {
    {
        var searchText = $('#search-input').val();
        var year = parseInt($('#year').val());
        var filtersData = {
            Year: year,
            SearchText: searchText
        };
        var paginationData = returnCurrentPaginationValues();
        if (filters) {
            filtersData = {
                Year: year,
                SearchText: searchText
            };
        }
        return {
            RequestFromFilters: filters,
            Pagination: paginationData,
            Filters: filtersData
        };
    }
}
function getHolidaysList(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/General/ConsultantHolidays/GetHolidaysList?model=" + encodeURIComponent(queryString);
    $.ajax({
        type: "GET",
        url: url,
        success: function (data) {
            var tbody = $(".global-table-container table tbody");
            var noResultsMessage = $(".no-results");
            noResultsMessage.empty();
            tbody.empty();
            data.HolidaysList.forEach(function (holiday) {
                var creationDate = new Date(holiday.CreationDate);
                var formattedDate = ('0' + creationDate.getDate()).slice(-2) + '/' +
                    ('0' + (creationDate.getMonth() + 1)).slice(-2) + '/' +
                    creationDate.getFullYear();
                var row = "<tr>" +
                    "<td class='table-col-big'>" + holiday.Year + "</td>" +
                    "<td class='table-col-little'>" + holiday.Name + "</td>" +
                    "<td class='table-col-medium'>" + formattedDate + "</td>" +
                    "<td class='table-col-medium'>" + holiday.CreatedByName + "</td>" +
                    "</tr>";
                tbody.append(row);
            });
            if (data.HolidaysList.length === 0) {
                noResultsMessage.text("NO SE ENCONTRARON REGISTROS");
            };
            //Pagination
            updatePagination(data.PaginationFilters.Pagination);
            hideSpinner();
        },
        error: function (error) {
            displayToasterError("More error details: " + error.responseJSON.detail);
            displayToasterError(error.responseJSON.errors + " Ponte en contacto con el administrador para solucionar el problema");
        }
    });
}
function updatePagination(paginationData) {
    updatePaginationValues(paginationData);
}

function enterInSearch(event) {
    if (event.keyCode === 13 || event.which === 13) {
        paginationSubmit(false, true);
    }
}
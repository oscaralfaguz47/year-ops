$(document).ready(function () {
    getHolidaysList(true, false);
});

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
                var row = "<tr>" +
                    "<td class='table-col-big'>" + holiday.Year + "</td>" +
                    "<td class='table-col-little'>" + holiday.Name + "</td>" +
                    "<td class='table-col-medium'>" + holiday.CreationDate + "</td>" +
                    "<td class='table-col-medium'>" + holiday.CreatedByName + "</td>" +
                    "</tr>";
                tbody.append(row);
            });
            if (data.HolidaysList.length === 0) {
                noResultsMessage.text("NO EXISTEN REGISTROS");
            };
            //Pagination
            updatePagination(data.PaginationFilters.Pagination);
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
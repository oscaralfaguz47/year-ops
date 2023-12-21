$(document).ready(function () {
    getHolidaysList(true);
});
//document.getElementById("filtersPaginationForm").addEventListener("submit", function (event) {
//    getHolidaysList(false);
//});
function paginationSubmit(firstTime) {
    getHolidaysList(firstTime);
}
function recolectDataFromForm() {
    {
        return {
            Pagination: {
                PageSize: parseInt($('#items-per-page').val()),  
                PageIndex: parseInt($('[name="PageIndex"]').val()) 
            },
            Filters: {
                Year: parseInt($('#year').val()),
                SearchText: $('#search-input').val()  
            }
        };
    }
}
function getHolidaysList(firstTime) {
    var formData = firstTime ? {} : recolectDataFromForm();
    var queryString = JSON.stringify(formData);
    console.log(queryString);
    var url = "/General/ConsultantHolidays/GetHolidaysList?model=" + encodeURIComponent(queryString);
    $.ajax({
        type: "GET",
        url: url,
        success: function (data) {
            var tbody = $(".global-table-container table tbody");
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
            //Pagination
            if (firstTime) {
                updatePagination(data.PaginationFilters.Pagination);
            }
        },
        error: function (error) {
            console.error("Error al obtener lista de Holidays:", error);
        }
    });
}
function updatePagination(paginationData) {
    updatePaginationValues(paginationData);
}
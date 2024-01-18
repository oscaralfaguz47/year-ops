$(document).ready(function () {
    getClientsList(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/ProjectManagement/Clients/GetClientsList?model=" + encodeURIComponent(queryString);

    fetch(url)
        .then(response => {
            return response.json();
        })
        .then(data => {
            if (data.success) {
                console.log(data);
                createUpdateForm.find('[name="consultantHolidayId"]').val(data.holidayData.consultantHolidayId);
                createUpdateForm.find('[name="holidayName"]').val(data.holidayData.name);
                createUpdateForm.find('[name="holidayYear"]').val(data.holidayData.year);
                data.holidayData.holidayDates.forEach(function (holiday) {
                    addNewDateRow(holiday, action)
                });
                showModal(modalId);
            } else {
                displayToasterError(data.error);
                console.error('There has been a problem with the fetch operation:', data.detail);
            }
            hideSpinner();
        });

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
                var formattedDate = ('0' + (creationDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + creationDate.getDate()).slice(-2) + '/' +
                    creationDate.getFullYear();
                var row = `<tr>
                <td>
                    <i onclick="deleteHolidaysList(${holiday.ConsultantHolidayId}, '${holiday.Name}')" class='bi bi-trash3 table-icon delete-table-icon' title="Delete"></i>
                    <i onclick="displayCreateUpdateModal('modal-create-holiday', 'UPDATE HOLIDAYS LIST', ${holiday.ConsultantHolidayId})" class='bi bi-pencil-square table-icon edit-table-icon' title="Edit"></i>
                    <span class="span-holiday-Name" onclick="displayCreateUpdateModal('modal-create-holiday', 'VIEW HOLIDAYS LIST', ${holiday.ConsultantHolidayId})" title="Click to see the Holidays">${holiday.Name}</span>
                </td>
                <td>${holiday.Year}</td>
                <td>${formattedDate}</td>
                <td>${holiday.CreatedByName}</td>
            </tr>`;

                tbody.append(row);
            });
            if (data.HolidaysList.length === 0) {
                noResultsMessage.text("NO SE ENCONTRARON REGISTROS");
            };
            updatePagination(data.PaginationFilters.PaginationWithoutFilters.Pagination);
            hideSpinner();
        },
        error: function (error) {
            displayToasterError("More error details: " + error.responseJSON.detail);
            displayToasterError(error.responseJSON.errors + " Ponte en contacto con el administrador para solucionar el problema");
        }
    });
}
//Pagination and Filters
function paginationSubmit(firstTime, filters) {
    getListOfResults(firstTime, filters);
}
function recolectDataFromForm(filters) {
    {
        var searchText = $('#search-input').val();
        var year = parseInt($('#year').val());
        var filtersData = {
            Year: year,
            SearchText: searchText
        };
        var inputFieldToOrder = document.getElementsByName('fieldToOrder')[0];
        var inputDirectionOrder = document.getElementsByName('directionOrder')[0];
        var orderByData = {
            FieldToOrder: inputFieldToOrder.value,
            DirectionOrder: inputDirectionOrder.value
        }
        var paginationData = returnCurrentPaginationValues();
        var paginationWithoutFilters = {
            Pagination: paginationData,
            RequestFromFilters: filters,
            OrderBy: orderByData
        }
        if (filters) {
            filtersData = {
                Year: year,
                SearchText: searchText
            };
        }
        return {
            Filters: filtersData,
            PaginationWithoutFilters: paginationWithoutFilters
        };
    }
}
function updatePagination(paginationData) {
    updatePaginationValues(paginationData);
}

function enterInSearch(event) {
    if (event.keyCode === 13 || event.which === 13) {
        paginationSubmit(false, true);
    }
}

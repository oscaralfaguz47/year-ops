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
        var inputFieldToOrder = document.getElementsByName('fieldToOrder')[0];
        var inputDirectionOrder = document.getElementsByName('directionOrder')[0];
        var orderByData = {
            FieldToOrder: inputFieldToOrder.value,
            DirectionOrder: inputDirectionOrder.value
        }
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
            Filters: filtersData,
            OrderBy: orderByData
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
                var row = `<tr>
                    <td><i class='bi bi-trash3 table-icon delete-table-icon'></i> 
                    <i onclick="displayCreateUpdateModal('modal-create-holiday', 'UPDATE HOLIDAYS LIST', ${holiday.ConsultantHolidayId})" class='bi bi-pencil-square table-icon edit-table-icon'></i>
                    ${holiday.Name}</td>
                    <td>${holiday.Year}</td>
                    <td>${formattedDate}</td>
                    <td>${holiday.CreatedByName}</td>
                    </tr>`;
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

function displayCreateUpdateModal(modalId, action, holidayId) {
    var modalTitle = $('#create-edit-holiday-title');
    modalTitle.text(action);
    inicializeModalButtons(modalId);
    resetForm('form-create-update');
    var createUpdateForm = $('#form-create-update');
    createUpdateForm.find('[name="consultantHolidayId"]').val("");
    var select = $('#selectYear');
    select.empty();
    // get actual year
    var actualYear = new Date().getFullYear();

    for (var year = 2023; year <= 2040; year++) {
        var option = document.createElement('option');
        option.value = year;
        option.text = year;
        select.append(option);

        if (year === actualYear) {
            option.selected = true;
        }
    }
    var permissionsContainer = $("#holidays-dates-container");
    permissionsContainer.empty();
    var url = "";
    if (holidayId !== null) {
        displaySpinner();
        url = "/General/ConsultantHolidays/GetHolidayListData?holidayId=" + encodeURIComponent(holidayId);
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
                        addNewDateRow(holiday)
                    });
                    showModal(modalId);
                } else {
                    displayToasterError(data.error);
                    console.error('There has been a problem with the fetch operation:', data.detail);
                }
                hideSpinner();
            });
    } else {
        addNewDateRow();
        showModal(modalId);
    }
}
function addNewDateRow(holiday) {
    // Create new row
    var row = document.createElement("div");
    row.className = "holidayRow";


    var inputHiddenId = document.createElement("input");
    inputHiddenId.type = "hidden";
    inputHiddenId.className = "inputHolidayDateId";
    inputHiddenId.value = "";
    row.appendChild(inputHiddenId);
    console.log("HOLIDAY OBJ: " + holiday);
    if (holiday !== null && holiday !== undefined) {
        inputHiddenId.value = holiday.consultantHolidayDateId;
    }
    // Create input text
    var inputText = document.createElement("input");
    inputText.type = "text";
    inputText.className = "inputName";
    inputText.placeholder = "Holiday Name";
    inputText.value = holiday ? holiday.name : '';
    row.appendChild(inputText);

    // Create input date
    var inputDate = document.createElement("input");
    inputDate.type = "date";
    inputDate.className = "inputDate";
    inputDate.value = holiday ? holiday.date.split("T")[0] : '';
    row.appendChild(inputDate);

    // Create delete button
    if (document.querySelectorAll(".holidayRow").length > 0) {
        var btnDelete = document.createElement("button");
        btnDelete.innerHTML = "Delete";
        btnDelete.onclick = function () {
            this.parentElement.remove();
        };
        row.appendChild(btnDelete);
    }
    // Agregar la fila al contenedor
    document.getElementById("holidays-dates-container").appendChild(row);
}
function createUpdateHoliday(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var holidaysListId = createUpdateForm.find('[name="consultantHolidayId"]').val() || null;
    var holidayName = createUpdateForm.find('[name="holidayName"]').val();
    var year = createUpdateForm.find('[name="holidayYear"]').val();
    var holidayDatesElements = document.querySelectorAll(".holidayRow");
    var holidayDatesData = [];
    holidayDatesData = Array.from(holidayDatesElements).map(function (fila) {
        var dateId = fila.querySelector(".inputHolidayDateId").value ? fila.querySelector(".inputHolidayDateId").value : null;
        var name = fila.querySelector(".inputName").value;
        var dateValue = fila.querySelector(".inputDate").value;
        var date = dateValue ? dateValue : null;
        return { consultantHolidayDateId: dateId, Name: name, Date: date };
    });

    var token = $('[name="__RequestVerificationToken"]').val();
    var data = {
        ConsultantHolidayId: holidaysListId,
        Name: holidayName,
        Year: year,
        HolidayDates: holidayDatesData
    };
    console.log(data);
    fetch('/General/ConsultantHolidays/CreateUpdateHoliday', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            RequestVerificationToken: token
        },
        body: JSON.stringify(data)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                createUpdateForm[0].reset();
                hideModal(modalId);
                displayToasterSuccess(data.message);
                getHolidaysList(false, false);
            }
            if (data.messageType === "Validation Error") {
                displayToasterWarningArray(data.errors);
                inicializeModalButtons(modalId);
            } else {
                displayToasterErrorArray(data.errors);
                inicializeModalButtons(modalId);
            }
        })
}

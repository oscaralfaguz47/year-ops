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

function displayCreateUpdateModal(modalId, action, holidayId) {
    var modalTitle = $('#create-edit-holiday-title');
    modalTitle.text(action);
    inicializeModalButtons(modalId);
    resetForm('form-create-update');
    var select = document.getElementById('selectYear');
    // get actual year
    var actualYear = new Date().getFullYear();

    for (var year = 2023; year <= 2040; year++) {
        var option = document.createElement('option');
        option.value = year;
        option.text = year;
        select.appendChild(option);

        if (year === actualYear) {
            option.selected = true;
        }
    }
    var permissionsContainer = $("#holidays-dates-container");
    permissionsContainer.empty();
    var url = "";
    if (holidayId !== null) {
        console.log("EDITAR");
        url = "/General/ConsultantHolidays/GetHolidayListData=" + encodeURIComponent(holidayId);
        fetch(url)
            .then(response => {
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    toastr.success(data.message);
                    hideSpinner();
                } else {
                    displayToasterError(data.error);
                    console.error('There has been a problem with the fetch operation:', data.detail);
                }
            });
    }


    $.ajax({
        type: "GET",
        url: url,
        contentType: "application/json",
        success: function (data) {
            console.log(data);
            showModal(modalId);
            var sysArea = "";
            var sysSubArea = "";
            document.getElementById("roleId").value = holidayId;
            document.getElementById("roleName").value = data.roleName;
            data.userClaims.forEach(function (element) {
                if (sysArea != element.systemAreaName) {
                    permissionsContainer.append('<h5 style="color:var(--clr-blueLight)"><strong>' + element.systemAreaName + '</strong> (Area)</h5>');
                }
                if (sysSubArea != element.systemSubAreaName) {
                    permissionsContainer.append('<h6 style="margin-left:12px"><strong>' + element.systemSubAreaName + '</strong> (Sub Area)</h6>');
                }
                sysArea = element.systemAreaName;
                sysSubArea = element.systemSubAreaName;

                permissionsContainer.append(`
        <div style="margin-left:24px; color:var(--clr-grayDark);font-size:17px;">
            <input id="pe-${element.claimId}" name="${element.claimDescription}" type="checkbox" value="${element.claimId}" ${element.isAddedToTheRole ? 'checked' : ''}>
            <label for="pe-${element.claimId}">&nbsp;${element.claimDescription}</label>
        </div>
    `);

            });
        },
        error: function (error) {
            console.error("Error al obtener lista de roles:", error);
        }
    });
}
function addNewDateRow() {
    // Create new row
    var row = document.createElement("div");
    row.className = "holidayRow";

    // Create input text
    var inputText = document.createElement("input");
    inputText.type = "text";
    inputText.className = "inputName";
    inputText.placeholder = "Holiday Name";
    row.appendChild(inputText);

    // Create input date
    var inputDate = document.createElement("input");
    inputDate.type = "date";
    inputDate.className = "inputDate";
    row.appendChild(inputDate);

    // Create delete button
    var btnDelete = document.createElement("button");
    btnDelete.innerHTML = "Delete";
    btnDelete.onclick = function () {
        this.parentElement.remove();
    };
    row.appendChild(btnDelete);
    // Agregar la fila al contenedor
    document.getElementById("holidays-dates-container").appendChild(row);
}
function createUpdateHoliday(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var holidayName = createUpdateForm.find('[name="holidayName"]').val();
    var year = createUpdateForm.find('[name="holidayYear"]').val();
    var holidayDatesElements = document.querySelectorAll(".holidayRow");
    console.log("ESTE: " + holidayDatesElements.length);
    var holidayDatesData = [{ Name: "", Date: null }];
    if (holidayDatesElements.length > 0) {
        holidayDatesData = Array.from(holidayDatesElements).map(function (fila) {
            var name = fila.querySelector(".inputName").value;
            var date = fila.querySelector(".inputDate").value;
            return { Name: name, Date: date };
        });
    }

    var token = $('[name="__RequestVerificationToken"]').val();
    var data = {
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
            } else {
                displayToasterErrorArray(data.errors);
                inicializeModalButtons();
            }
        })
}

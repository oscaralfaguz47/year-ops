$(document).ready(function () {
    getHolidaysList(true, false);
});

function paginationSubmit(firstTime, filters) {
    getHolidaysList(firstTime, filters);
}
function recolectDataFromForm(filters) {
    {
        var searchText = $('#search-input').val();
        var filtersData = {
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
                SearchText: searchText
            };
        }
        return {
            Filters: filtersData,
            PaginationWithoutFilters: paginationWithoutFilters
        };
    }
}
async function getHolidaysList(firstTime, filters) {
    displaySpinner();

    const formData = firstTime ? {} : recolectDataFromForm(filters);
    const queryString = JSON.stringify(formData);
    const url = `/General/ConsultantHolidays/GetHolidaysList?model=${encodeURIComponent(queryString)}`;

    try {
        const response = await fetch(url);

        if (!response.ok) {
            hideSpinner();
            displayToasterError("Something went wrong, please report this issue.");
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        const tbody = document.querySelector(".global-table-container table tbody");
        const noResultsMessage = document.querySelector(".no-results");

        noResultsMessage.textContent = '';
        tbody.innerHTML = '';

        data.holidaysList.forEach(function (holiday) {
            const row = `<tr class="hover-group">
                <td>
                    <i onclick="deleteHolidaysList(${holiday.consultantHolidayId}, '${holiday.name}')" class='bi bi-trash3 table-icon delete-table-icon' title="Delete"></i>
                    <i onclick="displayCreateUpdateModal('modal-create-holiday', 'UPDATE HOLIDAYS LIST', ${holiday.consultantHolidayId})" class='bi bi-pencil-square table-icon edit-table-icon' title="Edit"></i>
                    <span class="span-holiday-Name" onclick="displayCreateUpdateModal('modal-create-holiday', 'VIEW HOLIDAYS LIST', ${holiday.consultantHolidayId})" title="Click to see the Holidays">${holiday.name}</span>
                </td>
                <td>${holiday.numHolidays}</td>
                <td>${formatUtcToLocalMmDdYyyyTime(holiday.creationDate)}</td>
                <td>${holiday.createdByName}</td>
            </tr>`;
            tbody.insertAdjacentHTML('beforeend', row);
        });

        if (data.holidaysList.length === 0) {
            noResultsMessage.textContent = "NO RECORDS FOUND";
        }

        // Paginación
        updatePagination(data.paginationFilters.paginationWithoutFilters.pagination);
        hideSpinner();
    } catch (error) {
        hideSpinner();
        validateSessionExpiration(error.message);
        const errorResponse = await error.json();
        displayToasterError("More error details: " + errorResponse.detail);
        displayToasterError(errorResponse.errors + " Contact the administrator to resolve the issue.");
    }
}

function updatePagination(paginationData) {
    updatePaginationValues(paginationData);
}

function enterInSearch(event) {
    paginationSubmit(false, true);
}

async function displayCreateUpdateModal(modalId, action, holidayId) {
    var createUpdateForm = $('#form-create-update');
    if (action === 'VIEW HOLIDAYS LIST') {
        $('#btn-saving').css("display", "none");
        $('#btn-cancel').text("Close");
        $('#modal-btns-cont').css("justify-content", "end");
        $('.blue-btn').css("display", "none");

        createUpdateForm.find('[name="holidayName"]').prop('disabled', true);
    } else {
        $('#btn-saving').css("display", "block");
        $('#btn-cancel').text("Cancel");
        $('#modal-btns-cont').css("justify-content", "center");
        $('.blue-btn').css("display", "-webkit-inline-box");

        createUpdateForm.find('[name="holidayName"]').prop('disabled', false);
    }
    var modalTitle = $('#create-edit-holiday-title');
    modalTitle.text(action);
    inicializeModalButtons(modalId);
    resetForm('form-create-update');
    createUpdateForm.find('[name="consultantHolidayId"]').val("");

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
                    createUpdateForm.find('[name="consultantHolidayId"]').val(data.holidayData.consultantHolidayId);
                    createUpdateForm.find('[name="holidayName"]').val(data.holidayData.name);
                    data.holidayData.holidayDates.forEach(function (holiday) {
                        addNewDateRow(holiday, action)
                    });
                    showModal(modalId);
                } else {
                    displayToasterError(data.error);
                    console.error('There has been a problem with the fetch operation:', data.detail);
                }
                hideSpinner();
            })
            .catch(error => {
                validateSessionExpiration(error.message);
            });
    } else {
        addNewDateRow();
        showModal(modalId);
    }
}
function addNewDateRow(holiday, action) {
    // Create new row
    var row = document.createElement("div");
    row.className = "holidayRow";

    if (action === 'VIEW HOLIDAYS LIST') {
        var list = document.createElement("li");
        var span1 = document.createElement("span");
        span1.className = "span-name";
        var span2 = document.createElement("span");
        span1.textContent = document.querySelectorAll(".holidayRow").length + 1 + ". " + holiday.name + " - ";
        span2.textContent = holiday.date;

        var formattedDate = formatDateWeekDayMonthDaySuffix(holiday.date);
        span2.textContent = formattedDate;
        list.appendChild(span1);
        list.appendChild(span2);
        row.appendChild(list);
    } else {
        var inputHiddenId = document.createElement("input");
        inputHiddenId.type = "hidden";
        inputHiddenId.className = "inputHolidayDateId";
        inputHiddenId.value = "";
        row.appendChild(inputHiddenId);
        if (holiday !== null && holiday !== undefined) {
            inputHiddenId.value = holiday.consultantHolidayDateId;
        }
        // Create input text
        var inputText = document.createElement("input");
        inputText.type = "text";
        inputText.className = "inputName form-control";
        inputText.placeholder = "Holiday Name";
        inputText.value = holiday ? holiday.name : '';
        row.appendChild(inputText);

        // Create input date
        var inputDate = document.createElement("input");
        inputDate.type = "date";
        inputDate.className = "inputDate form-control";
        inputDate.value = holiday ? holiday.date.split("T")[0] : '';
        row.appendChild(inputDate);

        // Create delete button
        var btnDelete = document.createElement("button");
        btnDelete.innerHTML = '<i class="bi bi-trash3"></i>';
        btnDelete.className = "btn-delete";
        btnDelete.onclick = function () {
            this.parentElement.remove();
        };
        row.appendChild(btnDelete);
    }

    // Agregar la fila al contenedor
    document.getElementById("holidays-dates-container").appendChild(row);
}
async function createUpdateHoliday(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var holidaysListId = createUpdateForm.find('[name="consultantHolidayId"]').val() || null;
    var holidayName = createUpdateForm.find('[name="holidayName"]').val();
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
        HolidayDates: holidayDatesData
    };
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
        }).catch(error => {
            validateSessionExpiration(error.message);
        })
}
async function deleteHolidaysList(holidaysListId, listName) {
    const confirmation = await Swal.fire({
        title: "Delete Holidays List",
        text: 'Are you sure you want to delete the list "' + listName + '"?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete!',
        cancelButtonText: 'Cancel'
    });
    if (confirmation.isConfirmed) {
        try {
            displaySpinner();
            var token = $('[name="__RequestVerificationToken"]').val();
            var formData = new FormData();
            formData.append('holidaysListId', holidaysListId);
            const response = await fetch('/General/ConsultantHolidays/DeleteHolidaysList', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'RequestVerificationToken': token
                },
                body: formData
            });
            if (!response.ok) {
                const errorData = await response.json();
                switch (errorData.messageType) {
                    case "Validation Error":
                        displayToasterWarning(errorData.error);
                        break;
                    case "Not Found":
                        displayToasterError(errorData.detail);
                        break;
                    default:
                        displayToasterError('An unexpected error occurred: ' + errorData.error);
                }
                hideSpinner();
                return null;
            }
            const data = await response.json();
            hideSpinner();
            getHolidaysList(false, false);
            toastr.success(data.message);
            return data;
        } catch (error) {
            hideSpinner();
            validateSessionExpiration(error.message);
            console.error('Network or fetch error:', error);
            displayToasterError(error.message);
            return null;
        }
    }

}
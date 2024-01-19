$(document).ready(function () {
    getListOfResults(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/ProjectManagement/Clients/GetClientsList?model=" + encodeURIComponent(queryString);

    fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    throw new Error(errorData.detail);
                    displayToasterError(data.error + " Ponte en contacto con el administrador para solucionar el problema");
                });
            }
        })
        .then(data => {
            var tbody = $(".global-table-container table tbody");
            var noResultsMessage = $(".no-results");
            noResultsMessage.empty();
            tbody.empty();
            data.ClientsList.forEach(function (obj) {
                var admissionDate = new Date(obj.AdmissionDate);
                var formattedDate = ('0' + (admissionDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + admissionDate.getDate()).slice(-2) + '/' +
                    admissionDate.getFullYear();
                var isActive = false;
                var clientClass = obj.ClientClass;
                if (obj.ClientClass === 'B') {
                    clientClass = "AA";
                } else if (obj.ClientClass === 'C') {
                    clientClass = "Partner";
                }
                if (obj.IsActive === 'S') {
                    isActive = true;
                }
                var row = `<tr>
                  <td>
                  <i onclick="displayUpdateModal('modal-update-client', ${obj.ClientId})" class='bi bi-pencil-square table-icon edit-table-icon' title="Edit"></i>
                      ${obj.Name}
                  </td>
                  <td style="text-align:center"><label class="switch">
                    <input onchange="activateDeactivate(this, ${obj.ClientId}, '${obj.Name}', ${isActive})" value="${obj.IsActive}" ${isActive ? 'checked' : ''} type="checkbox">
                    <span class="slider round"></span>
                    </label>
                  </td>
                  <td>${obj.Contact === null ? "" : obj.Contact}</td>
                  <td>${obj.ContactOccupation === null ? "" : obj.ContactOccupation}</td>
                  <td>${obj.Emails === null ? "" : obj.Emails}</td>
                  <td style="text-align:center"><label class="switch">
                    <input onchange="activateDeactivateNotifications(this, ${obj.ClientId}, '${obj.Name}', ${obj.AllowSentLatePaymentNotifications})" value="${obj.AllowSentLatePaymentNotifications}" ${obj.AllowSentLatePaymentNotifications ? 'checked' : ''} type="checkbox">
                    <span class="slider round"></span>
                    </label>
                  </td>
                  <td>${formattedDate}</td>
                  <td>${obj.PaymentCondition}</td>
                  <td>${clientClass === null ? "" : clientClass}</td>
                  <td>${obj.Address === null ? "" : obj.Address}</td>
                  <td>${obj.CompanyId}</td>
                  <td>${obj.SuccessManager === null ? "" : obj.SuccessManager}</td>
                  <td>${obj.LatePaymentFee}</td>
                  <td>${obj.AdditionalEmailsForNotifications === null ? "" : obj.AdditionalEmailsForNotifications}</td>
              </tr>`;

                tbody.append(row);
            });
            if (data.ClientsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
            };
            updatePagination(data.PaginationFilters.PaginationWithoutFilters.Pagination);
            hideSpinner();
        });
}
//Activate and deactivate Clients
async function activateDeactivate(inputElement, clientId, name, status) {
    var title = status ? "Deactivate Client" : "Activate Client";
    var textAction = status ? "Deactivate" : "Activate";
    Swal.fire({
        title: title,
        text: 'Are you sure you want to ' + textAction + ' the client "' + name + '"?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, do it!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            displaySpinner();
            var token = $('[name="__RequestVerificationToken"]').val();
            var formData = new FormData();
            formData.append('clientId', clientId);
            fetch("/ProjectManagement/Clients/ActivateDeactivateClient"
                , {
                    method: 'POST',
                    headers: {
                        RequestVerificationToken: token
                    },
                    body: formData
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        toastr.success(data.message);
                        getListOfResults(false, false);
                    } else {
                        inputElement.checked = status;
                        displayToasterError(data.error);
                        console.error('There has been a problem with the fetch operation:', data.detail);
                    }
                })
                .finally(() => {
                    hideSpinner();
                });
        } else {
            inputElement.checked = status;
        }
    });
}

//Activate and deactivate Notifications
async function activateDeactivateNotifications(inputElement, clientId, name, status) {
    displaySpinner();
    var token = $('[name="__RequestVerificationToken"]').val();
    var formData = new FormData();
    formData.append('clientId', clientId);
    fetch("/ProjectManagement/Clients/ActivateDeactivateNotifications"
        , {
            method: 'POST',
            headers: {
                RequestVerificationToken: token
            },
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                toastr.success(data.message);
                getListOfResults(false, false);
            } else {
                inputElement.checked = status;
                displayToasterError(data.error);
                console.error('There has been a problem with the fetch operation:', data.detail);
            }
        })
        .finally(() => {
            hideSpinner();
        });
  
}
//Edit Client
function displayUpdateModal(modalId, clientId) {
    var createUpdateForm = $('#form-update');
    inicializeModalButtons(modalId);
    resetForm('form-update');
    showModal(modalId);
    var url = "/ProjectManagement/Clients/GetClientDataById?clientId=" + encodeURIComponent(clientId);
        //displaySpinner();
        //fetch(url)
        //    .then(response => {
        //        return response.json();
        //    })
        //    .then(data => {
        //        if (data.success) {
        //            console.log(data);
        //            createUpdateForm.find('[name="consultantHolidayId"]').val(data.holidayData.consultantHolidayId);
        //            createUpdateForm.find('[name="holidayName"]').val(data.holidayData.name);
        //            createUpdateForm.find('[name="holidayYear"]').val(data.holidayData.year);
        //            data.holidayData.holidayDates.forEach(function (holiday) {
        //                addNewDateRow(holiday, action)
        //            });
        //            showModal(modalId);
        //        } else {
        //            displayToasterError(data.error);
        //            console.error('There has been a problem with the fetch operation:', data.detail);
        //        }
        //        hideSpinner();
        //    });
}
//Pagination and Filters
function paginationSubmit(firstTime, filters) {
    getListOfResults(firstTime, filters);
}
function recolectDataFromForm(filters) {
    {
        var searchText = $('#search-input').val();
        var activeInactiveValue = document.querySelector('.active-inactive-rg input[type="radio"]:checked')?.value || null;
        var companyValue = document.querySelector('.company-radio-rg input[type="radio"]:checked')?.value || null;
        var datesValMessageSpan = document.getElementById("dates-val-message");
        datesValMessageSpan.textContent = "";
        var startDateValue = document.getElementById("startDate").value || null;
        var endDateValue = document.getElementById("endDate").value || null;
        if (startDateValue === null || endDateValue === null) {
            startDateValue = null;
            endDateValue = null;
        } else if (startDateValue !== null & endDateValue !== null) {
            if (startDateValue > endDateValue) {
                datesValMessageSpan.textContent = "Date From should be less than the Date Until";
            }
        }

        var filtersData = {
            SearchText: searchText,
            IsActive: activeInactiveValue,
            CompanyId: companyValue,
            StartDate: startDateValue,
            EndDate: endDateValue
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
        paginationSubmit(false, true);
}

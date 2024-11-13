let rightSidebarFiltersIsDiplayed = false;
let successManagersArray = [];
let successManagerSelectFilters = null;
let successManagerSelect = document.getElementById('successManager');
let activeInactiveRadioElement = null;
$(document).ready(function () {
    setAccountManagementItemActive();
    getListOfResults(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/AccountManagement/Clients/GetClientsList?model=" + encodeURIComponent(queryString);

    fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    if (errorData.messageType === "Validation Error") {
                        displayToasterWarningArray(errorData.errors);
                    } else {
                        displayToasterErrorArray(errorData.errors);
                    }
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .then(data => {
            var tbody = $(".global-table-container table tbody");
            var noResultsMessage = $(".no-results");
            noResultsMessage.empty();
            tbody.empty();
            data.clientsList.forEach(function (obj) {
                var admissionDate = new Date(obj.admissionDate);
                var formattedDate = ('0' + (admissionDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + admissionDate.getDate()).slice(-2) + '/' +
                    admissionDate.getFullYear();
                var isActive = false;
                var clientClass = "AAA";
                if (obj.clientClass === 'B') {
                    clientClass = "AA";
                } else if (obj.clientClass === 'C') {
                    clientClass = "Partner";
                }
                if (obj.isActive === 'S') {
                    isActive = true;
                }
                var row = `<tr class="hover-group">
                  <td><div class="first-colum-cont"> <img title="Edit" onclick="displayUpdateModal('modal-update-client', ${obj.clientId})" src="/icons/Shared/pensil-edit.svg" />
                      ${obj.name}
                      </div>
                  </td>
                  <td style="text-align:center;"><div style="display:flex"><label class="switch">
                    <input onchange="activateDeactivate(this, ${obj.clientId}, '${obj.name}', ${isActive})" value="${obj.isActive}" ${isActive ? 'checked' : ''} type="checkbox">
                    <span class="slider round"></span>
                    </label></div>
                  </td>
                  <td>${obj.contact === null ? "" : obj.contact}</td>
                  <td>${obj.contactOccupation === null ? "" : obj.contactOccupation}</td>
                  <td>${obj.emails === null ? "" : obj.emails}</td>
                  <td style="text-align:center"><div style="display:flex; justify-content:center"><label class="switch">
                    <input onchange="activateDeactivateNotifications(this, ${obj.clientId}, '${obj.name}', ${obj.allowSentLatePaymentNotifications})" value="${obj.allowSentLatePaymentNotifications}" ${obj.allowSentLatePaymentNotifications ? 'checked' : ''} type="checkbox">
                    <span class="slider round"></span>
                    </label></div>
                  </td>
                  <td>${formattedDate}</td>
                  <td style="text-align:center">${obj.paymentCondition}</td>
                  <td>${clientClass === null ? "" : clientClass}</td>
                  <td>${obj.address === null ? "" : obj.address}</td>
                  <td>${obj.companyId}</td>
                  <td>${obj.successManager === null ? "" : obj.successManager}</td>
                  <td style="text-align:center">${(obj.latePaymentFee * 100)}%</td>
                  <td>${obj.additionalEmailsForNotifications === null ? "" : obj.additionalEmailsForNotifications}</td>
              </tr>`;

                tbody.append(row);
            });

            if (data.clientsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
            };
            updatePagination(data.paginationFilters.paginationWithoutFilters.pagination);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            hideSpinner();
        });
}

//MORE FILTERS
async function displayMoreFiltersClients() {
    if (!rightSidebarFiltersIsDiplayed) {
        displaySpinner();
        let rightSidebarContainer = document.getElementById('right-sidebar-container');
        rightSidebarContainer.innerHTML = `
        <div class="header-btns-container">
         <button class="clear-btn" onclick="clearFilters('filters-form')"><img class="filter-icon" src="/icons/Shared/clear.svg">Clear filters </button>
        </div>
        <div class="scroll-container">
          <form id="filters-form">
           <div class="select-container">
             <label>Success Manager</label>
             <select onchange="paginationSubmit(false, true)" id="SuccessManagerIdFilters" class="form-select">
             </select>
           </div>
           <div class="radio-buttons-container">
            <div class="radio-group active-inactive-rg">
             <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" name="active-inactive" type="radio" value="S" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; Active
             </label>
             <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" name="active-inactive" type="radio" value="N" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; Inactive
             </label>
            </div>
           </div>
           <div class="radio-buttons-container">
            <div class="radio-group company-radio-rg">
             <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" type="radio" name="company" value="OCE" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; Oceans
             </label>
             <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" type="radio" name="company" value="LLC" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; LLC
             </label>
            </div>
           </div>
             <div>
              <div class="start-end-date-container">
                  <label>Admission Date</label>
                  <div class="datepickers-container">
                    <div>
                        <label for="startDate">Date From</label>
                        <input onchange="paginationSubmit(false, true)" class="form-control" type="date" id="startDate" />
                    </div>
                    <div>
                        <label for="endDate">Date Until</label>
                        <input onchange="paginationSubmit(false, true)" class="form-control" type="date" id="endDate" />
                    </div>
                  </div>
                  <label id="dates-val-message" class="validation-message"></label>
              </div>
           </div>
          </form>
        <div>`;

        successManagerSelectFilters = document.getElementById('SuccessManagerIdFilters');
        if (successManagersArray.length === 0) {
            successManagersArray = await getSuccessManagersList();
        }
        populateSelect('SuccessManagerIdFilters', successManagersArray.successManagers, 'All success managers', null);
        activeInactiveRadioElement = document.querySelector('.active-inactive-rg');
        rightSidebarFiltersIsDiplayed = true;
    }
    hideSpinner();
    openRightSidebar();
}
function clearFilters(formId) {
    resetFormElements(formId);
    getListOfResults(false, true);
}

async function getSuccessManagers() {
    const successManagerSelect = document.getElementById('successManager');

    const selectedValue = successManagerSelect.value;

    if (successManagerSelect.options.length > 1) {
        if (selectedValue) {
            successManagerSelect.value = selectedValue;
        }
        return;
    }

    successManagerSelect.innerHTML += '<option value="loading">Loading options… (⏳)</option>';

    try {
        const response = await fetch("/General/ConsultantDetails/GetSuccessManagers");
        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
        }
        const data = await response.json();
        successManagerSelect.innerHTML = '<option value="null">-Select a user-</option>';

        data.successManagers.forEach(obj => {
            const option = new Option(obj.userName, obj.userId);
            successManagerSelect.add(option);
        });
        if (selectedValue) {
            successManagerSelect.value = selectedValue;
        }

    } catch (error) {
            validateSessionExpiration(error.message);
        console.error('Error fetching success managers:', error);
    } finally {
        const loadingOption = successManagerSelect.querySelector('option[value="loading"]');
        if (loadingOption) {
            loadingOption.remove();
        }
    }
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
            fetch("/AccountManagement/Clients/ActivateDeactivateClient"
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
                .catch(error => {
                    validateSessionExpiration(error.message);
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
    fetch("/AccountManagement/Clients/ActivateDeactivateNotifications"
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
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            hideSpinner();
        });

}
//Edit Client
async function displayUpdateModal(modalId, clientId) {
    var createUpdateForm = $('#form-create-update');
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    var permissionsContainer = $("#emails-container");
    permissionsContainer.empty();

    if (successManagersArray.length === 0) {
        successManagersArray = await getSuccessManagersList();
    }
    populateSelect('successManager', successManagersArray.successManagers, '-Select a success manager-', null);

    var url = "/AccountManagement/Clients/GetClientDataById?clientId=" + encodeURIComponent(clientId);
    displaySpinner();
    fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    displayToasterError(errorData.error);
                    hideModal(modalId);
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .then(data => {
            createUpdateForm.find('[name="clientId"]').val(data.clientData.clientId);
            createUpdateForm.find('[name="clientName"]').val(data.clientData.name);
            createUpdateForm.find('[name="contact"]').val(data.clientData.contact);
            createUpdateForm.find('[name="contactOccupation"]').val(data.clientData.contactOccupation);
            createUpdateForm.find('[name="emails"]').val(data.clientData.emails);
            let adDate = new Date(data.clientData.admissionDate);
            createUpdateForm.find('[name="admissionDate"]').val(adDate.toISOString().split('T')[0]);
            createUpdateForm.find('[name="paymentCondition"]').val(data.clientData.paymentCondition);
            createUpdateForm.find('[name="latePaymentFee"]').val(Number(data.clientData.latePaymentFee * 100).toFixed(2));
            createUpdateForm.find('[name="clientClass"]').val(data.clientData.clientClass);
            createUpdateForm.find('[name="address"]').val(data.clientData.address);
            successManagerSelect.value = data.clientData.successManagerId;
            var isActive = data.clientData.isActive === "S" ? true : false;
            createUpdateForm.find('[name="isActive"]').val(isActive);
            createUpdateForm.find('[name="isActive"]').prop('checked', isActive);
            createUpdateForm.find('[name="allowSentLatePaymentNotifications"]').val(data.clientData.allowSentLatePaymentNotifications);
            createUpdateForm.find('[name="allowSentLatePaymentNotifications"]').prop('checked', data.clientData.allowSentLatePaymentNotifications);
            if (data.clientData.additionalEmailsForNotifications !== null) {
                var emailsArray = data.clientData.additionalEmailsForNotifications.split(";");
                emailsArray = emailsArray.map(email => email.trim()).filter(email => email !== "");
                emailsArray.forEach(function (email) {
                    addNewAdditionalEmailRow(email)
                });
            }
            showModal(modalId);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            hideSpinner();
        });
}
function addNewAdditionalEmailRow(email) {
    // Create new row
    var row = document.createElement("div");
    row.className = "emailRow";

    var inputEmail = document.createElement("input");
    inputEmail.type = "text";
    inputEmail.className = "inputEmail form-control";
    inputEmail.placeholder = "Insert an Email";
    inputEmail.value = email;
    row.appendChild(inputEmail);

    // Create delete button
    var btnDelete = document.createElement("button");
    btnDelete.innerHTML = '<i class="bi bi-trash3"></i>';
    btnDelete.className = "btn-delete";
    btnDelete.onclick = function () {
        this.parentElement.remove();
    };
    row.appendChild(btnDelete);

    document.getElementById("emails-container").appendChild(row);
}

//CreateUpdate Client
async function createUpdateClient(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var clientIdData = createUpdateForm.find('[name="clientId"]').val();
    var clientNameData = createUpdateForm.find('[name="clientName"]').val();
    var contactData = createUpdateForm.find('[name="contact"]').val();
    var contactOccupationData = createUpdateForm.find('[name="contactOccupation"]').val();
    var emailsData = createUpdateForm.find('[name="emails"]').val();
    var admissionDateData = createUpdateForm.find('[name="admissionDate"]').val();
    var paymentConditionData = createUpdateForm.find('[name="paymentCondition"]').val();
    var latePaymentFeeData = createUpdateForm.find('[name="latePaymentFee"]').val();
    var clientClassData = createUpdateForm.find('[name="clientClass"]').val();
    var addressData = createUpdateForm.find('[name="address"]').val();
    var isActive = createUpdateForm.find('[name="isActive"]').prop('checked');
    var isActiveData = isActive ? "S" : "N";
    var allowSentLatePaymentNotificationsData = createUpdateForm.find('[name="allowSentLatePaymentNotifications"]').prop('checked');
    var additionalEmailsElement = document.querySelectorAll(".emailRow");
    var additionalEmaislData = "";
    Array.from(additionalEmailsElement).map(function (fila) {
        var email = fila.querySelector(".inputEmail").value;
        if (email !== '' && email !== undefined && email !== null) {
            additionalEmaislData = additionalEmaislData + email + "; ";
        }
    });

    var token = $('[name="__RequestVerificationToken"]').val();
    var data = {
        ClientId: clientIdData,
        Name: clientNameData,
        Contact: contactData,
        ContactOccupation: contactOccupationData,
        Emails: emailsData,
        AdmissionDate: admissionDateData.toString(),
        PaymentCondition: paymentConditionData.toString(),
        LatePaymentFee: Number(latePaymentFeeData).toFixed(2),
        ClientClass: clientClassData,
        Address: addressData,
        SuccessManagerId: successManagerSelect.value === '' ? null : Number(successManagerSelect.value),
        IsActive: isActiveData,
        AllowSentLatePaymentNotifications: Boolean(allowSentLatePaymentNotificationsData),
        AdditionalEmailsForNotifications: additionalEmaislData
    };
    fetch('/AccountManagement/Clients/CreateUpdateClient', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            RequestVerificationToken: token
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    if (errorData.messageType === "Validation Error") {
                        displayToasterWarningArray(errorData.errors);
                        inicializeModalButtons(modalId);
                        throw new Error('Validation errors!');
                    } else {
                        displayToasterError(errorData.error);
                        hideModal(modalId);
                        throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                    }
                });
            }
        })
        .then(data => {
            hideModal(modalId);
            displayToasterSuccess(data.message);
            getListOfResults(false, false);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        });
}
function deleteHolidaysList(holidaysListId, listName) {
    Swal.fire({
        title: "Delete Holidays List",
        text: 'Are you sure you want to delete the list "' + listName + '"?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            displaySpinner();
            var token = $('[name="__RequestVerificationToken"]').val();
            var formData = new FormData();
            formData.append('holidaysListId', holidaysListId);
            fetch("/General/ConsultantHolidays/DeleteHolidaysList"
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
                    } else {
                        displayToasterError(data.error);
                        console.error('There has been a problem with the fetch operation:', data.detail);
                    }
                })
                .catch(error => {
                    validateSessionExpiration(error.message);
                })
                .finally(() => {
                    hideSpinner();
                    getHolidaysList(false, false);
                });
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
        var activeInactiveValue = activeInactiveRadioElement === null ? null : activeInactiveRadioElement.querySelector('.active-inactive-rg input[type="radio"]:checked')?.value || null;
        var companyValue = document.querySelector('.company-radio-rg input[type="radio"]:checked')?.value || null;
        var datesValMessageSpan = document.getElementById("dates-val-message");
        if (datesValMessageSpan !== null) {
            datesValMessageSpan.textContent = "";
        }
        var startDateValue = document.getElementById("startDate") === null ? null : document.getElementById("startDate").value || null;
        var endDateValue = document.getElementById("endDate")  === null ? null : document.getElementById("endDate").value || null;
        if (startDateValue === null || endDateValue === null) {
            startDateValue = null;
            endDateValue = null;
        } else if (startDateValue !== null & endDateValue !== null) {
            if (startDateValue > endDateValue) {
                datesValMessageSpan.textContent = "Date From must be less than Date Until";
            }
        }

        var filtersData = {
            SearchText: searchText,
            IsActive: activeInactiveValue,
            CompanyId: companyValue,
            StartDate: startDateValue,
            EndDate: endDateValue,
            SuccessManagerId: successManagerSelectFilters === null ? null : successManagerSelectFilters.value === '' ? null : Number(successManagerSelectFilters.value)
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


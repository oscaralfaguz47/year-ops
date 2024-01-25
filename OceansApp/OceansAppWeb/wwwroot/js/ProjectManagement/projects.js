$(document).ready(function () {
    getListOfResults(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/ProjectManagement/Projects/GetProjectsList?model=" + encodeURIComponent(queryString);

    fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    displayToasterErrorArray(errorData.errors);
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .then(data => {
            var tbody = $(".global-table-container table tbody");
            var noResultsMessage = $(".no-results");
            noResultsMessage.empty();
            tbody.empty();
            var count = 1;
            data.projectsList.forEach(function (obj) {
                var startDate = new Date(obj.startDate);
                var formattedDate = ('0' + (startDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + startDate.getDate()).slice(-2) + '/' +
                    startDate.getFullYear();
                var row = `<tr>
                  <td>
                  <i onclick="displayUpdateCreateModal('modal-update-client', ${obj.projectId})" class='bi bi-pencil-square table-icon edit-table-icon' title="Edit"></i>
                      ${obj.name}
                  </td>
                  <td>${obj.clientName}</td>
                  <td>${obj.description === null ? "" : obj.description}</td>
                  <td>${formattedDate}</td>
                  <td style="text-align:center"><label class="switch">
                    <input onchange="activateDeactivate(this, ${obj.projectId}, '${obj.name}', ${obj.isActive})" value="${obj.isActive}" ${obj.isActive ? 'checked' : ''} type="checkbox">
                    <span class="slider round"></span>
                    </label>
                  </td>
                  <td>${obj.successManagerName === null ? "" : obj.successManagerName}</td>
                  <td><div class="assigned-consultants-div" id="conAssigned${count}"></div></td>
                  <td class="tracking-tool-td">${obj.clientHasTrackingTool ? '<i class="bi bi-check green-label"></i>' : '<i class="bi bi-x red-label"></i>' }</td>
              </tr>`;
                tbody.append(row);
                addConsultantIcons(obj.numConsultantsAssigned, "conAssigned" + count);
                count++;
            });

            if (data.projectsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
            };
            updatePagination(data.paginationFilters.paginationWithoutFilters.pagination);
        }).finally(() => {
            hideSpinner();
        });
}
function addConsultantIcons(num, tdId) {
    const tdElement = document.getElementById(tdId);
    tdElement.innerHTML = "";
    for (let i = 0; i < Math.min(num, 3); i++) {
        tdElement.innerHTML += '<i style="z-index:' + i +'" class="bi bi-person-fill"></i>';
       // tdElement.innerHTML += '<img src="https://ca.slack-edge.com/TJV63SXV5-U047NF10QH5-b853bea57b67-72" style="z-index:' + i + ';width:35px; border-radius:50%; margin-right: -7px;border: solid 2px #fff;">';
    }
    if (num > 3) {
        tdElement.innerHTML += '<i class="more-consultants-span">+' + (num - 3)+'</i> ';
    }
    if (num === 0) {
        tdElement.innerHTML += '<i style="font-size:21px; margin-right:5px;" class="bi bi-person-x-fill red-label"></i><span class="red-label"> No assigned consultants.</span>';
    }
}

//CREATE / UPDATE PROJECT
async function displayUpdateModal(modalId, action, id) {
    var createUpdateForm = $('#form-create-update');
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    var consultantsContainer = $("#consultants-container");
    consultantsContainer.empty();

    const clientSelect = createUpdateForm.find('[name="client"]')[0];
    clientSelect.innerHTML = '<option>-Select a client-</option>';
    const successManagerSelect = createUpdateForm.find('[name="successManager"]')[0];
    successManagerSelect.innerHTML = '<option>-Select a user-</option>';
    showModal(modalId);
    var url = "/ProjectManagement/Projects/GetProjectDataById?projectId=" + encodeURIComponent(id);
    //displaySpinner();
    //fetch(url)
    //    .then(response => {
    //        if (response.ok) {
    //            return response.json();
    //        } else {
    //            return response.json().then(errorData => {
    //                displayToasterError(errorData.error);
    //                hideModal(modalId);
    //                throw new Error('The request to the server failed!. More details: ' + errorData.detail);
    //            });
    //        }
    //    })
    //    .then(data => {
    //        createUpdateForm.find('[name="clientId"]').val(data.clientData.clientId);
    //        createUpdateForm.find('[name="clientName"]').val(data.clientData.name);
    //        createUpdateForm.find('[name="contact"]').val(data.clientData.contact);
    //        createUpdateForm.find('[name="contactOccupation"]').val(data.clientData.contactOccupation);
    //        createUpdateForm.find('[name="emails"]').val(data.clientData.emails);
    //        let adDate = new Date(data.clientData.admissionDate);
    //        createUpdateForm.find('[name="admissionDate"]').val(adDate.toISOString().split('T')[0]);
    //        createUpdateForm.find('[name="paymentCondition"]').val(data.clientData.paymentCondition);
    //        createUpdateForm.find('[name="latePaymentFee"]').val(Number(data.clientData.latePaymentFee * 100).toFixed(2));
    //        createUpdateForm.find('[name="clientClass"]').val(data.clientData.clientClass);
    //        createUpdateForm.find('[name="address"]').val(data.clientData.address);
    //        if (data.clientData.successManagerId !== null) {
    //            var newOption = document.createElement('option');
    //            newOption.value = data.clientData.successManagerId;
    //            newOption.text = data.clientData.successManager;
    //            newOption.selected = true;
    //            successManagerSelect.appendChild(newOption);
    //        } else {
    //            var nullOption = document.createElement('option');
    //            nullOption.value = null;
    //            nullOption.text = "-Select a user-";
    //            successManagerSelect.appendChild(nullOption);
    //        }
    //        var isActive = data.clientData.isActive === "S" ? true : false;
    //        createUpdateForm.find('[name="isActive"]').val(isActive);
    //        createUpdateForm.find('[name="isActive"]').prop('checked', isActive);
    //        createUpdateForm.find('[name="allowSentLatePaymentNotifications"]').val(data.clientData.allowSentLatePaymentNotifications);
    //        createUpdateForm.find('[name="allowSentLatePaymentNotifications"]').prop('checked', data.clientData.allowSentLatePaymentNotifications);
    //        if (data.clientData.additionalEmailsForNotifications !== null) {
    //            var emailsArray = data.clientData.additionalEmailsForNotifications.split(";");
    //            emailsArray = emailsArray.map(email => email.trim()).filter(email => email !== "");
    //            emailsArray.forEach(function (email) {
    //                addNewAdditionalEmailRow(email)
    //            });
    //        }
    //        showModal(modalId);
    //    })
    //    .finally(() => {
    //        hideSpinner();
    //    });
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
        console.error('Error fetching success managers:', error);
    } finally {
        const loadingOption = successManagerSelect.querySelector('option[value="loading"]');
        if (loadingOption) {
            loadingOption.remove();
        }
    }
}

//Pagination and Filters
function paginationSubmit(firstTime, filters) {
    getListOfResults(firstTime, filters);
}
function recolectDataFromForm(filters) {
    {
        var searchText = $('#search-input').val();
        const activeInactiveRadioElement = document.querySelector('.active-inactive-rg input[type="radio"]:checked');
        var activeInactiveValue = null;
        if (activeInactiveRadioElement !== null) {
            activeInactiveValue = Boolean(document.querySelector('input[name="active-inactive"]:checked').value === 'true');
        }
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
        var clientIdValue = Number(document.getElementById("clientFilter").value) || null;
        var successManagerValue = Number(document.getElementById("succesManagerFilter").value) || null;

        var filtersData = {
            SearchText: searchText,
            IsActive: activeInactiveValue,
            StartDate: startDateValue,
            EndDate: endDateValue,
            ClientId: clientIdValue,
            SuccessManagerId: successManagerValue
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

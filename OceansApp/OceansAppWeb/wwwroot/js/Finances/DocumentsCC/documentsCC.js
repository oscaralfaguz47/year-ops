let rightSidebarFiltersIsDiplayed = false;
let companyRadioElement = null;
let clientsSelectFilters = null;
let documentTypeSelectFilters = null;
let clientsArray = [];

document.addEventListener('DOMContentLoaded', () => {
    setFinancesItemActive();
    getListOfResults(true, false);
});


// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);

    var queryString = JSON.stringify(formData);
    var url = "/Finances/DocumentsCC/GetDocumentsCCList?model=" + encodeURIComponent(queryString);

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
            var tableRows = $(".global-table-container table");
            var noResultsMessage = $(".no-results");
            noResultsMessage.empty();
            tableRows.css("display", "block");
            tbody.empty();
            data.documentsCCList.forEach(function (documentCC) {
                const showNotificationBtn = (
                    documentCC.clientCategory === "EXT" &&
                    documentCC.balanceAmount > 0 &&
                    documentCC.documentType === "FAC" &&
                    documentCC.numDaysToExpire < 0
                );

                const row = `
        <tr class="hover-group">
            <td>
                <div class="cel-with-btns-cont">
                    <span>${documentCC.clientName}</span>
                    ${showNotificationBtn ? `
                        <div class="send-btn-container">
                            <a 
                                title="Ver historial de envío de recordatorios"
                                onclick="getNotificationHistoryByDocument(${documentCC.documentCCId})"
                                id="count-btn-${documentCC.documentCCId}"
                                class="send-btn-num ${documentCC.numNotificationsSent > 0 ? "show-display-contents" : "hide"}">
                                <span id="count-${documentCC.documentCCId}">${documentCC.numNotificationsSent}</span> 
                                <i class="bi bi-envelope"></i>
                            </a>
                        </div>` : ''
                    }
                </div>
            </td>
            <td>${documentCC.documentNumber}</td>
            <td>${documentCC.documentType}</td>
            <td>${formatDate(documentCC.documentDate)}</td>
            <td>${formatDate(documentCC.expirationDate)}</td>
            <td class="${documentCC.numDaysToExpire < 0 && documentCC.documentType === "FAC" ? "red-label" : "green-label"}">
                <strong>${documentCC.numDaysToExpire}</strong>
            </td>
            <td>${formatNumber(documentCC.documentAmount)}</td>
            <td class="${documentCC.balanceAmount > 0 && documentCC.documentType === "FAC" ? "red-label" : "green-label"}">
                <strong>${formatNumber(documentCC.balanceAmount)}</strong>
            </td>
            <td>${documentCC.applicationDescription}</td>
            <td class="${documentCC.canceled === "S" ? "red-label" : "green-label"}">
                <strong><span>${documentCC.canceled === "S" ? "Sí" : "No"}</span></strong>
            </td>
            <td>${documentCC.companyId}</td>
        </tr>
    `;

                tbody.append(row);
            });

            if (data.documentsCCList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
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

//More filters
async function displayMoreFiltersConsultants() {
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
             <label>Client</label>
             <select onchange="paginationSubmit(false, true)" id="client" class="form-select">
             </select>
           </div>
           <div class="select-container">
             <label>Document Type</label>
             <select onchange="paginationSubmit(false, true)" id="documentType" class="form-select">
                <option selected="" value="">All Types</option>
                <option value="FAC">FAC</option>
                <option value="INT">INT</option>
                <option value="L/C">L/C</option>
                <option value="N/C">N/C</option>
                <option value="O/C">O/C</option>
                <option value="O/D">O/D</option>
                <option value="TEF">TEF</option>
             </select>
           </div>
           <div class="radio-buttons-container">
            <div class="radio-group company-rg">
             <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" name="active-inactive" type="radio" value="OCE" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; Oceans Consulting
             </label>
             <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" name="active-inactive" type="radio" value="LLC" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; OCE LLC
             </label>
            </div>
           </div>
             <div>
              <div class="start-end-date-container">
                  <label>Start Date</label>
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

        companyRadioElement = document.querySelector('.company-rg');
        clientsSelectFilters = document.getElementById('client');
        documentTypeSelectFilters = getElementById('documentType');
        if (clientsArray.length === 0) {
            clientsArray = await getClientsList();
        }
        populateSelect('client', clientsArray.clients, 'All clients', null);
        rightSidebarFiltersIsDiplayed = true;
    }
    hideSpinner();
    openRightSidebar();
}

const gearButton = getElementById('gearButton');
const gearDropdownMenu = getElementById('gearDropdownMenu');

// Toggle dropdown on click
gearButton.addEventListener('click', () => {
    gearDropdownMenu.classList.toggle('gear-show');
});

// Close dropdown if clicked outside
document.addEventListener('click', (e) => {
    if (!gearButton.contains(e.target) && !gearDropdownMenu.contains(e.target)) {
        gearDropdownMenu.classList.remove('gear-show');
    }
});
// Close dropdown when clicking on any option inside
gearDropdownMenu.querySelectorAll('a').forEach(option => {
    option.addEventListener('click', () => {
        gearDropdownMenu.classList.remove('gear-show');
    });
});
// Close dropdown when pressing Escape key
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        gearDropdownMenu.classList.remove('gear-show');
    }
});
function clearFilters(formId) {
    resetFormElements(formId);
    getListOfResults(false, true);
}
function formatDate(dateStr) {
    const date = new Date(dateStr);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
}

function formatNumber(num) {
    return Number(num).toLocaleString('es-ES', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}
function paginationSubmit(firstTime, filters) {
    getListOfResults(firstTime, filters);
}
function recolectDataFromForm(filters) {
    {
        var searchText = $('#search-input').val();
        var companyValue = null;
        if (companyRadioElement !== null) {
            const checkedElement = companyRadioElement.querySelector('input[type="radio"]:checked');
            companyValue = checkedElement === null ? null : companyRadioElement.querySelector('input[type="radio"]:checked').value;
        }

        var datesValMessageSpan = document.getElementById("dates-val-message");
        var datesValMessageSpan = document.getElementById("dates-val-message");
        if (datesValMessageSpan !== null) {
            datesValMessageSpan.textContent = "";
        }
        var startDateValue = document.getElementById("startDate") === null ? null : document.getElementById("startDate").value || null;
        var endDateValue = document.getElementById("endDate") === null ? null : document.getElementById("endDate").value || null;
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
            CompanyId: companyValue,
            ClientId: clientsSelectFilters === null ? null : clientsSelectFilters.value === '' || clientsSelectFilters.value === 'null' ? null : Number(clientsSelectFilters.value),
            DocumentType: documentTypeSelectFilters === null ? null : documentTypeSelectFilters.value === '' || documentTypeSelectFilters.value === 'null' ? null : documentTypeSelectFilters.value,
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



function getInvoicesWithDaysExpired() {
    displaySpinner();
    fetch('DocumentsCC/GetInvoicesWithDaysExpired')
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            var modal = document.getElementById("invoices-expired-modal");
            modal.style.display = "block";
            var tbody = $(".invoices-expired-table tbody");
            var noResultsMessage = $("#invoices-expired-modal .no-results");
            tbody.empty();
            noResultsMessage.empty();
            data.forEach(function (invoice) {
                var documentDate = new Date(invoice.documentDate);
                var docDateformattedDate = ('0' + documentDate.getDate()).slice(-2) + '/' +
                    ('0' + (documentDate.getMonth() + 1)).slice(-2) + '/' +
                    documentDate.getFullYear();
                var docExpDate = new Date(invoice.expirationDate);
                var docExpDateformattedDate = ('0' + docExpDate.getDate()).slice(-2) + '/' +
                    ('0' + (docExpDate.getMonth() + 1)).slice(-2) + '/' +
                    docExpDate.getFullYear();
                var row = '<tr class="hover-group">' +
                    "<td class='table-col-big'>" + invoice.clientName + "</td>" +
                    "<td class='table-col-little'>" + invoice.documentNumber + "</td>" +
                    "<td class='table-col-medium'>" + docDateformattedDate + "</td>" +
                    "<td class='table-col-medium'>" + docExpDateformattedDate + "</td>" +
                    "<td class='table-col-medium'>" + invoice.numDaysExpired + "</td>" +
                    "<td class='table-col-medium'>" + invoice.documentAmount + "</td>" +
                    "<td class='table-col-medium'>" + invoice.balanceAmount + "</td>" +
                    '<td class="table-col-little"><div class="cel-with-btns-cont"><button title="Enviar recordatorio de pago" onclick="SendNotification(\'' + invoice.clientName + '\', \'' + invoice.documentCCId + '\')"><i class="bi bi-send-fill"></i></button></div></td>' +
                    "</tr>";
                tbody.append(row);
            });
            if (data.length === 0) {
                noResultsMessage.text("NO TIENES NOTIFICACIONES PENDIENTES DE ENVÍO");
            };
            hideSpinner();
        })
        .catch(error => {
            validateSessionExpiration(error.message);
            console.error('There has been a problem with the fetch operation:', error);
            hideSpinner();
        });
}
function sendStatusToSM() {
    Swal.fire({
        title: "Enviar estado de las facturas pendientes",
        text: "¿Quieres enviarle un estado a los Success Managers?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Si, enviar!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            displaySpinner();
            fetch('DocumentsCC/SendCXCStatus', { method: 'POST' })
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
                    hideSpinner();
                })
                .catch(error => {
                    validateSessionExpiration(error.message);
                });
        }
    });
}
function closeInvoicesExpiredModal() {
    var modal = document.getElementById("invoices-expired-modal");
    modal.style.display = "none";
}

function SendNotification(clientName, documentId) {
    Swal.fire({
        title: "Enviar recordatorio de pago",
        text: "¿Quieres enviarle un recordatorio de pago a " + clientName + "?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Si, enviar!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            displaySpinner();
            $.ajax({
                url: "/Finances/DocumentsCC/SendNotification?documentId=" + documentId,
                type: 'POST',
                success: function (data) {
                    toastr.success(data.message);
                    updateNotificationCount(documentId);
                    getInvoicesWithDaysExpired();
                    hideSpinner();
                },
                error: function (data) {
                    validateSessionExpiration(error.message);
                    displayToasterError(data.responseJSON.error);
                    displayToasterError(data.responseJSON.detail);
                    hideSpinner();
                }
            })
        }
    })
}
function updateNotificationCount(documentId) {
    // Get the current notification count element
    const notificationCountElement = document.getElementById('count-' + documentId);
    const notificationCountBtn = document.getElementById('count-btn-' + documentId);

    notificationCountBtn.classList.add("show-display-contents");
    notificationCountBtn.classList.remove("hide");
    // Increment the notification count by 1
    const currentCount = parseInt(notificationCountElement.textContent, 10);
    const updatedCount = currentCount + 1;

    // Update the content of the notification count element
    notificationCountElement.textContent = updatedCount;
}

function openModal() {
    var modal = document.getElementById("notificationModal");
    modal.style.display = "block";
}

function closeModal() {
    var modal = document.getElementById("notificationModal");
    modal.style.display = "none";
}

function getNotificationHistoryByDocument(documentId) {
    openModal();
    $.ajax({
        url: 'DocumentsCC/GetNotificationsHistoryByDocument?documentId=' + documentId,
        type: 'POST',
        success: function (data) {
            if (data.success) {
                var tbody = document.querySelector("#notificationTable tbody");
                tbody.innerHTML = "";

                data.notificationHistory.result.forEach(function (notification) {
                    var sentDate = new Date(notification.sentDate);

                    var hours = sentDate.getHours();
                    var minutes = sentDate.getMinutes();
                    var ampm = hours >= 12 ? 'PM' : 'AM';
                    hours = hours % 12;
                    hours = hours ? hours : 12; // Si es 0, lo ajustamos a 12

                    var formattedTime = `${hours}:${minutes < 10 ? '0' : ''}${minutes} ${ampm}`;

                    var row = `
                            <tr>
                                <td>${sentDate.getDate()}/${sentDate.getMonth() + 1}/${sentDate.getFullYear()} ${formattedTime}</td>
                                <td>${notification.sentByUser}</td>
                            </tr>
                        `;
                    tbody.insertAdjacentHTML("beforeend", row);
                });
            } else {
                toastr.error(data.error);
            }
        },
        error: function () {
            toastr.error("Error de conexión.");
        }
    });
}

async function getSubtypesAndDocConsecutiveNumber(docTypeId, clientConsultantId, isClient, isCredit) {
    const url = `/Finances/DocumentsCC/GetSubtypesAndDocTypeConsecutiveNumber?docTypeId=${encodeURIComponent(docTypeId)}&clientConsultantId=${encodeURIComponent(clientConsultantId)}&isClient=${encodeURIComponent(isClient)}&isCredit=${encodeURIComponent(isCredit)}`;
    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
        }
        return await response.json();
    } catch (error) {
        validateSessionExpiration(error.message);
        displayToasterError("Internet connection failed");
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}
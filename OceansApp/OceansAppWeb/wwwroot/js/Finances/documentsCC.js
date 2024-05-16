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
            var noResultsMessage = $(".no-results");
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
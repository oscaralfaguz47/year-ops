const beNoResultsMessage = getElementById("be-no-results");
const beSpinner = getElementById('be-spinner');
const bePagination = getElementById('be-pagination-container');
const beTable = getElementById('book-entries-table');

async function getBookEntriesList(firstTime, filters) {
    beSpinner.style.display = 'block';
    beNoResultsMessage.innerText = '';
    bePagination.style.display = 'none';
    beTable.style.display = 'none';
    var formData = recolectDataFromFormBE(filters, firstTime);
    var queryString = JSON.stringify(formData);
    var url = "/Finances/ExportAccountingData/GetBookEntriesList?model=" + encodeURIComponent(queryString);

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
            var tbody = $(".book-entries-table-cont table tbody");
            var tableRows = $(".book-entries-table-cont table");
            tbody.empty();
            data.bookEntriesList.forEach(function (obj, index) {
                var creationDate = new Date(obj.creationDate);
                var creationDateformattedDate = ('0' + (creationDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + creationDate.getDate()).slice(-2) + '/' +
                    creationDate.getFullYear();

                var row = `<tr class="hover-group">
                  <td>${obj.parentId}</td>
                  <td>${creationDateformattedDate}</td>
                  <td>${obj.companyName}</td>
                  <td>${obj.numValidChildren}</td>
                  <td>${obj.numVoidedChildren}</td>
                  <td id="td-be-status-${obj.parentId}">${getStatusLabel(obj.transactionStatusName)}</td>
                  <td><button onclick="exportBookEntriesData(this, ${obj.parentId}, '${obj.transactionStatusName}')" class="export-btn"><img class="global-icon" src="/icons/Shared/download.svg">Export Data</button></td>
              </tr>`;
                tbody.append(row);
            });

            if (data.bookEntriesList.length === 0) {
                beNoResultsMessage.innerText = 'NO RECORDS FOUND';
                tableRows.css("display", "none");
            } else {
                tableRows.css("display", "block");
            };
            updatePaginationBE(data.paginationFilters.paginationWithoutFilters.pagination);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            bePagination.style.display = 'block';
            beTable.style.display = 'block';
            beSpinner.style.display = 'none';
        });
}

//Pagination and Filters
function paginationSubmitP2(firstTime, filters) {
    getBookEntriesList(firstTime, filters);
}
function recolectDataFromFormBE(filters) {
    {
        var filtersData = {
            CompanyId: null,
            TransactionStatusId: null
        };
        var inputFieldToOrder = document.getElementsByName('fieldToOrderP2')[0];
        var inputDirectionOrder = document.getElementsByName('directionOrderP2')[0];
        var orderByData = {
            FieldToOrder: inputFieldToOrder.value,
            DirectionOrder: inputDirectionOrder.value
        }
        var paginationData = returnCurrentPaginationValuesP2();
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
function updatePaginationBE(paginationData) {
    updatePaginationValuesP2(paginationData);
}



async function exportBookEntriesData(downloadButton, parentId, status) {
    if (status !== "Accounted") {
        const confirmation = await Swal.fire({
            title: "Download File",
            text: `By downloading the file, you accept that it will be imported into Softland and the status will change. Do you want to continue?`,
            icon: 'warning',
            showCancelButton: true,
            cancelButtonText: 'Cancel',
            cancelButtonColor: '#9ba8b8',
            confirmButtonColor: '#eeb30f',
            confirmButtonText: 'Yes, Download!'
        });

        if (!confirmation.isConfirmed) {
            return;
        }
        downloadButton.onclick = function () {
            exportBookEntriesData(downloadButton, parentId, "Accounted");
        };
    }
    downloadButton.innerHTML = `<div class="button-spinner"></div> In Progress...`;
    const tdStatus = getElementById(`td-be-status-${parentId}`);
    const buttonDefaultContent = `<img class="global-icon" src="/icons/Shared/download.svg">Export Data</button>`;
    try {
        downloadButton.disabled = true;
        url = `/Finances/ExportAccountingData/ExportBookEntries?parentId=${encodeURIComponent(parentId)}`;
        const response = await fetch(url);

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            downloadButton.disabled = false;
            downloadButton.innerHTML = buttonDefaultContent;
            return null;
        }
        const dataFromApi = await response.json();

        let dataToExportList = [];
        dataFromApi.bookEntriesData.forEach(function (obj, index) {
            let bookEntryMovement = {
                "Cuenta Bancaria": obj.bankAccount,
                "Fecha": formatDateMmDdYyyy(obj.accountingDate),
                "Número": obj.referenceNumber,
                "Concepto": obj.notes,
                "Monto": obj.paymentAmount,
                "Tipo Documento": obj.documentType,
                "Subtipo Documento": obj.documentSubType,
                "Fecha Contable": formatDateMmDdYyyy(obj.accountingDate),
                "Tipo Asiento": obj.entryType,
                "Paquete": obj.entryType,
                "Cod Impuesto": obj.taxCode
            };

            dataToExportList.push(bookEntryMovement);
        });

        const workbook = XLSX.utils.book_new();

        const worksheet1 = XLSX.utils.json_to_sheet(dataToExportList);

        XLSX.utils.book_append_sheet(workbook, worksheet1, 'movimientos_en_libros');

        const now = new Date().toLocaleString().replace(/[\/, ]/g, "_").replace(/:/g, "-");
        XLSX.writeFile(workbook, `Movimientos en libros_${now}.xlsx`);

        displayToasterSuccess("The file was downloaded sucessfully");
        tdStatus.innerHTML = getStatusLabel('Accounted');
        downloadButton.disabled = false;
        downloadButton.innerHTML = buttonDefaultContent;
    } catch (error) {
        downloadButton.disabled = false;
        downloadButton.innerHTML = buttonDefaultContent;
        validateSessionExpiration(error.message);
        console.error('Network or fetch error:', error.message);
        displayToasterError('Something went wrong, more details: ' + error);
    }
}
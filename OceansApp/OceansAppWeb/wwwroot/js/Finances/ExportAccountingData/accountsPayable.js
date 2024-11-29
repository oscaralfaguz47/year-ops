const noResultsMessage = getElementById('ap-no-results');
const apSpinner = getElementById('ap-spinner');
const apPagination = getElementById('ap-pagination-container');
const apTable = getElementById('accounts-payable-table');

async function getJournalAccountsPayableList(firstTime, filters) {
    apSpinner.style.display = 'block';
    noResultsMessage.innerText = '';
    apPagination.style.display = 'none';
    apTable.style.display = 'none';
    var formData = recolectDataFromForm(filters, firstTime);
    var queryString = JSON.stringify(formData);
    var url = "/Finances/ExportAccountingData/GetJournalAccountsPayableList?model=" + encodeURIComponent(queryString);

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
            var tbody = $(".accounts-payable-table-cont table tbody");
            var tableRows = $(".accounts-payable-table-cont table");
            tbody.empty();
            data.journalAccountsPayableList.forEach(function (obj, index) {
                var accountingDate = new Date(obj.accountingDate);
                var accountingDateformattedDate = ('0' + (accountingDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + accountingDate.getDate()).slice(-2) + '/' +
                    accountingDate.getFullYear();

                var startPeriodDate = new Date(obj.startDatePeriod);
                var startPeriodDateformattedDate = ('0' + (startPeriodDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + startPeriodDate.getDate()).slice(-2) + '/' +
                    startPeriodDate.getFullYear();

                var endPeriodDate = new Date(obj.endDatePeriod);
                var endPeriodDateformattedDate = ('0' + (endPeriodDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + endPeriodDate.getDate()).slice(-2) + '/' +
                    endPeriodDate.getFullYear();

                var row = `<tr class="hover-group">
                  <td>${obj.seatNumber}</td>
                  <td>${obj.companyName}</td>
                  <td>${accountingDateformattedDate}</td>
                  <td>${startPeriodDateformattedDate}</td>
                  <td>${endPeriodDateformattedDate}</td>
                  <td id="td-ap-status-${obj.journalId}">${getStatusLabel(obj.transactionStatusName)}</td>
                  <td><button onclick="exportAccountsPayableData(this,${obj.journalId}, '${obj.transactionStatusName}')" class="export-btn"><img class="global-icon" src="/icons/Shared/download.svg">Export Data</button></td>
              </tr>`;
                tbody.append(row);
            });

            if (data.journalAccountsPayableList.length === 0) {
                noResultsMessage.innerText = 'NO RECORDS FOUND';
                tableRows.css("display", "none");
            } else {
                tableRows.css("display", "block");
            };
            updatePagination(data.paginationFilters.paginationWithoutFilters.pagination);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {
            apPagination.style.display = 'block';
            apTable.style.display = 'block';
            apSpinner.style.display = 'none';
        });
}

//Pagination and Filters

function paginationSubmit(firstTime, filters) {
    getJournalAccountsPayableList(firstTime, filters);
}
function recolectDataFromForm(filters) {
    {
        var filtersData = {
            CompanyId: null,
            TransactionStatusId: null
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

async function exportAccountsPayableData(downloadButton, journalId, status) {
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
            exportAccountsPayableData(downloadButton, journalId, "Accounted"); 
        };
    }
    downloadButton.innerHTML = `<div class="button-spinner"></div> In Progress...`;
    const tdStatus = getElementById(`td-ap-status-${journalId}`);
    const buttonDefaultContent = `<img class="global-icon" src="/icons/Shared/download.svg">Export Data</button>`;
    try {
        downloadButton.disabled = true;
        url = `/Finances/ExportAccountingData/ExportJournalAccountsPayable?journalId=${encodeURIComponent(journalId)}`;
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

        // Create a new Excel workbook
        const workbook = XLSX.utils.book_new();

        const parentData = [
            {
                "Asiento": dataFromApi.journalAccountPayableData.entry,
                "Paquete": dataFromApi.journalAccountPayableData.accountingPackage,
                "Tipo Asiento": dataFromApi.journalAccountPayableData.entryType,
                "Fecha": cleanValue(formatDateMmDdYyyy(dataFromApi.journalAccountPayableData.accountingDate)),
                "Contabilidad": dataFromApi.journalAccountPayableData.accounting
            }
        ];

        let entriesData = [];
        dataFromApi.journalAccountPayableData.entriesList.forEach(function (obj, index) {
            let entryData =
            {
                "Asiento": cleanValue(dataFromApi.journalAccountPayableData.entry),
                "Consecutivo": cleanValue(index + 1),
                "Nit": cleanValue(obj.nit),
                "Centro De Costo": cleanValue(obj.costCenter),
                "Cuenta Contable": cleanValue(obj.accountingAccount),
                "Fuente": cleanValue(obj.source),
                "Referencia": cleanValue(obj.reference),
                "Débito Local": obj.debit === 0 ? undefined : obj.debit,
                "Débito Dólar": obj.debit === 0 ? undefined : obj.debit,
                "Crédito Local": obj.credit === 0 ? undefined : obj.credit,
                "Crédito Dólar": obj.credit === 0 ? undefined : obj.credit
            }
            entriesData.push(entryData);
        });

        // Convert each data set to Excel sheets and add them to the workbook
        const worksheet1 = XLSX.utils.json_to_sheet(parentData);
        const worksheet2 = XLSX.utils.json_to_sheet(entriesData);

        for (let i = 0; i < entriesData.length; i++) {
            const cellDate = `D${i + 2}`; 
            if (worksheet1[cellDate]) {
                worksheet1[cellDate].z = 'mm/dd/yyyy'; 
            }
        }

        // Add the sheets to the book, with custom names
        XLSX.utils.book_append_sheet(workbook, worksheet1, 'asiento_de_diario');
        XLSX.utils.book_append_sheet(workbook, worksheet2, 'diario');

        XLSX.writeFile(workbook, `plantilla_asiento_diario_CXP_${dataFromApi.journalAccountPayableData.entry}.xlsx`);
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


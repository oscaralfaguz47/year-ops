const beNoResultsMessage = $(".b-e-no-results");
const bookEntriesContent = getElementById('book-entriies-tab-content');

async function getBookEntriesList(firstTime, filters) {
    accountsPayablePartial = null;
    accountsPayableContent.innerHTML = '';
    if (bookEntriesPartial === null) {
        booEntriesPartial = await getBookEntriesPartialView();
        bookEntriesContent.innerHTML = booEntriesPartial;
    }
    var formData = recolectDataFromForm(filters, firstTime);
    console.log(formData);
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
            beNoResultsMessage.empty();
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
                  <td>${obj.transactionStatusName}</td>
                  <td>${obj.numValidChildren}</td>
                  <td>${obj.numVoidedChildren}</td>
                  <td></td>
              </tr>`;
                tbody.append(row);
            });

            if (data.bookEntriesList.length === 0) {
                beNoResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
            } else {
                tableRows.css("display", "block");
            };
            console.log(data.paginationFilters.paginationWithoutFilters.pagination);
            updatePagination(data.paginationFilters.paginationWithoutFilters.pagination);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        })
        .finally(() => {

        });
}

//Pagination and Filters

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


async function getBookEntriesPartialView() {
    const url = `/Finances/ExportAccountingData/GetBookEntriesPartial`;

    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorData = await response.text();
            displayToasterError("Error loading component");
            throw new Error(`The request to the server failed! More details: ${errorData}`);
        }

        const htmlContent = await response.text();
        return htmlContent;
    } catch (error) {
        validateSessionExpiration(error.message);
        displayToasterError("Internet connection failed");
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}
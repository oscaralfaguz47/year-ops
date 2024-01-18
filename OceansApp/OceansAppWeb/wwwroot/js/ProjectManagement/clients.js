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
            console.log(data);
            var tbody = $(".global-table-container table tbody");
            var noResultsMessage = $(".no-results");
            noResultsMessage.empty();
            tbody.empty();
            data.ClientsList.forEach(function (obj) {
                var creationDate = new Date(obj.CreationDate);
                var formattedDate = ('0' + (creationDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + creationDate.getDate()).slice(-2) + '/' +
                    creationDate.getFullYear();
                var row = `<tr>
                  <td>
                      ${obj.Name}
                  </td>
                  <td>${obj.Contact}</td>
                  <td>${obj.ContactOccupation}</td>
                  <td>${obj.Emails}</td>
                  <td>${obj.AdmissionDate}</td>
                  <td>${obj.PaymentCondition}</td>
                  <td>${obj.IsActive}</td>
                  <td>${obj.ClientClass}</td>
                  <td>${obj.Address}</td>
                  <td>${obj.CompanyId}</td>
                  <td>${obj.SuccessManager}</td>
                  <td>${obj.LatePaymentFee}</td>
                  <td>${obj.AdditionalEmailsForNotifications}</td>
                  <td>${obj.AllowSentLatePaymentNotifications}</td>
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
//Pagination and Filters
function paginationSubmit(firstTime, filters) {
    getListOfResults(firstTime, filters);
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
        var paginationWithoutFilters = {
            Pagination: paginationData,
            RequestFromFilters: filters,
            OrderBy: orderByData
        }
        if (filters) {
            filtersData = {
                Year: year,
                SearchText: searchText
            };
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

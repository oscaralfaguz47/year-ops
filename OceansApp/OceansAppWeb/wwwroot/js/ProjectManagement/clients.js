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
                      ${obj.Name}
                  </td>
                  <td style="text-align:center"><label class="switch">
                    <input value="${obj.IsActive}" ${isActive ? 'checked' : ''} type="checkbox">
                    <span class="slider round"></span>
                    </label>
                  </td>
                  <td>${obj.Contact === null ? "" : obj.Contact}</td>
                  <td>${obj.ContactOccupation === null ? "" : obj.ContactOccupation}</td>
                  <td>${obj.Emails === null ? "" : obj.Emails}</td>
                  <td style="text-align:center"><label class="switch">
                    <input value="${obj.AllowSentLatePaymentNotifications}" ${obj.AllowSentLatePaymentNotifications ? 'checked' : ''} type="checkbox">
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

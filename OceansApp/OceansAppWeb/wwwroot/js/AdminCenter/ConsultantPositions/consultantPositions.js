$(document).ready(function () {
    getListOfResults(true, false);
});

async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = recolectDataFromForm(filters, firstTime);
    var queryString = JSON.stringify(formData);
    var url = "/AdminCenter/ConsultantPositions/GetConsultantsToPayList?model=" + encodeURIComponent(queryString);

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
            tbody.empty();

            console.log(data);
            data.positionsList.forEach(function (obj) {

                var editBtn = ``;
                var menuBtn = `<i title="You are not able to edit, it already has status: ${obj.transactionStatusName}" style="cursor:pointer; color: var(--clr-blueLight);" class="bi bi-exclamation-circle"></i> `;
                if (obj.transactionStatusName !== "Rejected" && (obj.transactionStatusName === "Approved" || obj.transactionStatusName === "Waiting to be approved")) {
                    editBtn = `<li onclick="displayUpdateCreateInterviewModal('modal-update-create-interview', ${obj.interviewId})""><i class="bi bi-pencil-square"></i> Edit</li>`;
                    menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.interviewId}', 'menuIcon-${obj.interviewId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.interviewId}"></i>
                              <div class="menu-options" id="menuOptions-${obj.interviewId}">
                               <ul>
                                 ${editBtn}
                               </ul>
                              </div>`;
                }

                var row = `<tr class="hover-group">
                  <td>
                      ${menuBtn}
                      ${obj.consultantName}
                  </td>
                  <td>${obj.durationMinutes.toFixed(2)} minutes</td>
                  <td>${((1 / 60) * obj.durationMinutes).toFixed(2)} hours</td>
              </tr>`;
                tbody.append(row);
            });

            if (data.interviewsList.length === 0) {
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


//Pagination and Filters
function paginationSubmit(firstTime, filters) {
    getListOfResults(firstTime, filters);
}
function recolectDataFromForm(filters, firstTime) {
    {
        var searchText = $('#search-input').val();
        let startDateData = new Date(dateFromInput.value).toISOString();
        let endDateData = new Date(dateToInput.value).toISOString();

        var filtersData = {
            SearchText: searchText,
            StartDate: startDateData,
            EndDate: endDateData,
            PaymentPeriod: Number(paymentPeriodSelect.value)
        };
        var inputFieldToOrder = document.getElementsByName('fieldToOrder')[0];
        var inputDirectionOrder = document.getElementsByName('directionOrder')[0];
        var orderByData = {
            FieldToOrder: inputFieldToOrder.value,
            DirectionOrder: inputDirectionOrder.value
        }
        var paginationData = returnCurrentPaginationValues();
        if (firstTime) {
            paginationData.PageSize = 50;
        }
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
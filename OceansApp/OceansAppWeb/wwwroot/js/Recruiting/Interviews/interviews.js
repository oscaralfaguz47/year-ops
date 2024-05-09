$(document).ready(function () {
    getListOfResults(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/Recruiting/Interviews/GetInterviewsList?model=" + encodeURIComponent(queryString);

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
            data.interviewsList.forEach(function (obj) {
                var actionDate = new Date(obj.date);
                var actionformattedDate = ('0' + (actionDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + actionDate.getDate()).slice(-2) + '/' +
                    actionDate.getFullYear();

                var creationDate = new Date(obj.creationDate);
                var creationformattedDate = ('0' + (creationDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + creationDate.getDate()).slice(-2) + '/' +
                    creationDate.getFullYear();

                var updateDate = new Date(obj.lastUpdateDate);
                var updateformattedDate = ('0' + (updateDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + updateDate.getDate()).slice(-2) + '/' +
                    updateDate.getFullYear();

                var rejectBtn = ``;
                var editBtn = ``;
                var menuBtn = `<i title="You are not able to edit, it already has status: ${obj.transactionStatusName}" style="cursor:pointer; color: var(--clr-blueLight);" class="bi bi-exclamation-circle"></i> `;
                if (obj.transactionStatusName !== "Rejected" && (obj.transactionStatusName === "Approved" || obj.transactionStatusName === "Waiting to be approved")) {
                    rejectBtn = `<li onclick="rejectInterview(${obj.interviewId}, '${obj.consultantName}', '${obj.transactionTypeName}')""><i class="red-label bi bi-x-lg"></i> Reject</li>`;
                    editBtn = `<li onclick="displayUpdateCreateInterviewModal('modal-update-create-interview', ${obj.interviewId})""><i class="bi bi-pencil-square"></i> Edit</li>`;
                    menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.interviewId}', 'menuIcon-${obj.interviewId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.interviewId}"></i>
                              <div class="menu-options" id="menuOptions-${obj.interviewId}">
                               <ul>
                                 ${editBtn}
                                 ${rejectBtn}
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
                  <td>${actionformattedDate}</td>
                  <td>${getStatusLabel(obj.transactionStatusName) }</td>
                  <td>${obj.createdBy}</td>
                  <td>${creationformattedDate}</td>
                  <td>${obj.lastUpdatedBy === null ? "Not updated" : obj.lastUpdatedBy}</td>
                  <td>${obj.lastUpdateDate === null ? "Not updated" : updateformattedDate}</td>
              </tr>`;
                tbody.append(row);
            });

            if (data.interviewsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
            };
            updatePagination(data.paginationFilters.paginationWithoutFilters.pagination);
        }).finally(() => {
            hideSpinner();
        });
}

// REJECT DEBIT CREDIT
async function rejectInterview(interviewId, consultantName) {
    Swal.fire({
        title: "Reject Interview",
        text: 'Are you sure you want to reject the interview for ' + consultantName + '?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, Reject!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            displaySpinner();
            var token = $('[name="__RequestVerificationToken"]').val();
            var formData = new FormData();
            formData.append('interviewId', interviewId);
            fetch("/Recruiting/Interviews/RejectInterview"
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
                    getListOfResults(false, false);
                })
                .finally(() => {
                    hideSpinner();
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

        var filtersData = {
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

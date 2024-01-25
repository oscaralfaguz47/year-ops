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
    for (let i = 0; i < Math.min(num, 4); i++) {
        tdElement.innerHTML += '<i style="z-index:' + i +'" class="bi bi-person-fill"></i>';
    }
    if (num > 4) {
        tdElement.innerHTML += '<i class="more-consultants-span">+' + (num - 4)+'</i> ';
    }
    if (num === 0) {
        tdElement.innerHTML += '<i style="font-size:21px; margin-right:5px;" class="bi bi-person-x-fill red-label"></i><span class="red-label"> No assigned consultants.</span>';
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

$(document).ready(function () {
    getListOfResults(true, false);
});

// -Get list
async function getListOfResults(firstTime, filters) {
    displaySpinner();
    var formData = firstTime ? {} : recolectDataFromForm(filters);
    var queryString = JSON.stringify(formData);
    var url = "/General/Consultants/GetConsultantsList?model=" + encodeURIComponent(queryString);

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
            console.log(data);
            var tbody = $(".global-table-container table tbody");
            var tableRows = $(".global-table-container table");
            var noResultsMessage = $(".no-results");
            noResultsMessage.empty();
            tableRows.css("display", "block");
            tbody.empty();
            var count = 5;
            data.consultantsList.forEach(function (obj) {
                var projectsJson = JSON.parse(obj.consultantProjects);
                var projectsSpan = "";
                if (projectsJson !== null) {
                    projectsJson.forEach(function (pos) {
                        projectsSpan += `<span class="project-span"><span>${pos.Name}</span>${pos.IsActive ? '<span class="green-label"> (Active)</span>' : '<span class="red-label"> (Inactive)</span>'}</span>`;
                    });
                } else {
                    projectsSpan = '<strong class="red-label">Not yet assigned to a project.</strong>';
                }
                var resetTwoFactorBtn = '';
                if (obj.twoFactorEnabled) {
                    resetTwoFactorBtn = `<li onclick="resetTwoFactorAuth(${obj.consultantId}, '${obj.consultantName}')"><i class="bi bi-arrow-counterclockwise"></i> Reset Two-Factor</li>`;
                }
                var row = `<tr>
                  <td>
                        <i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.consultantId}', 'menuIcon-${obj.consultantId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.consultantId}"></i>
                          <div class="menu-options" id="menuOptions-${obj.consultantId}">
                           <ul>
                             <li onclick="activateInactivateConsultantUser(${obj.consultantId}, '${obj.consultantName}', ${obj.isActive})" id="activate-deactivate-li-${obj.consultantId}">${obj.isActive ? '<i class="bi bi-x-lg red-label"></i>' : '<i class="bi bi-plus-lg green-label"></i>'}${obj.isActive ? ' Deactivate user' : ' Activate user'}</li>
                             <li onclick="displayUpdateCreateConsultantModal('modal-update-create-consultant', ${obj.consultantId})""><i class="bi bi-pencil-square"></i> Edit Consultant</li>
                             ${resetTwoFactorBtn}
                           </ul>
                         </div>
                       <i onclick="displayUpdateCreateConsultantModal('modal-update-create-consultant', ${obj.consultantId})" class='bi bi-pencil-square table-icon edit-table-icon' title="Edit"></i>
                      ${obj.consultantName}
                  </td>
                  <td>${obj.internalEmail}</td>
                  <td>${obj.personalEmail === null ? "" : obj.personalEmail}</td>
                  <td>${obj.countryName}</td>
                  <td>${obj.userCategoryName}</td>
                  <td>${obj.consultantPositions === null ? "" : obj.consultantPositions}</td>
                  <td>${projectsSpan}</td>
                  <td>${obj.phoneNumber === null ? "" : obj.phoneNumber}</td>
                  <td>${obj.phone2 === null ? "" : obj.phone2}</td>
                  <td>${obj.address === null ? "" : obj.address}</td>
                  <!--<td>${obj.location === null ? "" : `<div class="location-cont">
                        <button onclick="copyToClipboard('${obj.location}', 'The location of: ' + '${obj.consultantName}' + ' was copied to the clipboard!')" >
                            <i class="bi bi-clipboard-fill"></i> Copy location
                        </button> &nbsp;
                        <a href="${obj.location}" target="_blank" class="link"><i class="bi bi-geo-alt-fill"></i> Redirect to location</a>
                                    </div>`}
                  </td>-->
                   <td class="shared-table-td">${obj.isActive ? '<span class="green-label">Active</span>' : '<span class="red-label">Inactive</span>'}</td>
                  <td class="shared-table-td">${obj.twoFactorEnabled ? '<i class="bi bi-check green-label"></i>' : '<i class="bi bi-x red-label"></i>'}</td>
                  <td class="shared-table-td">${obj.emailConfirmed ? '<i class="bi bi-check green-label"></i>' : '<i class="bi bi-x red-label"></i>'}</td>
                  <td class="shared-table-td">${!obj.isLockedOut ? '<i class="bi bi-unlock-fill green-label"></i>' : '<i class="bi bi-lock-fill red-label"></i>'}</td>
              </tr>`;
                tbody.append(row);
                count++;
            });

            if (data.consultantsList.length === 0) {
                noResultsMessage.text("NO RECORDS FOUND");
                tableRows.css("display", "none");
            };
            updatePagination(data.paginationFilters.paginationWithoutFilters.pagination);
        }).finally(() => {
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
        const activeInactiveRadioElement = document.querySelector('.active-inactive-rg input[type="radio"]:checked');
        var activeInactiveValue = null;
        if (activeInactiveRadioElement !== null) {
            activeInactiveValue = Boolean(document.querySelector('input[name="active-inactive"]:checked').value === 'true');
        }
        const twoFactorRadioElement = document.querySelector('.twoFactor-rg input[type="radio"]:checked');
        var isTwoFactorEnabledValue = null;
        if (twoFactorRadioElement !== null) {
            isTwoFactorEnabledValue = Boolean(document.querySelector('input[name="two-factor"]:checked').value === 'true');
        }
        var countryValue = document.getElementById("CountrySelect").value || null;

        var filtersData = {
            SearchText: searchText,
            IsActive: activeInactiveValue,
            CountryId: countryValue,
            IsTwoFactorEnabled: isTwoFactorEnabledValue
        };
        console.log(filtersData);
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

// Reset Two-Factor Auth
async function resetTwoFactorAuth(consultantId, name) {
    Swal.fire({
        title: "Reset Two-Factor Auth",
        text: 'Are you sure you want to reset the Two-Factor Authentication from ' + name + '?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, do it!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            displaySpinner();
            var token = $('[name="__RequestVerificationToken"]').val();
            var formData = new FormData();
            formData.append('consultantId', consultantId);
            fetch("/General/Consultants/ResetAuthenticatorFromUser"
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
                        getListOfResults(false, false);
                    } else {
                        displayToasterError(data.error);
                        console.error('There has been a problem with the fetch operation:', data.detail);
                    }
                })
                .finally(() => {
                    hideSpinner();
                });
        }
    });
}
// Activate - Inactivate Consultant User
async function activateInactivateConsultantUser(consultantId, name, status) {
    var title = status ? "Deactivate Consultant" : "Activate Consultant";
    var textAction = status ? "Deactivate" : "Activate";
    Swal.fire({
        title: title,
        text: 'Are you sure you want to ' + textAction + ' "' + name + '" from the Team?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, do it!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            displaySpinner();
            var token = $('[name="__RequestVerificationToken"]').val();
            var formData = new FormData();
            formData.append('consultantId', consultantId);
            fetch("/General/Consultants/ActivateDeactivateConsultantUser"
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
                        getListOfResults(false, false);
                    } else {
                        displayToasterError(data.error);
                        console.error('There has been a problem with the fetch operation:', data.detail);
                    }
                })
                .finally(() => {
                    hideSpinner();
                });
        }
    });
}
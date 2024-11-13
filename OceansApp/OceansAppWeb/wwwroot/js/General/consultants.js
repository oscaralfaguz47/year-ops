let rightSidebarFiltersIsDiplayed = false;
let activeInactiveRadioElement = null;
let countrySelectFilters = null;
let countriesArray = [];
let holidaysArray = [];
$(document).ready(function () {
    setGeneralItemActive();
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
            var tbody = $(".global-table-container table tbody");
            var tableRows = $(".global-table-container table");
            var noResultsMessage = $(".no-results");
            noResultsMessage.empty();
            tableRows.css("display", "block");
            tbody.empty();
            data.consultantsList.forEach(function (obj) {
                var projectsJson = null; JSON.parse('[' + obj.consultantProjects + ']');
                if (obj.consultantProjects !== null) {
                    projectsJson = JSON.parse('[' + obj.consultantProjects + ']');
                }
                var projectsSpan = "";
                if (projectsJson !== null) {
                    projectsJson.forEach(function (pos) {
                        projectsSpan += `<span class="project-span"><span>${pos.Name}</span>${pos.IsActive ? '<span class="check-label"> (Active)</span>' : '<span class="red-label"> (Inactive)</span>'}</span>`;
                    });
                } else {
                    projectsSpan = '<strong class="no-project-yet">Not yet assigned to a project.</strong>';
                }
                var resetTwoFactorBtn = '';
                if (obj.twoFactorEnabled) {
                    resetTwoFactorBtn = `<li onclick="resetTwoFactorAuth(${obj.consultantId}, '${obj.consultantName}')"><i class="bi bi-arrow-counterclockwise"></i> Reset Two-Factor</li>`;
                }
                var activateDeactivateTwoFactorBtn = `<li onclick="activateDeactivateTwoFactorAuth(${obj.consultantId}, '${obj.consultantName}', ${obj.twoFactorRequired})"><i class="fa-solid fa-lock${obj.twoFactorRequired ? '-open red-label' :' check-label'}"></i> ${obj.twoFactorRequired ? 'Deactivate':'Activate'} Two-Factor</li>`;

                var resendInviteBtn = '';
                if (!obj.emailConfirmed) {
                    resendInviteBtn = `<li onclick="resendInviteToConsultant(${obj.consultantId}, '${obj.consultantName}')""><i class="bi bi-send"></i> Resend Invite</li>`;
                }
                var row = `<tr class="hover-group">
                  <td>
                        <i onclick="displayMenuListFromMenuIcon('menuOptions-${obj.consultantId}', 'menuIcon-${obj.consultantId}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.consultantId}"></i>
                          <div class="menu-options" id="menuOptions-${obj.consultantId}">
                           <ul>
                             <li onclick="activateInactivateConsultantUser(${obj.consultantId}, '${obj.consultantName}', ${obj.isActive})" id="activate-deactivate-li-${obj.consultantId}">${obj.isActive ? '<i class="bi bi-x-lg red-label"></i>' : '<i class="bi bi-plus-lg check-label"></i>'}${obj.isActive ? ' Deactivate user' : ' Activate user'}</li>
                             <li onclick="displayUpdateCreateConsultantModal('modal-update-create-consultant', ${obj.consultantId})""><i class="bi bi-pencil-square"></i> Edit Consultant</li>
                             ${resendInviteBtn}
                             ${resetTwoFactorBtn}
                             ${activateDeactivateTwoFactorBtn}
                           </ul>
                         </div>
                      ${obj.consultantName}
                  </td>
                  <td class="shared-table-td">${obj.isActive ? '<span class="check-label">Active</span>' : '<span class="red-label">Inactive</span>'}</td>
                  <td class="shared-table-td">${obj.twoFactorEnabled ? '<i class="bi bi-check check-label"></i>' : '<i class="bi bi-x red-label"></i>'}</td>
                  <td class="shared-table-td">${obj.twoFactorRequired ? '<i class="bi bi-check check-label"></i>' : '<i class="bi bi-x red-label"></i>'}</td>
                  <td class="shared-table-td">${obj.emailConfirmed ? '<i class="bi bi-check check-label"></i>' : '<i class="bi bi-x red-label"></i>'}</td>
                  <td class="shared-table-td">${!obj.isLockedOut ? '<i class="bi bi-unlock-fill check-label"></i>' : '<i class="bi bi-lock-fill red-label"></i>'}</td>
                  <td>${obj.internalEmail}</td>
                  <td>${obj.personalEmail === null ? "" : obj.personalEmail}</td>
                  <td>${projectsSpan}</td>
                  <td>${obj.countryName}</td>
                  <td>${obj.userCategoryName}</td>
                  <td>${obj.consultantPositions === null ? "" : obj.consultantPositions}</td>
                  <td>${obj.phoneNumber === null ? "" : obj.phoneNumber}</td>
                  <td>${obj.phone2 === null ? "" : obj.phone2}</td>
                  <td>${obj.address === null ? "" : obj.address}</td>
                  <td>${obj.location === null ? "" : `<div class="location-cont">
                        <button onclick="copyToClipboard('${obj.location}', 'The location of: ' + '${obj.consultantName}' + ' was copied to the clipboard!')" >
                            <i class="bi bi-clipboard-fill"></i> Copy location
                        </button> &nbsp;
                        <a href="${obj.location}" target="_blank" class="link"><i class="bi bi-geo-alt-fill"></i> Redirect to location</a>
                                    </div>`}
                  </td>
              </tr>`;
                tbody.append(row);
            });

            if (data.consultantsList.length === 0) {
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

//More filters
async function displayMoreFiltersConsultants() {
    if (!rightSidebarFiltersIsDiplayed) {
        displaySpinner();
        let rightSidebarContainer = document.getElementById('right-sidebar-container');
        rightSidebarContainer.innerHTML = `
        <div class="header-btns-container">
         <button class="clear-btn" onclick="clearFilters('filters-form')"><img class="filter-icon" src="/icons/Shared/clear.svg">Clear filters </button>
        </div>
        <div class="scroll-container">
          <form id="filters-form">
           <div class="radio-buttons-container">
             <div class="radio-group active-inactive-rg">
               <label class="radio-label">
                   <input onchange="paginationSubmit(false, true)" name="active-inactive" type="radio" value="true" class="radio-input">
                   <span class="radio-custom"></span>
                   &nbsp; Active
               </label>
               <label class="radio-label">
                   <input onchange="paginationSubmit(false, true)" name="active-inactive" type="radio" value="false" class="radio-input">
                   <span class="radio-custom"></span>
                   &nbsp; Inactive
               </label>
             </div>
           </div>
           <div class="radio-buttons-container">
             <div class="radio-group twoFactor-rg">
               <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" name="two-factor" type="radio" value="true" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; Two-Factor Auth Enabled
               </label>
               <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" name="two-factor" type="radio" value="false" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; Two-Factor Auth Disabled
               </label>
             </div>
           </div>
           <div class="radio-buttons-container">
             <div class="radio-group confirmedEmail-rg">
               <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" name="confirmedEmail" type="radio" value="true" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; Confirmed Email
               </label>
               <label class="radio-label">
                 <input onchange="paginationSubmit(false, true)" name="confirmedEmail" type="radio" value="false" class="radio-input">
                 <span class="radio-custom"></span>
                 &nbsp; Unconfirmed Email
               </label>
             </div>
           </div>
           <div class="select-container">
             <label>Country</label>
             <select onchange="paginationSubmit(false, true)" id="countrySelectFilters" class="form-select">
             </select>
           </div>
          </form>
        <div>`;

        activeInactiveRadioElement = document.querySelector('.active-inactive-rg');
        countrySelectFilters = document.getElementById('countrySelectFilters');
        if (countriesArray.length === 0) {
            countriesArray = await getCountriesList();
        }
        populateSelect('countrySelectFilters', countriesArray.countries, 'All countries', null);
        rightSidebarFiltersIsDiplayed = true;
    }
    hideSpinner();
    openRightSidebar();
}
function clearFilters(formId) {
    resetFormElements(formId);
    getListOfResults(false, true);
}

//Pagination and Filters
function paginationSubmit(firstTime, filters) {
    getListOfResults(firstTime, filters);
}
function recolectDataFromForm(filters) {
    {
        var searchText = $('#search-input').val();
        var activeInactiveValue = null;
        if (activeInactiveRadioElement !== null) {
            const checkedElement = activeInactiveRadioElement.querySelector('input[type="radio"]:checked');
            activeInactiveValue = checkedElement === null ? null : Boolean(activeInactiveRadioElement.querySelector('input[type="radio"]:checked').value === 'true');
        }
        const twoFactorRadioElement = document.querySelector('.twoFactor-rg input[type="radio"]:checked');
        var isTwoFactorEnabledValue = null;
        if (twoFactorRadioElement !== null) {
            isTwoFactorEnabledValue = Boolean(document.querySelector('input[name="two-factor"]:checked').value === 'true');
        }
        const confirmedEmailElement = document.querySelector('.confirmedEmail-rg input[type="radio"]:checked');
        var confirmedEmailValue = null;
        if (confirmedEmailElement !== null) {
            confirmedEmailValue = Boolean(document.querySelector('input[name="confirmedEmail"]:checked').value === 'true');
        }

        var filtersData = {
            SearchText: searchText,
            IsActive: activeInactiveValue,
            CountryId: countrySelectFilters === null ? null : countrySelectFilters.value === '' || countrySelectFilters.value === 'null' ? null : countrySelectFilters.value,
            IsTwoFactorEnabled: isTwoFactorEnabledValue,
            EmailConfirmed: confirmedEmailValue
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
                    } else {
                        displayToasterError(data.error);
                        console.error('There has been a problem with the fetch operation:', data.detail);
                    }
                    getListOfResults(false, false);
                })
                .catch(error => {
                    validateSessionExpiration(error.message);
                })
                .finally(() => {
                    hideSpinner();
                });
        }
    });
}

// Activate - Deactivate Two-Factor Auth
async function activateDeactivateTwoFactorAuth(consultantId, name, status) {
    let statusName = status ? 'Deactivate' : 'Activate';
    Swal.fire({
        title: `${statusName} Two-Factor Auth`,
        text: `Are you sure you want to ${statusName} the Two-Factor Authentication from ${name}?`,
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
            fetch("/General/Consultants/ActivateDeactivateAuthenticatorFromUser"
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
                .catch(error => {
                    validateSessionExpiration(error.message);
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
                    } else {
                        displayToasterError(data.error);
                        console.error('There has been a problem with the fetch operation:', data.detail);
                    }
                    getListOfResults(false, false);
                })
                .catch(error => {
                    validateSessionExpiration(error.message);
                })
                .finally(() => {
                    hideSpinner();
                });
        }
    });
}
// Resend invite to consultant
async function resendInviteToConsultant(consultantId, name) {
    Swal.fire({
        title: 'Resend Invite',
        text: 'Are you sure you want to resend the invite to ' + name + '?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, send!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            displaySpinner();
            var token = $('[name="__RequestVerificationToken"]').val();
            var formData = new FormData();
            formData.append('consultantId', consultantId);
            fetch("/General/Consultants/ResentInvite"
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
                .catch(error => {
                    validateSessionExpiration(error.message);
                })
                .finally(() => {
                    hideSpinner();
                });
        }
    });
}
function copyToClipboard(text, message) {
    const textarea = document.createElement('textarea');
    textarea.value = text;
    document.body.appendChild(textarea);

    textarea.select();
    textarea.setSelectionRange(0, 99999); 

    document.execCommand('copy');

    document.body.removeChild(textarea);
    displayToasterSuccess(message);
}
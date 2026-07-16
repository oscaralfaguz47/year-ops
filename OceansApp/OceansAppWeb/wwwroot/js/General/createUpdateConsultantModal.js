var countrySelectCreateUpdate = document.getElementById('CountrySelect');
var holidaySelectCreateUpdate = document.getElementById('HolidaysSelect');
//CREATE / UPDATE CONSULTANT MODAL
async function displayUpdateCreateConsultantModal(modalId, id) {
    if (countriesArray.length === 0) {
        countriesArray = await getCountriesList();
    }
    if (holidaysArray.length === 0) {
        holidaysArray = await getHolidaysList();
    }
    populateSelect('CountrySelect', countriesArray.countries, '-Select a country-', null);
    populateSelect('HolidaysSelect', holidaysArray.holidays, '-Select a holiday-', null);
    var modalTitle = document.getElementById('create-consultant-modal-title');
    modalTitle.textContent = "ADD NEW CONSULTANT";
    var createUpdateForm = $('#form-create-update');
    var otherConfigForm = $('#form-other-config');
    inicializeModalButtons(modalId);
    resetForm('form-create-update');
    resetForm('form-other-config');
    var projectsContainer = $("#projects-container");
    projectsContainer.empty();
    createUpdateForm.find('[name="consultantId"]').val("");
    var companySelect = createUpdateForm.find('[name="CompanyId"]');
    companySelect.html(`<option value>-Select the company-</option>
                        <option value="OCE">Oceans Consulting</option>
                        <option value="LLC">OCE LLC</option>`);
    companySelect.val();
    var paymentMethodSelect = createUpdateForm.find('[name="idPaymentMethod"]');
    document.getElementById("saved-consultant-message").style.display = "none";
    var projectsAssignedSection = document.getElementById("projects-assigned-section");
    projectsAssignedSection.style.display = "none";
    if (id !== null) {
        modalTitle.textContent = "UPDATE CONSULTANT";
        var url = "/General/Consultants/GetConsultantDataById?consultantId=" + encodeURIComponent(id);
        displaySpinner();
        fetch(url)
            .then(response => {
                if (response.ok) {
                    return response.json();
                } else {
                    return response.json().then(errorData => {
                        displayToasterError(errorData.error);
                        hideModal(modalId);
                        getListOfResults(false, false);
                        const error = new Error('The request to the server failed!. More details: ' + errorData.detail);
                        error.status = response.status; 
                        throw error;
                    });
                }
            })
            .then(data => {
                createUpdateForm.find('[name="consultantId"]').val(data.consultantData.consultantId);
                createUpdateForm.find('[name="name"]').val(data.consultantData.name);
                createUpdateForm.find('[name="lastName"]').val(data.consultantData.lastName);
                createUpdateForm.find('[name="userName"]').val(data.consultantData.email);
                createUpdateForm.find('[name="userName"]').prop('disabled', true);
                createUpdateForm.find('[name="personalEmail"]').val(data.consultantData.personalEmail);
                createUpdateForm.find('[name="phoneNumber"]').val(data.consultantData.phoneNumber);
                createUpdateForm.find('[name="phone2"]').val(data.consultantData.phone2);
                data.consultantData.companyId !== null ? paymentMethodSelect.prop('disabled', false) : paymentMethodSelect.prop('disabled', true);
                companySelect.val(data.consultantData.companyId);
                selectCompany('CompanySelect', data.consultantData.companyId, true, data.consultantData.paymentMethodId);
                paymentMethodSelect.val(data.consultantData.paymentMethodId);
                selectCategory(data.consultantData.userCategoryName, data.consultantData.positions, true, data.consultantData.userRole);
                createUpdateForm.find('[name="userCategoryName"]').val(data.consultantData.userCategoryName);
                setWeeklyPulseFields(
                    data.consultantData.isWeeklyPulseParticipant || false,
                    data.consultantData.roleGrantsWeeklyPulseAccess || false);
                countrySelectCreateUpdate.value = data.consultantData.idCountry;
                createUpdateForm.find('[name="address"]').val(data.consultantData.address);
                createUpdateForm.find('[name="location"]').val(data.consultantData.location);
                createUpdateForm.find('[name="idPaymentPeriod"]').val(data.consultantData.paymentPeriod);
                createUpdateForm.find('[name="workingModel"]').val(data.consultantData.workingModel);
                holidaySelectCreateUpdate.value = data.consultantData.consultantHolidayId;
                let startDateDateFormat = new Date(data.consultantData.startDate);
                createUpdateForm.find('[name="startDate"]').val(startDateDateFormat.toISOString().split('T')[0]);

                // PTO fields
                document.getElementById('isPtoEligible').checked = data.consultantData.isEligibleForPaidTimeOff || false;
                document.getElementById('annualPaidDays').value = data.consultantData.annualPaidTimeOffDays || '';
                document.getElementById('initialAdminPtoBalance').value = data.consultantData.initialAdminPtoBalance ?? '';
                togglePtoFields();

                showModal(modalId);
            })
            .catch(error => {
                validateSessionExpiration(error.message, error.status);
            })
            .finally(() => {
                hideSpinner();
            });
    } else {
        selectCategory('Consultant', undefined, false);
        setWeeklyPulseFields(false, false);
        createUpdateForm.find('[name="userName"]').prop('disabled', false);
        paymentMethodSelect.prop('disabled', true);
        paymentMethodSelect.html('');
        paymentMethodSelect.html('<option value>-First select a Company-</option>');
        showModal(modalId);
    }
}
function displayOtherConfigModal(modalId) {
    showModal(modalId);
}
function togglePtoFields() {
    const isChecked = document.getElementById('isPtoEligible').checked;
    document.getElementById('pto-days-container').style.display = isChecked ? 'block' : 'none';
    if (!isChecked) document.getElementById('annualPaidDays').value = '';
}
function toggleCategoryPtoSections(isAdministrative) {
    const consultantSection = document.getElementById('consultant-pto-section');
    const adminSection = document.getElementById('admin-pto-section');
    if (consultantSection) consultantSection.style.display = isAdministrative ? 'none' : 'block';
    if (adminSection) adminSection.style.display = isAdministrative ? 'block' : 'none';
}

//CreateUpdate Consultant METHOD
async function createUpdateConsultant(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var otherConfigForm = $('#form-other-config');
    var consultantIdData = Number(createUpdateForm.find('[name="consultantId"]').val()) || null;
    var consultantNameData = createUpdateForm.find('[name="name"]').val();
    var consultantLastNameData = createUpdateForm.find('[name="lastName"]').val();
    var emailData = createUpdateForm.find('[name="userName"]').val();
    var userCategoryNameData = createUpdateForm.find('[name="userCategoryName"]').val() === undefined ? 'Consultant' : createUpdateForm.find('[name="userCategoryName"]').val();
    var phoneNumberData = createUpdateForm.find('[name="phoneNumber"]').val() || null;
    var phone2Data = createUpdateForm.find('[name="phone2"]').val() || null;
    var companyIdData = createUpdateForm.find('[name="CompanyId"]').val();
    var paymentMethodIdData = Number(createUpdateForm.find('[name="idPaymentMethod"]').val()) || null;
    var paymentPeriodIdData = Number(createUpdateForm.find('[name="idPaymentPeriod"]').val()) || null;
    var workingModelData = Number(createUpdateForm.find('[name="workingModel"]').val()) || null;
    var addressData = createUpdateForm.find('[name="address"]').val() || null;
    var personalEmailData = createUpdateForm.find('[name="personalEmail"]').val() || null;
    var locationData = createUpdateForm.find('[name="location"]').val() || null;
    var userRoleData = createUpdateForm.find('[name="userRole"]').val() === undefined ? 'Computer Consultant' : createUpdateForm.find('[name="userRole"]').val();
    var weeklyPulseCheckbox = document.getElementById('isWeeklyPulseParticipant');
    var isWeeklyPulseParticipantData = weeklyPulseCheckbox !== null && weeklyPulseCheckbox.checked;

    const startDateInput = createUpdateForm.find('[name="startDate"]').val();

    var token = $('[name="__RequestVerificationToken"]').val();

    var positionsData = [];
    const checkboxes = optionsContainer.querySelectorAll('input[type="checkbox"]');
    checkboxes.forEach(checkbox => {
        if (checkbox.checked) {
            positionsData.push({ consultantPositionId: checkbox.value });
        }
    });

    var data = {
        ConsultantId: consultantIdData,
        Name: consultantNameData,
        LastName: consultantLastNameData,
        Email: emailData,
        UserCategoryName: userCategoryNameData,
        IdCountry: countrySelectCreateUpdate.value === '' || countrySelectCreateUpdate.value === 'null' ? null : countrySelectCreateUpdate.value,
        PhoneNumber: phoneNumberData,
        Phone2: phone2Data,
        CompanyId: companyIdData,
        PaymentMethodId: paymentMethodIdData,
        PaymentPeriod: paymentPeriodIdData,
        ConsultantHolidayId: holidaySelectCreateUpdate.value === '' || holidaySelectCreateUpdate.value === 'null' ? null : holidaySelectCreateUpdate.value,
        Address: addressData,
        PersonalEmail: personalEmailData,
        Location: locationData,
        UserRole: userRoleData,
        IsWeeklyPulseParticipant: isWeeklyPulseParticipantData,
        Positions: positionsData,
        WorkingModel: workingModelData,
        StartDate: startDateInput ? startDateInput.toString() : null,
        IsEligibleForPaidTimeOff: document.getElementById('isPtoEligible').checked,
        AnnualPaidTimeOffDays: document.getElementById('annualPaidDays').value ? Number(document.getElementById('annualPaidDays').value) : null,
        InitialAdminPtoBalance: document.getElementById('initialAdminPtoBalance').value !== '' ? Number(document.getElementById('initialAdminPtoBalance').value) : null
    };
    console.log(data);
    fetch('/General/Consultants/CreateUpdateConsultant', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            RequestVerificationToken: token
        },
        body: JSON.stringify(data)
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    if (errorData.messageType === "Validation Error") {
                        displayToasterWarningArray(errorData.errors);
                        inicializeModalButtons(modalId);
                        throw new Error('Validation errors!');
                    } else {
                        displayToasterError(errorData.error);
                        hideModal(modalId);
                        throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                    }
                });
            }
        })
        .then(data => {
            document.getElementById("projects-assigned-section").style.display = "block";
            inicializeModalButtons(modalId);
            displayToasterSuccess(data.message);
            if (data.consultantId > 0) {
                document.getElementById("saved-consultant-message").style.display = "block";
                createUpdateForm.find('[name="consultantId"]').val(data.consultantId);
            } else {
                hideModal(modalId);
            }
            getListOfResults(false, false);
        })
        .catch(error => {
            validateSessionExpiration(error.message);
        });
}

//SELECT CATEGORY
function selectCategory(selectedValue, selectedOptions, isEditingConsultant, userRole) {
    var selectedOptionsArray = [];
    if (selectedOptions !== undefined) {
        selectedOptionsArray = selectedOptions;
    }
    var isAdministrative = selectedValue === 'Administrative' ? true : false;
    toggleCategoryPtoSections(isAdministrative);
    displaySpinner();
    getPositionsList(isAdministrative)
        .then(data => {
            hideSpinner();
            const options = data.positions;
            const selectedCount = document.getElementById('selectedCount');
            selectedCount.textContent = `Selected Positions: 0`;

            const optionsContainer = document.getElementById('optionsContainer');
            optionsContainer.innerHTML = '';
            // Generate checkboxes
            options.forEach(option => {
                const checkbox = document.createElement('input');
                checkbox.type = 'checkbox';
                checkbox.id = 'positionId-' + option.value;
                checkbox.name = 'positionName-' + option.value;
                checkbox.value = option.value;

                const label = document.createElement('label');
                label.htmlFor = 'positionId-' + option.value;
                label.appendChild(document.createTextNode(option.text));

                const div = document.createElement('div');
                div.appendChild(checkbox);
                div.appendChild(label);
                optionsContainer.appendChild(div);
            });
            if (selectedOptionsArray !== undefined) {
                selectedCount.textContent = `Selected Positions: ${selectedOptionsArray.length}`;
                selectedOptionsArray.forEach(function (item, index, arr) {
                    var checkbox = document.getElementById('positionId-' + item.consultantPositionId);
                    if (checkbox) {
                        checkbox.checked = true;
                    }
                });
            }
        })
        .catch(error => {
            hideSpinner();
            console.error('Error fetching the positions:', error);
        });
    fillRolesForSelect(isAdministrative, isEditingConsultant, userRole);
}
// The checkbox reflects one thing only: membership of the 'Weekly Pulse Participant'
// role. It stays enabled for every category, because UserCategory does not imply any
// role — an Administrative user whose role is not Admin/Master gets no Pulse access,
// and previously the modal showed them as enrolled anyway.
// roleGrantsAccess is a read-only note: their job role already carries the claim, so
// the role is redundant for them. It never checks or disables the box.
function setWeeklyPulseFields(isParticipant, roleGrantsAccess) {
    var weeklyPulseCheckbox = document.getElementById('isWeeklyPulseParticipant');
    if (weeklyPulseCheckbox === null) {
        return;
    }
    weeklyPulseCheckbox.checked = isParticipant;
    var roleHint = document.getElementById('weekly-pulse-role-hint');
    if (roleHint !== null) {
        roleHint.style.display = roleGrantsAccess ? 'inline' : 'none';
    }
}
function fillRolesForSelect(isAdministrative, isEditingConsultant, userRole) {
    var selectElement = document.getElementById("UserRoleSelect");
    if (selectElement !== null) {
        selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
        getRolesList()
            .then(data => {
                selectElement.innerHTML = '';
                if (isAdministrative) {
                    data.roles.forEach(obj => {
                        if (obj.text !== "Computer Consultant" && obj.text !== "Weekly Pulse Participant") {
                            selectElement.add(new Option(obj.text, obj.value));
                        }
                    });
                    if (!isEditingConsultant) {
                        selectElement.value = 'Simple';
                    } else {
                        selectElement.value = userRole;
                    }
                } else {
                    selectElement.add(new Option('Computer Consultant', 'Computer Consultant'));
                    if (!isEditingConsultant) {
                        selectElement.value = 'Computer Consultant';
                    } else {
                        // userRole is null for a consultant with no role in the database;
                        // defaulting the single option means saving repairs the missing role.
                        selectElement.value = userRole || 'Computer Consultant';
                    }
                }
            })
            .catch(error => {
                console.error('Error fetching roles:', error);
            });
    }
}

//SELECT COMPANY
function selectCompany(selectElementId, selectedValue, isEditing, selectedValuePaymentMethod) {
    if (selectedValue !== null) {
        var selectElement = document.getElementById(selectElementId);
        for (var i = 0; i < selectElement.options.length; i++) {
            if (selectElement.options[i].value === "" || selectElement.options[i].value === null) {
                selectElement.remove(i);
                break;
            }
        }
        fillPaymentMethodsForSelect(selectedValue, isEditing, selectedValuePaymentMethod);
    }
}
function fillPaymentMethodsForSelect(selectedValue, isEditing, selectedValuePaymentMethod) {
    var selectElement = document.getElementById("PaymentMethodSelect");
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    displaySpinner();
    getPaymentMethodsWhereCompanyList(selectedValue)
        .then(data => {
            selectElement.innerHTML = '';
            selectElement.innerHTML = '<option value>-Select a Payment Method-</option>';
            selectElement.disabled = false;
            data.paymentMethods.forEach(obj => {
                selectElement.add(new Option(obj.text, obj.value));
            });
            if (isEditing) {
                selectElement.value = selectedValuePaymentMethod;
            }
            hideSpinner();
        })
        .catch(error => {
            hideSpinner();
            console.error('Error fetching roles:', error);
        });
}

// Dropdown with checkboxes
document.addEventListener('DOMContentLoaded', function () {
    const optionsContainer = document.getElementById('optionsContainer');
    const dropdownContent = document.querySelector('.dropdown-content');
    const selectedCount = document.getElementById('selectedCount');

    selectCategory('Consultant');
    // Mostrar/ocultar dropdown
    document.querySelector('.dropbtn').addEventListener('click', function (event) {
        dropdownContent.style.display = 'block';
        event.stopPropagation(); // Previene el cierre cuando se hace clic en el botón
    });

    // Cerrar dropdown al hacer clic fuera
    document.addEventListener('click', function () {
        dropdownContent.style.display = 'none';
    });

    // Prevenir el cierre del dropdown al hacer clic en el mismo
    dropdownContent.addEventListener('click', function (event) {
        event.stopPropagation();
    });

    // Cerrar dropdown con ESC
    document.addEventListener('keydown', function (e) {
        if (e.key === "Escape") {
            dropdownContent.style.display = 'none';
        }
    });

    // Filtrar opciones
    document.getElementById('filterInput').addEventListener('keyup', function (e) {
        const text = e.target.value.toLowerCase();
        const divs = optionsContainer.querySelectorAll('div');

        divs.forEach(div => {
            const label = div.querySelector('label');
            if (label.textContent.toLowerCase().indexOf(text) > -1) {
                div.style.display = '';
            } else {
                div.style.display = 'none';
            }
        });
    });

    // Escuchar cambios en los checkboxes y mostrar resultados seleccionados
    optionsContainer.addEventListener('change', function (e) {
        if (e.target.type === 'checkbox') {
            updateSelectedCount();
        }
    });

    function updateSelectedCount() {
        const checkboxes = optionsContainer.querySelectorAll('input[type="checkbox"]');
        const selectedOptions = [];

        checkboxes.forEach(checkbox => {
            if (checkbox.checked) {
                selectedOptions.push({ value: checkbox.value, selected: checkbox.checked });
            }
        });
        selectedCount.textContent = `Selected Positions: ${selectedOptions.length}`;
    }
});

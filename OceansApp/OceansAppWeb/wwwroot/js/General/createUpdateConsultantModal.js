//CREATE / UPDATE CONSULTANT MODAL
async function displayUpdateCreateConsultantModal(modalId, id) {
    var modalTitle = document.getElementById('create-consultant-modal-title');
    modalTitle.textContent = "ADD NEW CONSULTANT";
    var createUpdateForm = $('#form-create-update');
    inicializeModalButtons(modalId);
    resetForm('form-create-update')
    var projectsContainer = $("#projects-container");
    projectsContainer.empty();
    createUpdateForm.find('[name="consultantId"]').val("");
    document.getElementById("saved-consultant-message").style.display = "none";
    var projectsAssignedSection = document.getElementById("projects-assigned-section");
    projectsAssignedSection.style.display = "none";
    const countrySelect = createUpdateForm.find('[name="idCountry"]')[0];
    countrySelect.innerHTML = '<option value="">-Select a country-</option>';
    selectCategory('Consultant');
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
                        throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                    });
                }
            })
            .then(data => {
                console.log(data);
                //createUpdateForm.find('[name="projectId"]').val(data.projectData.projectId);
                //createUpdateForm.find('[name="projectName"]').val(data.projectData.name);
                //createUpdateForm.find('[name="description"]').val(data.projectData.description);

                //var newOptionClient = document.createElement('option');
                //newOptionClient.value = data.projectData.clientId;
                //newOptionClient.text = data.projectData.clientName;
                //newOptionClient.selected = true;
                //clientSelect.appendChild(newOptionClient);
                //clientSelect.disabled = true;

                //successManagerSelect.innerHTML = '';
                //var newOptionSuccessManager = document.createElement('option');
                //newOptionSuccessManager.value = data.projectData.successManagerId;
                //newOptionSuccessManager.text = data.projectData.successManagerName;
                //newOptionSuccessManager.selected = true;
                //successManagerSelect.appendChild(newOptionSuccessManager);
                //successManagerSelect.disabled = false;

                //let startDateDateFormat = new Date(data.projectData.startDate);
                //createUpdateForm.find('[name="startDate"]').val(startDateDateFormat.toISOString().split('T')[0]);

                //createUpdateForm.find('[name="isActive"]').val(data.projectData.isActive);
                //createUpdateForm.find('[name="isActive"]').prop('checked', data.projectData.isActive);
                //createUpdateForm.find('[name="isBillable"]').val(data.projectData.isBillable);
                //createUpdateForm.find('[name="isBillable"]').prop('checked', data.projectData.isBillable);
                //createUpdateForm.find('[name="clientHasTrackingTool"]').val(data.projectData.clientHasTrackingTool);
                //createUpdateForm.find('[name="clientHasTrackingTool"]').prop('checked', data.projectData.clientHasTrackingTool);
                //data.projectData.assignedConsultants.forEach(function (item, index, arr) {
                //    addNewConsultantRow(item.consultantName, item.projectConsultantAssignedId, item.consultantId, item.positionDetail,
                //        item.hourlyClientRate, item.monthlyClientRate, item.hourlySalary, item.monthlySalary, item.isActive)
                //});
                showModal(modalId);
            })
            .finally(() => {
                hideSpinner();
            });
    } else {
        showModal(modalId);
    }
}

//CreateUpdate Consultant METHOD
async function createUpdateConsultant(modalId) {
    waitingForPostMethod();
    var createUpdateForm = $('#form-create-update');
    var consultantIdData = createUpdateForm.find('[name="consultantId"]').val() || null;
    var consultantNameData = createUpdateForm.find('[name="name"]').val();
    var consultantLastNameData = createUpdateForm.find('[name="lastName"]').val();
    var emailData = createUpdateForm.find('[name="userName"]').val();
    var userCategoryNameData = createUpdateForm.find('[name="userCategoryName"]').val() === undefined ? 'Consultant' : createUpdateForm.find('[name="userCategoryName"]').val();
    var idCountryData = createUpdateForm.find('[name="idCountry"]').val();
    var phoneNumberData = createUpdateForm.find('[name="phoneNumber"]').val() || null;
    var phone2Data = createUpdateForm.find('[name="phone2"]').val() || null;
    var addressData = createUpdateForm.find('[name="address"]').val() || null;
    var personalEmailData = createUpdateForm.find('[name="personalEmail"]').val() || null;
    var locationData = createUpdateForm.find('[name="location"]').val() || null;
    var userRoleData = createUpdateForm.find('[name="userRole"]').val() === undefined ? 'Computer Consultant' : createUpdateForm.find('[name="userRole"]').val();

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
        IdCountry: idCountryData,
        PhoneNumber: phoneNumberData,
        Phone2: phone2Data,
        Address: addressData,
        PersonalEmail: personalEmailData,
        Location: locationData,
        UserRole: userRoleData,
        Positions: positionsData
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
            console.log(data.consultantId);
            if (data.consultantId > 0) {
                document.getElementById("saved-consultant-message").style.display = "block";
                createUpdateForm.find('[name="consultantId"]').val(data.consultantId);
            } else {
                hideModal(modalId);
            }
            getListOfResults(false, false);
        });
}

//SELECT CATEGORY
function selectCategory(selectedValue) {
    var isAdministrative = selectedValue === 'Administrative' ? true : false;
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
                checkbox.id = option.value;
                checkbox.value = option.value;

                const label = document.createElement('label');
                label.htmlFor = option.value;
                label.appendChild(document.createTextNode(option.text));

                const div = document.createElement('div');
                div.appendChild(checkbox);
                div.appendChild(label);
                optionsContainer.appendChild(div);
            });
        })
        .catch(error => {
            hideSpinner();
            console.error('Error fetching the positions:', error);
        });
    fillRolesForSelect(isAdministrative);
}
function fillRolesForSelect(isAdministrative) {
    var selectElement = document.getElementById("UserRoleSelect");
    if (selectElement !== null) {
        selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
        getRolesList()
            .then(data => {
                selectElement.innerHTML = '';
                if (isAdministrative) {
                    data.roles.forEach(obj => {
                        if (obj.name !== "Computer Consultant") {
                            selectElement.add(new Option(obj.name, obj.value));
                        }
                    });
                    selectElement.value = 'Simple';
                } else {
                    selectElement.add(new Option('Computer Consultant', 'Computer Consultant'));
                    selectElement.value = 'Computer Consultant';
                }
            })
            .catch(error => {
                console.error('Error fetching roles:', error);
            });
    }
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

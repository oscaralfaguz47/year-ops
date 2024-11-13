let tbody = $(".cl-removed-projects-table table tbody");
let tableRows = $(".cl-removed-projects-table table");
let noResultsMessage = $(".cl-removed-projects-table .no-results");
async function displayRemovedProjectsModal(modalId) {
    let url = "/Finances/PaymentSheets/GetRemovedProjectsInPeriod?"
        + "startDate=" + encodeURIComponent(dateFromInput.value)
        + "&endDate=" + encodeURIComponent(dateToInput.value);
    displaySpinner();

    try {
        const response = await fetch(url);

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            hideModal(modalId);
            return null;
        }

        const dataFromApi = await response.json();

        noResultsMessage.empty();
        tableRows.css("display", "block");
        tbody.empty();
        dataFromApi.removedProjectsList.forEach(function (obj) {

            let addProjectBtn = `<li onclick="addProjectToThePeriod(${obj.id})"><img src="/icons/Shared/square-plus.svg"> Add Project in this period</li>`;
            let menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-rp-${obj.id}', 'menuIcon-${obj.id}')" class="bi bi-three-dots-vertical" id="menuIcon-${obj.id}"></i>
                              <div class="menu-options" id="menuOptions-rp-${obj.id}">
                               <ul>
                                 ${addProjectBtn}
                               </ul>
                              </div>`;
            var row = `<tr id="rp-${obj.id}">
                  <td>${menuBtn} ${obj.consultantName}</td>
                  <td>${obj.projectName}</td>
                  <td>${formatUtcToLocalMmDdYyyyTime(obj.removedDate)}</td>
                  <td>${obj.userRemovedBy}</td>
              </tr>`;
            tbody.append(row);
        });

        if (dataFromApi.removedProjectsList.length === 0) {
            noResultsMessage.text("NO RECORDS FOUND");
            tableRows.css("display", "none");
        };

        hideSpinner();
        showModal(modalId);
        return dataFromApi;
    }
    catch (err) {
        hideSpinner();
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err.message);
        displayToasterError('Something went wrong, more details: ' + err);
        return null;
    }
}

async function addProjectToThePeriod(id) {

    const token = $('[name="__RequestVerificationToken"]').val();
    const formData = new FormData();
    formData.append('id', id);
    displaySpinner();
    try {
        const response = await fetch("/Finances/PaymentSheets/AddProjectInPeriod", {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body: formData
        });

        if (!response.ok) {
            const err = await response.json();
            console.error('There has been a problem with your fetch operation:', err.error);
            switch (err.messageType) {
                case 'Validation Error':
                    displayToasterWarning('Validation Error: ' + err.error);
                    break;
                    return true;
                default:
                    displayToasterError('Error: ' + err.error);
            }
            hideSpinner();
            return null;
        }
        hideSpinner();
        let rowToDelete = getElementById('rp-' + id);
        rowToDelete.remove();
        if (tbody.find("tr").length === 0) {
            noResultsMessage.text("NO RECORDS FOUND");
            tableRows.css("display", "none");
        }
        const dataFromApi = await response.json();
        displayToasterSuccess(dataFromApi.message);
        getListOfResults(false, true);
    } catch (err) {
        hideSpinner();
        validateSessionExpiration(err.message);
        displayToasterError('There has been a problem with your fetch operation:', err);
        console.error('There has been a problem with your fetch operation:', err);
    }
}
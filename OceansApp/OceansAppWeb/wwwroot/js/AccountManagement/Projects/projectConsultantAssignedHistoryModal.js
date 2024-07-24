// GET CONSULTANT HISTORY
async function getProjectConsultantHistory(projectConsultantAssignedId, modalId) {
    displaySpinner();
    const bodyList = document.getElementById('consultant-history-body');
    await getProjectConsultantHistoryHttps(projectConsultantAssignedId).then((data) => {

        console.log(data);
        displayFirstRecord(data.historyList[0]);
        displayChanges(data.historyList);
        data.historyList.forEach(function (obj) {

        });
        hideSpinner();
        showModal(modalId);
    });
}
// Function to display the first record
function displayFirstRecord(record) {
    const table = document.getElementById('first-record-table');
    table.innerHTML = '';
    const thead = document.createElement('thead');
    const tbody = document.createElement('tbody');
    const headerRow = document.createElement('tr');

    Object.keys(record).forEach(key => {
        const th = document.createElement('th');
        th.textContent = key;
        headerRow.appendChild(th);
    });

    thead.appendChild(headerRow);
    const dataRow = document.createElement('tr');
    Object.values(record).forEach(value => {
        const td = document.createElement('td');
        td.textContent = value !== null ? value : 'NULL';
        dataRow.appendChild(td);
    });

    tbody.appendChild(dataRow);
    table.appendChild(thead);
    table.appendChild(tbody);
}

// Function to display changes
function displayChanges(data) {
    const tbody = document.getElementById('changes-table').querySelector('tbody');
    tbody.innerHTML = '';
    const keysToIgnore = ['Id', 'ActionDate', 'UserActionedBy', 'CreationDate'];
    let previousRecord = data[0];

    for (let i = 1; i < data.length; i++) {
        const currentRecord = data[i];
        Object.keys(currentRecord).forEach(key => {
            if (!keysToIgnore.includes(key) && currentRecord[key] !== previousRecord[key]) {
                const row = document.createElement('tr');
                const columnCell = document.createElement('td');
                const oldValueCell = document.createElement('td');
                const newValueCell = document.createElement('td');

                columnCell.textContent = key;
                oldValueCell.textContent = previousRecord[key] !== null ? previousRecord[key] : 'NULL';
                newValueCell.textContent = currentRecord[key] !== null ? currentRecord[key] : 'NULL';

                row.appendChild(columnCell);
                row.appendChild(oldValueCell);
                row.appendChild(newValueCell);
                tbody.appendChild(row);
            }
        });
        previousRecord = currentRecord;
    }
}
async function getProjectConsultantHistoryHttps(projectConsultantAssignedId) {
    var url = "/AccountManagement/Projects/GetProjectConsultantAssignedHistoryById?projectConsultantAssignedId="
        + encodeURIComponent(projectConsultantAssignedId);
    try {
        let response = await fetch(url);
        if (response.ok) {
            return await response.json();
        } else {
            if (response.status === 404) {
                displayToasterError("Resource not found (404).");
                throw new Error('404 Not Found: The requested resource could not be found!');
            } else {
                let errorData = await response.json();
                displayToasterError(errorData.error || 'An unknown error occurred.');
                throw new Error('The request to the server failed!. More details: ' + errorData.error);
            }
            hideSpinner();
        }
    } catch (error) {
        validateSessionExpiration(error.message);
        displayToasterError('Error fetching data: ' + error);
        console.error('Error fetching data:', error);
        hideSpinner();
        return null;
    }
}
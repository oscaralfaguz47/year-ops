// GET CONSULTANT HISTORY
async function getProjectConsultantHistory(projectConsultantAssignedId, modalId) {
    displaySpinner();
    await getProjectConsultantHistoryHttps(projectConsultantAssignedId).then((data) => {
        displayFirstRecord(data.historyList[0]);
        displayChanges(data.historyList);
        data.historyList.forEach(function (obj) {

        });
        hideSpinner();
        showModal(modalId);
    });
}
// Function to display the first record

// Custom headers mapping
const customHeaders = {
    "positionName": "Position Name",
    "hourlySalary": "Hourly Salary",
    "monthlySalary": "Monthly Salary",
    "isMonthlySalaryCalculatedPerHour": "Is Monthly Salary Calculated Per Hour",
    "monthlySalaryPartner": "Monthly Salary Partner",
    "partnerName": "Partner Name",
    "partnerPaysBenefits": "Partner Pays Benefits",
    "hourlyClientRate": "Hourly Client Rate",
    "monthlyClientRate": "Monthly Client Rate",
    "accessToTrackingTool": "Access To Tracking Tool",
    "holidaysMustBePaid": "Holidays Must Be Paid",
    "isActive": "Is Active",
    "isDefaultProject": "Is Default Project",
    "userActionedBy": "Assigned By"
};
// Function to display the first record
function displayFirstRecord(record) {
    const header = getElementById('first-record-header');
    header.innerHTML = `<label>
    <span class="action-date-span"><strong>Action Date:</strong> ${formatDateMmDdYyyy(record.actionDate)}</span> |
    <strong>Actioned by:</strong> ${record.userActionedBy} |
    <strong>Registration Date:</strong> ${formatUtcToLocalMmDdYyyyTime(record.creationDate)}
    </label>`;

    const table = getElementById('first-record-table');
    table.innerHTML = '';
    const thead = document.createElement('thead');
    const tbody = document.createElement('tbody');
    const headerRow = document.createElement('tr');

    Object.keys(record).forEach(key => {
        if (!['id', 'actionDate', 'creationDate', 'userActionedBy'].includes(key) && record[key] !== null && record[key] !== 0) {
            if (key === 'partnerPaysBenefits' && record.partnerName === null) {
                return; // Skip adding this column
            }
            if (key === 'isMonthlySalaryCalculatedPerHour' && record.monthlySalary === 0) {
                return; // Skip adding this column
            }
            const th = document.createElement('th');
            th.textContent = customHeaders[key] || key;
            headerRow.appendChild(th);
        }
    });

    thead.appendChild(headerRow);
    const dataRow = document.createElement('tr');
    Object.keys(record).forEach(key => {
        if (!['id', 'actionDate', 'creationDate', 'userActionedBy'].includes(key) && record[key] !== null && record[key] !== 0) {
            if (key === 'partnerPaysBenefits' && record.partnerName === null) {
                return; // Skip adding this column
            }
            if (key === 'isMonthlySalaryCalculatedPerHour' && record.monthlySalary === 0) {
                return; // Skip adding this column
            }
            const td = document.createElement('td');
            td.innerHTML = record[key] !== null
                ? (record[key] === true
                    ? `<img src="/icons/Shared/check.svg">`
                    : (record[key] === false
                        ? `<img src="/icons/Shared/fail.svg">`
                        : (Number.isFinite(record[key])
                            ? `$${new Intl.NumberFormat().format(record[key])}`
                            : record[key])))
                : 'NULL';

            dataRow.appendChild(td);
        }
    });

    tbody.appendChild(dataRow);
    table.appendChild(thead);
    table.appendChild(tbody);
}

// Function to display changes

function displayChanges(data) {
    const changesSection = getElementById('changes-section');
    changesSection.innerHTML = '';
    const keysToIgnore = ['id', 'actionDate', 'userActionedBy', 'creationDate'];
    let previousRecord = data[0];

    for (let i = 1; i < data.length; i++) {
        const currentRecord = data[i];
        const recordHeader = document.createElement('div');
        recordHeader.classList.add('record-header');
        recordHeader.innerHTML = `<label>
        <span class="action-date-span"><strong>Action Date:</strong> ${formatDateMmDdYyyy(currentRecord.actionDate)}</span> | 
        <strong>Actioned by:</strong> ${currentRecord.userActionedBy} | 
        <strong>Registration Date:</strong> ${formatUtcToLocalMmDdYyyyTime(currentRecord.creationDate)}
        </label>`;
        changesSection.appendChild(recordHeader);

        const table = document.createElement('table');
        const thead = document.createElement('thead');
        const tbody = document.createElement('tbody');
        const headerRow = document.createElement('tr');

        const columnHeader = document.createElement('th');
        columnHeader.textContent = 'Updated Field';
        const oldValueHeader = document.createElement('th');
        oldValueHeader.textContent = 'Old Value';
        const newValueHeader = document.createElement('th');
        newValueHeader.textContent = 'New Value';

        headerRow.appendChild(columnHeader);
        headerRow.appendChild(oldValueHeader);
        headerRow.appendChild(newValueHeader);
        thead.appendChild(headerRow);

        let hasChanges = false;

        Object.keys(currentRecord).forEach(key => {
            if (!keysToIgnore.includes(key) && currentRecord[key] !== previousRecord[key]) {
                hasChanges = true;
                const row = document.createElement('tr');
                const columnCell = document.createElement('td');
                const oldValueCell = document.createElement('td');
                oldValueCell.className = 'old-value-cell';
                const newValueCell = document.createElement('td');
                newValueCell.className = 'new-value-cell';

                columnCell.innerHTML = customHeaders[key] || key;
                oldValueCell.innerHTML = previousRecord[key] !== null
                    ? (previousRecord[key] === true
                        ? `<img src="/icons/Shared/check.svg">`
                        : (previousRecord[key] === false
                            ? `<img src="/icons/Shared/fail.svg">`
                            : (Number.isFinite(previousRecord[key])
                                ? `$${new Intl.NumberFormat().format(previousRecord[key])}`
                                : previousRecord[key])))
                    : 'No Partner';

                newValueCell.innerHTML = currentRecord[key] !== null
                    ? (currentRecord[key] === true
                        ? `<img src="/icons/Shared/check.svg">`
                        : (currentRecord[key] === false
                            ? `<img src="/icons/Shared/fail.svg">`
                            : (Number.isFinite(currentRecord[key])
                                ? `$${new Intl.NumberFormat().format(currentRecord[key])}`
                                : currentRecord[key])))
                    : 'No Partner';

                row.appendChild(columnCell);
                row.appendChild(oldValueCell);
                row.appendChild(newValueCell);
                tbody.appendChild(row);
            }
        });

        if (hasChanges) {
            table.appendChild(thead);
            table.appendChild(tbody);
            changesSection.appendChild(table);
        }

        previousRecord = currentRecord;
    }
    if (data.length === 1) {
        changesSection.innerHTML = `<label class="no-changes-label">There are no change movements for this consultant.</label>`
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
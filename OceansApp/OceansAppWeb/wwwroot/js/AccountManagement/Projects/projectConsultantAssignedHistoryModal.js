// GET CONSULTANT HISTORY
async function getProjectConsultantHistory(projectConsultantAssignedId, modalId) {
    displaySpinner();
    await getProjectConsultantHistoryHttps(projectConsultantAssignedId).then((data) => {
        displayFirstRecord(data.historyList[0]);
        displayChanges(data.historyList);

        hideSpinner();
        showModal(modalId);
    });
}

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
    "monthlyClientRateNumDays": "Monthly Client Num Days",
    "accessToTrackingTool": "Access To Tracking Tool",
    "participatesInOnCalls": "Participates in On Calls",
    "holidaysMustBePaid": "Holidays Must Be Paid",
    "isActive": "Is Active",
    "isDefaultProject": "Is Default Project",
    "userActionedBy": "Assigned By",
    "numHoursForHoliday" : "Num Hours for Holidays",
    "primaryReportTrackingToolName": "Primary Tracking Tool",
    "secondReportTrackingToolName": "Second Tracking Tool"
};

// Helper to render values with special formatting
function renderCellValue(key, value) {
    if (value === true) return `<img src="/icons/Shared/check.svg" alt="Yes">`;
    if (value === false) return `<img src="/icons/Shared/fail.svg" alt="No">`;
    if (value === null) return 'Nothing';

    // Fields that should NOT have a dollar sign
    const plainNumberFields = ['numHoursForHoliday', 'monthlyClientRateNumDays'];
    if (Number.isFinite(value)) {
        return plainNumberFields.includes(key)
            ? `${value}`
            : `$${new Intl.NumberFormat().format(value)}`;
    }

    return value;
}

// Display first record in table
function displayFirstRecord(record) {
    const header = getElementById('first-record-header');
    header.innerHTML = `
        <label>
            <span class="action-date-span"><strong>Action Date:</strong> ${formatDateMmDdYyyy(record.actionDate)}</span> |
            <strong>Actioned by:</strong> ${record.userActionedBy} |
            <strong>Registration Date:</strong> ${formatUtcToLocalMmDdYyyyTime(record.creationDate)}
        </label>`;

    const table = getElementById('first-record-table');
    table.innerHTML = '';

    const thead = document.createElement('thead');
    const tbody = document.createElement('tbody');
    const headerRow = document.createElement('tr');
    const dataRow = document.createElement('tr');

    Object.keys(record).forEach(key => {
        const value = record[key];
        if (
            !['id', 'actionDate', 'creationDate', 'userActionedBy'].includes(key) &&
            value !== null && value !== 0
        ) {
            if (key === 'partnerPaysBenefits' && record.partnerName === null) return;
            if (key === 'isMonthlySalaryCalculatedPerHour' && record.monthlySalary === 0) return;

            const th = document.createElement('th');
            th.textContent = customHeaders[key] || key;
            headerRow.appendChild(th);

            const td = document.createElement('td');
            td.innerHTML = renderCellValue(key, value);
            dataRow.appendChild(td);
        }
    });

    thead.appendChild(headerRow);
    tbody.appendChild(dataRow);
    table.appendChild(thead);
    table.appendChild(tbody);
}

// Display changes between records
function displayChanges(data) {
    const changesSection = getElementById('changes-section');
    changesSection.innerHTML = '';

    const keysToIgnore = ['id', 'actionDate', 'userActionedBy', 'creationDate'];
    let previousRecord = data[0];

    for (let i = 1; i < data.length; i++) {
        const currentRecord = data[i];

        const recordHeader = document.createElement('div');
        recordHeader.classList.add('record-header');
        recordHeader.innerHTML = `
            <label>
                <span class="action-date-span"><strong>Action Date:</strong> ${formatDateMmDdYyyy(currentRecord.actionDate)}</span> |
                <strong>Actioned by:</strong> ${currentRecord.userActionedBy} |
                <strong>Registration Date:</strong> ${formatUtcToLocalMmDdYyyyTime(currentRecord.creationDate)}
            </label>`;
        changesSection.appendChild(recordHeader);

        const table = document.createElement('table');
        const thead = document.createElement('thead');
        const tbody = document.createElement('tbody');

        const headerRow = document.createElement('tr');
        ['Updated Field', 'Old Value', 'New Value'].forEach(text => {
            const th = document.createElement('th');
            th.textContent = text;
            headerRow.appendChild(th);
        });
        thead.appendChild(headerRow);

        let hasChanges = false;

        Object.keys(currentRecord).forEach(key => {
            const currentValue = currentRecord[key];
            const previousValue = previousRecord[key];

            if (!keysToIgnore.includes(key) && currentValue !== previousValue) {
                hasChanges = true;

                const row = document.createElement('tr');

                const columnCell = document.createElement('td');
                columnCell.innerHTML = customHeaders[key] || key;

                const oldValueCell = document.createElement('td');
                oldValueCell.className = 'old-value-cell';
                oldValueCell.innerHTML = renderCellValue(key, previousValue);

                const newValueCell = document.createElement('td');
                newValueCell.className = 'new-value-cell';
                newValueCell.innerHTML = renderCellValue(key, currentValue);

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
        changesSection.innerHTML = `<label class="no-changes-label">There are no change movements for this consultant.</label>`;
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
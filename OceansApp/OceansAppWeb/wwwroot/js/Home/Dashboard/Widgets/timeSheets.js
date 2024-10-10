let timeSheetSubTitle = getElementById('timesheetSubTitle');
let pendingTimesheetsCont = getElementById('pendingTimesheetsCont');
async function getPendingTimesheets() {
    const url = `/GetPendingTimesheets`;
    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
        }
        const data = await response.json();
        return data;
    } catch (error) {
        validateSessionExpiration(error.message);
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}

document.addEventListener("DOMContentLoaded", async function () {
    try {
        let data = await getPendingTimesheets();
        let dataItems = data.pendingTimesheets;
        console.log(dataItems);
        timeSheetSubTitle.innerHTML = `${dataItems.length > 0 ? "You have <span>" + dataItems.length + " pending timesheet" + (dataItems.length > 1 ? "s" : "") + "</span>." : "You don't have pending timesheets"}`;
        var timesheetsUrl = window.location.origin + '/TrackingTool/ReportingMyTime';
        dataItems.forEach(function (obj, index) {
            let pendingTimesheetsRow = document.createElement('div');
            let startDateItem = new Date(obj.startDate);
            let endDateItem = new Date(obj.endDate);
            let goBtn = document.createElement('button');
            goBtn.textContent = 'Go';
            goBtn.addEventListener('click', function () {
                window.location.href = timesheetsUrl;
            });
            let formattedDates = `<span> ${getMonthName(startDateItem.getMonth())} ${startDateItem.getDate()} - ${getMonthName(endDateItem.getMonth())} ${endDateItem.getDate()}</span>`;
            pendingTimesheetsRow.innerHTML = `<img src="/icons/Shared/circle-exclamation.svg">
            ${formattedDates} ${`<span>${obj.projectName}</span>`}`;
            pendingTimesheetsRow.appendChild(goBtn);
            pendingTimesheetsCont.appendChild(pendingTimesheetsRow);
        });

    } catch (error) {
        console.error(`Failed to load pending timesheets: ${error.message}`);
    }
});

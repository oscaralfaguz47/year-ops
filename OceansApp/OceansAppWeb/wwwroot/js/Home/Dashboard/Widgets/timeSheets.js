//Pending timesheets
const timeSheetSubTitle = getElementById('timesheetSubTitle');
const pendingTimesheetsCont = getElementById('pendingTimesheetsCont');
const firstCardContent = pendingTimesheetsCont.closest('.card-content');

function getPendingTimesheets() {
    return (async () => {
        firstCardContent.style.justifyContent = 'center';
        pendingTimesheetsCont.innerHTML = loadingISpinner();
        const url = `/GetPendingTimesheets`;
        try {
            const response = await fetch(url);
            if (!response.ok) {
                pendingTimesheetsCont.innerHTML = cardErrorInfo('Error loading pending timesheets!', 'getPendingTimesheets()');
                const errorData = await response.json();
                throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
            }
            const data = await response.json();
            return data;
        } catch (error) {
            validateSessionExpiration(error.message);
            throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
        }
    })();
}

//Last timesheets submitted
const lastTimesheetsSubmittedCont = getElementById('lastTimesheetsSubmittedCont');
const firstCardLTSContent = lastTimesheetsSubmittedCont.closest('.card-content');
function getLastTimesheetsSubmitted() {
    return (async () => {
        if (firstCardLTSContent) {
            firstCardLTSContent.style.justifyContent = 'center';
        }
        lastTimesheetsSubmittedCont.innerHTML = loadingISpinner();
        const url = `/GetLastTimesheetsSubmitted`;
        try {
            const response = await fetch(url);
            if (!response.ok) {
                lastTimesheetsSubmittedCont.innerHTML = cardErrorInfo('Error loading last timesheets submitted!', 'getLastTimesheetsSubmitted()');
                const errorData = await response.json();
                throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
            }
            const data = await response.json();
            return data;
        } catch (error) {
            validateSessionExpiration(error.message);
            throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
        }
    })();
}

//Global
document.addEventListener("DOMContentLoaded", function () {
    // Pending Timesheets
    getPendingTimesheets()
        .then(data => {
            pendingTimesheetsCont.innerHTML = '';
            let dataItems = data.pendingTimesheets;
            if (dataItems.length > 0) {
                firstCardContent.style.justifyContent = 'left';
                firstCardContent.style.display = 'block';
                timeSheetSubTitle.innerHTML = `${dataItems.length > 0 ? "You have <span class='red-label'>" + dataItems.length + " pending timesheet" + (dataItems.length > 1 ? "s" : "") + "</span>." : "You don't have pending timesheets"}`;
                var timesheetsUrl = window.location.origin + '/TrackingTool/ReportingMyTime';
                dataItems.forEach(function (obj, index) {
                    let pendingTimesheetsRow = document.createElement('div');
                    pendingTimesheetsRow.className = 'time-row';
                    let startDateItem = new Date(obj.startDate);
                    let endDateItem = new Date(obj.endDate);
                    let goBtn = document.createElement('button');
                    goBtn.textContent = 'Go';
                    goBtn.title = 'Go to Timesheet';
                    goBtn.addEventListener('click', function () {
                        window.location.href = timesheetsUrl;
                    });
                    let formattedDates = `<span class="date-project"><span class="period"> ${getMonthName(startDateItem.getMonth()).slice(0, 3)} ${startDateItem.getDate()} - ${getMonthName(endDateItem.getMonth()).slice(0, 3)} ${endDateItem.getDate()}</span>`;
                    pendingTimesheetsRow.innerHTML = `<img src="/icons/Shared/circle-exclamation.svg">
                ${formattedDates} ${`<span class="project-name" title="${obj.projectName}">${obj.projectName}</span></span>`}`;
                    pendingTimesheetsRow.appendChild(goBtn);
                    pendingTimesheetsCont.appendChild(pendingTimesheetsRow);
                });
            } else {
                pendingTimesheetsCont.innerHTML = `<div><div style="text-align:center"><img src="/icons/Shared/check.svg"></div>
                <span>You don't have pending timesheets</span></div>`;
                pendingTimesheetsCont.style.alignItems = 'center';
                pendingTimesheetsCont.style.display = 'flex';
            }
        })
        .catch(error => {
            console.error(`Failed to load pending timesheets: ${error.message}`);
        });

    // Last Timesheets Submitted
    getLastTimesheetsSubmitted()
        .then(data => {
            lastTimesheetsSubmittedCont.innerHTML = '';
            let dataItems = data.lastTimesheetSubmitted;
            if (dataItems.length > 0) {
                firstCardLTSContent.style.justifyContent = 'left';
                firstCardLTSContent.style.display = 'block';
                dataItems.forEach(function (obj, index) {
                    let submittedTimesheetsRow = document.createElement('div');
                    submittedTimesheetsRow.className = 'time-row';
                    if (index % 2 === 0) {
                        submittedTimesheetsRow.classList.add('back-dark');
                    }
                    let startDateItem = new Date(obj.startDate);
                    let endDateItem = new Date(obj.endDate);

                    const statusClass = obj.status === 'Approved' ? 'approved-st' : obj.status === 'Rejected' ? 'rejected-st' : 'waiting-st';

                    let formattedDates = `<span class="date-project"><span class="period"> ${getMonthName(startDateItem.getMonth()).slice(0, 3)} ${startDateItem.getDate()} - ${getMonthName(endDateItem.getMonth()).slice(0, 3)} ${endDateItem.getDate()}</span>`;
                    submittedTimesheetsRow.innerHTML = `
                ${formattedDates} ${`<span class="project-name" title="${obj.projectName}">${obj.projectName[0]}</span></span><span class="hours-lb"><img src="/icons/Shared/clock.svg"> ${obj.totalHours}h</span><span class="status-lb ${statusClass}">${obj.status === 'Waiting to be approved' ? 'Pending' : obj.status}</span>`}`;
                    lastTimesheetsSubmittedCont.appendChild(submittedTimesheetsRow);
                });
            } else {
                lastTimesheetsSubmittedCont.innerHTML = `<div>
                <span>You don't have submitted timesheets</span></div>`;
                lastTimesheetsSubmittedCont.style.alignItems = 'center';
                lastTimesheetsSubmittedCont.style.display = 'flex';
            }
        })
        .catch(error => {
            console.error(`Failed to load last timesheets submitted: ${error.message}`);
        });
});




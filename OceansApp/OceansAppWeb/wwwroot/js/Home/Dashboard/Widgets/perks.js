//Balance Program
const balanceProgramTitle = getElementById('BalanceProgram-title');
const balanceProgramCont = getElementById('BalanceProgram-cont');
const firstCardBPContent = balanceProgramCont.closest('.card-content');
function getBalanceProgramInfo() {
    return (async () => {
        balanceProgramTitle.textContent = 'Your remaining Balance Program is...';
        firstCardBPContent.style.justifyContent = 'center';
        balanceProgramCont.innerHTML = loadingISpinner();
        const url = `/GetBalanceProgramInfo`;
        try {
            const response = await fetch(url);
            if (!response.ok) {
                balanceProgramCont.innerHTML = cardErrorInfo('Error loading Balance Program info!', 'getBalanceProgramInfo()');
                const errorData = await response.json();
                throw {
                    status: response.status,
                    message: `${errorData.message}`
                };
            }
            const data = await response.json();
            return data;
        } catch (error) {
            validateSessionExpiration(error.message, error.status);
            throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
        }
    })();
}

//Global
document.addEventListener("DOMContentLoaded", function () {
    // BalanceProgram
    getBalanceProgramInfo()
        .then(data => {
            balanceProgramCont.innerHTML = '';
            let balanceAmount = data.balanceProgramInfo.balanceAmount;
            let dataItems = data.balanceProgramInfo.lastRequests;
            if (dataItems.length > 0) {
                firstCardBPContent.style.justifyContent = 'left';
                firstCardBPContent.style.display = 'block';
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
                    balanceProgramCont.appendChild(pendingTimesheetsRow);
                });
            } else {
                firstCardBPContent.innerHTML = `<div><div style="text-align:center"><img src="/icons/Shared/check.svg"></div>
                <span>You don't have pending timesheets</span></div>`;
                firstCardBPContent.style.alignItems = 'center';
                firstCardBPContent.style.display = 'flex';
            }
        })
        .catch(error => {
            console.error(`Failed to load balance program info: ${error.message}`);
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
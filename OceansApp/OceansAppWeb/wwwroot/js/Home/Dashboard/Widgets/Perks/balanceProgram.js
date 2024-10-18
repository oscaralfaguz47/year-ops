//Balance Program
const balanceProgramTitle = getElementById('BalanceProgram-title');
const balanceProgramCont = getElementById('BalanceProgram-cont');
const firstCardBPContent = balanceProgramCont.closest('.card-content');
function getBalanceProgramInfo() {
    return (async () => {
        balanceProgramTitle.textContent = 'Your remaining Balance Program is...';
        firstCardBPContent.style.justifyContent = 'center';
        firstCardBPContent.style.overflowY = 'hidden';
        const firstCardBP = balanceProgramCont.closest('.card');
        firstCardBP.style.maxHeight = '220px';
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
            let balanceAmount = data.balanceProgramInfo.balanceAmount;
            balanceProgramCont.innerHTML = balanceAmount !== null ? `
            <div class="balance-header">
            <span>${balanceAmount}<span class="usd-lb">USD</span></span><a target="blanck" href="https://app.fillout.com/t/9QGFtqwy6yus" class="claim-btn">Request</a>
            <div>` : ``;
            let dataItems = data.balanceProgramInfo.lastRequests;
            if (dataItems.length > 0) {
                const lastRequestTitle = document.createElement('label');
                lastRequestTitle.textContent = `Last requests`;
                balanceProgramCont.appendChild(lastRequestTitle);
                firstCardBPContent.style.justifyContent = 'left';
                firstCardBPContent.style.display = 'block';
                const rowsContainer = document.createElement('div');
                rowsContainer.className = 'rows-container';
                dataItems.forEach(function (obj, index) {
                    const requestRow = document.createElement('div');
                    requestRow.className = 'request-row';
                    let requestDate = new Date(obj.date);

                    requestRow.innerHTML = `<span class="date-lb">${getMonthName(requestDate.getMonth()).slice(0, 3)} ${requestDate.getDate()}, ${requestDate.getFullYear()}</span>
                    <span class="bp-status">${getStatusLabel(obj.status === 'Waiting to be approved' ? 'Pending approved' : obj.status)}</span><span class="amount-lb">$${obj.amount}</span>`;
                    rowsContainer.appendChild(requestRow);
                });
                balanceProgramCont.appendChild(rowsContainer);
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
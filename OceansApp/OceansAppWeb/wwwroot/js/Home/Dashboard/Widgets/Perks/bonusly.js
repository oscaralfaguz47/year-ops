//Bonusly
const bonuslyTitle = getElementById('Bonusly-title');
const bonuslyCont = getElementById('Bonusly-cont');
const firstCardBoContent = bonuslyCont.closest('.card-content');
function getBonuslyInfo() {
    return (async () => {
        bonuslyTitle.textContent = 'Your remaining Bonusly balance is...';
        firstCardBoContent.style.justifyContent = 'center';
        firstCardBoContent.style.overflowY = 'hidden';
        const firstCardBP = bonuslyCont.closest('.card');
        firstCardBP.style.maxHeight = '220px';
        bonuslyCont.innerHTML = loadingISpinner();
        const url = `/GetBonuslyInfo`;
        try {
            const response = await fetch(url);
            if (!response.ok) {
                bonuslyCont.innerHTML = cardErrorInfo('Error loading Bonusly info!', 'getBonuslyInfo()');
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
    getBonuslyInfo()
        .then(data => {
            let balanceAmount = data.balanceProgramInfo.balanceAmount;
            bonuslyCont.innerHTML = balanceAmount !== null ? `
            <div class="balance-header">
            <span>${balanceAmount}<span class="usd-lb">XPs</span></span><a target="blanck" href="https://app.bonus.ly/" class="claim-btn">Claim XP</a>
            <div>` : ``;
            let dataItems = data.balanceProgramInfo.lastRequests;
            if (dataItems.length > 0) {
                const lastRequestTitle = document.createElement('label');
                lastRequestTitle.textContent = `Last requests`;
                bonuslyCont.appendChild(lastRequestTitle);
                firstCardBoContent.style.justifyContent = 'left';
                firstCardBoContent.style.display = 'block';
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
                bonuslyCont.appendChild(rowsContainer);
            } else {
                firstCardBoContent.innerHTML = `<div><div style="text-align:center"><img src="/icons/Shared/check.svg"></div>
                <span>You don't have pending timesheets</span></div>`;
                firstCardBoContent.style.alignItems = 'center';
                firstCardBoContent.style.display = 'flex';
            }
        })
        .catch(error => {
            console.error(`Failed to load bonusly info: ${error.message}`);
        });
});
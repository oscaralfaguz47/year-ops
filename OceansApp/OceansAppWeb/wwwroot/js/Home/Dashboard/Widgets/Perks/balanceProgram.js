//Balance Program
const balanceProgramTitle = getElementById('BalanceProgram-title');
const balanceProgramCont = getElementById('BalanceProgram-cont');
const firstCardBPContent = balanceProgramCont.closest('.card-content');
function getBalanceProgramInfo() {
    return (async () => {
        balanceProgramTitle.innerHTML = '<img src="/icons/Shared/hands-holding-circle.svg"> Your remaining Balance Program is...';
        firstCardBPContent.style.justifyContent = 'center';
        firstCardBPContent.style.overflowY = 'hidden';
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
            const rowsContainer = document.createElement('div');
            rowsContainer.className = 'rows-container';
            const lastRequestTitle = document.createElement('label');
            lastRequestTitle.textContent = `Last requests`;
            balanceProgramCont.appendChild(lastRequestTitle);
            firstCardBPContent.style.justifyContent = 'left';
            firstCardBPContent.style.display = 'block';
            if (dataItems.length > 0) {
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
                rowsContainer.innerHTML = `<p>You have not made any requests yet.</p>`;
                balanceProgramCont.appendChild(rowsContainer);
            }
        })
        .catch(error => {
            console.error(`Failed to load balance program info: ${error.message}`);
        });
});
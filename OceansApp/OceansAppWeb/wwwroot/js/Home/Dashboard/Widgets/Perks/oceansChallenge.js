//Oceans Challenge
const oceansChallengeTitle = getElementById('OceansChallenge-title');
const oceansChallengeCont = getElementById('OceansChallenge-cont');
const firstCardOCContent = oceansChallengeCont.closest('.card-content');
function getOceansChallengeInfo() {
    return (async () => {
        oceansChallengeTitle.innerHTML = `<img src="/icons/Shared/books.svg"> Your remaining Oceans Challenge is...`;
        firstCardOCContent.style.justifyContent = 'center';
        firstCardOCContent.style.overflowY = 'hidden';
        const firstCardBP = oceansChallengeCont.closest('.card');
        oceansChallengeCont.innerHTML = loadingISpinner();
        const url = `/GetOceansChallengeInfo`;
        try {
            const response = await fetch(url);
            if (!response.ok) {
                oceansChallengeCont.innerHTML = cardErrorInfo('Error loading Balance Program info!', 'getOceansChallengeInfo()');
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
    // Oceans Challenge
    getOceansChallengeInfo()
        .then(data => {
            let balanceAmount = data.oceansChallengeInfo.balanceAmount;
            oceansChallengeCont.innerHTML = balanceAmount !== null ? `
            <div class="balance-header">
            <span>${balanceAmount}<span class="usd-lb">USD</span></span><a target="blanck" href="https://app.fillout.com/t/9QGFtqwy6yus" class="claim-btn">Request</a>
            <div>` : ``;
            let dataItems = data.oceansChallengeInfo.lastRequests;
            const rowsContainer = document.createElement('div');
            rowsContainer.className = 'rows-container';
            const lastRequestTitle = document.createElement('label');
            lastRequestTitle.textContent = `Last requests`;
            oceansChallengeCont.appendChild(lastRequestTitle);
            firstCardOCContent.style.justifyContent = 'left';
            firstCardOCContent.style.display = 'block';
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
                oceansChallengeCont.appendChild(rowsContainer);
            } else {
                rowsContainer.innerHTML = `<p>You have not made any requests yet.</p>`;
                oceansChallengeCont.appendChild(rowsContainer);
            }
        })
        .catch(error => {
            console.error(`Failed to load oceans challenge info: ${error.message}`);
        });
});
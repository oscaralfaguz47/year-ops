//Bonusly
const bonuslyTitle = getElementById('Bonusly-title');
const bonuslyCont = getElementById('Bonusly-cont');
const firstCardBoContent = bonuslyCont.closest('.card-content');
function getBonuslyInfo() {
    return (async () => {
        bonuslyTitle.innerHTML = `<img src="/img/globalIcons/bonusly-icon.ico"> <a target="blanck" href="https://app.bonus.ly/">Bonus.ly</a>`;
        firstCardBoContent.style.justifyContent = 'center';
        firstCardBoContent.style.overflowY = 'hidden';
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
    // Bonusly
    getBonuslyInfo()
        .then(data => {
            if (data.bonuslyIsDown !== undefined && data.bonuslyIsDown) {
                bonuslyCont.innerHTML = `<div class="bonusly-down">
                <img src="/icons/Shared/face-down.svg">
                <p>It seems that Bonusly is having some issues with their application. Please wait until they restore the service.</p>
                </div>`;
                return;
            }

            if (!data.bonuslyUserExists) {
                bonuslyCont.innerHTML = `<div class="no-user-message">
                 <img src="/icons/Shared/headset.svg">
                <p>It appears you don't have a Bonusly account yet. Please contact the administrative team to request an invite.</p>
                </div>`;
                return;
            }

            bonuslyTitle.innerHTML = `<img src="/img/globalIcons/bonusly-icon.ico">Your remaining <a target="blanck" href="https://app.bonus.ly/"> Bonus.ly </a> balance is...`;
            let balanceAmount = data.bonuslyInfo.balanceAmount;
            bonuslyCont.innerHTML = balanceAmount !== null ? `
            <div class="balance-header">
            <span>${balanceAmount}<span class="usd-lb">XPs</span></span><a target="blanck" href="https://app.bonus.ly/" class="claim-btn">Claim XP</a>
            <div>` : ``;
            let dataItems = data.bonuslyInfo.lastRequests;
            const rowsContainer = document.createElement('div');
            rowsContainer.className = 'rows-container';
            const lastRequestTitle = document.createElement('label');
            lastRequestTitle.textContent = `Last requests`;
            bonuslyCont.appendChild(lastRequestTitle);
            firstCardBoContent.style.justifyContent = 'left';
            firstCardBoContent.style.display = 'block';
            if (dataItems.length > 0) {
                dataItems.forEach(function (obj, index) {
                    const requestRow = document.createElement('div');
                    requestRow.className = 'request-row';
                    let requestDate = new Date(obj.creationDate);

                    requestRow.innerHTML = `<span class="date-lb">${getMonthName(requestDate.getMonth()).slice(0, 3)} ${requestDate.getDate()}, ${requestDate.getFullYear()}</span>
                   <img title="${obj.name}" src="${obj.imageUrl}"><span class="status-lb">${obj.status === 'new' ? 'Pending' : 'Approved'}</span><span class="amount-lb">${obj.displayPrice}</span>`;
                    rowsContainer.appendChild(requestRow);
                });
                bonuslyCont.appendChild(rowsContainer);
            } else {
                rowsContainer.innerHTML = `<p>You have not made any requests yet.</p>`;
                bonuslyCont.appendChild(rowsContainer);
            }
        })
        .catch(error => {
            console.error(`Failed to load bonusly info: ${error.message}`);
        });
});
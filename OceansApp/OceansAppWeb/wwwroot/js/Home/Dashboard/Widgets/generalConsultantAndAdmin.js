const generalConsultantCont = getElementById('GeneralConsultantCont');
const firstCardGCContent = generalConsultantCont.closest('.card-content');

function getActiveProjectsInfo() {
    return (async () => {
        firstCardGCContent.style.justifyContent = 'center';
        firstCardGCContent.style.overflowY = 'hidden';
        generalConsultantCont.innerHTML = loadingISpinner();
        const url = `/GetActiveProjectsInfo`;
        try {
            const response = await fetch(url);
            if (!response.ok) {
                generalConsultantCont.innerHTML = cardErrorInfo('Error loading Success Managers info!', 'getActiveProjectsInfo()');
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
    // Active Projects Info - Success Managers
    getActiveProjectsInfo()
        .then(data => {
            console.log(data);
            generalConsultantCont.innerHTML = '';
            let dataItems = data.activeProjectsInfo;

            firstCardGCContent.style.justifyContent = 'left';
            firstCardGCContent.style.display = 'block';

            const successManagersTitle = document.createElement('label');
            successManagersTitle.textContent = 'Success Managers';
            generalConsultantCont.appendChild(successManagersTitle);

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
                generalConsultantCont.appendChild(rowsContainer);
            } else {
                generalConsultantCont.innerHTML = `<p>You don't have an assigned project yet, please contact the admin team to assign you a project.</p>`;
            }
            const adminTeamTitle = document.createElement('label');
            adminTeamTitle.textContent = 'Admin Team';
        })
        .catch(error => {
            console.error(`Failed to load projects success managers: ${error.message}`);
        });
});
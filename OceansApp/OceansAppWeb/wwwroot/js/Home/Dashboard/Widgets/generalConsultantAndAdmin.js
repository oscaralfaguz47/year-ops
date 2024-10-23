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

function getActiveAdminUsers() {
    return (async () => {
        firstCardGCContent.style.justifyContent = 'center';
        firstCardGCContent.style.overflowY = 'hidden';
        generalConsultantCont.innerHTML = loadingISpinner();
        const url = `/GetActiveAdminUsers`;
        try {
            const response = await fetch(url);
            if (!response.ok) {
                generalConsultantCont.innerHTML = cardErrorInfo('Error loading Success Managers info!', 'getActiveAdminUsers()');
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
            generalConsultantCont.innerHTML = '';
            let dataItems = data.activeProjectsInfo;

            firstCardGCContent.style.justifyContent = 'left';
            firstCardGCContent.style.display = 'block';

            const successManagersTitle = document.createElement('label');
            successManagersTitle.className = 'subtitles';
            successManagersTitle.textContent = `Success Manager${dataItems.length > 1 ? 's': ''}`;
            generalConsultantCont.appendChild(successManagersTitle);

            if (dataItems.length > 0) {
                const rowsContainer = document.createElement('div');
                rowsContainer.className = 'rows-container';
                dataItems.forEach(function (obj, index) {
                    const rowEl = document.createElement('div');
                    rowEl.className = 'row-el';
                    rowEl.innerHTML = `<label class="project-name">${obj.projectName}</label>
                    <div class="person-cont">
                      <div class="media-cont">
                        <a href="mailto:${obj.successManagerEmail}"><img title="${obj.successManagerEmail}" src="/icons/Shared/envelope-blue-oceans.svg" /></a>
                        <a href="slack://open"><img title="Contant by Slack" src="/icons/Shared/slack-blueLight-oceans.svg" /></a>
                        <a href="tel:${obj.successManagerPhone.replace(/\s+/g, '')}"><img title="${obj.successManagerPhone}" src="/icons/Shared/phone-blueLight-oceans.svg" /></a>
                      </div>
                      <div class="profile-img-cont">
                        <img src="${obj.profileUrl === null ? '/icons/Shared/profile-user.svg' : obj.profileUrl}" />
                      </div>
                      <div class="person-description">
                        <label>${obj.successManagerName}</label>
                        <p>Primary contact, client and project related queries, overtime, time off.</p>
                      </div>
                    </div>`;
                    rowsContainer.appendChild(rowEl);
                });
                generalConsultantCont.appendChild(rowsContainer);
            } else {
                const noProjectLb = document.createElement('label');
                noProjectLb.className = 'no-project-lb';
                noProjectLb.textContent = `You don't have an assigned project yet, please contact the admin team to assign you a project.`;
                generalConsultantCont.appendChild(noProjectLb);
            }
        })
        .catch(error => {
            console.error(`Failed to load success managers: ${error.message}`);
        });

    // Active Admin Users
    getActiveAdminUsers()
        .then(data => {
            let dataItems = data.activeAdminUsers;

            console.log(data);

            if (dataItems.length > 0) {
                const successManagersTitle = document.createElement('label');
                successManagersTitle.className = 'subtitles';
                successManagersTitle.textContent = `Success Manager${dataItems.length > 1 ? 's' : ''}`;
                generalConsultantCont.appendChild(successManagersTitle);

                const rowsContainer = document.createElement('div');
                rowsContainer.className = 'rows-container';
                dataItems.forEach(function (obj, index) {
                    const rowEl = document.createElement('div');
                    rowEl.className = 'row-el';
                    rowEl.innerHTML = `<label class="project-name">${obj.projectName}</label>
                    <div class="person-cont">
                      <div class="media-cont">
                        <a href="mailto:${obj.successManagerEmail}"><img title="${obj.successManagerEmail}" src="/icons/Shared/envelope-blue-oceans.svg" /></a>
                        <a href="slack://open"><img title="Contant by Slack" src="/icons/Shared/slack-blueLight-oceans.svg" /></a>
                        <a href="tel:${obj.successManagerPhone.replace(/\s+/g, '')}"><img title="${obj.successManagerPhone}" src="/icons/Shared/phone-blueLight-oceans.svg" /></a>
                      </div>
                      <div class="profile-img-cont">
                        <img src="${obj.profileUrl === null ? '/icons/Shared/profile-user.svg' : obj.profileUrl}" />
                      </div>
                      <div class="person-description">
                        <label>${obj.successManagerName}</label>
                        <p>Primary contact, client and project related queries, overtime, time off.</p>
                      </div>
                    </div>`;
                    rowsContainer.appendChild(rowEl);
                });
                generalConsultantCont.appendChild(rowsContainer);
            } else {
                const noProjectLb = document.createElement('label');
                noProjectLb.className = 'no-project-lb';
                noProjectLb.textContent = `You don't have an assigned project yet, please contact the admin team to assign you a project.`;
                generalConsultantCont.appendChild(noProjectLb);
            }
        })
        .catch(error => {
            console.error(`Failed to load admin users: ${error.message}`);
        });
});


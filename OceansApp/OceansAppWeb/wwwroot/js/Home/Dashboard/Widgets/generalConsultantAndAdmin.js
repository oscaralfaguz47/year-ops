const successManagersCont = getElementById('SuccessManagersCont');
const firstCardGCContent = successManagersCont.closest('.card-content');
const adminTeamSection = getElementById('AdminSectionCont');
const firstCardAUContent = adminTeamSection.closest('.card-content');
const holidaysCont = getElementById('HolidaysCont');
const firstCardHoContent = holidaysCont.closest('.card-content');

function getActiveProjectsInfo() {
    return (async () => {
        firstCardGCContent.style.justifyContent = 'center';
        firstCardGCContent.style.overflowY = 'hidden';
        successManagersCont.innerHTML = loadingISpinner();
        const url = `/GetActiveProjectsInfo`;
        try {
            const response = await fetch(url);
            if (!response.ok) {
                successManagersCont.innerHTML = cardErrorInfo('Error loading Success Managers info!', 'getActiveProjectsInfo()');
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
        firstCardAUContent.style.justifyContent = 'center';
        firstCardAUContent.style.overflowY = 'hidden';
        adminTeamSection.innerHTML = loadingISpinner();
        const url = `/GetActiveAdminUsers`;
        try {
            const response = await fetch(url);
            if (!response.ok) {
                adminTeamSection.innerHTML = cardErrorInfo('Error loading Admin team!', 'getActiveAdminUsers()');
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

function getHolidaysList() {
    return (async () => {
        firstCardHoContent.style.justifyContent = 'center';
        firstCardHoContent.style.overflowY = 'hidden';
        holidaysCont.innerHTML = loadingISpinner();
        const url = `/GetConsultantHolidays`;
        try {
            const response = await fetch(url);
            if (!response.ok) {
                holidaysCont.innerHTML = cardErrorInfo('Error loading Holidays section!', 'getHolidaysList()');
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
            successManagersCont.innerHTML = '';
            let dataItems = data.activeProjectsInfo;

            firstCardGCContent.style.justifyContent = 'left';
            firstCardGCContent.style.display = 'block';

            const successManagersTitle = document.createElement('label');
            successManagersTitle.className = 'subtitles';
            successManagersTitle.textContent = `Success Manager${dataItems.length > 1 ? 's': ''}`;
            successManagersCont.appendChild(successManagersTitle);

            if (dataItems.length > 0) {
                const rowsContainer = document.createElement('div');
                rowsContainer.className = 'rows-container';
                dataItems.forEach(function (obj, index) {
                    const rowEl = document.createElement('div');
                    rowEl.className = 'row-el';
                    rowEl.innerHTML = `<label class="determ-name">${obj.projectName}</label>
                    <div class="person-cont">
                      <div class="media-cont">
                        <a href="mailto:${obj.successManagerEmail}"><img title="${obj.successManagerEmail}" src="/icons/Shared/envelope-blue-oceans.svg" /></a>
                        <a href="slack://open"><img title="Contant by Slack" src="/icons/Shared/slack-blueLight-oceans.svg" /></a>
                        <a href="tel:${obj.successManagerPhone === null ? '': obj.successManagerPhone.replace(/\s+/g, '')}"><img title="${obj.successManagerPhone}" src="/icons/Shared/phone-blueLight-oceans.svg" /></a>
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
                successManagersCont.appendChild(rowsContainer);
            } else {
                const noProjectLb = document.createElement('label');
                noProjectLb.className = 'no-project-lb';
                noProjectLb.textContent = `You don't have an assigned project yet, please contact the admin team to assign you a project.`;
                successManagersCont.appendChild(noProjectLb);
            }
        })
        .catch(error => {
            console.error(`Failed to load success managers: ${error.message}`);
        });

    // Active Admin Users
    getActiveAdminUsers()
        .then(data => {
            firstCardAUContent.style.justifyContent = 'left';
            firstCardAUContent.style.display = 'block';
            adminTeamSection.innerHTML = '';
            let dataItems = data.activeAdminUsers;
            if (dataItems.length > 0) {
                let areaName = '';
                dataItems.forEach(function (obj, index) {
                    if (areaName !== obj.areaName) {
                        const adminPersonTitle = document.createElement('label');
                        adminPersonTitle.className = 'subtitles';
                        adminPersonTitle.textContent = `${obj.areaName === 'Area Finanzas' ? 'Finance Team': 'People & Culture Team'}`;
                        adminTeamSection.appendChild(adminPersonTitle);
                    }
                    areaName = obj.areaName;

                    const rowEl = document.createElement('div');
                    rowEl.className = 'row-el';
                    let personDescription = obj.areaName === 'Area Finanzas' ? 'Payments, banking info changes, payment method, calculation doubts.' :
                        'Benefits, activities, voluntary time, company culture, accesses.'
                    rowEl.innerHTML = `<label class="determ-name">${obj.positionName}</label>
                    <div class="person-cont">
                      <div class="media-cont">
                        <a href="mailto:${obj.email}"><img title="${obj.email}" src="/icons/Shared/envelope-blue-oceans.svg" /></a>
                        <a href="slack://open"><img title="Contant by Slack" src="/icons/Shared/slack-blueLight-oceans.svg" /></a>
                        <a href="tel:${obj.phoneNumber === null ? '': obj.phoneNumber.replace(/\s+/g, '')}"><img title="${obj.phoneNumber}" src="/icons/Shared/phone-blueLight-oceans.svg" /></a>
                      </div>
                      <div class="profile-img-cont">
                        <img src="${obj.profileUrl === null ? '/icons/Shared/profile-user.svg' : obj.profileUrl}" />
                      </div>
                      <div class="person-description">
                        <label>${obj.consultantName}</label>
                        <p>${personDescription}</p>
                      </div>
                    </div>`;
                    adminTeamSection.appendChild(rowEl);
                });
            }
        })
        .catch(error => {
            console.error(`Failed to load admin users: ${error.message}`);
        });

    // Holidays List
    getHolidaysList()
        .then(data => {
            holidaysCont.innerHTML = '';
            let dataItems = data.holidaysList;
            if (dataItems.length > 0) {
                const holidaysTitle = document.createElement('label');
                holidaysTitle.className = 'card-title';
                holidaysTitle.textContent = `Your ${new Date().getFullYear()} holidays`;

                holidaysCont.appendChild(holidaysTitle);
                dataItems.forEach(function (obj, index) {
                    let holidayDate = new Date(obj.date);
                    const rowEl = document.createElement('div');
                    rowEl.className = 'row-el';
                    rowEl.innerHTML = `
                    <div class="holiday-row">
                     <label class="h-date">${formatDateMonthDateSuffix(holidayDate)}</label>
                     <label class="h-name">${obj.holidayName}</label>
                    </div>`;
                    holidaysCont.appendChild(rowEl);
                });
            }
        })
        .catch(error => {
            console.error(`Failed to load holidays: ${error.message}`);
        });
});


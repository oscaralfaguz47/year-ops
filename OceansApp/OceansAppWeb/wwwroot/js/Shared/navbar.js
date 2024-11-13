document.addEventListener('click', function (event) {
    const navbarCollapse = document.querySelector('.navbar-collapse');
    const isNavbarTogglerClicked = event.target.closest('.navbar-toggler');
    const isNavbarCollapseClicked = event.target.closest('.navbar-collapse');

    if (!isNavbarTogglerClicked && !isNavbarCollapseClicked && navbarCollapse.classList.contains('show')) {
        new bootstrap.Collapse(navbarCollapse).hide();
    }
});

const blueOceans = 'var(--clr-blueLight)';

//Dashboard item
const dashboardIconBlue = '/icons/Shared/Navbar/dashboard-blue.svg';
const dashboardIconWhite = '/icons/Shared/Navbar/dashboard-white.svg';

const dashboardItem = document.getElementById('dashboardItem');
const dashboardIcon = dashboardItem.querySelector('img');

dashboardItem.addEventListener('mouseenter', () => {
    if (!dashboardItem.classList.contains('nav-item-active')) {
        dashboardIcon.src = dashboardIconBlue;
    }
});

dashboardItem.addEventListener('mouseleave', () => {
    if (!dashboardItem.classList.contains('nav-item-active')) {
        dashboardIcon.src = dashboardIconWhite;
    }
});
function setDashboardItemActive() {
    dashboardItem.classList.add('nav-item-active');
    const alink = dashboardItem.querySelector('a');
    alink.style.color = blueOceans;
    const aImg = dashboardItem.querySelector('img');
    aImg.src = dashboardIconBlue;
}

//Timesheet item
const timesheetIconBlue = '/icons/Shared/Navbar/timesheet-blue.svg';
const timesheetIconWhite = '/icons/Shared/Navbar/timesheet-white.svg';

const timesheetItem = document.getElementById('timesheetItem');
const timesheetIcon = timesheetItem.querySelector('img');

timesheetItem.addEventListener('mouseenter', () => {
    if (!timesheetItem.classList.contains('nav-item-active')) {
        timesheetIcon.src = timesheetIconBlue;
    }
});

timesheetItem.addEventListener('mouseleave', () => {
    if (!timesheetItem.classList.contains('nav-item-active')) {
        timesheetIcon.src = timesheetIconWhite;
    }
});

function setTimesheetItemActive() {
    timesheetItem.classList.add('nav-item-active');
    const alink = timesheetItem.querySelector('a');
    alink.style.color = blueOceans;
    const aImg = timesheetItem.querySelector('img');
    aImg.src = timesheetIconBlue;
}

//General item
const generalIconBlue = '/icons/Shared/Navbar/general-blue.svg';
const generalIconWhite = '/icons/Shared/Navbar/general-white.svg';

const generalItem = document.getElementById('generalItem');
const generalIcon = generalItem.querySelector('img');

generalItem.addEventListener('mouseenter', () => {
    if (!generalItem.classList.contains('nav-item-active')) {
        generalIcon.src = generalIconBlue;
    }
});

generalItem.addEventListener('mouseleave', () => {
    if (!generalItem.classList.contains('nav-item-active')) {
        generalIcon.src = generalIconWhite;
    }
});

function setGeneralItemActive() {
    generalItem.classList.add('nav-item-active');
    const alink = generalItem.querySelector('a');
    alink.style.color = blueOceans;
    const aImg = generalItem.querySelector('img');
    aImg.src = generalIconBlue;
}

//Account Management item
const accountManagementIconBlue = '/icons/Shared/Navbar/account-management-blue.svg';
const accountManagementIconWhite = '/icons/Shared/Navbar/account-management-white.svg';

const accountManagementItem = document.getElementById('accountManagementItem');
const accountManagementIcon = accountManagementItem.querySelector('img');

accountManagementItem.addEventListener('mouseenter', () => {
    if (!accountManagementItem.classList.contains('nav-item-active')) {
        accountManagementIcon.src = accountManagementIconBlue;
    }
});

accountManagementItem.addEventListener('mouseleave', () => {
    if (!accountManagementItem.classList.contains('nav-item-active')) {
        accountManagementIcon.src = accountManagementIconWhite;
    }
});

function setAccountManagementItemActive() {
    accountManagementItem.classList.add('nav-item-active');
    const alink = accountManagementItem.querySelector('a');
    alink.style.color = blueOceans;
    const aImg = accountManagementItem.querySelector('img');
    aImg.src = accountManagementIconBlue;
}

//Finances item
const financesIconBlue = '/icons/Shared/Navbar/finances-blue.svg';
const financesIconWhite = '/icons/Shared/Navbar/finances-white.svg';

const financesItem = document.getElementById('financesItem');
const financesIcon = financesItem.querySelector('img');

financesItem.addEventListener('mouseenter', () => {
    if (!financesItem.classList.contains('nav-item-active')) {
        financesIcon.src = financesIconBlue;
    }
});

financesItem.addEventListener('mouseleave', () => {
    if (!financesItem.classList.contains('nav-item-active')) {
        financesIcon.src = financesIconWhite;
    }
});

function setFinancesItemActive() {
    financesItem.classList.add('nav-item-active');
    const alink = financesItem.querySelector('a');
    alink.style.color = blueOceans;
    const aImg = financesItem.querySelector('img');
    aImg.src = financesIconBlue;
}

//Recruiting item
const recruitingIconBlue = '/icons/Shared/Navbar/recruiting-blue.svg';
const recruitingIconWhite = '/icons/Shared/Navbar/recruiting-white.svg';

const recruitingItem = document.getElementById('recruitingItem');
const recruitingIcon = recruitingItem.querySelector('img');

recruitingItem.addEventListener('mouseenter', () => {
    if (!recruitingItem.classList.contains('nav-item-active')) {
        recruitingIcon.src = recruitingIconBlue;
    }
});

recruitingItem.addEventListener('mouseleave', () => {
    if (!recruitingItem.classList.contains('nav-item-active')) {
        recruitingIcon.src = recruitingIconWhite;
    }
});

function setRecruitingItemActive() {
    recruitingItem.classList.add('nav-item-active');
    const alink = recruitingItem.querySelector('a');
    alink.style.color = blueOceans;
    const aImg = recruitingItem.querySelector('img');
    aImg.src = recruitingIconBlue;
}
//Admin Center item
const adminCenterIconBlue = '/icons/Shared/Navbar/admin-center-blue.svg';
const adminCenterIconWhite = '/icons/Shared/Navbar/admin-center-white.svg';

const adminCenterItem = document.getElementById('adminCenterItem');
const adminCenterIcon = adminCenterItem.querySelector('img');

adminCenterItem.addEventListener('mouseenter', () => {
    if (!adminCenterItem.classList.contains('nav-item-active')) {
        adminCenterIcon.src = adminCenterIconBlue;
    }
});

adminCenterItem.addEventListener('mouseleave', () => {
    if (!adminCenterItem.classList.contains('nav-item-active')) {
        adminCenterIcon.src = adminCenterIconWhite;
    }
});

function setAdminCenterItemActive() {
    adminCenterItem.classList.add('nav-item-active');
    const alink = adminCenterItem.querySelector('a');
    alink.style.color = blueOceans;
    const aImg = adminCenterItem.querySelector('img');
    aImg.src = adminCenterIconBlue;
}
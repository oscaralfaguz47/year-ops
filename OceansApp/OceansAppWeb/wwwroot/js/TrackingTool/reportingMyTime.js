let paymentPeriod = 0;
let currentDate = new Date();
let trackingToolTimeEntrySection = document.getElementById('tracking-tool-time-entry');
let trackingToolReportEntrySection = document.getElementById('tracking-tool-report-entry');
let dateFromInput = document.getElementById('dateFromInput');

async function fillProjectsDropdown() {
    const dropdownList = document.querySelector('.dropdown-list');
    dropdownList.innerHTML = '<li class="spinner-cont"><div class="spinner"></div></li>';

    try {
        const response = await getProjectsWhereConsultantAssigned();
        const projects = response.projects;

        dropdownList.innerHTML = '';
        projects.forEach(project => {
            const listItem = document.createElement('li');
            listItem.innerHTML = `<div class="circle circle-li">${project.name.charAt(0)}</div>${project.name}`;
            listItem.dataset.value = project.projectId;
            listItem.addEventListener('click', function () {
                document.querySelector('.dropdown-selected').innerHTML = `<div class="circle">${project.name.charAt(0)}</div>`;
                document.getElementById('project-name').innerHTML = `${project.name}`;
                document.querySelector('.dropdown-list').style.display = 'none';
                selectProject(project.projectId);
            });
            dropdownList.appendChild(listItem);
        });
    } catch (error) {
        console.error("Error filling the projects dropdown:", error.message);
        dropdownList.innerHTML = '<li>Error loading options</li>';
    }
}
const header = document.getElementById('header');
const loadingBox = document.getElementById('loading-box');
const errorMessageBox = document.getElementById('error-Message-box');
const contentBox = document.getElementById('content-box');
const noProjectsBox = document.getElementById('no-projects-box');

async function getProjectInfo() {
    loadingBox.style.display = 'flex';
    errorMessageBox.style.display = 'none';
    contentBox.style.display = 'none';
    try {
        const response = await getSelectedProjectInfo();
        const projectInfo = response.projectInfoData;
        if (projectInfo !== null) {
            console.log(projectInfo);
            document.getElementById('projectId').value = projectInfo.projectId;
            document.getElementById('on-call-section').style.display = projectInfo.participatesInOnCalls ? 'block' : 'none';
            trackingToolTimeEntrySection.style.display = projectInfo.clientHasTrackingTool ? 'none' : 'block';
            trackingToolReportEntrySection.style.display = projectInfo.clientHasTrackingTool ? 'block' : 'none';
            paymentPeriod = projectInfo.paymentPeriod;
            document.getElementById('payment-period-container').innerHTML = `<span>Your payment period is <strong>${paymentPeriod === 1 ? 'Biweekly' : 'Monthly'}.</strong></span>`;
            let currentDateNoChange = new Date();
            calculatePeriod(currentDateNoChange, paymentPeriod, projectInfo.clientHasTrackingTool);
            document.querySelector('.dropdown-selected').innerHTML = `<div class="circle">${projectInfo.projectName.charAt(0)}</div>`;
            document.getElementById('project-name').innerHTML = `${projectInfo.projectName}`;
            document.getElementById('questions').innerHTML = `<span>Questions on reporting? Contact the Success Manager,
            <strong>${projectInfo.sucessManagerName}</strong> at <a href="mailto:${projectInfo.successManagerEmail}">
            ${projectInfo.successManagerEmail}</a> or via Slack.</span>`;
            header.style.display = 'flex';
            loadingBox.style.display = 'none';
            contentBox.style.display = 'block';
        } else {
            noProjectsBox.style.display = 'block';
            loadingBox.style.display = 'none';
        }
    } catch (error) {
        console.error("Error filling the projects dropdown:", error.message);
        loadingBox.style.display = 'none';
        errorMessageBox.style.display = 'flex';
    }
}
document.addEventListener('DOMContentLoaded', function () {
    let dataLoaded = false;
    const dropdownHeader = document.querySelector('.dropdown-header');
    const dropdownList = document.querySelector('.dropdown-list');

    dropdownHeader.addEventListener('click', function () {
        if (!dataLoaded) {
            fillProjectsDropdown();
            dataLoaded = true;
        }
        dropdownList.style.display = dropdownList.style.display === 'block' ? 'none' : 'block';
    });

    document.addEventListener('click', function (event) {
        if (!dropdownHeader.contains(event.target)) {
            dropdownList.style.display = 'none';
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === "Escape") {
            dropdownList.style.display = 'none';
        }
    });
    getProjectInfo();
});

async function selectProject(projectId) {
    loadingBox.style.display = 'flex';
    contentBox.style.display = 'none';
    var token = $('[name="__RequestVerificationToken"]').val();
    var formData = new FormData();
    formData.append('projectId', projectId);
    fetch("/AccountManagement/ProjectsConsultantsAssigned/SelectConsultantProject"
        , {
            method: 'POST',
            headers: {
                RequestVerificationToken: token
            },
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                header.style.display = 'flex';
                loadingBox.style.display = 'none';
            } else {
                displayToasterError(data.error);
                console.error('There has been a problem with the fetch operation:', data.detail);
                loadingBox.style.display = 'none';
            }
            getProjectInfo();
        });
}

//DATE NAVEGATION BUTTONS ------------------

// Function to calculate the new period based on direction and mode.
function adjustDate(direction, mode) {
    // Calculate the number of days to adjust based on the payment method.

    const dayAdjustment = mode === 1 ? 15 : new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 0).getDate();

    if (direction === 'left') {
        currentDate = new Date(currentDate.setDate(currentDate.getDate() - dayAdjustment));
    } else if (direction === 'right') {
        currentDate = new Date(currentDate.setDate(currentDate.getDate() + dayAdjustment));
    }
}

const formatDate = (date) => {
    let month = '' + (date.getMonth() + 1),
        day = '' + date.getDate(),
        year = date.getFullYear();

    if (month.length < 2)
        month = '0' + month;
    if (day.length < 2)
        day = '0' + day;

    return [month, day, year].join('/');
};
function formatDateYyyyMmDd(date) {
    let day = date.getDate().toString().padStart(2, '0');
    let month = (date.getMonth() + 1).toString().padStart(2, '0');
    let year = date.getFullYear();
    return `${year}-${month}-${day}`;
}
// Calculates and displays start and end dates based on the click direction.
const handleButtonClick = (direction) => {
    adjustDate(direction, paymentPeriod, null);

    let { startDate, endDate } = calculatePeriod(currentDate, paymentPeriod);
    console.log(`Fecha desde: ${formatDate(startDate)}, Fecha hasta: ${formatDate(endDate)}`);
};

const calculatePeriod = (date, mode, clientHasTrackingTool) => {
    let startDate, endDate;

    if (mode === 1) { // Biweekly
        // Adjusts to the nearest fortnight before the current date
        const dayOfMonth = date.getDate();
        if (dayOfMonth <= 15) {
            startDate = new Date(date.getFullYear(), date.getMonth(), 1);
            endDate = new Date(date.getFullYear(), date.getMonth(), 15);
        } else {
            startDate = new Date(date.getFullYear(), date.getMonth(), 16);
            endDate = new Date(date.getFullYear(), date.getMonth() + 1, 0);
        }
    } else if (mode === 2) { // Montly
        startDate = new Date(date.getFullYear(), date.getMonth(), 1);
        endDate = new Date(date.getFullYear(), date.getMonth() + 1, 0);
    }
    generateDateList(formatDateYyyyMmDd(startDate), formatDateYyyyMmDd(endDate));
    document.getElementById('previous-date').innerHTML = `<span>${formatDate(startDate)}</span>`;
    document.getElementById('next-date').innerHTML = `<span>${formatDate(endDate)}</span>`;
    dateFromInput.value = formatDate(endDate);
    return { startDate, endDate };
};

async function submitReportToBePaid() {
    try {
        const confirmation = await Swal.fire({
            title: "",
            text: `Confirm report submission? No changes are allowed afterward.`,
            icon: 'warning',
            showCancelButton: true,
            cancelButtonText: 'Cancel',
            cancelButtonColor: '#9ba8b8',
            confirmButtonColor: '#eeb30f',
            confirmButtonText: 'Yes, Submit!'
        });

        if (!confirmation.isConfirmed) {
            return; 
        }
        displaySpinner();

        const token = $('[name="__RequestVerificationToken"]').val();
        const formData = new FormData();
        //formData.append('dateFrom', dateFrom);
        //formData.append('dateTo', dateTo);

        //const response = await fetch("/Recruiting/Interviews/RejectInterview", {
        //    method: 'POST',
        //    headers: {
        //        'RequestVerificationToken': token
        //    },
        //    body: formData
        //});

        //const data = await response.json();

        //if (data.success) {
        //    toastr.success(data.message);
        //} else {
        //    displayToasterError(data.error);
        //    console.error('There has been a problem with the fetch operation:', data.detail);
        //}

    } catch (error) {
        console.error('An error occurred:', error);
    } finally {
        hideSpinner();
    }
}

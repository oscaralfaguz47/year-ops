let trackingToolTimeEntrySection = document.getElementById('tracking-tool-time-entry');
let trackingToolReportEntrySection = document.getElementById('tracking-tool-report-entry');
let dateToInput = document.getElementById('dateToInput');
let dateFromInput = document.getElementById('dateFromInput');
let errorMessageIntern = document.getElementById('error-message-intern');
let loadingBoxIntern = document.getElementById('loading-box-intern');
let clientHasTrackingToolValue = false;
let submissionInfo = document.getElementById('submission-info');
let submissionError = document.getElementById('submission-errors');

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
        validateSessionExpiration(error.message);
        console.error("Error filling the projects dropdown:", error.message);
        dropdownList.innerHTML = '<li>Error loading options</li>';
    }
}
const header = document.getElementById('header');
const loadingBox = document.getElementById('loading-box-global');
const errorMessageBox = document.getElementById('error-message-rep-time');
const contentBox = document.getElementById('content-box');
const noProjectsBox = document.getElementById('no-projects-box');

const dropdownSelect = document.querySelector('.dropdown-selected');
const projectNamelabelSelect = document.getElementById('project-name');

async function getProjectInfo() {
    loadingBox.style.display = 'flex';
    errorMessageBox.style.display = 'none';
    contentBox.style.display = 'none';
    try {
        const response = await getSelectedProjectInfo();
        const projectInfo = response.projectInfoData;
        console.log(projectInfo);
        if (!projectInfo.accessToTrackingTool) {
            noProjectsBox.style.display = 'block';
            noProjectsBox.innerHTML = `<div>
            <div class="background-cont">
                <div><i>Non reportable project</i></div>
                <p><strong>You are part of the <span>"${projectInfo.projectName}"</span> project, but you are not allowed to report any time here.</strong></p>
                <p><strong>Please contact the administrator if you need to report any time.</strong></p>
            </div>
        </div>`;
        }
        if (projectInfo.numAssignedProjects > 1 || projectInfo.accessToTrackingTool) {
            dropdownSelect.innerHTML = `<div class="circle">${projectInfo.projectName.charAt(0)}</div>`;
            projectNamelabelSelect.innerHTML = `${projectInfo.projectName}`;
            header.style.display = 'flex';
        }
        if (projectInfo !== null && projectInfo.accessToTrackingTool) {
            noProjectsBox.style.display = 'none';
            document.getElementById('projectId').value = projectInfo.projectId;
            document.getElementById('on-call-section').style.display = projectInfo.participatesInOnCalls ? 'block' : 'none';
            clientHasTrackingToolValue = projectInfo.clientHasTrackingTool;
            trackingToolTimeEntrySection.style.display = projectInfo.clientHasTrackingTool ? 'none' : 'block';
            trackingToolReportEntrySection.style.display = projectInfo.clientHasTrackingTool ? 'block' : 'none';
            paymentPeriod = projectInfo.paymentPeriod;
            document.getElementById('payment-period-container').innerHTML = `<span>Your payment period is <strong>${paymentPeriod === 1 ? 'Biweekly' : 'Monthly'}.</strong></span>`;
            let currentDateNoChange = new Date();
            calculatePeriod(currentDateNoChange, paymentPeriod);
            document.getElementById('questions').innerHTML = `<span>Questions on reporting? Contact the Success Manager,
            <strong>${projectInfo.sucessManagerName}</strong> at <a href="mailto:${projectInfo.successManagerEmail}">
            ${projectInfo.successManagerEmail}</a> or via Slack.</span>`;
            header.style.display = 'flex';
            loadingBox.style.display = 'none';
            contentBox.style.display = 'block';
        } else if (projectInfo === null) {
            noProjectsBox.style.display = 'block';
            noProjectsBox.innerHTML = `<div>
            <div class="background-cont">
                <div><i>Wow!</i></div>
                <p><strong>Looks like you do not have assigned projects yet.</strong></p>
                <p><strong>Please contact the administrator to assign you a project</strong></p>
            </div>
        </div>`;
        }
        loadingBox.style.display = 'none';
    } catch (error) {
        validateSessionExpiration(error.message);
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
    currentDate = new Date();
    loadingBox.style.display = 'flex';
    contentBox.style.display = 'none';
    try {
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
            }).catch(error => {
                validateSessionExpiration(error.message);
            });
    }
    catch (err) {
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        loadingBox.style.display = 'none';
        return null;
    }
}
//Navitate between dates
function navitateBetweenDates(startDate, endDate, button) {
    submissionError.innerHTML = '';
    submissionInfo.innerHTML = `<div class="spinner"></div>`;
    if (clientHasTrackingToolValue) {
        getProjectMovementsClientHasTrackTool().then(() => {
            if (button) button.disabled = false;
        });
    } else {
        getTrackingToolProjectMovements().then(movements => {
            generateDateList(startDate, endDate, movements.movementsList);
            if (button) button.disabled = false;
        });
    }
}

async function submitReportToBePaid() {
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

    try {
        displaySpinner();

        let startDateData = new Date(dateFromInput.value).toISOString();
        let endDateData = new Date(dateToInput.value).toISOString();

        var token = $('[name="__RequestVerificationToken"]').val();

        var data = {
            ProjectId: Number(projectIdInput.value),
            StartPeriodDate: startDateData,
            EndPeriodDate: endDateData
        };

        const response = await fetch('/TrackingTool/ReportingMyTime/SubmitReport', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: token
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
                        return acc.concat(current);
                    }, []);
                    if (errorData.errors.Report !== undefined || errorData.errors.Hours !== undefined) {
                        submissionError.style.display = 'block';
                        submissionError.innerHTML = `<span>${errorData.errors.Report}</span>`;
                    }
                    if (errorData.errors.Hours !== undefined) {
                        submissionError.style.display = 'block';
                        submissionError.innerHTML = `<span>${errorData.errors.Hours}</span>`;
                    }
                    break;
                case "Not Found":
                    displayToasterError(errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }
        const dataFromApi = await response.json();
        displayToasterSuccess(dataFromApi.message);
        if (clientHasTrackingToolValue) {
            getProjectMovementsClientHasTrackTool().then(() => { });
        } else {
            getTrackingToolProjectMovements().then(movements => {
                let dateFrom = new Date(dateFromInput.value);
                let dateTo = new Date(dateToInput.value);
                generateDateList(formatDateYyyyMmDd(dateFrom), formatDateYyyyMmDd(dateTo), movements.movementsList);
            });
        }
        submissionError.innerHTML = '';
        hideSpinner();
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        hideSpinner();
        return null;
    }
}

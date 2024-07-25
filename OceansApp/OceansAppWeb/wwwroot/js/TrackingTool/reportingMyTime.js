
const trackingToolTimeEntrySection = getElementById('tracking-tool-time-entry');
const trackingToolReportEntrySection = getElementById('tracking-tool-report-entry');
const dateToInput = getElementById('dateToInput');
const dateFromInput = getElementById('dateFromInput');
const errorMessageIntern = getElementById('error-message-intern');
const loadingBoxIntern = getElementById('loading-box-intern');
let clientHasTrackingToolValue = false;
const submissionInfo = getElementById('submission-info');
const submissionError = getElementById('submission-errors');
const projectIdInput = getElementById('projectId');
const onCallSectionEl = getElementById('on-call-section');
let selectedProjectName = '';
const inactiveNoTrackingToolSection = getElementById('inactive-no-tracking-in-project-sec');
const totalHoursLabelEl = getElementById('total-hours-label');
async function fillProjectsDropdown(dropdownList) {
    dropdownList.innerHTML = `<li class="spinner-cont"><div class="spinner"></div></li>`;
    dropdownList.style.display = 'block';

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
                dropdownList.style.display = 'none';
                selectProject(project.projectId);
            });
            dropdownList.appendChild(listItem);
        });
    } catch (error) {
        validateSessionExpiration(error.message);
        console.error("Error populating the projects dropdown:", error.message);
        dropdownList.innerHTML = `<li>Error loading options</li>`;
    }
}

const header = getElementById('header');
const loadingBox = getElementById('loading-box-global');
const errorMessageBox = getElementById('error-message-rep-time');
const contentBox = getElementById('content-box');
const noProjectsBox = getElementById('no-projects-box');
const dropdownSelect = document.querySelector('.dropdown-selected');
const projectNamelabelSelect = getElementById('project-name');

async function getProjectInfo() {
    loadingBox.style.display = 'flex';
    errorMessageBox.style.display = 'none';
    contentBox.style.display = 'none';
    try {
        const response = await getSelectedProjectInfo();
        const projectInfo = response.projectInfoData;

        if (projectInfo !== null) {
            if (projectInfo.numAssignedProjects >= 1 || projectInfo.accessToTrackingTool) {
                dropdownSelect.innerHTML = `<div class="circle">${projectInfo.projectName.charAt(0)}</div>`;
                projectNamelabelSelect.innerHTML = `${projectInfo.projectName}`;
                header.style.display = 'flex';
            }
            console.log(projectInfo);
            projectIdInput.value = projectInfo.projectId;
            onCallSectionEl.style.display = projectInfo.participatesInOnCalls ? 'block' : 'none';
            clientHasTrackingToolValue = projectInfo.clientHasTrackingTool;
            trackingToolTimeEntrySection.style.display = projectInfo.clientHasTrackingTool ? 'none' : 'block';
            trackingToolReportEntrySection.style.display = projectInfo.clientHasTrackingTool ? 'block' : 'none';
            paymentPeriod = projectInfo.paymentPeriod;
            selectedProjectName = projectInfo.projectName;
            getElementById('payment-period-container').innerHTML = `<div><span class="strong-label">Your payment period is</span> <span class="gray-bold-span">${paymentPeriod === 1 ? 'Biweekly' : 'Monthly'}</span></div>`;

            getElementById('questions').innerHTML = `<span class="strong-label" style="display:block">Questions? </span> <span>Please reach out to your Success Manager
            </span> <strong style="color:var(--clr-blueLight); display:block;">${projectInfo.successManagerName}</strong> <a class="envelope-link" href="mailto:${projectInfo.successManagerEmail}">
            <div class="envelope-container"><img src="/img/globalIcons/envelope.webp"></div></a>`;

            let currentDateNoChange = new Date();
            calculatePeriod(currentDateNoChange, paymentPeriod);

        } else {
            noProjectsBox.style.display = 'block';
            noProjectsBox.innerHTML = `<div>
            <div class="background-cont">
                <div><i>Wow!</i></div>
                <p><strong>Looks like you do not have assigned projects yet.</strong></p>
                <p><strong>Please contact the administrator to assign you a project</strong></p>
            </div>
        </div>`;
        }
    } catch (error) {
        validateSessionExpiration(error.message);
        console.error("Error filling the projects dropdown:", error.message);
        loadingBox.style.display = 'none';
        errorMessageBox.style.display = 'flex';
    }
}
document.addEventListener('DOMContentLoaded', async function () {
    let dataLoaded = false;
    const dropdownHeader = document.querySelector('.dropdown-header');
    const dropdownList = document.querySelector('.dropdown-list');

    dropdownHeader.addEventListener('click', async function () {
        if (!dataLoaded) {
            try {
                await fillProjectsDropdown(dropdownList);
                dataLoaded = true;
            } catch (error) {
                console.error('Error loading projects:', error);
                displayToasterError('Error loading projects.');
            }
        }
        if (dataLoaded) {
            dropdownList.style.display = 'block';
        }
    });

    document.addEventListener('click', function (event) {
        if (!dropdownHeader.contains(event.target) && !dropdownList.contains(event.target)) {
            dropdownList.style.display = 'none';
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === "Escape") {
            dropdownList.style.display = 'none';
        }
    });

    try {
        await getProjectInfo();
    } catch (error) {
        console.error('Error fetching project info:', error);
        displayToasterError('Error fetching project info.');
    }
});
async function selectProject(projectId) {
    currentDate = new Date();
    loadingBox.style.display = 'flex';
    contentBox.style.display = 'none';
    try {
        var token = $('[name="__RequestVerificationToken"]').val();
        var formData = new FormData();
        formData.append('projectId', projectId);

        const response = await fetch("/AccountManagement/ProjectsConsultantsAssigned/SelectConsultantProject", {
            method: 'POST',
            headers: {
                RequestVerificationToken: token
            },
            body: formData
        });

        const data = await response.json();

        if (data.success) {
            header.style.display = 'flex';
            loadingBox.style.display = 'none';
        } else {
            displayToasterError(data.error);
            console.error('There has been a problem with the fetch operation:', data.detail);
            loadingBox.style.display = 'none';
        }

        await getProjectInfo();
    } catch (error) {
        validateSessionExpiration(error.message);
        console.error('Network or fetch error:', error);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        loadingBox.style.display = 'none';
        return null;
    }
}

//Navitate between dates
async function navitateBetweenDates(startDate, endDate, buttons) {
    try {
        const consultantStatusresponse = await getConsultantStatusInTheProject(dateFromInput.value, dateToInput.value);
        loadingBox.style.display = 'none';
        const statusInfo = consultantStatusresponse.consultantStatusInTheProject;
        console.log(statusInfo);
        inactiveNoTrackingToolSection.style.display = 'none';
        contentBox.style.display = 'block';

        if (statusInfo.isActive && statusInfo.accessToTrackingTool) {
            submissionInfo.style.display = 'block';
            totalHoursLabelEl.style.display = 'block';
            noProjectsBox.style.display = 'none';
            header.style.display = 'flex';
            loadingBox.style.display = 'none';
        } else {
            trackingToolTimeEntrySection.style.display = 'none';
            trackingToolReportEntrySection.style.display = 'none';
            submissionInfo.style.display = 'none';
            totalHoursLabelEl.style.display = 'none';
            inactiveNoTrackingToolSection.style.display = 'block';
        }
        if (!statusInfo.isActive) {
            inactiveNoTrackingToolSection.innerHTML = `<div><img src="/icons/Shared/question.svg"><br><label>You are 
                <strong>Inactive</strong> for this period.<br>Please contact your Success Manager if you need to report any time here.
                </label></div>`;
        }
        if (!statusInfo.accessToTrackingTool) {
            inactiveNoTrackingToolSection.innerHTML = `<div><img src="/icons/Shared/question.svg"><br><label>It seems that you don't need to report your time.
            <br>Please contact your Success Manager if you need to report any time here.
                </label></div>`;
        }
        loadingBox.style.display = 'none';
        submissionError.innerHTML = '';
        submissionInfo.innerHTML = `<div class="spinner"></div>`;
        displayElement(loadingBoxIntern, 'none');

        if (clientHasTrackingToolValue) {
            totalHoursLabelEl.style.display = 'none';
            if (statusInfo.isActive && statusInfo.accessToTrackingTool) {
                trackingToolReportEntrySection.style.display = 'block';
                await getProjectMovementsClientHasTrackTool();
            }
            if (buttons) {
                buttons.forEach(btn => {
                    if (btn) btn.disabled = false;
                });
            }
        } else {
            if (statusInfo.isActive && statusInfo.accessToTrackingTool) {
                totalHoursLabelEl.style.display = 'block';
                trackingToolTimeEntrySection.style.display = 'block';
                const movements = await getTrackingToolProjectMovements();
                generateDateList(startDate, endDate, movements.movementsList);
            }
            if (buttons) {
                buttons.forEach(btn => {
                    if (btn) btn.disabled = false;
                });
            }
        }
    } catch (error) {
        console.error('Error navigating between dates:', error);
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
            await getProjectMovementsClientHasTrackTool();
        } else {
            const movements = await getTrackingToolProjectMovements();
            let dateFrom = new Date(dateFromInput.value);
            let dateTo = new Date(dateToInput.value);
            generateDateList(formatDateYyyyMmDd(dateFrom), formatDateYyyyMmDd(dateTo), movements.movementsList);
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


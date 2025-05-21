
const trackingToolTimeEntrySection = getElementById('tracking-tool-time-entry');
const trackingToolReportEntrySection = getElementById('tracking-tool-report-entry');
let isActiveInThePeriod = false;
const dateToInput = getElementById('dateToInput');
const dateFromInput = getElementById('dateFromInput');
const errorMessageIntern = getElementById('error-message-intern');
let clientHasTrackingToolValue = getElementById('ClientHasTT').value;
const submissionInfo = getElementById('submission-info');
const submissionError = getElementById('submission-errors');
const noHoursError = getElementById('no-hours-errors');
const projectIdInput = getElementById('projectId');
let selectedProjectName = '';
const inactiveNoTrackingToolSection = getElementById('inactive-no-tracking-in-project-sec');
const totalHoursLabelEl = getElementById('total-hours-label');
const loadingBoxIntern = getElementById('loading-box-intern');
let participatesInOnCalls = false;
const autofillMobilebtn = getElementById('autofill-sec-mobile');
const autofillDeskbtn = getElementById('autofill-sec-desk');
let isAdministrative = false;
const noHoursSection = getElementById('add-remove-pr-in-period');
let projectIsActiveInThePeriod = false;
let projectIsBillable = false;
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
const errorMessageBox = getElementById('error-message-rep-time');
const contentBox = getElementById('content-box');

document.addEventListener('DOMContentLoaded', async function () {
    setTimesheetItemActive();
    let dataLoaded = false;
    const dropdownHeader = document.querySelector('.dropdown-header');
    const dropdownList = document.querySelector('.dropdown-list');
    paymentPeriod = getElementById('PaymentPeriodInput').value;

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
    contentBox.style.display = 'block';
});
async function selectProject(projectId) {
    try {
        displaySpinner();
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
            window.location.href = "/TrackingTool/ReportingMyTime";
        } else {
            displayToasterError(data.error);
            console.error('There has been a problem with the fetch operation:', data.detail);
        }
    } catch (error) {
        validateSessionExpiration(error.message);
        console.error('Network or fetch error:', error);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        return null;
    } finally {
        hideSpinner();
    }
}
function initializeNavigation() {
    loadingBoxIntern.style.display = 'flex';
    errorMessageIntern.style.display = 'none';
    inactiveNoTrackingToolSection.style.display = 'none';
    trackingToolReportEntrySection.style.display = 'none';
    trackingToolTimeEntrySection.style.display = 'none';
}
function updateAutofillButtons() {
    if (window.matchMedia('(max-width: 768px)').matches) {
        autofillDeskbtn.style.display = 'none';
        autofillMobilebtn.style.display = 'flex';
    } else {
        autofillDeskbtn.style.display = 'block';
        autofillMobilebtn.style.display = 'none';
    }
}
//Navitate between dates
async function navitateBetweenDates(startDate, endDate, buttons) {
    try {
        noHoursError.innerHTML = '';
        initializeNavigation();
        const consultantStatusresponse = await getConsultantStatusInTheProject(dateFromInput.value, dateToInput.value);
        const statusInfo = consultantStatusresponse.consultantStatusInTheProject;
        projectIsBillable = statusInfo.projectIsBillable;
        isAdministrative = statusInfo.userCategory === 'Administrative' ? true : false;
        isActiveInThePeriod = statusInfo.isActive;

        participatesInOnCalls = statusInfo.participatesInOnCalls;
        contentBox.style.display = 'block';

        if (!isAdministrative) {
            autofillMobilebtn.style.display = 'none';
        }
        projectIsActiveInThePeriod = statusInfo.projectIsActiveInThePeriod;
        if (statusInfo.isActive && statusInfo.accessToTrackingTool && projectIsActiveInThePeriod) {
            submissionInfo.style.display = 'flex';
            totalHoursLabelEl.style.display = 'block';
            header.style.display = 'flex';
        } else {
            trackingToolTimeEntrySection.style.display = 'none';
            trackingToolReportEntrySection.style.display = 'none';
            submissionInfo.style.display = 'none';
            totalHoursLabelEl.style.display = 'none';
            inactiveNoTrackingToolSection.style.display = 'block';
        }
        let todaysDate = new Date();

        function formatStringToDate(dateString) {
            const [month, day, year] = dateString.split('/');
            return new Date(year, month - 1, day);
        }
        let inactiveNoTrackingMiddleMessage = '';
        let inactiveNoTrackingEndMessae = '';

        inactiveNoTrackingMiddleMessage = (formatStringToDate(dateToInput.value) < todaysDate) ? inactiveNoTrackingMiddleMessage = `were` : `are`;
        inactiveNoTrackingMiddleMessage === 'are' ? inactiveNoTrackingEndMessae = `Please contact your Success Manager if you need to report any time here.
                ` : inactiveNoTrackingEndMessae = `You have nothing reported.`;

        if (!statusInfo.isActive) {
            inactiveNoTrackingToolSection.innerHTML = `<div><img src="/icons/Shared/question.svg"><br><label>You ${inactiveNoTrackingMiddleMessage} 
                <strong>Inactive</strong> for this period.<br> ${inactiveNoTrackingEndMessae}</label></div>`;
            autofillDeskbtn.style.display = 'none';
            autofillMobilebtn.style.display = 'none';
        }
        if (!statusInfo.accessToTrackingTool && statusInfo.isActive) {
            inactiveNoTrackingToolSection.innerHTML = `<div><img src="/icons/Shared/question.svg"><br><label>You ${inactiveNoTrackingMiddleMessage} Active in this project, but you ${inactiveNoTrackingMiddleMessage} not needed to report time for this period.
            <br>${inactiveNoTrackingEndMessae}</label></div>`;
        }
        if (!projectIsActiveInThePeriod) {
            noHoursSection.style.display = 'none';
            inactiveNoTrackingToolSection.innerHTML = `<div><img src="/icons/Shared/question.svg"><br><label>You are Active in this project, but It looks like you haven’t reported hours for this period.<br> ${inactiveNoTrackingEndMessae}</label></div>`;
            autofillDeskbtn.style.display = 'none';
            autofillMobilebtn.style.display = 'none';
        } else {
            noHoursSection.style.display = 'block';
        }
        submissionError.innerHTML = '';
        submissionInfo.innerHTML = `<div class="spinner"></div>`;

        if (clientHasTrackingToolValue) {
            totalHoursLabelEl.style.display = 'none';
            if (statusInfo.isActive && statusInfo.accessToTrackingTool && statusInfo.projectIsActiveInThePeriod) {
                await getProjectMovementsClientHasTrackTool(statusInfo.participatesInOnCalls);
                trackingToolReportEntrySection.style.display = 'block';
            }
            if (buttons) {
                buttons.forEach(btn => {
                    if (btn) btn.disabled = false;
                });
            }
        } else {
            if (statusInfo.isActive && statusInfo.accessToTrackingTool && statusInfo.projectIsActiveInThePeriod) {
                totalHoursLabelEl.style.display = 'block';

                if (typeof getTrackingToolProjectMovements === 'function') {
                    const movements = await getTrackingToolProjectMovements();
                    generateDateList(startDate, endDate, movements.movementsList);
                    trackingToolTimeEntrySection.style.display = 'block';
                } else {
                    console.warn("getTrackingToolProjectMovements is not defined.");
                }
            }

            if (buttons) {
                buttons.forEach(btn => {
                    if (btn) btn.disabled = false;
                });
            }
        }
        loadingBoxIntern.style.display = 'none';

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
        noHoursError.innerHTML = '';
        displaySpinner();

        const datesFromTo = getNormalizedDates(dateFromInput, dateToInput);
        let startDateData = datesFromTo.startDate;
        let endDateData = datesFromTo.endDate;

        var data = {
            ProjectId: Number(projectIdInput.value),
            StartPeriodDate: startDateData,
            EndPeriodDate: endDateData
        };

        var token = $('[name="__RequestVerificationToken"]').val();

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
        noHoursSection.style.display = 'none';
        displayToasterSuccess(dataFromApi.message);
        if (clientHasTrackingToolValue) {
            await getProjectMovementsClientHasTrackTool(participatesInOnCalls);
        } else {
            const movements = await getTrackingToolProjectMovements();
            generateDateList(startDateData.split('T')[0], endDateData.split('T')[0], movements.movementsList);
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

// No hours to report

const messageBox = document.getElementById("confirmation-message");
const toggleButton = document.getElementById("toggle-message-btn");
const cancelButton = document.getElementById("cancel-btn");
const confirmButton = document.getElementById("confirm-btn");

function toggleMessage(event) {
    event.stopPropagation();
    messageBox.style.display = messageBox.style.display === "block" ? "none" : "block";
}

toggleButton.addEventListener("click", toggleMessage);

cancelButton.addEventListener("click", (event) => {
    event.stopPropagation();
    noHoursError.innerHTML = '';
    messageBox.style.display = "none";
});

confirmButton.addEventListener("click", (event) => {
    event.stopPropagation();
    noHoursToReportInPeriod(projectIdInput.value);
    messageBox.style.display = "none";
});

document.addEventListener("click", () => {
    messageBox.style.display = "none";
});

messageBox.addEventListener("click", (event) => {
    event.stopPropagation();
});

document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
        messageBox.style.display = "none";
    }
});

async function noHoursToReportInPeriod(projectId) {
    displaySpinner();
    var token = $('[name="__RequestVerificationToken"]').val();

    const datesFromTo = getNormalizedDates(dateFromInput, dateToInput);
    let startDateData = datesFromTo.startDate;
    let endDateData = datesFromTo.endDate;

    var data = {
        ProjectId: Number(projectIdInput.value),
        StartPeriodDate: startDateData,
        EndPeriodDate: endDateData
    };
    try {
        const response = await fetch('/TrackingTool/ReportingMyTime/NoHoursToReportInPeriod', {
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

                    if (errorData.errors.HoursReported !== undefined) {
                        noHoursError.style.display = 'block';
                        noHoursError.innerHTML = `<span>${errorData.errors.HoursReported}</span>`;
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
        noHoursSection.style.display = 'none';
        displayToasterSuccess(dataFromApi.message);
        navitateBetweenDates(startDateData, endDateData);

        submissionError.innerHTML = '';
        noHoursError.innerHTML = '';
        hideSpinner();
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Something went wrong, more details: ' + err);
        hideSpinner();
        return null;
    }
}
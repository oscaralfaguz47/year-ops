async function getProjectsWhereConsultantAssigned() {
    try {
        const url = "/AccountManagement/ProjectsConsultantsAssigned/GetProjectsWhereConsultantAssigned";
        const response = await fetch(url);

        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
        }

        return await response.json();
    } catch (error) {
        validateSessionExpiration(error.message);
        displayToasterError(error.message);
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}

async function getSelectedProjectInfo() {
    try {
        const url = `/AccountManagement/ProjectsConsultantsAssigned/GetConsultantSelectedProjectInfo`;
        const response = await fetch(url);

        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
        }

        return await response.json();
    } catch (error) {
        validateSessionExpiration(error.message);
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}

async function getConsultantStatusInTheProject(startDate, endDate) {
    try {
        var startDateValue = encodeURIComponent(startDate);
        var endDateValue = encodeURIComponent(endDate);
        const url = `/AccountManagement/ProjectsConsultantsAssigned/GetConsultantStatusInTheProject?startDate=
        ${startDateValue}&endDate=${endDateValue}`;
        const response = await fetch(url);

        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
        }

        return await response.json();
    } catch (error) {
        validateSessionExpiration(error.message);
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}

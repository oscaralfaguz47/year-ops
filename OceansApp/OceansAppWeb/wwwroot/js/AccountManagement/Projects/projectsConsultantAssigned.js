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
        displayToasterError("Internet connection failed or server is unreachable");
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}

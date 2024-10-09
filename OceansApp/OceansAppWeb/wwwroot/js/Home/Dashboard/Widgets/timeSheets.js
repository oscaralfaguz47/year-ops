async function getPendingTimesheets() {
    const url = `/GetPendingTimesheets`;
    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
        }
        const data = await response.json();
        return data; 
    } catch (error) {
        validateSessionExpiration(error.message);
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}

document.addEventListener("DOMContentLoaded", async function () {
    try {
        let pendingTimesheets = await getPendingTimesheets();
        console.log(pendingTimesheets);
    } catch (error) {
        console.error(`Failed to load pending timesheets: ${error.message}`);
    }
});

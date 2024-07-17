async function getHolidaysList() {
    var url = "/General/ConsultantHolidays/GetHolidaysListForSelect";
    try {
        const response = await fetch(url);
        if (response.ok) {
            return await response.json();
        } else {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
        }
    } catch (error) {
        validateSessionExpiration(error.message);
        displayToasterError("Internet connection failed");
        throw new Error('Network error or unable to reach the server. More details: ' + error.message);
    }
}

async function getConsultantsBySearchText(searchText) {
    var url = "/General/ConsultantDetails/GetConsultantsBySearchText?searchText=" + encodeURIComponent(searchText);
    try {
        let response = await fetch(url);
        if (response.ok) {
            return await response.json();
        } else {
            let errorData = await response.json();
            if (errorData.messageType === "Validation Error") {
                displayToasterWarningArray(errorData.errors);
            } else {
                displayToasterErrorArray(errorData.error);
            }
            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
        }
    } catch (error) {
        console.error('Error fetching consultants:', error);
        return null;
    }
}


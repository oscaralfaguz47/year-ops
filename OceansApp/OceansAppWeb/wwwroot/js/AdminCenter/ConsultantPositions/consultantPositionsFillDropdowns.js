
async function getPositionsList(isAdministrative) {
    var url = `/AdminCenter/ConsultantPositions/GetAllConsultantPositionsListForSelect?isAdministrative=${isAdministrative}`;
    return fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    displayToasterError(errorData.error);
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .catch(error => {
            displayToasterError("Internet connection failed");
            throw new Error('Network error or unable to reach the server. More details: ' + error.message);
        });
}

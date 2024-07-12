async function getMovementTypesList() {
    var url = `/AdminCenter/MovementTypes/GetMovementTypeForTrackingTool`;
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
//GET MOVEMENT TYPES
function fillMovementTypesSelect(selectElement, data) {
    if (selectElement.length > 1) {
        return;
    }
    data.forEach(obj => {
        let optionText = obj.text === 'Normal Hours' ? 'Hours Worked (Payable)' : obj.text;
        let optionValue = obj.text === 'Normal Hours' ? obj.text : obj.value;
        var option = new Option(optionText, optionValue);
        selectElement.add(option);
    });
}
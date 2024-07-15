function populateSelect(selectElementId, data, defaultOptionText, defaultOptionValue) {
    const selectElement = document.getElementById(selectElementId);
    if (selectElement.length > 1) {
        return;
    }
    if (defaultOptionText !== null) {
        selectElement.innerHTML = `<option value="${defaultOptionValue}">${defaultOptionText}</option>`;
    }
    data.forEach(obj => {
        var option = new Option(obj.text, obj.value);
        selectElement.add(option);
    });
}

function generateDateList(startDateString, endDateString) {
    const startDate = new Date(startDateString + 'T00:00:00');
    const endDate = new Date(endDateString + 'T23:59:59');
    const dateListContainer = document.getElementById('dateList');
    dateListContainer.innerHTML = '';
    let currentDate = new Date(startDate.getTime());

    while (currentDate <= endDate) {
        const dayItem = document.createElement('div');
        dayItem.className = 'day-item';
        const formattedDate = currentDate.toLocaleDateString('en-US', { weekday: 'long', day: 'numeric', month: 'long' });
        const weekday = currentDate.toLocaleDateString('en-US', { weekday: 'long' });
        const noReportNeededLabel = weekday === 'Sunday' || weekday === 'Saturday' ? '<label class="red-label">Generally Non-reportable day</label>' : '';
        const dateValue = `${currentDate.getFullYear()}-${String(currentDate.getMonth() + 1).padStart(2, '0')}-${String(currentDate.getDate()).padStart(2, '0')}`;
        dayItem.innerHTML = `<label class="day-label">${formattedDate}</label> <button class="btn-add-time" onclick="addTimeEntry(this, '${dateValue}')">+ Add Time</button> ${noReportNeededLabel}`;
        dateListContainer.appendChild(dayItem);
        currentDate.setDate(currentDate.getDate() + 1); 
    }   
}

function addTimeEntry(button, date) {
    const timeEntryDiv = document.createElement('div');
    timeEntryDiv.className = 'time-entry';
    timeEntryDiv.innerHTML = `
        <span>From</span><input type="time" class="time-from input-time"/><span>To</span>
        <input type="time" class="time-to input-time"/>
        <input type="text" placeholder="Detail" class="time-detail input-time"/>
        <button class="btn-delete-time" onclick="deleteTimeEntry(this)"><i class="bi bi-trash"></i></button>
        <button class="btn-save-time" onclick="saveTimeEntry(this, '${date}')"><i class="fa-solid fa-floppy-disk"></i></button>
        <label class="time-label"></label>
    `;
    button.parentElement.appendChild(timeEntryDiv);
}

function deleteTimeEntry(button) {
    button.parentElement.remove();
}

function saveTimeEntry(button, date) {
    const timeFrom = button.parentElement.querySelector('.time-from').value;
    const timeTo = button.parentElement.querySelector('.time-to').value;
    const detail = button.parentElement.querySelector('.time-detail').value;
    const timeLabel = button.parentElement.querySelector('.time-label');

    timeLabel.textContent = `Saved: ${date}, From: ${timeFrom}, To: ${timeTo}, Detail: ${detail}`;
}

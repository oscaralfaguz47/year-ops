function calculateTimeDifference(startTime, endTime) {
    const startTimeDate = new Date(`1970-01-01T${startTime}:00`);
    const endTimeDate = new Date(`1970-01-01T${endTime}:00`);
    let difference = endTimeDate - startTimeDate;
    if (difference < 0) {
        difference += 24 * 60 * 60 * 1000;
    }
    const hours = Math.floor(difference / (1000 * 60 * 60));
    const minutes = Math.round((difference % (1000 * 60 * 60)) / (1000 * 60));
    return { hours, minutes };
}

function formatTimeTo12Hour(timeStr) {
    let [hours, minutes] = timeStr.split(':');
    hours = parseInt(hours, 10);
    const period = hours >= 12 ? 'pm' : 'am';
    hours = hours % 12 || 12;
    return `${hours}:${minutes}${period}`;
}
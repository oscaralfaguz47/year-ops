// ── Time Off Dashboard Widget ──

const timeOffWidgetCont = getElementById('timeOffWidgetCont');
const timeOffFirstCardContent = timeOffWidgetCont.closest('.card-content');

function getTimeOffWidgetData() {
    return (async () => {
        timeOffFirstCardContent.style.justifyContent = 'center';
        timeOffWidgetCont.innerHTML = loadingISpinner();
        const url = '/GetTimeOffWidgetData';
        try {
            const response = await fetch(url);
            if (!response.ok) {
                timeOffWidgetCont.innerHTML = cardErrorInfo('Error loading time off data!', 'getTimeOffWidgetData()');
                const errorData = await response.json();
                throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
            }
            const data = await response.json();
            return data;
        } catch (error) {
            validateSessionExpiration(error.message);
            throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
        }
    })();
}

document.addEventListener('DOMContentLoaded', function () {
    getTimeOffWidgetData()
        .then(data => {
            timeOffFirstCardContent.style.justifyContent = 'flex-start';
            let html = '';

            // Upcoming approved time off
            if (data.widgetData.upcomingApproved && data.widgetData.upcomingApproved.length > 0) {
                html += '<div class="widget-upcoming">';
                html += '<div class="widget-upcoming-title">Upcoming Time Off</div>';
                data.widgetData.upcomingApproved.forEach(entry => {
                    const start = new Date(entry.startDate);
                    const end = new Date(entry.endDate);
                    const startStr = start.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
                    const endStr = end.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
                    const dateLabel = startStr === endStr ? startStr : `${startStr} - ${endStr}`;
                    html += `<div class="widget-entry">
                        <span class="widget-type-dot ${entry.timeOffType}"></span>
                        <span class="widget-entry-dates">${dateLabel}</span>
                        <span class="widget-entry-type ${entry.timeOffType}">${entry.timeOffType}</span>
                    </div>`;
                });
                html += '</div>';
            } else {
                html += '<div class="widget-empty">No upcoming time off scheduled.</div>';
            }

            // Pending requests count
            if (data.widgetData.pendingCount > 0) {
                html += `<div class="widget-pending">
                    <span class="widget-pending-count">${data.widgetData.pendingCount}</span>
                    <span>request${data.widgetData.pendingCount > 1 ? 's' : ''} pending approval</span>
                </div>`;
            }

            // Link to Time Off page
            html += '<a class="widget-link" href="/General/TimeOff">View Time Off Requests &rarr;</a>';

            timeOffWidgetCont.innerHTML = html;
        })
        .catch(error => {
            console.error('Error loading time off widget:', error);
        });
});

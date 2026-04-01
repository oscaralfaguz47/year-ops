// ── Time Off Dashboard Widget ──

const timeOffWidgetCont = getElementById('timeOffWidgetCont');
const timeOffFirstCardContent = timeOffWidgetCont.closest('.card-content');

const TYPE_LABELS = {
    'PTO': 'Paid Time Off',
    'UPTO': 'Unpaid Time Off',
    'VTO': 'Voluntary Time Off'
};

const STATUS_LABELS = {
    'Waiting to be approved': 'Pending',
    'Approved': 'Approved',
    'Rejected': 'Rejected'
};

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

            // Header with title and Request button
            html += `<div class="to-widget-header">
                <span class="to-widget-title">Time off requests</span>
                <a href="/General/TimeOff" class="to-widget-request-btn">Request</a>
            </div>`;

            // Request list
            if (data.widgetData.upcomingApproved && data.widgetData.upcomingApproved.length > 0) {
                html += '<div class="to-widget-list">';
                data.widgetData.upcomingApproved.forEach(entry => {
                    const start = new Date(entry.startDate);
                    const end = new Date(entry.endDate);
                    const startStr = start.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
                    const endStr = end.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
                    const startOnly = start.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
                    const isSameDay = start.toDateString() === end.toDateString();
                    const dateLabel = isSameDay ? startOnly : `${startStr} \u2013 ${endStr}`;

                    const typeLabel = TYPE_LABELS[entry.timeOffType] || entry.timeOffType;
                    const statusLabel = STATUS_LABELS[entry.status] || entry.status;
                    const statusClass = entry.status === 'Approved' ? 'approved'
                        : entry.status === 'Rejected' ? 'rejected' : 'pending';

                    html += `<div class="to-widget-row">
                        <span class="to-widget-type-badge ${entry.timeOffType}">${typeLabel}</span>
                        <span class="to-widget-dates">${dateLabel}</span>
                        <span class="to-widget-status ${statusClass}">${statusLabel}</span>
                    </div>`;
                });
                html += '</div>';
            } else {
                html += '<div class="to-widget-empty">No time off requests yet.</div>';
            }

            timeOffWidgetCont.innerHTML = html;
        })
        .catch(error => {
            console.error('Error loading time off widget:', error);
        });
});

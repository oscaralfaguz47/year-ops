// ── Time Off Dashboard Widget ──

const timeOffWidgetCont = document.getElementById('timeOffWidgetCont');
const timeOffWidgetCard = document.getElementById('timeOffWidgetCard');

const TYPE_LABELS_W = {
    'PTO': 'Paid Time Off',
    'UPTO': 'Unpaid Time Off',
    'VTO': 'Voluntary Time Off'
};

const STATUS_LABELS_W = {
    'Waiting to be approved': 'Pending',
    'Approved': 'Approved',
    'Rejected': 'Rejected'
};

function getTimeOffWidgetData() {
    return (async () => {
        const url = '/GetTimeOffWidgetData';
        try {
            const response = await fetch(url);
            if (!response.ok) return null;
            return await response.json();
        } catch (error) {
            return null;
        }
    })();
}

document.addEventListener('DOMContentLoaded', function () {
    if (!timeOffWidgetCont) return;
    getTimeOffWidgetData()
        .then(data => {
            if (!data) return;

            timeOffWidgetCard.style.display = '';
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

                    const typeLabel = TYPE_LABELS_W[entry.timeOffType] || entry.timeOffType;
                    const statusLabel = STATUS_LABELS_W[entry.status] || entry.status;
                    let statusClass = 'pending';
                    if (entry.status === 'Approved') statusClass = 'approved';
                    else if (entry.status === 'Rejected') statusClass = 'rejected';

                    html += `<div class="to-widget-row">
                        <span class="to-widget-type-badge ${entry.timeOffType}">${typeLabel}</span>
                        <span class="to-widget-dates">${dateLabel}</span>
                        <span class="to-widget-status ${statusClass}">${statusLabel}</span>
                    </div>`;
                });
                html += '</div>';

                if (data.widgetData.totalCount > 4) {
                    html += '<a class="to-widget-more-link" href="/General/TimeOff">... more</a>';
                }
            } else {
                html += '<div class="to-widget-empty">No time off requests yet.</div>';
            }

            timeOffWidgetCont.innerHTML = html;
        })
        .catch(() => {});
});

// ── Team Time Off Dashboard Widget ──

const teamTimeOffCont = document.getElementById('teamTimeOffWidgetCont');
const teamTimeOffCard = document.getElementById('teamTimeOffCard');
let teamSelectedRequestId = null;

function getTeamTimeOffData() {
    return (async () => {
        const url = '/GetTeamTimeOffWidgetData';
        try {
            const response = await fetch(url);
            if (!response.ok) return null;
            return await response.json();
        } catch (error) {
            return null;
        }
    })();
}

function renderTeamWidget(data) {
    let html = '';

    html += `<div class="team-to-header">
        <span class="team-to-title">My team's requests</span>
        <a href="/General/TimeOffApprovals" class="team-to-manage-btn">Manage</a>
    </div>`;

    if (data.requests && data.requests.length > 0) {
        html += '<div class="team-to-list">';
        data.requests.forEach(r => {
            const start = new Date(r.startDate);
            const end = new Date(r.endDate);
            const startStr = start.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
            const endStr = end.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
            const startOnly = start.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
            const isSameDay = start.toDateString() === end.toDateString();
            const dateLabel = isSameDay ? startOnly : `${startStr} \u2013 ${endStr}`;

            const isPending = r.status === 'Waiting to be approved';
            let statusClass = 'pending';
            if (r.status === 'Approved') statusClass = 'approved';
            else if (r.status === 'Rejected') statusClass = 'rejected';
            const statusLabel = isPending ? 'Pending' : r.status;

            const name = escapeHtmlTeam(r.consultantName);

            let actionsHtml = '';
            if (isPending) {
                actionsHtml = `<div class="team-to-actions">
                    <button class="team-to-btn reject-btn" onclick="teamOpenReject(${r.timeOffRequestId}, '${name}')" title="Reject">&#10005;</button>
                    <button class="team-to-btn approve-btn" onclick="teamOpenApprove(${r.timeOffRequestId}, '${name}')" title="Approve">&#10003;</button>
                </div>`;
            }

            html += `<div class="team-to-row">
                <span class="team-to-name">${name}</span>
                <span class="team-to-dates">${dateLabel}</span>
                <span class="team-to-status ${statusClass}">${statusLabel}</span>
                ${actionsHtml}
            </div>`;
        });
        html += '</div>';
    } else {
        html += '<div class="team-to-empty">No time off requests from your team.</div>';
    }

    teamTimeOffCont.innerHTML = html;
}

document.addEventListener('DOMContentLoaded', function () {
    if (!teamTimeOffCont) return;
    getTeamTimeOffData()
        .then(data => {
            if (data && data.requests && data.requests.length > 0) {
                teamTimeOffCard.style.display = '';
                renderTeamWidget(data);
            }
        })
        .catch(() => {});
});

// ── Approve / Reject from widget ──

function teamOpenApprove(requestId, consultantName) {
    teamSelectedRequestId = requestId;
    document.getElementById('team-approve-details').innerHTML =
        `Approve time off request for <strong>${consultantName}</strong>?`;
    showModal('modal-team-approve');
}

function teamOpenReject(requestId, consultantName) {
    teamSelectedRequestId = requestId;
    document.getElementById('team-reject-details').innerHTML =
        `Reject time off request for <strong>${consultantName}</strong>?`;
    document.getElementById('team-rejection-comment').value = '';
    showModal('modal-team-reject');
}

async function teamConfirmApprove() {
    await teamProcessDecision('Approved', null);
}

async function teamConfirmReject() {
    const comment = document.getElementById('team-rejection-comment').value.trim();
    if (!comment) {
        toastr.warning('Please provide a rejection comment.');
        return;
    }
    await teamProcessDecision('Rejected', comment);
}

async function teamProcessDecision(status, rejectionComment) {
    displaySpinner();
    const token = document.querySelector('[name="__RequestVerificationToken"]')?.value;

    try {
        const response = await fetch('/General/TimeOffApprovals/ApproveReject', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: token
            },
            body: JSON.stringify({
                TimeOffRequestId: teamSelectedRequestId,
                TransactionStatus: status,
                RejectionComment: rejectionComment
            })
        });

        const result = await response.json();

        if (response.ok && result.success) {
            toastr.success(result.message);
            hideModal('modal-team-approve');
            hideModal('modal-team-reject');
            // Reload the widget
            getTeamTimeOffData()
                .then(data => renderTeamWidget(data))
                .catch(error => console.error(error));
        } else if (result.errors) {
            displayToasterErrorArray(result.errors);
        } else {
            toastr.error(result.error || 'An error occurred.');
        }
    } catch (error) {
        toastr.error('An unexpected error occurred.');
    } finally {
        hideSpinner();
    }
}

function escapeHtmlTeam(str) {
    if (!str) return '';
    return str.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;').replaceAll("'", '&#039;');
}

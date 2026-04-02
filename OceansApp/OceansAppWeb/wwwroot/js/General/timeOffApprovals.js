// ── Time Off Approvals - Manager Page ──

let selectedRequestId = null;
const mainPaginationContainerSelector = '.principal-header-container .pagination-container';

document.addEventListener('DOMContentLoaded', function () {
    setTimesheetItemActive();
    getListOfResults(true, false);
});

function setTimesheetItemActive() {
    const el = document.getElementById('timesheetItem');
    if (el) el.classList.add('nav-item-active');
}

async function getListOfResults(firstTime, fromFilters) {
    displaySpinner();
    const formData = firstTime ? buildInitialFilters() : collectFormData(fromFilters);
    const queryString = JSON.stringify(formData);
    const url = '/General/TimeOffApprovals/GetRequests?model=' + encodeURIComponent(queryString);

    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorData = await response.json();
            if (errorData.errors) displayToasterErrorArray(errorData.errors);
            throw new Error(errorData.error || 'Request failed');
        }

        const data = await response.json();
        const tbody = document.getElementById('approvals-tbody');
        tbody.innerHTML = '';

        if (data.requestsList.length === 0) {
            document.querySelector('.no-results').style.display = 'block';
        } else {
            document.querySelector('.no-results').style.display = 'none';
            data.requestsList.forEach(obj => {
                let statusClass = 'pending';
                if (obj.status === 'Approved') statusClass = 'approved';
                else if (obj.status === 'Rejected') statusClass = 'rejected';

                const actionsHtml = obj.status === 'Waiting to be approved'
                    ? `<div class="approval-actions">
                        <button class="btn-approve" onclick="openApproveModal(${obj.timeOffRequestId}, '${escapeHtml(obj.consultantName)}', '${obj.timeOffType}')">Approve</button>
                        <button class="btn-reject" onclick="openRejectModal(${obj.timeOffRequestId}, '${escapeHtml(obj.consultantName)}', '${obj.timeOffType}')">Reject</button>
                       </div>`
                    : '';

                const row = `<tr>
                    <td>${escapeHtml(obj.consultantName)}</td>
                    <td><span class="type-badge ${obj.timeOffType}">${obj.timeOffType}</span></td>
                    <td>${formatDate(obj.startDate)}</td>
                    <td>${formatDate(obj.endDate)}</td>
                    <td>${obj.businessDays}</td>
                    <td><span class="status-badge ${statusClass}">${obj.status}</span></td>
                    <td>${formatDate(obj.creationDate)}</td>
                    <td>${obj.actionDate ? formatDate(obj.actionDate) : '-'}</td>
                    <td class="rejection-comment-cell" title="${escapeHtml(obj.rejectionComment || '')}">${escapeHtml(obj.rejectionComment || '-')}</td>
                    <td>${actionsHtml}</td>
                </tr>`;
                tbody.innerHTML += row;
            });
        }

        // Update pagination component
        if (data.paginationFilters?.paginationWithoutFilters?.pagination) {
            updatePaginationValues(
                data.paginationFilters.paginationWithoutFilters.pagination,
                document.querySelector(mainPaginationContainerSelector)
            );
        }
    } catch (error) {
        validateSessionExpiration(error.message);
    } finally {
        hideSpinner();
    }
}

function buildInitialFilters() {
    const preFilterConsultantId = document.getElementById('preFilterConsultantId')?.value;
    const filters = {};
    if (preFilterConsultantId && preFilterConsultantId !== '') {
        filters.ConsultantId = Number(preFilterConsultantId);
    }
    return {
        Filters: filters,
        PaginationWithoutFilters: {
            Pagination: { PageSize: 0, PageIndex: 1 }
        }
    };
}

function collectFormData(fromFilters) {
    const searchText = document.getElementById('search-input')?.value || '';
    const statusValue = document.getElementById('statusFilter')?.value || '';
    const typeValue = document.getElementById('typeFilter')?.value || '';

    const filters = {};
    if (searchText) filters.SearchText = searchText;
    if (statusValue) filters.StatusName = statusValue;
    if (typeValue) filters.TimeOffType = typeValue;

    const paginationContainer = document.querySelector(mainPaginationContainerSelector);
    const paginationData = returnCurrentPaginationValues(paginationContainer);

    const inputFieldToOrder = paginationContainer.querySelector('input[name="fieldToOrder"]');
    const inputDirectionOrder = paginationContainer.querySelector('input[name="directionOrder"]');

    return {
        Filters: filters,
        PaginationWithoutFilters: {
            RequestFromFilters: fromFilters,
            Pagination: paginationData,
            OrderBy: {
                FieldToOrder: inputFieldToOrder?.value || 'ANY',
                DirectionOrder: inputDirectionOrder?.value || 'ANY'
            }
        }
    };
}

function onSearchChange() {
    const container = document.querySelector(mainPaginationContainerSelector);
    paginationSubmit(true, false, container);
}

function onFiltersChange() {
    const container = document.querySelector(mainPaginationContainerSelector);
    paginationSubmit(true, false, container);
}

globalThis.handlePaginationSubmit = function (args) {
    getListOfResults(false, args.reloadTable);
};

// ── Approve/Reject Modals ──

function openApproveModal(requestId, consultantName, type) {
    selectedRequestId = requestId;
    document.getElementById('approve-details').innerHTML =
        `Are you sure you want to <strong>approve</strong> the <strong>${type}</strong> request for <strong>${consultantName}</strong>?`;
    showModal('modal-approve-request');
}

function openRejectModal(requestId, consultantName, type) {
    selectedRequestId = requestId;
    document.getElementById('reject-details').innerHTML =
        `Are you sure you want to <strong>reject</strong> the <strong>${type}</strong> request for <strong>${consultantName}</strong>?`;
    document.getElementById('rejection-comment-textarea').value = '';
    showModal('modal-reject-request');
}

async function confirmApprove() {
    await processDecision('Approved', null);
}

async function confirmReject() {
    const comment = document.getElementById('rejection-comment-textarea').value.trim();
    if (!comment) {
        toastr.warning('Please provide a rejection comment.');
        return;
    }
    await processDecision('Rejected', comment);
}

async function processDecision(status, rejectionComment) {
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
                TimeOffRequestId: selectedRequestId,
                TransactionStatus: status,
                RejectionComment: rejectionComment
            })
        });

        const result = await response.json();

        if (response.ok && result.success) {
            toastr.success(result.message);
            hideModal('modal-approve-request');
            hideModal('modal-reject-request');
            getListOfResults(true, false);
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

// ── Helpers ──

function formatDate(dateStr) {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

function escapeHtml(str) {
    if (!str) return '';
    return str.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;').replaceAll("'", '&#039;');
}

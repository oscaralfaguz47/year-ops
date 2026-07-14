// Manual Hours Upload — admin files hours on behalf of a consultant who can't self-report.
// Lives as a modal on the Payment Sheets page, opened prefilled-and-locked from a row's
// "Upload hours on behalf" button (consultant, project and period are fixed; the admin sets the
// PERIOD TOTAL, defaulting to the server-computed workable days × 8). See docs/adr/0003.

// Server-computed workable-day count for the open period; null until the preview call succeeds.
// Gates the Review button so a total can't be filed before N (the spread divisor) is known.
let mhuWorkableDays = null;

// Bumped on every modal open. The async preview captures the token at request time and discards
// its response if a newer open has happened since — so a slow response for consultant A can't
// prefill the total for consultant B after the admin reopened the modal. See docs/adr/0003.
let mhuOpenToken = 0;

function mhuSetMessage(text, isError) {
    const el = document.getElementById('mhu-message');
    el.textContent = text;
    el.style.color = isError ? '#c0392b' : '#2e7d32';
}

function mhuSetReviewEnabled(enabled) {
    const btn = document.getElementById('mhu-review-btn');
    if (btn) { btn.disabled = !enabled; }
}

// Fetch the server-owned workable-day count for the open consultant/project/period and prefill the
// total (N × 8). The client never computes N — the displayed default, "N days" label and the spread
// divisor are one server value, so they can't diverge. On failure: error + Review stays disabled
// (no hardcoded fallback to 8). See docs/adr/0003.
function mhuLoadWorkableDays(consultantId, projectId, startIso) {
    mhuWorkableDays = null;
    mhuSetReviewEnabled(false);
    document.getElementById('mhu-hours').value = '';
    document.getElementById('mhu-days').textContent = '';

    // Bump first so this open supersedes any still-in-flight request, even on the early bail below.
    const token = ++mhuOpenToken;

    // No usable period means we can't compute or trust N — bail before hitting the server (an empty
    // periodDate would otherwise bind to year 1 server-side and return a nonsense count).
    if (!startIso) {
        mhuSetMessage('No pay period is selected on the page, so workable days cannot be determined.', true);
        return;
    }

    $.ajax({
        url: '/Finances/PaymentSheets/PreviewWorkableDays',
        type: 'GET',
        data: { consultantId: consultantId, projectId: projectId, periodDate: startIso }
    })
    .done(function (res) {
        if (token !== mhuOpenToken) { return; } // a newer open superseded this request
        const days = res && typeof res.workableDays === 'number' ? res.workableDays : 0;
        if (days <= 0) {
            document.getElementById('mhu-days').textContent = '';
            mhuSetMessage('This period has no workable days for the consultant, so hours cannot be uploaded.', true);
            return;
        }
        mhuWorkableDays = days;
        document.getElementById('mhu-days').textContent = '(' + days + ' workable day' + (days === 1 ? '' : 's') + ')';
        document.getElementById('mhu-hours').value = res.suggestedTotal;
        mhuSetReviewEnabled(true);
    })
    .fail(function () {
        if (token !== mhuOpenToken) { return; }
        mhuSetMessage('Could not load the workable days for this period. Please close and try again.', true);
    });
}

// Open the modal prefilled with a row's consultant + project and the period currently shown on
// the page, so the admin only sets the period total. Period is read from the page's canonical
// dateFrom/dateTo hidden inputs (kept in sync as the user navigates periods), in MM/DD/YYYY.
function openManualHoursUploadModal(consultantId, consultantNameEnc, projectId, projectNameEnc) {
    // Names arrive URL-encoded from the row button (so apostrophes/quotes can't break the
    // onclick string); decode them back for display.
    const consultantName = decodeURIComponent(consultantNameEnc);
    const projectName = decodeURIComponent(projectNameEnc);

    mhuSetMessage('', false);
    document.getElementById('mhu-confirm').style.display = 'none';

    const consultantSel = document.getElementById('mhu-consultant');
    consultantSel.innerHTML = '';
    const cOpt = document.createElement('option');
    cOpt.value = consultantId;
    cOpt.textContent = consultantName;
    consultantSel.appendChild(cOpt);
    consultantSel.value = consultantId;
    consultantSel.disabled = true;

    const toIso = v => (v || '').replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2');
    const start = toIso(document.getElementById('dateFromInput').value);
    const end = toIso(document.getElementById('dateToInput').value);
    const startInput = document.getElementById('mhu-start');
    const endInput = document.getElementById('mhu-end');
    if (start) { startInput.value = start; }
    if (end) { endInput.value = end; }
    startInput.disabled = true;
    endInput.disabled = true;

    const projectSel = document.getElementById('mhu-project');
    projectSel.innerHTML = '';
    const pOpt = document.createElement('option');
    pOpt.value = projectId;
    pOpt.textContent = projectName;
    projectSel.appendChild(pOpt);
    projectSel.value = projectId;
    projectSel.disabled = true;

    mhuSetMessage('Filing on behalf of ' + consultantName + ' for the selected period — confirm or adjust the total hours.', false);
    showModal('modal-manual-hours-upload');

    // Pull the server-owned workable-day count and prefill the suggested total (N × 8).
    mhuLoadWorkableDays(consultantId, projectId, start);
    document.getElementById('mhu-hours').focus();
}

function mhuReview() {
    mhuSetMessage('', false);
    const consultant = document.getElementById('mhu-consultant');
    const project = document.getElementById('mhu-project');
    const start = document.getElementById('mhu-start').value;
    const end = document.getElementById('mhu-end').value;
    const hours = document.getElementById('mhu-hours').value;

    if (mhuWorkableDays === null) {
        mhuSetMessage('Workable days are still loading for this period — please wait a moment.', true);
        return;
    }
    if (!consultant.value || !project.value || !start || !end || !hours || parseFloat(hours) <= 0) {
        mhuSetMessage('Please complete consultant, period, project and a positive total number of hours.', true);
        return;
    }

    document.getElementById('mhu-summary').textContent =
        'File a total of ' + hours + ' hour(s) for ' + consultant.options[consultant.selectedIndex].text +
        ' on project "' + project.options[project.selectedIndex].text + '" for the period ' +
        start + ' to ' + end + ', spread across ' + mhuWorkableDays + ' workable day(s) (Monday–Friday, ' +
        'weekends and holidays skipped). The submission will be created as "Waiting to be approved".';
    document.getElementById('mhu-confirm').style.display = 'block';
}

function mhuConfirm() {
    const token = $('#modal-manual-hours-upload [name="__RequestVerificationToken"]').val();
    const payload = {
        ConsultantId: parseInt(document.getElementById('mhu-consultant').value, 10),
        ProjectId: parseInt(document.getElementById('mhu-project').value, 10),
        StartPeriodDate: document.getElementById('mhu-start').value,
        EndPeriodDate: document.getElementById('mhu-end').value,
        TotalHours: parseFloat(document.getElementById('mhu-hours').value)
    };

    $.ajax({
        url: '/Finances/PaymentSheets/UploadHoursOnBehalf',
        type: 'POST',
        contentType: 'application/json',
        headers: { 'RequestVerificationToken': token },
        data: JSON.stringify(payload)
    })
    .done(function (res) {
        document.getElementById('mhu-confirm').style.display = 'none';
        hideModal('modal-manual-hours-upload');
        // Refresh the list so the row reflects the new "Waiting to be approved" submission.
        getListOfResults(false, true);
        displayToasterSuccess(res.message || 'Hours uploaded on behalf of the consultant.');
    })
    .fail(function (xhr) {
        let msg = 'Upload failed.';
        if (xhr.responseJSON) {
            if (xhr.responseJSON.error) {
                msg = xhr.responseJSON.error;
            } else if (xhr.responseJSON.errors) {
                msg = Object.values(xhr.responseJSON.errors).flat().join(' ');
            }
        }
        mhuSetMessage(msg, true);
    });
}

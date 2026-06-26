// Manual Hours Upload — admin files hours on behalf of a consultant who can't self-report.
// Lives as a modal on the Payment Sheets page, opened prefilled-and-locked from a row's
// "Upload hours on behalf" button (consultant, project and period are fixed; only hours is set).

function mhuSetMessage(text, isError) {
    const el = document.getElementById('mhu-message');
    el.textContent = text;
    el.style.color = isError ? '#c0392b' : '#2e7d32';
}

// Open the modal prefilled with a row's consultant + project and the period currently shown on
// the page, so the admin only sets hours per day. Period is read from the page's canonical
// dateFrom/dateTo hidden inputs (kept in sync as the user navigates periods), in MM/DD/YYYY.
function openManualHoursUploadModal(consultantId, consultantNameEnc, projectId, projectNameEnc) {
    // Names arrive URL-encoded from the row button (so apostrophes/quotes can't break the
    // onclick string); decode them back for display.
    const consultantName = decodeURIComponent(consultantNameEnc);
    const projectName = decodeURIComponent(projectNameEnc);

    mhuSetMessage('', false);
    document.getElementById('mhu-confirm').style.display = 'none';
    document.getElementById('mhu-hours').value = '8';

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

    mhuSetMessage('Filing on behalf of ' + consultantName + ' for the selected period — just set the hours per day.', false);
    showModal('modal-manual-hours-upload');
    document.getElementById('mhu-hours').focus();
}

function mhuReview() {
    mhuSetMessage('', false);
    const consultant = document.getElementById('mhu-consultant');
    const project = document.getElementById('mhu-project');
    const start = document.getElementById('mhu-start').value;
    const end = document.getElementById('mhu-end').value;
    const hours = document.getElementById('mhu-hours').value;

    if (!consultant.value || !project.value || !start || !end || !hours || parseFloat(hours) <= 0) {
        mhuSetMessage('Please complete consultant, period, project and a positive number of hours per day.', true);
        return;
    }

    document.getElementById('mhu-summary').textContent =
        'Fill ' + hours + ' hour(s) per weekday for ' + consultant.options[consultant.selectedIndex].text +
        ' on project "' + project.options[project.selectedIndex].text + '" for the period ' +
        start + ' to ' + end + ' (Monday–Friday, weekends and holidays skipped). The submission will be created as "Waiting to be approved".';
    document.getElementById('mhu-confirm').style.display = 'block';
}

function mhuConfirm() {
    const token = $('#modal-manual-hours-upload [name="__RequestVerificationToken"]').val();
    const payload = {
        ConsultantId: parseInt(document.getElementById('mhu-consultant').value, 10),
        ProjectId: parseInt(document.getElementById('mhu-project').value, 10),
        StartPeriodDate: document.getElementById('mhu-start').value,
        EndPeriodDate: document.getElementById('mhu-end').value,
        HoursPerDay: parseFloat(document.getElementById('mhu-hours').value)
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

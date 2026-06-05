// ── Time Off Request - Calendar Page ──

let calYear, calMonth;
let calendarEntries = [];
let balances = {};
let holidayDates = [];
let isDragging = false;
let dragStart = null;
let dragEnd = null;

const MONTHS = ['January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'];

document.addEventListener('DOMContentLoaded', function () {
    setTimesheetItemActive();
    const today = new Date();
    calYear = today.getFullYear();
    calMonth = today.getMonth();

    // Render calendar immediately with empty data
    renderCalendar();

    // Then load data from server
    loadInitialData();
});

function setTimesheetItemActive() {
    const el = document.getElementById('timesheetItem');
    if (el) el.classList.add('nav-item-active');
}

async function loadInitialData() {
    try {
        const balanceRes = await fetch('/General/TimeOff/GetBalances');
        if (balanceRes.ok) {
            const balanceData = await balanceRes.json();
            balances = balanceData.balances;
            renderBalancesCard();
        }
    } catch (e) {
        console.error('Failed to load balances:', e);
    }

    try {
        const holidayRes = await fetch('/General/TimeOff/GetHolidayDates');
        if (holidayRes.ok) {
            const holidayData = await holidayRes.json();
            holidayDates = holidayData.holidayDates.map(d => new Date(d).toDateString());
        }
    } catch (e) {
        console.error('Failed to load holidays:', e);
    }

    // Reload calendar with entries
    await loadCalendarMonth();

    // Load request history
    loadRequestHistory();
}

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

async function loadRequestHistory() {
    const container = document.getElementById('request-history-list');
    if (!container) return;

    try {
        const res = await fetch('/General/TimeOff/GetAllMyRequests');
        if (!res.ok) return;
        const data = await res.json();

        if (!data.requests || data.requests.length === 0) {
            container.innerHTML = '<div class="history-empty">No time off requests yet.</div>';
            return;
        }

        let html = '';
        data.requests.forEach(r => {
            const start = new Date(r.startDate);
            const end = new Date(r.endDate);
            const startStr = start.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
            const endStr = end.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
            const startOnly = start.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
            const isSameDay = start.toDateString() === end.toDateString();
            const dateLabel = isSameDay ? startOnly : `${startStr} \u2013 ${endStr}`;

            const typeLabel = TYPE_LABELS[r.timeOffType] || r.timeOffType;
            const statusLabel = STATUS_LABELS[r.status] || r.status;
            let statusClass = 'pending';
            if (r.status === 'Approved') statusClass = 'approved';
            else if (r.status === 'Rejected') statusClass = 'rejected';

            html += `<div class="history-row">
                <span class="history-type ${r.timeOffType}">${typeLabel}</span>
                <span class="history-dates">${dateLabel}</span>
                <span class="history-days">${r.businessDays} day${r.businessDays !== 1 ? 's' : ''}</span>
                <span class="history-status ${statusClass}">${statusLabel}</span>
            </div>`;
        });

        container.innerHTML = html;
    } catch (e) {
        console.error('Failed to load request history:', e);
    }
}

function renderBalancesCard() {
    const container = document.getElementById('balances-container');
    let html = '';

    if (balances.isAdminPtoEnabled) {
        const available = parseFloat(balances.adminPtoAvailable ?? 0).toFixed(2);
        const accrued = parseFloat(balances.adminPtoAccruedToDate ?? 0).toFixed(2);
        const used = parseFloat(balances.adminPtoUsed ?? 0).toFixed(2);
        const monthly = parseFloat(balances.adminPtoMonthlyRate ?? 0).toFixed(2);
        html += `<div class="bal-row">
            <div class="bal-name"><span class="bal-dot pto"></span>Paid Time Off</div>
            <div class="bal-val">${available}d</div>
        </div>
        <div class="bal-sub-row">
            <span class="bal-sub-label">Accrued to date</span>
            <span class="bal-sub-val">${accrued}d</span>
        </div>
        <div class="bal-sub-row">
            <span class="bal-sub-label">Used / Pending</span>
            <span class="bal-sub-val">${used}d</span>
        </div>
        <div class="bal-sub-row">
            <span class="bal-sub-label">Monthly rate</span>
            <span class="bal-sub-val">${monthly}d</span>
        </div>`;
    } else {
        if (balances.isPtoEnabled) {
            html += `<div class="bal-row">
                <div class="bal-name"><span class="bal-dot pto"></span>Paid Time Off</div>
                <div class="bal-val">${balances.ptoAvailable}d</div>
            </div>`;
        }

        html += `<div class="bal-row">
            <div class="bal-name"><span class="bal-dot upto"></span>Unpaid Time Off</div>
            <div><span class="bal-unlimited">Unlimited</span></div>
        </div>`;

        html += `<div class="bal-row">
            <div class="bal-name"><span class="bal-dot vto"></span>Voluntary Time Off</div>
            <div class="bal-val">${balances.vtoAvailable}d</div>
        </div>`;
    }

    container.innerHTML = html;
}

async function loadCalendarMonth() {
    const monthStart = `${calYear}-${String(calMonth + 1).padStart(2, '0')}-01`;
    const lastDay = new Date(calYear, calMonth + 1, 0).getDate();
    const monthEnd = `${calYear}-${String(calMonth + 1).padStart(2, '0')}-${String(lastDay).padStart(2, '0')}`;

    try {
        const data = await fetch(`/General/TimeOff/GetCalendarEntries?monthStart=${monthStart}&monthEnd=${monthEnd}`)
            .then(r => { if (!r.ok) throw r; return r.json(); });
        calendarEntries = data.calendarEntries;
    } catch (e) {
        calendarEntries = [];
    }
    renderCalendar();
}

function changeMonth(delta) {
    calMonth += delta;
    if (calMonth < 0) { calMonth = 11; calYear--; }
    if (calMonth > 11) { calMonth = 0; calYear++; }
    loadCalendarMonth();
}

function renderCalendar() {
    document.getElementById('cal-month-label').textContent = MONTHS[calMonth] + ' ' + calYear;
    const grid = document.getElementById('cal-grid');

    // Keep day headers (first 7 children)
    while (grid.children.length > 7) grid.removeChild(grid.lastChild);

    const firstDay = new Date(calYear, calMonth, 1).getDay();
    const daysInMonth = new Date(calYear, calMonth + 1, 0).getDate();
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // Empty cells before first day
    for (let i = 0; i < firstDay; i++) {
        const el = document.createElement('div');
        el.className = 'cal-day empty';
        grid.appendChild(el);
    }

    for (let d = 1; d <= daysInMonth; d++) {
        const date = new Date(calYear, calMonth, d);
        const dow = date.getDay();
        const isWeekend = dow === 0 || dow === 6;
        const isPast = date < today;
        const isToday = date.getTime() === today.getTime();
        const dateStr = toDateStr(date);

        const el = document.createElement('div');
        el.className = 'cal-day';
        if (isWeekend) el.classList.add('weekend');
        if (isPast && !isToday) el.classList.add('past');
        if (isToday) el.classList.add('today');
        el.dataset.date = dateStr;

        // Check if this date is part of a confirmed entry
        const entry = getEntryForDate(dateStr);
        if (entry) {
            el.classList.add('conf-' + entry.timeOffType);
            const start = entry.startDate.split('T')[0];
            const end = entry.endDate.split('T')[0];
            const isSingle = start === end;
            if (isSingle) el.classList.add('range-single');
            else if (dateStr === start) el.classList.add('range-start');
            else if (dateStr === end) el.classList.add('range-end');
            else el.classList.add('range-mid');
        }

        const numEl = document.createElement('div');
        numEl.className = 'cal-day-num';
        numEl.textContent = d;
        el.appendChild(numEl);

        if (!isWeekend && !isPast) {
            el.addEventListener('mousedown', onDayMouseDown);
            el.addEventListener('mouseover', onDayMouseOver);
            el.addEventListener('mouseup', onDayMouseUp);
        }

        grid.appendChild(el);
    }

    document.addEventListener('mouseup', () => {
        if (isDragging && dragStart) {
            finalizeDragSelection();
        }
        isDragging = false;
    });
}

function getEntryForDate(dateStr) {
    return calendarEntries.find(e => {
        const start = e.startDate.split('T')[0];
        const end = e.endDate.split('T')[0];
        return dateStr >= start && dateStr <= end;
    });
}

function toDateStr(date) {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
}

function onDayMouseDown(e) {
    isDragging = true;
    dragStart = this.dataset.date;
    dragEnd = this.dataset.date;
    clearSelection();
    this.classList.add('selecting');
}

function onDayMouseOver() {
    if (!isDragging) return;
    dragEnd = this.dataset.date;
    highlightDragRange();
}

function onDayMouseUp() {
    if (!isDragging) return;
    dragEnd = this.dataset.date;
    finalizeDragSelection();
    isDragging = false;
}

function clearSelection() {
    document.querySelectorAll('.cal-day.selecting, .cal-day.sel-start, .cal-day.sel-end, .cal-day.sel-range')
        .forEach(el => el.classList.remove('selecting', 'sel-start', 'sel-end', 'sel-range'));
}

function highlightDragRange() {
    clearSelection();
    const start = dragStart < dragEnd ? dragStart : dragEnd;
    const end = dragStart < dragEnd ? dragEnd : dragStart;

    document.querySelectorAll('.cal-day[data-date]').forEach(el => {
        const d = el.dataset.date;
        if (d >= start && d <= end && !el.classList.contains('weekend') && !el.classList.contains('past')) {
            if (d === start) el.classList.add('sel-start');
            else if (d === end) el.classList.add('sel-end');
            else el.classList.add('sel-range');
        }
    });
}

function finalizeDragSelection() {
    const start = dragStart < dragEnd ? dragStart : dragEnd;
    const end = dragStart < dragEnd ? dragEnd : dragStart;
    clearSelection();
    openRequestModal(start, end);
    dragStart = null;
    dragEnd = null;
}

function openRequestModal(startDate, endDate) {
    document.getElementById('startDateInput').value = startDate;
    document.getElementById('endDateInput').value = endDate;
    document.getElementById('timeOffTypeSelect').value = '';
    document.getElementById('businessDaysDisplay').value = '';
    document.getElementById('balance-display').innerHTML = '';
    document.getElementById('balance-display').className = '';

    // Add PTO option if eligible (consultant or admin)
    const select = document.getElementById('timeOffTypeSelect');
    const existingPto = select.querySelector('option[value="PTO"]');
    const ptoEnabled = balances.isPtoEnabled || balances.isAdminPtoEnabled;
    if (ptoEnabled && !existingPto) {
        const opt = document.createElement('option');
        opt.value = 'PTO';
        opt.textContent = 'Paid Time Off';
        select.insertBefore(opt, select.options[1]);
    } else if (!ptoEnabled && existingPto) {
        existingPto.remove();
    }

    recalculateDays();
    showModal('modal-time-off-request');
}

function onTimeOffTypeChange() {
    const type = document.getElementById('timeOffTypeSelect').value;
    const balDisplay = document.getElementById('balance-display');

    if (type === 'PTO') {
        balDisplay.className = 'pto';
        const available = balances.isAdminPtoEnabled
            ? parseFloat(balances.adminPtoAvailable ?? 0).toFixed(2)
            : balances.ptoAvailable;
        balDisplay.innerHTML = `Available: <strong>${available} days</strong>`;
    } else if (type === 'VTO') {
        balDisplay.className = 'vto';
        balDisplay.innerHTML = `Available: <strong>${balances.vtoAvailable} day</strong>`;
    } else if (type === 'UPTO') {
        balDisplay.className = 'upto';
        balDisplay.innerHTML = `<strong>Unlimited</strong>`;
    } else {
        balDisplay.className = '';
        balDisplay.innerHTML = '';
    }
}

function recalculateDays() {
    const startStr = document.getElementById('startDateInput').value;
    const endStr = document.getElementById('endDateInput').value;
    if (!startStr || !endStr) {
        document.getElementById('businessDaysDisplay').value = '';
        return;
    }

    const start = new Date(startStr + 'T00:00:00');
    const end = new Date(endStr + 'T00:00:00');
    if (start > end) {
        document.getElementById('businessDaysDisplay').value = 'Invalid range';
        return;
    }

    let count = 0;
    for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
        const dow = d.getDay();
        if (dow === 0 || dow === 6) continue;
        if (holidayDates.includes(d.toDateString())) continue;
        count++;
    }

    document.getElementById('businessDaysDisplay').value = count + ' business day' + (count !== 1 ? 's' : '');
}

async function submitTimeOffRequest() {
    const type = document.getElementById('timeOffTypeSelect').value;
    const startDate = document.getElementById('startDateInput').value;
    const endDate = document.getElementById('endDateInput').value;

    if (!type) { toastr.warning('Please select a time off type.'); return; }
    if (!startDate) { toastr.warning('Please select a start date.'); return; }
    if (!endDate) { toastr.warning('Please select an end date.'); return; }

    displaySpinner();
    const token = document.querySelector('[name="__RequestVerificationToken"]')?.value;

    try {
        const response = await fetch('/General/TimeOff/SubmitRequest', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: token
            },
            body: JSON.stringify({
                TimeOffType: type,
                StartDate: startDate,
                EndDate: endDate
            })
        });

        const result = await response.json();

        if (response.ok && result.success) {
            toastr.success(result.message);
            hideModal('modal-time-off-request');
            await loadInitialData();
        } else {
            if (result.errors) {
                displayToasterErrorArray(result.errors);
            } else {
                toastr.error(result.error || 'An error occurred.');
            }
        }
    } catch (error) {
        toastr.error('An unexpected error occurred.');
        validateSessionExpiration(error.message);
    } finally {
        hideSpinner();
    }
}

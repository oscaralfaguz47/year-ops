//Display modal review for approval
async function displayReviewForApprovalModal(modalId, submissionId) {
    document.getElementById('submissionId-input').value = submissionId;
    var url = "/Finances/PaymentSheets/GetReportDetailsFromSubmissionById?submissionId=" + encodeURIComponent(submissionId);
    displaySpinner();
    try {
        const response = await fetch(url);

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }

        const dataFromApi = await response.json();

        let submissionDetailsContainer = document.getElementById('submission-details-container');
        submissionDetailsContainer.innerHTML = '';
        let movementsDetailsArray = JSON.parse(dataFromApi.reportDetails.movements);
        let startPeriodDateFromDb = new Date(dataFromApi.reportDetails.startPeriodDate);
        let endPeriodDateFromDb = new Date(dataFromApi.reportDetails.endPeriodDate);
        document.getElementById('review-for-approval-modal-title').innerHTML = `<span class="strong-label">${getMonthName(startPeriodDateFromDb.getMonth())} ${startPeriodDateFromDb.getDate()} - ${getMonthName(endPeriodDateFromDb.getMonth())} ${endPeriodDateFromDb.getDate()}, <span style="color:var(--clr-grayDark)">${startPeriodDateFromDb.getFullYear()}</span></span>`;
        let movementsBody = document.createElement('div');

        const editMovementsValue = document.createElement('div');
        editMovementsValue.className = 'edit-hours-container';
        const editMovementsButton = document.createElement('button');
        editMovementsButton.innerHTML = `<img title="Edit Hours" src="/icons/Shared/pensil-edit.svg"> Edit Hours`;
        editMovementsButton.addEventListener('click', function () {
            displayEditHoursValue(JSON.parse(dataFromApi.reportDetails.movements), dataFromApi.reportDetails.clientHasTrackingTool);
        });
        editMovementsValue.appendChild(editMovementsButton);

        movementsBody.appendChild(editMovementsValue);
        movementsBody.className = 'movement-body';
        let headerInfo = document.createElement('div');
        let attachmentsSection = document.createElement('div');
        attachmentsSection.className = 'attachments-section';
        let contentForRightHeaderDiv = ``;
        let totalHolidaysHours = 0;

        if (!dataFromApi.reportDetails.clientHasTrackingTool) {
            let ul = document.createElement('ul');
            ul.className = 'movements-section';
            let totalHoursFormatted = 0;
            let totalHours = 0;
            let totalMinutes = 0;
            let totalPayableHours = 0;
            let totalPayableMinutes = 0;
            let totalNonPayableHours = 0;
            let totalNonPayableMinutes = 0;

            movementsDetailsArray.forEach(function (obj, index) {
                let li = document.createElement('li');
                let actionDateReportedTime = obj.ActionDate.split('-'); 
                let year = actionDateReportedTime[0];
                let month = actionDateReportedTime[1] - 1; 
                let day = actionDateReportedTime[2];

                let formattedActionDate = new Date(year, month, day).toLocaleDateString('en-US', {
                    weekday: 'short',
                    day: 'numeric',
                    month: 'long',
                    year: 'numeric'
                });

                const hoursMinutes = calculateTimeDifference(obj.TimeFrom, obj.TimeTo);
                li.innerHTML = `<label class="date-reported">${formattedActionDate}</label>
                ${obj.MovementTypeName !== 'Holidays' ? `<label class="time-reported ${obj.MovementTypeName.includes('(Non-payable)') ? 'non-payable' : ''}">${formatTimeTo12Hour(obj.TimeFrom)}</label> - 
                <label class="time-reported ${obj.MovementTypeName.includes('(Non-payable)') ? 'non-payable' : ''}">${formatTimeTo12Hour(obj.TimeTo)}</label>` : `<label class="time-reported-holiday">Holiday</label>`} 
                <span class="hours-minutes">${obj.MovementTypeName === 'Holidays' ? '<i class="fa-solid fa-gift"></i>' : obj.MovementTypeName.includes('(Non-payable)') ? '<i class="non-payable-icon">$</i>' : '<i class="fa-regular fa-clock"></i>'} ${hoursMinutes.hours} Hours, ${hoursMinutes.minutes} Minutes</span>
                ${obj.Notes !== '' ? `<span class="notes-reported"><i class="fa-regular fa-comment-dots tooltip-target" data-tooltip="${obj.Notes}"></i></span>` : ''}`;
                ul.appendChild(li);
                totalHoursFormatted += obj.Quantity;
                totalHours += hoursMinutes.hours;
                totalMinutes += hoursMinutes.minutes;
                totalPayableHours += obj.MovementTypeName === 'Normal Hours' ? hoursMinutes.hours : 0;
                totalPayableMinutes += obj.MovementTypeName === 'Normal Hours' ? hoursMinutes.minutes : 0;
                totalNonPayableHours += obj.MovementTypeName.includes('(Non-payable)') ? hoursMinutes.hours : 0;
                totalNonPayableMinutes += obj.MovementTypeName.includes('(Non-payable)') ? hoursMinutes.minutes : 0;
                totalHolidaysHours += obj.MovementTypeName === 'Holidays' ? hoursMinutes.hours : 0;
            });
            let liTotalHoursMinutes = document.createElement('li');
            liTotalHoursMinutes.innerHTML = `<label class="total-label"><strong>TOTAL HOURS/MINUTES:</strong> ${totalHours} Hours, ${totalMinutes} Minutes.</label><br>
            <label><strong>TOTAL HOURS FORMATTED:</strong> ${totalHoursFormatted} Hours.</label>`;
            ul.appendChild(liTotalHoursMinutes);
            contentForRightHeaderDiv = `<span><strong><i class="fa-solid fa-clock"></i> Total worked time: </strong> <span class="total-amount">${totalPayableHours}h, ${totalPayableMinutes}m</span></span>
            ${totalHolidaysHours > 0 ? `<span><strong><i class="fa-solid fa-gift"></i> Total holidays time: </strong> <span class="total-amount">${totalHolidaysHours}h, 0m</span></span>` : ``}
            ${totalNonPayableHours > 0 || totalNonPayableMinutes > 0 ? `<span><strong><i style="text-decoration: line-through">$</i> Total non-payable time: </strong> <span class="total-amount">${totalNonPayableHours}h, ${totalNonPayableMinutes}m</span></span>` : ``}`;
            movementsBody.appendChild(ul);
        } else {
            let ul = document.createElement('ul');
            let additionalNotes = '';
            let holidaysSection = document.createElement('div');
            holidaysSection.className = 'holidays-section';
            holidaysSection.innerHTML = `<p><strong>Holidays</strong></p>`;
            let holidayItem = document.createElement('span');
            const firstNonHolidayMovement = movementsDetailsArray.find(element => element.MovementTypeName !== 'Holidays');
            movementsDetailsArray.forEach(function (obj, index) {
                totalHolidaysHours += obj.MovementTypeName === 'Holidays' ? Number(obj.Quantity) : 0;
                if (firstNonHolidayMovement.Notes !== '' && firstNonHolidayMovement.Notes !== undefined) {
                    additionalNotes = document.createElement('div');
                    additionalNotes.innerHTML = `<p><strong>Additional Notes:</strong></p><p>${firstNonHolidayMovement.Notes}</p>`;
                }

                let li = document.createElement('li');
                contentForRightHeaderDiv += `${obj.MovementTypeName === 'Holidays' ? '' : `<span><strong>${obj.MovementTypeName === 'Normal Hours' ? '<i class="fa-solid fa-clock"></i>' : obj.MovementTypeName === 'On Call Flate Rate' ? '<i class="fa-solid fa-person-military-pointing"></i>' : '<i class="fa-solid fa-laptop-code"></i>'}${obj.MovementTypeName}:</strong> <span class="total-amount">${obj.Quantity}${obj.MovementTypeName !== 'On Call Flate Rate' ? 'h':''}</span></span>`}`;
                if (obj.MovementTypeName === 'Holidays') {
                    holidayItem.innerHTML += `<span class="holiday-item"><i class="fa-solid fa-gift"></i> ${obj.Notes}<span>`;
                    holidaysSection.appendChild(holidayItem);
                }
            });
            if (totalHolidaysHours > 0) {
                contentForRightHeaderDiv += `<span><strong><i class="fa-solid fa-gift"></i>Holidays: </strong><span class="total-amount">${totalHolidaysHours}h</span>`;
            }
            if (holidaysSection.querySelector('span') !== null) {
                ul.appendChild(holidaysSection);
            }
            if (additionalNotes !== '' && additionalNotes !== undefined) {
                ul.appendChild(additionalNotes);
            }
            movementsBody.appendChild(ul);

            if (firstNonHolidayMovement.Blobs !== undefined) {
                JSON.parse(firstNonHolidayMovement.Blobs).forEach(function (blob, index) {
                    let fileType = getFileType(getFileTypeFromUrl(blob.BlobUrl));
                    if (fileType === 'image') {
                        let image = document.createElement('img');
                        image.src = blob.BlobUrl;
                        attachmentsSection.appendChild(image);
                    }
                    if (fileType === 'pdf') {
                        let pdf = document.createElement('iframe');
                        pdf.src = blob.BlobUrl;
                        pdf.width = '100%';
                        attachmentsSection.appendChild(pdf);
                    }
                    if (fileType === 'other') {
                        let other = document.createElement('a');
                        other.href = blob.BlobUrl;
                        other.textContent = cleanBlobName(blob.BlobName);
                        attachmentsSection.appendChild(other);
                    }
                });
            }
        }
        headerInfo.innerHTML = `<div class="header-container row"><div class="col-7"><p><i class="fa-solid fa-user"></i><strong>Consultant: </strong>${dataFromApi.reportDetails.consultantName}</p>
        <p class=""><i class="fa-solid fa-briefcase"></i><strong>Project Name: </strong>${dataFromApi.reportDetails.projectName}</p></div><div class="col-5 right-header-div">${contentForRightHeaderDiv}</div></div>`;
        submissionDetailsContainer.appendChild(headerInfo);
        submissionDetailsContainer.appendChild(movementsBody);
        submissionDetailsContainer.appendChild(attachmentsSection);

        initializeTooltips();
        hideSpinner();
        showModal(modalId);
        return dataFromApi;
    }
    catch (err) {
        hideSpinner();
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err.message);
        displayToasterError('Something went wrong, more details: ' + err);
        return null;
    }
}

//EDIT HOURS FROM APPROVALS
// Minimal styles (idempotent)
(function ensureStyle() {
    if (document.getElementById('hours-edit-style')) return;
    const s = document.createElement('style');
    s.id = 'hours-edit-style';
    s.textContent = `
    .hours-line {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 6px;
      border-bottom: 1px solid #e6e6e6;
      flex-wrap: wrap;            /* keeps one line on wide, wraps gracefully on narrow */
    }
    /* Only dim specific parts (NOT the date label) when removed */
    .soft-removed .dim-on-remove { 
      opacity: .45; 
      filter: grayscale(35%); 
    }

    .label-date, .label-type {
      white-space: nowrap;
      font-weight: 900;
      margin-right: 6px;
    }

    /* Inputs sizing for a tidy single row layout */
    .hours-line input[type="time"] {
      min-width: 120px;
    }
    .hours-line input[type="number"] {
      min-width: 120px;
      max-width: 160px;
    }

    .hours-badge {
      font-size: 0.875rem;
      padding: 2px 8px;
      border: 1px solid #e0e0e0;
      border-radius: 999px;
      white-space: nowrap;
    }

    .btn {
      padding: 6px 10px;
      border: 1px solid #d0d0d0;
      background: #fafafa;
      border-radius: 6px;
      cursor: pointer;
      white-space: nowrap;        /* keep button text on one line */
    }
    .btn:hover { background: #f0f0f0; }
    .btn-danger { border-color: #f5c2c7; background: #f8d7da; }
    .btn-danger:hover { background: #f5c2c7; }
    .btn-secondary { border-color: #cfe2ff; background: #e7f1ff; }
    .btn-secondary:hover { background: #cfe2ff; }

    .delete-note {
      color: #b42318;             /* red-ish */
      font-size: 0.85rem;
      font-weight: 500;
      white-space: nowrap;
    }

    /* Small tweak so disabled fields still look aligned */
    .hours-line input:disabled {
      background: #f9fafb;
    }
  `;
    document.head.appendChild(s);
})();

/** Parses "HH:MM" to minutes from midnight, returns null if invalid */
function parseTimeToMinutes(value) {
    if (!value || typeof value !== 'string') return null;
    const m = /^(\d{2}):(\d{2})$/.exec(value.trim());
    if (!m) return null;
    const hh = Number(m[1]);
    const mm = Number(m[2]);
    if (hh < 0 || hh > 23 || mm < 0 || mm > 59) return null;
    return hh * 60 + mm;
}

/** Returns hours (decimal) between t1 and t2; supports overnight if t2 < t1 */
function diffHours(t1, t2) {
    const m1 = parseTimeToMinutes(t1);
    const m2 = parseTimeToMinutes(t2);
    if (m1 == null || m2 == null) return null;
    let delta = m2 - m1;
    if (delta < 0) delta += 24 * 60; // treat as next day
    return Math.round((delta / 60) * 100) / 100; // round to 2 decimals
}

/**
 * Your base, extended:
 * - clientHasTrackingTool === true  -> Quantity (sin Delete/Undo)
 * - clientHasTrackingTool === false -> TimeFrom/TimeTo + Delete/Undo + Remove flag + hours badge
 * - Save button logs payload:
 *   * true  -> [{ MovementId, Quantity }]
 *   * false -> [{ MovementId, TimeFrom, TimeTo, Remove }]
 */
function displayEditHoursValue(movements, clientHasTrackingTool) {
    // Attach a single save handler
    const saveBtn = document.getElementById('save-btn-edit-hours');
    saveBtn.onclick = async () => { await updateHoursFromApprovals(); };

    console.log(movements);
    showModal('modal-edit-hours-value');

    const hoursToEditContainer = document.getElementById('hours-to-edit-container');
    hoursToEditContainer.innerHTML = '';

    // Helper: format 'YYYY-MM-DD' to readable date for the time branch
    const formatYmdToLong = (ymd) => {
        const [y, m, d] = (ymd || '').split('-');
        if (!y || !m || !d) return ymd || '';
        const dt = new Date(Number(y), Number(m) - 1, Number(d));
        return dt.toLocaleDateString('en-US', { weekday: 'short', day: 'numeric', month: 'long', year: 'numeric' });
    };

    movements.forEach((movement) => {
        if (movement.MovementTypeName === 'Holidays') return;

        const line = document.createElement('div');
        line.className = 'hours-line';
        line.dataset.movementId = String(movement.MovementId);
        line.dataset.hasTracking = String(!!clientHasTrackingTool); // used later in updateHoursFromApprovals

        // Common hidden Remove flag (string "true"/"false")
        const removeHidden = document.createElement('input');
        removeHidden.type = 'hidden';
        removeHidden.value = 'false';
        removeHidden.setAttribute('data-role', 'remove-flag');

        // Hidden MovementId like yours
        const movementIdHidden = document.createElement('input');
        movementIdHidden.type = 'hidden';
        movementIdHidden.value = movement.MovementId;

        // Left label
        const leftSpan = document.createElement('span');

        if (clientHasTrackingTool) {
            // === TRUE => Quantity (no Delete/Undo) ===
            leftSpan.className = 'label-type';
            leftSpan.textContent = movement.MovementTypeName || 'Hours';

            const qty = document.createElement('input');
            qty.type = 'number';
            qty.step = '0.01';
            qty.placeholder = 'Enter the num of hours';
            qty.value = (movement.Quantity ?? '') === '' ? '' : String(movement.Quantity);
            qty.setAttribute('data-role', 'quantity');
            // Optional: mark as dimmable if alguna vez aplicas removed en true-branch
            // qty.classList.add('dim-on-remove');

            line.appendChild(movementIdHidden);
            line.appendChild(leftSpan);
            line.appendChild(qty);
            line.appendChild(removeHidden);
        } else {
            // === FALSE => TimeFrom/TimeTo + Delete/Undo + hours label ===
            leftSpan.className = 'label-date';
            leftSpan.textContent = movement.ActionDate
                ? formatYmdToLong(movement.ActionDate)
                : (movement.MovementTypeName || '');

            const timeFrom = document.createElement('input');
            timeFrom.type = 'time';
            timeFrom.value = movement.TimeFrom || '';
            timeFrom.setAttribute('data-role', 'time-from');
            timeFrom.classList.add('dim-on-remove');

            const timeTo = document.createElement('input');
            timeTo.type = 'time';
            timeTo.value = movement.TimeTo || '';
            timeTo.setAttribute('data-role', 'time-to');
            timeTo.classList.add('dim-on-remove');

            // Hours badge
            const hoursBadge = document.createElement('span');
            hoursBadge.className = 'hours-badge dim-on-remove';
            hoursBadge.setAttribute('data-role', 'hours-badge');

            // Function to recalc hours
            const renderBadge = () => {
                const hrs = diffHours(timeFrom.value, timeTo.value);
                hoursBadge.textContent = (hrs == null) ? '-- h' : `${hrs} h`;
            };
            renderBadge();
            timeFrom.addEventListener('input', renderBadge);
            timeTo.addEventListener('input', renderBadge);

            // Delete button (initial)
            const deleteBtn = document.createElement('button');
            deleteBtn.type = 'button';
            deleteBtn.className = 'btn btn-danger dim-on-remove';
            deleteBtn.textContent = 'Delete';
            deleteBtn.setAttribute('data-role', 'btn-delete');

            // Undo block (contains message + clickable text)
            const undoBlock = document.createElement('div');
            undoBlock.className = 'undo-block dim-on-remove';
            undoBlock.style.display = 'none';
            undoBlock.innerHTML = `
  <div class="undo-message">Will be deleted</div>
  <div class="undo-action">Click to undo</div>
`;
            undoBlock.addEventListener('click', () => {
                removeHidden.value = 'false';
                line.classList.remove('soft-removed');
                timeFrom.disabled = false;
                timeTo.disabled = false;
                undoBlock.style.display = 'none';
                deleteBtn.style.display = '';
            });

            // Delete behavior
            deleteBtn.addEventListener('click', () => {
                removeHidden.value = 'true';
                line.classList.add('soft-removed');
                timeFrom.disabled = true;
                timeTo.disabled = true;
                deleteBtn.style.display = 'none';
                undoBlock.style.display = '';
            });

            // Assemble
            line.appendChild(movementIdHidden);
            line.appendChild(leftSpan);
            line.appendChild(timeFrom);
            line.appendChild(timeTo);
            line.appendChild(hoursBadge);
            line.appendChild(deleteBtn);
            line.appendChild(undoBlock);
            line.appendChild(removeHidden);
        }

        document.getElementById('hours-to-edit-container').appendChild(line);
    });
}

async function updateHoursFromApprovals() {

    const root = document.getElementById('hours-to-edit-container');
    if (!root) {
        console.warn('#hours-to-edit-container not found.');
        return;
    }

    const first = root.querySelector('.hours-line');
    const clientHasTrackingTool =
        first ? first.getAttribute('data-has-tracking') === 'true' : false;

    const payload = [];

    root.querySelectorAll('.hours-line').forEach((row) => {
        const movementId = Number(row.dataset.movementId);
        const remove = (row.querySelector('input[data-role="remove-flag"]')?.value || 'false') === 'true';

        if (clientHasTrackingTool) {
            // TRUE => send MovementId, Quantity
            const quantityRaw = row.querySelector('input[data-role="quantity"]')?.value ?? '';
            payload.push({
                MovementId: movementId,
                Quantity: quantityRaw === '' ? null : Number(quantityRaw)
            });
        } else {
            // FALSE => send MovementId, TimeFrom, TimeTo, Remove
            const timeFrom = row.querySelector('input[data-role="time-from"]')?.value ?? '';
            const timeTo = row.querySelector('input[data-role="time-to"]')?.value ?? '';
            payload.push({
                MovementId: movementId,
                TimeFrom: timeFrom,
                TimeTo: timeTo,
                Remove: remove
            });
        }
    });

    console.log(JSON.stringify(payload));

    const confirmation = await Swal.fire({
        title: "Save Hours Changes",
        text: `Are you sure you want to save the applied changes?`,
        icon: 'warning',
        showCancelButton: true,
        cancelButtonText: 'Cancel',
        cancelButtonColor: '#9ba8b8',
        confirmButtonColor: '#eeb30f',
        confirmButtonText: 'Yes, save the changes!'
    });

    if (!confirmation.isConfirmed) {
        return;
    }
    displaySpinner();
    var token = $('[name="__RequestVerificationToken"]').val();

    try {
        const response = await fetch('/Finances/PaymentSheets/EditHoursFromApprovals', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: token
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
                        return acc.concat(current);
                    }, []);
                    displayToasterWarningArray(allErrors);
                    break;
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }

        const dataFromApi = await response.json();
        displayToasterSuccess(dataFromApi.message);
        hideModal('modal-edit-hours-value');
        hideModal('modal-review-for-approval');
        displayReviewForApprovalModal('modal-review-for-approval', document.getElementById('submissionId-input').value);
        hideSpinner();
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Something went wrong, more details: ' + err);
        hideSpinner();
        return null;
    }
}

function displayApproveRejectConfirmation(action, from, submissionId) {
    showModal('modal-approve-reject-submission');
    document.getElementById('action-input').value = action;
    let buttonAction = action === 'Approved' ? 'Approve' : 'Reject';
    let confirmBtn = document.getElementById('confirm-approve-reject-btn');
    let newConfirmBtn = confirmBtn.cloneNode(true);
    confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);
    newConfirmBtn.addEventListener('click', function () {
        if (from === 'ReviewForApproval') {
            approveRejectSubmission();
        } else {
            submissionIdToReject = submissionId;
            rejectApprovement();
        }
    });
    newConfirmBtn.textContent = buttonAction;
    newConfirmBtn.className = action === 'Approved' ? 'btn-approve' : 'btn-reject';
    let bodyContainer = document.getElementById('body-container');
    if (action === 'Approved') {
        bodyContainer.innerHTML = `<p>Are you sure you want to <strong>APPROVE</strong> this submission?</p>`;
    } else {
        bodyContainer.innerHTML = `<p>Are you sure you want to <strong>REJECT</strong> this submission?</p>
        <div><textarea placeholder="Why are you rejecting this submission?, Please leave a comment." id="comment-input"></textarea>
        <span id="val-mess-message">* The message is required</span>
        </div>`;
    }
}
//Approve - Reject Submission
async function approveRejectSubmission() {
    let commentInputValue = null;
    let confirmModal = 'modal-approve-reject-submission';
    let reviewModal = 'modal-review-for-approval';
    let actionInput = document.getElementById('action-input');
    let submissionInput = document.getElementById('submissionId-input');
    if (actionInput.value === 'Rejected') {
        let commentInput = document.getElementById('comment-input');
        if (commentInput.value === '' || commentInput.value === null) {
            document.getElementById('val-mess-message').style.display = 'block';
            return;
        }
        commentInputValue = commentInput.value;
    }

    displaySpinner();
    let token = $('[name="__RequestVerificationToken"]').val();

    let data = {
        SubmissionId: Number(submissionInput.value),
        Body: commentInputValue,
        TransactionStatus: actionInput.value
    };

    try {
        const response = await fetch('/Finances/PaymentSheets/RejectApproveSubmission', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: token
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
                        return acc.concat(current);
                    }, []);
                    displayToasterWarningArray(allErrors);
                    break;
                case "Not Found":
                    displayToasterError('Resource not found: ' + errorData.detail);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }

        const dataFromApi = await response.json();
        hideModal(confirmModal);
        hideModal(reviewModal);
        getListOfResults(false, true);
        displayToasterSuccess(dataFromApi.message);
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Something went wrong, more details: ' + err);
        hideSpinner();
        return null;
    }
}
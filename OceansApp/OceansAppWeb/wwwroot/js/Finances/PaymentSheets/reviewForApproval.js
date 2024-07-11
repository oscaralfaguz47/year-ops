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
        console.log(dataFromApi);
        let submissionDetailsContainer = document.getElementById('submission-details-container');
        submissionDetailsContainer.innerHTML = '';
        let movementsDetailsArray = JSON.parse(dataFromApi.reportDetails.movements);
        console.log(movementsDetailsArray);
        let startPeriodDateFromDb = new Date(dataFromApi.reportDetails.startPeriodDate);
        let endPeriodDateFromDb = new Date(dataFromApi.reportDetails.endPeriodDate);
        document.getElementById('review-for-approval-modal-title').innerHTML = `<span class="strong-label">${getMonthName(startPeriodDateFromDb.getMonth())} ${startPeriodDateFromDb.getDate()} - ${getMonthName(endPeriodDateFromDb.getMonth())} ${endPeriodDateFromDb.getDate()}, <span style="color:var(--clr-grayDark)">${startPeriodDateFromDb.getFullYear()}</span></span>`;
        let movementsBody = document.createElement('div');
        movementsBody.className = 'movement-body';
        let headerInfo = document.createElement('div');
        let attachmentsSection = document.createElement('div');
        attachmentsSection.className = 'attachments-section';
        let contentForRightHeaderDiv = ``;

        if (!dataFromApi.reportDetails.clientHasTrackingTool) {
            let ul = document.createElement('ul');
            ul.className = 'movements-section';
            let totalHoursFormatted = 0;
            let totalHours = 0;
            let totalMinutes = 0;
            movementsDetailsArray.forEach(function (obj, index) {
                let li = document.createElement('li');
                let actionDateReportedTime = new Date(obj.ActionDate);
                const formattedActionDate = actionDateReportedTime.toLocaleDateString('en-US', { weekday: 'short', day: 'numeric', month: 'long' });
                const hoursMinutes = calculateTimeDifference(obj.TimeFrom, obj.TimeTo);
                li.innerHTML = `<label class="date-reported">${formattedActionDate}</label>
                ${obj.MovementTypeName !== 'Holidays' ? `<label class="time-reported ${obj.MovementTypeName.includes('(Non-payable)') ? 'non-payable' : ''}">${formatTimeTo12Hour(obj.TimeFrom)}</label> - 
                <label class="time-reported ${obj.MovementTypeName.includes('(Non-payable)') ? 'non-payable' : ''}">${formatTimeTo12Hour(obj.TimeTo)}</label>` : `<label class="time-reported-holiday">Holiday</label>` } 
                <span class="hours-minutes">${obj.MovementTypeName === 'Holidays' ? '<i class="fa-solid fa-gift"></i>' :'<i class="fa-regular fa-clock"></i>'} ${hoursMinutes.hours} Hours, ${hoursMinutes.minutes} Minutes</span>
                ${obj.Notes !== '' ? `<span class="notes-reported"><i class="fa-regular fa-comment-dots tooltip-target" data-tooltip="${obj.Notes}"></i></span>` : ''}`;
                ul.appendChild(li);
                totalHoursFormatted += obj.Quantity;
                totalHours += hoursMinutes.hours;
                totalMinutes += hoursMinutes.minutes;
            });
            let liTotalHoursMinutes = document.createElement('li');
            liTotalHoursMinutes.innerHTML = `<label class="total-label"><strong>TOTAL:</strong> ${totalHours} Hours, ${totalMinutes} Minutes. This is equals to: ${totalHoursFormatted} Hours.</label>`;
            ul.appendChild(liTotalHoursMinutes);
            contentForRightHeaderDiv = `<strong><i class="fa-solid fa-clock"></i> Total Hours Reported: </strong> <span class="total-amount">${totalHoursFormatted}</span>`;
            movementsBody.appendChild(ul);
        } else {
            let ul = document.createElement('ul');
            let additionalNotes = '';
            movementsDetailsArray.forEach(function (obj, index) {
                if (index === 0) {
                    additionalNotes = document.createElement('div');
                    additionalNotes.innerHTML = `<p><strong>Additional Notes:</strong></p><p>${obj.Notes}</p>`;
                }
                let li = document.createElement('li');
                contentForRightHeaderDiv += `<span><strong>${obj.MovementTypeName === 'Normal Hours' ? '<i class="fa-solid fa-clock"></i>' : obj.MovementTypeName === 'On Call Flate Rate' ? '<i class="fa-solid fa-person-military-pointing"></i>' : '<i class="fa-solid fa-laptop-code"></i>'}${obj.MovementTypeName}:</strong> <span class="total-amount">${obj.Quantity}</span></span><br>`;
            });
            if (additionalNotes !== '') {
                ul.appendChild(additionalNotes);
            }
            movementsBody.appendChild(ul);
            if (movementsDetailsArray[0] !== undefined && movementsDetailsArray[0].Blobs !== undefined) {
                JSON.parse(movementsDetailsArray[0].Blobs).forEach(function (blob, index) {
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
function displayApproveRejectConfirmation(action) {
    showModal('modal-approve-reject-submission');
    document.getElementById('action-input').value = action;
    let buttonAction = action === 'Approved' ? 'Approve' : 'Reject';
    let confirmBtn = document.getElementById('confirm-approve-reject-btn');
    confirmBtn.textContent = buttonAction;
    confirmBtn.className = action === 'Approved' ? 'btn-approve' : 'btn-reject';
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
    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
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
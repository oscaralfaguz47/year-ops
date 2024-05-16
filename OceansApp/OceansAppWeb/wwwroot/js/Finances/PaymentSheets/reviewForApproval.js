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
        document.getElementById('review-for-approval-modal-title').textContent = dataFromApi.reportDetails.consultantName;
        let movementsBody = document.createElement('div');
        movementsBody.className = 'movement-body';
        let headerInfo = document.createElement('div');
        let totalHoursLabel = document.createElement('h6');
        let attachmentsSection = document.createElement('div');
        attachmentsSection.className = 'attachments-section';

        if (!dataFromApi.reportDetails.clientHasTrackingTool) {
            let ul = document.createElement('ul');
            ul.innerHTML = `<h4><strong>More details</strong></h4>`;
            let totalHours = 0;
            movementsDetailsArray.forEach(function (obj, index) {
                let li = document.createElement('li');
                li.innerHTML = `${formatDateMmDdYyyy(obj.ActionDate)}: <input disabled type="time" value=${obj.TimeFrom}> - <input disabled type="time" value=${obj.TimeTo}> <label class="total-hours-per-line">${obj.Quantity} hours</label> <input type="text" disabled value="${obj.Notes}" />`;
                ul.appendChild(li);
                totalHours += obj.Quantity;
            });
            totalHoursLabel.innerHTML = `<strong>TOTAL HOURS: </strong> ${totalHours}`;
            movementsBody.appendChild(ul);
        } else {
            let ul = document.createElement('ul');
            ul.innerHTML = `<h4><strong>More details</strong></h4>`;
            let notesInput = document.createElement('textarea');
            movementsDetailsArray.forEach(function (obj, index) {
                if (index === 0) {
                    notesInput.value = obj.Notes;
                    notesInput.disabled = true;
                }
                let li = document.createElement('li');
                li.innerHTML = `${obj.MovementTypeName}: ${obj.Quantity}`;
                ul.appendChild(li);
            });
                if(notesInput.value !== 'undefined') {
                ul.appendChild(notesInput);
            };
            movementsBody.appendChild(ul);
            if (movementsDetailsArray[0].Blobs !== undefined) {
                movementsDetailsArray[0].Blobs.forEach(function (blob, index) {
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
        headerInfo.innerHTML = `<p><strong>Project Name: </strong>${dataFromApi.reportDetails.projectName}</p>
        <p><strong>Period: </strong>${formatDateMmDdYyyy(dataFromApi.reportDetails.startPeriodDate)} - ${formatDateMmDdYyyy(dataFromApi.reportDetails.endPeriodDate)}</p>`;
        submissionDetailsContainer.appendChild(headerInfo);
        submissionDetailsContainer.appendChild(totalHoursLabel);
        submissionDetailsContainer.appendChild(movementsBody);
        submissionDetailsContainer.appendChild(attachmentsSection);

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
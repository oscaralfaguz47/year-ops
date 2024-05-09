//Display modal review for approval
async function displayReviewForApprovalModal(modalId, submissionId) {
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
                        let other = document.createElement('label');
                        other.textContent = blob.BlobUrl;
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
        console.error('Network or fetch error:', err);
        displayToasterError('Something went wrong, more details: ' + err);
        return null;
    }
}
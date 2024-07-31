
const reviewForApprovalContainer = getElementById('review-for-payment-container');
async function displayReviewForPaymentModal(modalId, consultantId) {
    let url = "/Finances/PaymentSheets/GetReportToMakePayment?consultantId=" + encodeURIComponent(consultantId)
        + "&startDate=" + encodeURIComponent(dateFromInput.value)
        + "&endDate=" + encodeURIComponent(dateToInput.value);

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
            hideModal(modalId);
            return null;
        }

        const dataFromApi = await response.json();
        console.log(dataFromApi);
        getElementById('make-payment-modal-title').textContent = dataFromApi.reportDetails.consultantName;
        reviewForApprovalContainer.innerHTML = '';
        const tableThead = document.createElement('thead');
        const tableTr = document.createElement('tr');
        const descriptionTh = document.createElement('th');
        descriptionTh.textContent = 'Description';
        const quantityTh = document.createElement('th');
        quantityTh.textContent = 'Quantity';
        const unitPriceTh = document.createElement('th');
        unitPriceTh.textContent = 'Unit Price';
        const subTotalTh = document.createElement('th');
        subTotalTh.textContent = 'Subtotal';

        tableTr.appendChild(descriptionTh);
        tableTr.appendChild(quantityTh);
        tableTr.appendChild(unitPriceTh);
        tableTr.appendChild(subTotalTh);
        tableThead.appendChild(tableTr);

        if (dataFromApi.reportDetails.projectMovements.length > 0) {
            const projectMovementsSection = document.createElement('div');
            projectMovementsSection.className = 'global-table-container';
            const projectMovementsTable = document.createElement('table');
            projectMovementsTable.appendChild(tableThead);
            const tableBody = document.createElement('tbody');
            let projectName = '';
            let projectTotal = 0;

            dataFromApi.reportDetails.projectMovements.forEach(function (obj, index) {
                if (projectName !== obj.projectName) {
                    if (projectName !== '') {
                        // Add total row for the previous project
                        const trTotal = document.createElement('tr');
                        const tdTotalLabel = document.createElement('td');
                        tdTotalLabel.textContent = 'Total';
                        tdTotalLabel.colSpan = 3;
                        const tdTotalValue = document.createElement('td');
                        tdTotalValue.textContent = '$' + projectTotal.toFixed(2);

                        trTotal.appendChild(tdTotalLabel);
                        trTotal.appendChild(tdTotalValue);
                        tableBody.appendChild(trTotal);

                        projectTotal = 0; // Reset project total
                    }

                    // Add project name row
                    const trProjectName = document.createElement('tr');
                    const tdProjectName = document.createElement('td');
                    tdProjectName.textContent = obj.projectName;
                    tdProjectName.colSpan = 4; // Make the td span all columns
                    trProjectName.appendChild(tdProjectName);
                    tableBody.appendChild(trProjectName);
                    projectName = obj.projectName; // Update the project name
                }

                // Add movement row
                const tr = document.createElement('tr');
                const tdDescription = document.createElement('td');
                tdDescription.textContent = obj.movementTypeName;
                const tdQuantity = document.createElement('td');
                tdQuantity.textContent = obj.quantity.toFixed(2);
                const tdUnitPrice = document.createElement('td');
                tdUnitPrice.textContent = '$' + obj.unitPrice.toFixed(2);
                const tdSubtotal = document.createElement('td');
                tdSubtotal.textContent = '$' + obj.totalAmount.toFixed(2);

                tr.appendChild(tdDescription);
                tr.appendChild(tdQuantity);
                tr.appendChild(tdUnitPrice);
                tr.appendChild(tdSubtotal);
                tableBody.appendChild(tr);

                projectTotal += parseFloat(obj.totalAmount); // Add to project total
            });

            // Add total row for the last project
            const trTotal = document.createElement('tr');
            const tdTotalLabel = document.createElement('td');
            tdTotalLabel.textContent = 'Total';
            tdTotalLabel.colSpan = 3;
            const tdTotalValue = document.createElement('td');
            tdTotalValue.textContent = '$' + projectTotal.toFixed(2);

            trTotal.appendChild(tdTotalLabel);
            trTotal.appendChild(tdTotalValue);
            tableBody.appendChild(trTotal);

            projectMovementsTable.appendChild(tableBody);
            projectMovementsSection.appendChild(projectMovementsTable);
            reviewForApprovalContainer.appendChild(projectMovementsSection);
        }





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

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

        // Create the table header once
        const tableThead = document.createElement('thead');
        const tableTr = document.createElement('tr');
        const descriptionTh = document.createElement('th');
        descriptionTh.textContent = 'Description';
        const quantityTh = document.createElement('th');
        quantityTh.textContent = 'Quantity';
        const unitPriceTh = document.createElement('th');
        const subTotalTh = document.createElement('th');
        unitPriceTh.textContent = 'Unit Price';
        subTotalTh.textContent = 'Subtotal';

        tableTr.appendChild(descriptionTh);
        tableTr.appendChild(quantityTh);
        tableTr.appendChild(unitPriceTh);
        tableTr.appendChild(subTotalTh);
        tableThead.appendChild(tableTr);

        let totalCredits = 0;
        let totalDebits = 0;

        // Project movements
        if (dataFromApi.reportDetails.listOfMovements.projectMovements.length > 0) {
            const projectMovementsSection = document.createElement('div');
            const projectMovementsTitle = document.createElement('label');
            projectMovementsTitle.innerHTML = `<img class="icon" src="/icons/Shared/laptop-code.svg"> <label>PROJECTS</label>`;
            projectMovementsTitle.className = 'section-title';
            projectMovementsSection.appendChild(projectMovementsTitle);
            projectMovementsSection.className = 'global-table-container';
            const projectMovementsTable = document.createElement('table');
            projectMovementsTable.appendChild(tableThead.cloneNode(true)); // Clone the header
            const tableBody = document.createElement('tbody');
            let projectName = '';
            let projectTotal = 0;

            dataFromApi.reportDetails.listOfMovements.projectMovements.forEach(function (obj, index) {
                if (projectName !== obj.projectName) {
                    if (projectName !== '') {
                        // Add total row for the previous project
                        const trTotal = document.createElement('tr');
                        const tdTotalLabel = document.createElement('td');
                        tdTotalLabel.textContent = "Project Total Amount";
                        tdTotalLabel.className = 'total-label';
                        tdTotalLabel.colSpan = 3;
                        const tdTotalValue = document.createElement('td');
                        tdTotalValue.className = 'td-total-value';
                        tdTotalValue.textContent = '$' + projectTotal.toFixed(2);

                        trTotal.appendChild(tdTotalLabel);
                        trTotal.appendChild(tdTotalValue);
                        trTotal.className = 'credit-background';
                        tableBody.appendChild(trTotal);

                        totalCredits += projectTotal; // Sum project total to credits

                        projectTotal = 0; // Reset project total
                    }

                    // Add project name row
                    const trProjectName = document.createElement('tr');
                    const tdProjectName = document.createElement('td');
                    tdProjectName.textContent = obj.projectName;
                    tdProjectName.colSpan = 4; // Make the td span all columns
                    tdProjectName.className = 'project-name';
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
            tdTotalLabel.textContent = "Project Total Amount";
            tdTotalLabel.className = 'total-label';
            tdTotalLabel.colSpan = 3;
            const tdTotalValue = document.createElement('td');
            tdTotalValue.className = 'td-total-value';
            tdTotalValue.textContent = '$' + projectTotal.toFixed(2);

            trTotal.appendChild(tdTotalLabel);
            trTotal.appendChild(tdTotalValue);
            trTotal.className = 'credit-background';
            tableBody.appendChild(trTotal);

            totalCredits += projectTotal; // Sum last project total to credits

            projectMovementsTable.appendChild(tableBody);
            projectMovementsSection.appendChild(projectMovementsTable);
            reviewForApprovalContainer.appendChild(projectMovementsSection);
        }

        // Benefits and credits
        if (dataFromApi.reportDetails.listOfMovements.benefitsAndOtherMovements.length > 0) {
            const creditsSection = document.createElement('div');
            creditsSection.className = 'global-table-container';
            const creditsTitle = document.createElement('label');
            creditsTitle.innerHTML = `<img class="icon-debit-credit" src="/icons/Shared/square-plus.svg"> <label>CREDITS</label>`;
            creditsTitle.className = 'section-title';
            creditsSection.appendChild(creditsTitle);
            const creditsTable = document.createElement('table');
            creditsTable.appendChild(tableThead.cloneNode(true)); // Clone the header
            const tableBody = document.createElement('tbody');
            let otherCreditsTotal = 0;

            dataFromApi.reportDetails.listOfMovements.benefitsAndOtherMovements.forEach(function (obj, index) {

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

                otherCreditsTotal += parseFloat(obj.totalAmount); // Add to other credits total
            });

            // Add total row for the last credits category
            const trTotal = document.createElement('tr');
            const tdTotalLabel = document.createElement('td');
            tdTotalLabel.textContent = 'Total Credits';
            tdTotalLabel.className = 'total-label';
            tdTotalLabel.colSpan = 3;
            const tdTotalValue = document.createElement('td');
            tdTotalValue.className = 'td-total-value';
            tdTotalValue.textContent = '$' + otherCreditsTotal.toFixed(2);

            trTotal.appendChild(tdTotalLabel);
            trTotal.appendChild(tdTotalValue);
            trTotal.className = 'credit-background';
            tableBody.appendChild(trTotal);

            totalCredits += otherCreditsTotal; // Add other credits total to total credits

            creditsTable.appendChild(tableBody);
            creditsSection.appendChild(creditsTable);
            reviewForApprovalContainer.appendChild(creditsSection);
        }

        // Debits
        if (dataFromApi.reportDetails.listOfMovements.debitsMovements.length > 0) {
            const debitsSection = document.createElement('div');
            debitsSection.className = 'global-table-container';
            const debitsTitle = document.createElement('label');
            debitsTitle.innerHTML = `<img class="icon-debit-credit" src="/icons/Shared/square-minus.svg"> <label>DEBITS</label>`;
            debitsTitle.className = 'section-title';
            debitsSection.appendChild(debitsTitle);
            const debitsTable = document.createElement('table');
            debitsTable.appendChild(tableThead.cloneNode(true));
            const tableBody = document.createElement('tbody');
            let debitsTotal = 0;

            dataFromApi.reportDetails.listOfMovements.debitsMovements.forEach(function (obj, index) {

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

                debitsTotal += parseFloat(obj.totalAmount); // Add to debits total
            });

            // Add total row for the last debits category
            const trTotal = document.createElement('tr');
            const tdTotalLabel = document.createElement('td');
            tdTotalLabel.textContent = 'Total Debits';
            tdTotalLabel.className = 'total-label';
            tdTotalLabel.colSpan = 3;
            const tdTotalValue = document.createElement('td');
            tdTotalValue.className = 'td-total-value';
            tdTotalValue.textContent = '$' + debitsTotal.toFixed(2);

            trTotal.appendChild(tdTotalLabel);
            trTotal.appendChild(tdTotalValue);
            trTotal.className = 'debit-background';
            tableBody.appendChild(trTotal);

            totalDebits += debitsTotal; // Set the total debits

            debitsTable.appendChild(tableBody);
            debitsSection.appendChild(debitsTable);
            reviewForApprovalContainer.appendChild(debitsSection);
        }

        // Final Summary Table with "Resume" Label
        if (totalCredits > 0 || totalDebits > 0) {
            const finalSummarySection = document.createElement('div');
            finalSummarySection.className = 'global-table-container';

            // Add "Resume" label
            const resumeLabel = document.createElement('label');
            resumeLabel.textContent = 'RESUME';
            resumeLabel.className = 'resume-title';
            finalSummarySection.appendChild(resumeLabel);

            const finalSummaryTable = document.createElement('table');
            const tableBody = document.createElement('tbody');

            // Add Credits row
            const trCredits = document.createElement('tr');
            trCredits.className = 'credit-background';
            const tdCreditsLabel = document.createElement('td');
            tdCreditsLabel.innerHTML = `<span style="color:#1ad30a">+</span> CREDITS`;
            tdCreditsLabel.colSpan = 3;
            const tdCreditsValue = document.createElement('td');
            tdCreditsValue.textContent = '$' + totalCredits.toFixed(2);

            trCredits.appendChild(tdCreditsLabel);
            trCredits.appendChild(tdCreditsValue);
            tableBody.appendChild(trCredits);

            // Add Debits row
            const trDebits = document.createElement('tr');
            trDebits.className = 'debit-background';
            const tdDebitsLabel = document.createElement('td');
            tdDebitsLabel.innerHTML = `<span style="color:red">-</span> DEBITS`;
            tdDebitsLabel.colSpan = 3;
            const tdDebitsValue = document.createElement('td');
            tdDebitsValue.textContent = '$' + totalDebits.toFixed(2);

            trDebits.appendChild(tdDebitsLabel);
            trDebits.appendChild(tdDebitsValue);
            tableBody.appendChild(trDebits);

            // Add Total to Pay row
            const trTotalToPay = document.createElement('tr');
            const tdTotalToPayLabel = document.createElement('td');
            tdTotalToPayLabel.textContent = 'TOTAL TO PAY';
            tdTotalToPayLabel.colSpan = 3;
            const tdTotalToPayValue = document.createElement('td');
            const totalToPay = totalCredits - totalDebits;
            tdTotalToPayValue.textContent = '$' + totalToPay.toFixed(2);
            tdTotalToPayValue.className = 'total-to-pay';

            trTotalToPay.appendChild(tdTotalToPayLabel);
            trTotalToPay.appendChild(tdTotalToPayValue);
            trTotalToPay.className = 'total-to-pay';
            tableBody.appendChild(trTotalToPay);

            finalSummaryTable.appendChild(tableBody);
            finalSummarySection.appendChild(finalSummaryTable);
            reviewForApprovalContainer.appendChild(finalSummarySection);
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
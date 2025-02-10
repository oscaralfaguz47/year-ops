
const reviewForApprovalContainer = getElementById('review-for-payment-container');
const consultantIdInputMP = getElementById('ConsultantIdInput');
const setAsAccountsPayableBtn = getElementById('btn-set-account-payable');
const reportPaymentBtn = getElementById('btn-report-payment');
async function displayReviewForPaymentModal(modalId, consultantId) {
    consultantIdInputMP.value = null;
    setAsAccountsPayableBtn.style.display = 'none';
    reportPaymentBtn.style.display = 'none';
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
        consultantIdInputMP.value = consultantId;
        setAsAccountsPayableBtn.style.display = dataFromApi.reportDetails.accountPayableBalance === null ? 'inline' : 'none';
        reportPaymentBtn.style.display = dataFromApi.reportDetails.accountPayableBalance === null || dataFromApi.reportDetails.accountPayableBalance > 0 ? 'inline' : 'none';
        getElementById('review-for-payment-modal-title').textContent = dataFromApi.reportDetails.consultantName;
        reviewForApprovalContainer.innerHTML = '';
        const divMessageContainer = document.createElement('div');
        reviewForApprovalContainer.appendChild(divMessageContainer);

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
        let paymentsAmount = 0;
        let totalFinal = 0;

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
            let projectTotal = 0;

            // Group movements by project Name and then by movement TypeName
            const movementsGroupedByProject = {};

            dataFromApi.reportDetails.listOfMovements.projectMovements.forEach(function (obj) {
                totalFinal += obj.totalAmount;
                // Group movements by projectName
                if (!movementsGroupedByProject[obj.projectName]) {
                    movementsGroupedByProject[obj.projectName] = {};
                }

                // Group movements by movementTypeName within the project
                const projectGroup = movementsGroupedByProject[obj.projectName];
                if (!projectGroup[obj.movementTypeName]) {
                    projectGroup[obj.movementTypeName] = {
                        movementTypeName: obj.movementTypeName,
                        quantity: 0,
                        unitPrice: obj.unitPrice,
                        totalAmount: 0
                    };
                }

                // Add the amount and recalculate the totalAmount
                projectGroup[obj.movementTypeName].quantity += obj.quantity;
                projectGroup[obj.movementTypeName].totalAmount = projectGroup[obj.movementTypeName].quantity * projectGroup[obj.movementTypeName].unitPrice;
            });

            // After grouping, we go through the projects and then their grouped movements
            Object.keys(movementsGroupedByProject).forEach(function (projectName) {
                const projectGroup = movementsGroupedByProject[projectName];

                // Add project name row
                const trProjectName = document.createElement('tr');
                const tdProjectName = document.createElement('td');
                tdProjectName.textContent = projectName;
                tdProjectName.colSpan = 4; // Que abarque todas las columnas
                tdProjectName.className = 'project-name';
                trProjectName.appendChild(tdProjectName);
                tableBody.appendChild(trProjectName);

                // Initialize the total project
                projectTotal = 0;

                // Loop through movements grouped by movementTypeName
                Object.keys(projectGroup).forEach(function (movementTypeName) {
                    const obj = projectGroup[movementTypeName];

                    // Create the queue for each movement
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

                    projectTotal += parseFloat(obj.totalAmount.toFixed(2));
                });

                // Add project total row
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

                totalCredits += projectTotal; //Add up the total of the current project
            });

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
                totalFinal += obj.totalAmount;
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
                totalFinal -= obj.totalAmount;
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
            totalToPay = totalCredits - totalDebits;
            tdTotalToPayValue.textContent = '$' + totalFinal.toFixed(2);
            tdTotalToPayValue.className = 'total-to-pay';

            trTotalToPay.appendChild(tdTotalToPayLabel);
            trTotalToPay.appendChild(tdTotalToPayValue);
            trTotalToPay.className = 'total-to-pay';
            tableBody.appendChild(trTotalToPay);

            finalSummaryTable.appendChild(tableBody);
            finalSummarySection.appendChild(finalSummaryTable);
            reviewForApprovalContainer.appendChild(finalSummarySection);
        }

        // Payments
        if (dataFromApi.reportDetails.paymentsList.length > 0) {
            const paymentsSection = document.createElement('div');
            paymentsSection.className = 'global-table-container payments-table-container';

            const paymentsTitle = document.createElement('div');
            paymentsTitle.innerHTML = `<label>PAYMENTS</label>`;
            paymentsTitle.className = 'secundary-title';

            const paymentsTable = document.createElement('table');

            const paymentTableThead = document.createElement('thead');
            const paymentTableTr = document.createElement('tr');
            const actionsTh = document.createElement('th');
            const accountingDateTh = document.createElement('th');
            accountingDateTh.textContent = 'Accounting Date';
            const paymentMethodTh = document.createElement('th');
            paymentMethodTh.textContent = 'Payment Method';
            const bankAccountTh = document.createElement('th');
            bankAccountTh.textContent = 'Bank Account';
            const companyTh = document.createElement('th');
            companyTh.textContent = 'Company';
            const referenceNumberTh = document.createElement('th');
            referenceNumberTh.textContent = 'Reference Number';
            const AmountTh = document.createElement('th');
            AmountTh.textContent = 'Amount';

            paymentTableTr.appendChild(actionsTh);
            paymentTableTr.appendChild(AmountTh);
            paymentTableTr.appendChild(accountingDateTh);
            paymentTableTr.appendChild(paymentMethodTh);
            paymentTableTr.appendChild(bankAccountTh);
            paymentTableTr.appendChild(companyTh);
            paymentTableTr.appendChild(referenceNumberTh);

            paymentTableThead.appendChild(paymentTableTr);

            paymentsTable.appendChild(paymentTableThead);

            const tableBody = document.createElement('tbody');

            dataFromApi.reportDetails.paymentsList.forEach(function (obj, index) {
                var deleteBtn = ``;
                var editBtn = ``;
                var menuBtn = ``;

                paymentsAmount += obj.paymentAmount;

                deleteBtn = `<li onclick="deletePayment(${obj.consultantPaymentId})"><i class="red-label bi bi-x-lg"></i> Delete Payment</li>`;
                editBtn = `<li onclick="displayMakePaymentModal('modal-make-payment', ${obj.consultantPaymentId})""><i class="bi bi-pencil-square"></i> Edit Payment</li>`;
                menuBtn = `<i onclick="displayMenuListFromMenuIcon('menuOptions-p${obj.consultantPaymentId}', 'menuIcon-p${obj.consultantPaymentId}')" class="bi bi-three-dots-vertical" id="menuIcon-p${obj.consultantPaymentId}"></i>
                              <div class="menu-options" id="menuOptions-p${obj.consultantPaymentId}">
                               <ul>
                                 ${editBtn}
                                 ${deleteBtn}
                               </ul>
                              </div>`;
                // Add movement row
                const tr = document.createElement('tr');

                const tdActions = document.createElement('td');
                tdActions.innerHTML = menuBtn;
                var accountingDate = new Date(obj.accountingDate);
                var accountingDateformattedDate = ('0' + (accountingDate.getMonth() + 1)).slice(-2) + '/' +
                    ('0' + accountingDate.getDate()).slice(-2) + '/' +
                    accountingDate.getFullYear();
                const tdAccountingDate = document.createElement('td');
                tdAccountingDate.textContent = accountingDateformattedDate;
                const tdpaymentMethod = document.createElement('td');
                tdpaymentMethod.textContent = obj.paymentMethodName;
                const tdbankAccount = document.createElement('td');
                tdbankAccount.textContent = obj.bankAccountName;
                const tdcompany = document.createElement('td');
                tdcompany.textContent = obj.companyId === 'OCE' ? 'Oceans Consulting Firm' : 'OCE LLC';
                const tdreferenceNumber = document.createElement('td');
                tdreferenceNumber.textContent = obj.referenceNumber;
                const tdAmount = document.createElement('td');
                tdAmount.innerHTML = '$' + obj.paymentAmount.toFixed(2);

                tr.appendChild(tdActions);
                tr.appendChild(tdAmount);
                tr.appendChild(tdAccountingDate);
                tr.appendChild(tdpaymentMethod);
                tr.appendChild(tdbankAccount);
                tr.appendChild(tdcompany);
                tr.appendChild(tdreferenceNumber);
                tableBody.appendChild(tr);
            });

            paymentsTable.appendChild(tableBody);
            paymentsSection.appendChild(paymentsTable);
            reviewForApprovalContainer.appendChild(paymentsTitle);
            reviewForApprovalContainer.appendChild(paymentsSection);
        }

        if (dataFromApi.reportDetails.accountPayableAmount !== null && (dataFromApi.reportDetails.accountPayableAmount.toFixed(2) !== totalFinal.toFixed(2))) {
            const informationSectionDiv = document.createElement('div');
            const headerDiv = document.createElement('div');
            const actionsBtnsDiv = document.createElement('div');
            actionsBtnsDiv.className = 'actions-container';
            let differenceAmount = (Number(totalFinal.toFixed(2)) - Number(dataFromApi.reportDetails.accountPayableAmount.toFixed(2))).toFixed(2);
            let savedAccountsPayableAmount = dataFromApi.reportDetails.accountPayableAmount.toFixed(2);
            let existsPayment = dataFromApi.reportDetails.existsPayment;

            reportPaymentBtn.style.display = 'none';

            informationSectionDiv.className = 'information-section-container';
            informationSectionDiv.appendChild(headerDiv);
            informationSectionDiv.appendChild(actionsBtnsDiv);
            headerDiv.innerHTML = `<div>
            <div class="info-cont">
            <img src="/icons/Shared/red-info.svg">
            <p>There's a difference between the account payable and payment amount. Please choose an action bellow to resolve it.</p>
             <label>Current Account Payable Amount: <span>$${savedAccountsPayableAmount}</span></label>
                <label>Total Report Amount: <span>$${totalFinal.toFixed(2)}</span></label>
                <div class="difference-container">
                <label class="${differenceAmount > 0 ? 'green-difference' : 'red-difference'}">Difference: <span>$${differenceAmount}</span></label>
                </div>
            </div>
            </div>`;
            let paymentBalance = totalFinal.toFixed(2) - paymentsAmount.toFixed(2);
            let adjustedAmount = paymentBalance.toFixed(2) - (differenceAmount < 0 ? Math.abs(differenceAmount) : differenceAmount);
            const fixBtn = `<button onclick="fixDifference(${existsPayment}, ${adjustedAmount.toFixed(2)})" class="fix-btn"><img style="width:25px" src="/icons/Shared/fix.svg"> ${!existsPayment ? 'Fix Difference' : adjustedAmount.toFixed(2) > 0 && differenceAmount < 0 ? 'Fix Difference with balance' : 'Fix Difference and Pay Today'
        }</button >`;
            const deferBtn = `<button onclick="displayReviewForDeferModal('modal-defer-next-period', ${consultantIdInputMP.value})" class="defer-btn"><img style="width:25px" src="/icons/Shared/next-arrow.svg"> Defer To Next Period</button>`;

            if (existsPayment) {
                if (adjustedAmount.toFixed(2) > 0) {
                    actionsBtnsDiv.innerHTML = `${fixBtn}`; // If positive
                } else {
                    actionsBtnsDiv.innerHTML = `${(differenceAmount > 0 ? fixBtn : '') + deferBtn}`; // If negative or zero
                }
            } else {
                actionsBtnsDiv.innerHTML = `${fixBtn}`;
            }

            divMessageContainer.appendChild(informationSectionDiv);
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

async function setAsAccountPayable() {

    const confirmation = await Swal.fire({
        title: "Set as Account Payable",
        text: `Are you sure you want to set this as account payable?, the consultant will receive the payment details.`,
        icon: 'warning',
        showCancelButton: true,
        cancelButtonText: 'Cancel',
        cancelButtonColor: '#9ba8b8',
        confirmButtonColor: '#eeb30f',
        confirmButtonText: 'Yes, Do it!'
    });

    if (!confirmation.isConfirmed) {
        return;
    }
    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        ConsultantId: consultantIdInputMP.value === '' ? null : Number(consultantIdInputMP.value),
        StartDatePeriod: dateFromInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2'),
        EndDatePeriod: dateToInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2')
    };

    displaySpinner();
    try {
        const response = await fetch('/Finances/PaymentSheets/SetAsAccountPayable', {
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
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }

        const dataFromApi = await response.json();
        displayToasterSuccess(dataFromApi.message);
        hideSpinner();
        await displayReviewForPaymentModal('modal-review-for-payment', consultantIdInputMP.value);
        return dataFromApi;

    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        hideSpinner();
        return null;
    }
}

async function fixDifference(existsPayment, adjustedAmount) {    
    if (existsPayment && adjustedAmount <= 0) {
        const confirmation = await Swal.fire({
            title: "Fix Difference Today",
            text: `Keep in mind that you already made a payment. Are you sure you want to make another payment today for the difference?`,
            icon: 'warning',
            showCancelButton: true,
            cancelButtonText: 'Cancel',
            cancelButtonColor: '#9ba8b8',
            confirmButtonColor: '#eeb30f',
            confirmButtonText: 'Yes, Do it!'
        });

        if (!confirmation.isConfirmed) {
            return;
        }
    }

    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        ConsultantId: consultantIdInputMP.value === '' ? null : Number(consultantIdInputMP.value),
        StartDatePeriod: dateFromInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2'),
        EndDatePeriod: dateToInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2')
    };

    displaySpinner();
    try {
        const response = await fetch('/Finances/PaymentSheets/FixDifferenceToMakePaymentToday', {
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
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }

        const dataFromApi = await response.json();
        displayToasterSuccess(dataFromApi.message);
        hideSpinner();
        await displayReviewForPaymentModal('modal-review-for-payment', consultantIdInputMP.value);
        return dataFromApi;

    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        hideSpinner();
        return null;
    }
}


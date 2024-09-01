let paymentMethodsArray = [];
const paymentMethodsSelectMP = getElementById('PaymentMethodSelect');
const bankAccountSelectMP = getElementById('BankAccountSelect');
const consultantDetailsMP = getElementById('consultant-details-div');
const accountingDateInputMP = getElementById('accounting-date');
const referenceNumberInputMP = getElementById('reference-number');
const totalAmountToPayInputMP = getElementById('total-amount-to-pay');
const consultantPaymentInputMP = getElementById('ConsultantPaymentInput');
const companyNameDiv = getElementById('company-name-div');
let currentPaymentMethodId = null;
let companyIdMP = null;
const submitBtnsInitialize = [{ id: 'btn-save-payment', text: 'Save' }];
const otherBtnsInitialize = ['close-payment-modal-x-btn', 'btn-cancel-payment-modal'];
const otherBtns = ['btn-cancel-payment-modal', 'close-payment-modal-x-btn'];

async function displayMakePaymentModal(modalId, paymentId) {
    consultantPaymentInputMP.value = paymentId;
    let url = paymentId === null ? "/Finances/PaymentSheets/GetAmountAndDetailsToMakePayment?consultantId=" + encodeURIComponent(consultantIdInputMP.value)
        + "&startDate=" + encodeURIComponent(dateFromInput.value)
        + "&endDate=" + encodeURIComponent(dateToInput.value) : "/Finances/PaymentSheets/GetPaymentDataByPaymentId?paymentId=" + encodeURIComponent(paymentId);

    accountingDateInputMP.value = null;
    referenceNumberInputMP.value = null;
    displaySpinner();
    enableModalButtons(submitBtnsInitialize, otherBtns, 'spinner-border');
    if (paymentMethodsArray.length === 0) {
        paymentMethodsArray = await getAllPaymentMethodsList();
        populateSelect('PaymentMethodSelect', paymentMethodsArray.paymentMethods, null, null);
    }
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
        getElementById('make-payment-modal-title').textContent = paymentId === null ? "You'll report a payment" : "Editing Payment";
        consultantDetailsMP.innerHTML = `<label>Consultant Name: <span>${dataFromApi.reportDetails.consultantName}</span></label>
        <label>Country Name: <span>${dataFromApi.reportDetails.countryName}</span></label>
        <label>Report total amount: <span>$${dataFromApi.reportDetails.amountToPay.toFixed(2)}</span></label>`;
        paymentMethodsSelectMP.value = dataFromApi.reportDetails.paymentMethodId;
        currentPaymentMethodId = dataFromApi.reportDetails.paymentMethodId;
        await getBankAccounts(dataFromApi.reportDetails.paymentMethodId)
        companyNameDiv.textContent = dataFromApi.reportDetails.companyId === "OCE" ? 'Oceans Consulting Firm' : 'OCE LLC';
        totalAmountToPayInputMP.value = dataFromApi.reportDetails.amountToPay.toFixed(2);
        companyIdMP = dataFromApi.reportDetails.companyId;
        let accountingDateFormat = new Date(dataFromApi.reportDetails.accountingDate);
        accountingDateInputMP.value = dataFromApi.reportDetails.accountingDate === null ? null : accountingDateFormat.toISOString().split('T')[0];
        referenceNumberInputMP.value = dataFromApi.reportDetails.referenceNumber;

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

async function changePaymentMethod(select) {
    const confirmation = await Swal.fire({
        title: "Change Payment Method",
        text: `Are you sure you want to change the default payment method for this consultant?`,
        icon: 'warning',
        showCancelButton: true,
        cancelButtonText: 'Cancel',
        cancelButtonColor: '#9ba8b8',
        confirmButtonColor: '#eeb30f',
        confirmButtonText: 'Yes, Change it!'
    });

    if (!confirmation.isConfirmed) {
        paymentMethodsSelectMP.value = currentPaymentMethodId;
        return;
    }
    let url = "/AdminCenter/PaymentMethods/GetCompanyByPaymentMethod?paymentMethodId=" + encodeURIComponent(select.value);

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
            paymentMethodsSelectMP.value = currentPaymentMethodId;
            return null;
        }

        const dataFromApi = await response.json();
        var bankAccounts = await getBankAccounts(select.value);
        currentPaymentMethodId = select.value;
        companyNameDiv.textContent = dataFromApi.companyId === 'OCE' ? 'Oceans Consulting Firm' : 'OCE LLC';
        companyIdMP = dataFromApi.companyId;
        hideSpinner();
        return dataFromApi;
    }
    catch (err) {
        hideSpinner();
        paymentMethodsSelectMP.value = currentPaymentMethodId;
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err.message);
        displayToasterError('Something went wrong, more details: ' + err);
        return null;
    }
}
async function getBankAccounts(paymentMethodId) {
    let url = "/Finances/BankAccounts/GetBankAccountsByPaymentMethodList?paymentMethodId=" + encodeURIComponent(paymentMethodId);

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
            paymentMethodsSelectMP.value = currentPaymentMethodId;
            return null;
        }

        const dataFromApi = await response.json();
        getElementById('BankAccountSelect').innerHTML = '';
        populateSelect('BankAccountSelect', dataFromApi.bankAccounts, null, null);
        return dataFromApi;
    }
    catch (err) {
        hideSpinner();
        paymentMethodsSelectMP.value = currentPaymentMethodId;
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err.message);
        displayToasterError('Something went wrong, more details: ' + err);
        return null;
    }
}

async function createUpdatePayment(modalId) {
    disableButtonsWaitingForPostMethod('btn-save-payment', otherBtns, 'spinner-border')

    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        ConsultantPaymentId: consultantPaymentInputMP.value === '' ? null : Number(consultantPaymentInputMP.value),
        ConsultantId: consultantIdInputMP.value === '' ? null : Number(consultantIdInputMP.value),
        StartDatePeriod: dateFromInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2'),
        EndDatePeriod: dateToInput.value.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$1-$2'),
        ReferenceNumber: referenceNumberInputMP.value,
        PaymentMethodId: Number(paymentMethodsSelectMP.value),
        PaymentAmount: totalAmountToPayInputMP.value === '' ? null : Number(totalAmountToPayInputMP.value),
        AccountingDate: accountingDateInputMP.value === '' ? null : accountingDateInputMP.value.toString(),
        CompanyId: companyIdMP,
        BankAccountId: Number(bankAccountSelectMP.value)
    };
    console.log(data);
    try {
        const response = await fetch('/Finances/PaymentSheets/CreateUpdatePayment', {
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
            enableModalButtons(submitBtnsInitialize, otherBtnsInitialize, 'spinner-border');
            return null;
        }

        const dataFromApi = await response.json();
        displayToasterSuccess(dataFromApi.message);
        hideModal(modalId);
        enableModalButtons(submitBtnsInitialize, otherBtnsInitialize, 'spinner-border');
        await displayReviewForPaymentModal('modal-review-for-payment', consultantIdInputMP.value);
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        enableModalButtons(submitBtnsInitialize, otherBtnsInitialize, 'spinner-border');
        return null;
    }
}

async function deletePayment(paymentId) {
    const confirmation = await Swal.fire({
        title: "Delete Payment",
        text: `Are you sure you want to delete the payment?`,
        icon: 'warning',
        showCancelButton: true,
        cancelButtonText: 'Cancel',
        cancelButtonColor: '#9ba8b8',
        confirmButtonColor: '#eeb30f',
        confirmButtonText: 'Yes, Delete it!'
    });

    if (!confirmation.isConfirmed) {
        return;
    }
    displaySpinner();
    try {
        var token = $('[name="__RequestVerificationToken"]').val();
        const response = await fetch(`/Finances/PaymentSheets/DeletePayment?paymentId=${paymentId}`, {
            method: 'DELETE',
            headers: {
                RequestVerificationToken: token
            }
        });

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => acc.concat(current), []);
                    displayToasterWarningArray(allErrors);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            hideSpinner();
            return null;
        }
        const dataFromApi = await response.json();
        await displayReviewForPaymentModal('modal-review-for-payment', consultantIdInputMP.value);
        displayToasterSuccess(dataFromApi.message);
        hideSpinner();
        return dataFromApi;
    } catch (err) {
        hideSpinner();
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        displayToasterError('Failed to connect to the server. Please check your network connection and try again.');
        return null;
    }
}
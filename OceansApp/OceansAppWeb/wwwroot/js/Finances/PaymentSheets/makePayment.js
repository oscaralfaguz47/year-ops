let paymentMethodsArray = [];
const paymentMethodsSelectMP = getElementById('PaymentMethodSelect');
const consultantDetailsMP = getElementById('consultant-details-div');
const accountingDateInputMP = getElementById('accounting-date');
const referenceNumberInputMP = getElementById('reference-number');
const totalAmountToPayInputMP = getElementById('total-amount-to-pay');
const companyNameDiv = getElementById('company-name-div');
let currentPaymentMethodId = null;
async function displayMakePaymentModal(modalId) {
    let url = "/Finances/PaymentSheets/GetAmountAndDetailsToMakePayment?consultantId=" + encodeURIComponent(consultantIdInputMP.value)
        + "&startDate=" + encodeURIComponent(dateFromInput.value)
        + "&endDate=" + encodeURIComponent(dateToInput.value);

    accountingDateInputMP.value = null;
    referenceNumberInputMP.value = null;
    displaySpinner();
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
        getElementById('make-payment-modal-title').textContent = "You'll report a payment";
        consultantDetailsMP.innerHTML = `<label>Consultant Name: <span>${dataFromApi.reportDetails.consultantName}</span></label>
        <label>Country Name: <span>${dataFromApi.reportDetails.countryName}</span></label>
        <label>Report total amount: <span>$${dataFromApi.reportDetails.amountToPay.toFixed(2)}</span></label>`;
        paymentMethodsSelectMP.value = dataFromApi.reportDetails.paymentMethodId;
        currentPaymentMethodId = dataFromApi.reportDetails.paymentMethodId;
        companyNameDiv.textContent = dataFromApi.reportDetails.companyId === "OCE" ? 'Oceans Consulting Firm' : 'OCE LLC';
        totalAmountToPayInputMP.value = dataFromApi.reportDetails.amountToPay.toFixed(2);
        console.log(dataFromApi);
     
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
        text: `Are you sure you want to change the default payment method?`,
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
        currentPaymentMethodId = select.value;
        companyNameDiv.textContent = dataFromApi.companyId === 'OCE' ? 'Oceans Consulting Firm' : 'OCE LLC';
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
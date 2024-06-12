function getStatusLabel(transactionStatusName) {
    var statusLabel = ``;
    if (transactionStatusName === 'Rejected') {
        statusLabel = `<span class="cel-status red-label"><i class="bi bi-x"></i>${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Approved') {
        statusLabel = `<span class="cel-status"><i class="bi bi-check"></i>${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Sent to be paid') {
        statusLabel = `<span class="cel-status blueLight-lable"><i class="bi bi-send-check"></i>${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Paid') {
        statusLabel = `<span class="cel-status paid-label"><i class="bi bi-credit-card-2-back"></i>${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Waiting to be approved') {
        statusLabel = `<span class="cel-status gray-lable"><i class="bi bi-hourglass-split"></i>${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Accounted - Accounts Payable') {
        statusLabel = `<span class="cel-status orange-label"><i class="bi bi-journal-bookmark-fill"></i>${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Done') {
        statusLabel = `<span class="cel-status green-label"><i class="bi bi-check-circle-fill"></i>${transactionStatusName}</span>`;
    } if (transactionStatusName === 'Pending') {
        statusLabel = `<span class="cel-status red-label">Pending</span>`;
    }
    return statusLabel;
}

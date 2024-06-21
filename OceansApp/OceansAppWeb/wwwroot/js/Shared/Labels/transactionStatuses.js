function getStatusLabel(transactionStatusName) {
    var statusLabel = ``;
    if (transactionStatusName === 'Rejected') {
        statusLabel = `<span class="cel-status rejected-label"><img class="status-img" src="/img/globalIcons/rejected.webp">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Approved') {
        statusLabel = `<span class="cel-status approved-label"><img class="status-img" src="/img/globalIcons/approved.webp">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Sent to be paid') {
        statusLabel = `<span class="cel-status sending-to-paid-label"><img class="status-img" src="/img/globalIcons/sending-to-paid.webp">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Paid') {
        statusLabel = `<span class="cel-status paid-label"><img class="status-img" src="/img/globalIcons/paid.webp">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Waiting to be approved') {
        statusLabel = `<span class="cel-status waiting-label"><img class="status-img" src="/img/globalIcons/waiting.webp">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Accounted - Accounts Payable') {
        statusLabel = `<span class="cel-status accounted-label"><img class="status-img" src="/img/globalIcons/accounted.webp">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Done') {
        statusLabel = `<span class="cel-status done-label"><img class="status-img" src="/img/globalIcons/done.webp">${transactionStatusName}</span>`;
    } if (transactionStatusName === 'Pending') {
        statusLabel = `<span class="cel-status pending-label"><img class="status-img" src="/img/globalIcons/pending.webp">Pending</span>`;
    }
    return statusLabel;
}

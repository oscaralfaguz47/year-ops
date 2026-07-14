function getStatusLabel(transactionStatusName) {
    var statusLabel = ``;
    if (transactionStatusName === 'Rejected') {
        statusLabel = `<span class="cel-status rejected-label"><img class="status-img" src="/icons/Shared/Statuses/rejected.svg">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Approved') {
        statusLabel = `<span class="cel-status approved-label"><img class="status-img" src="/icons/Shared/Statuses/approved.svg">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Sent to be paid') {
        statusLabel = `<span class="cel-status sending-to-paid-label"><img class="status-img" src="/icons/Shared/Statuses/envelope-sent-to-paid.svg">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Paid') {
        statusLabel = `<span class="cel-status paid-label"><img class="status-img" src="/icons/Shared/Statuses/paid.svg">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Waiting to be approved' || transactionStatusName === 'Pending approved') {
        statusLabel = `<span class="cel-status waiting-label"><img class="status-img" src="/icons/Shared/Statuses/waiting.svg">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Awaiting upload') {
        statusLabel = `<span class="cel-status pending-label"><img class="status-img" src="/icons/Shared/Statuses/pending.svg">${transactionStatusName}</span>`;
    } else if (transactionStatusName === 'Accounted') {
        statusLabel = `<span class="cel-status done-label"><img class="status-img" src="/icons/Shared/Statuses/green-check.svg">${transactionStatusName}</span>`;
    } if (transactionStatusName === 'Pending' || transactionStatusName === 'Pending Accounting') {
        statusLabel = `<span class="cel-status pending-label"><img class="status-img" src="/icons/Shared/Statuses/pending.svg">${transactionStatusName}</span>`;
    }
    if (transactionStatusName === 'Updated - Pending Review') {
        statusLabel = `<span class="cel-status pending-review-label"><img src="/icons/Shared/Statuses/pending-review.svg"> ${transactionStatusName}</span>`;
    }
    return statusLabel === `` ? transactionStatusName : statusLabel;
}

function getStatusColor(statusName) {
    switch (statusName) {
        case 'Approved':
            return '#01bfb7';
            break;
        case 'Waiting to be approved':
            return '#eeb30f';
            break;
        case 'Paid':
            return '#232323';
            break;
        default:
            return 'var(--clr-blueLight)';
    }
}
function getStatusWhiteIcon(statusName) {
    switch (statusName) {
        case 'Approved':
            return '<i class="fa-solid fa-check"></i>';
            break;
        case 'Waiting to be approved':
            return '<i class="fa-solid fa-hourglass-start"></i>';
            break;
        case 'Paid':
            return '<i class="fa-regular fa-credit-card"></i>';
            break;
        default:
            return '<img class="status-img" src="/icons/Shared/Statuses/send.svg">';
    }
}
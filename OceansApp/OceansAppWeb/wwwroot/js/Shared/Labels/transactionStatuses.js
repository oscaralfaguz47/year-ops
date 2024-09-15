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
    } else if (transactionStatusName === 'Accounted') {
        statusLabel = `<span class="cel-status done-label"><img class="status-img" src="/img/globalIcons/done.webp">${transactionStatusName}</span>`;
    } if (transactionStatusName === 'Pending' || transactionStatusName === 'Pending Accounting') {
        statusLabel = `<span class="cel-status pending-label"><img class="status-img" src="/img/globalIcons/pending.webp">${transactionStatusName}</span>`;
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
            return '<img class="status-img" src="/img/globalIcons/send.webp">';
    }
}
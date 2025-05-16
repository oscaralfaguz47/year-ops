async function getDocumentTypesList() {
    const url = `/AdminCenter/DocumentTypes/GetAllDocumentTypesListForSelect`;
    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
        }
        return await response.json();
    } catch (error) {
        validateSessionExpiration(error.message);
        displayToasterError("Internet connection failed");
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}

async function getDocumentTypesListByTransactionType(transactionTypeId) {
    const url = `/AdminCenter/DocumentTypes/GetAllDocumentTypesListByTransactionType?transactionTypeId=${encodeURIComponent(transactionTypeId)}`;
    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorData = await response.json();
            displayToasterError(errorData.error);
            throw new Error(`The request to the server failed! More details: ${errorData.detail}`);
        }
        return await response.json();
    } catch (error) {
        validateSessionExpiration(error.message);
        displayToasterError("Internet connection failed");
        throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
    }
}
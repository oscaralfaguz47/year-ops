async function getConsultantsBySearchText(searchText) {
    var url = "/General/ConsultantDetails/GetConsultantsBySearchText?searchText=" + encodeURIComponent(searchText);
    try {
        let response = await fetch(url);
        if (response.ok) {
            return await response.json();
        } else {
            let errorData = await response.json();
            if (errorData.messageType === "Validation Error") {
                displayToasterWarningArray(errorData.errors);
            } else {
                displayToasterErrorArray(errorData.error);
            }
            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
        }
    } catch (error) {
        console.error('Error fetching consultants:', error);
        return null;
    }
}

async function getAllActiveConsultantsBySearchText(searchText, userCategoryName) {
    var url = "/General/ConsultantDetails/GetAllActiveConsultantsBySearchText?searchText=" + encodeURIComponent(searchText);
    if (userCategoryName !== null) {
        url += "&userCategoryName=" + encodeURIComponent(userCategoryName);
    }
    try {
        let response = await fetch(url);
        if (response.ok) {
            return await response.json();
        } else {
            let errorData = await response.json();
            if (errorData.messageType === "Validation Error") {
                displayToasterWarningArray(errorData.errors);
            } else {
                displayToasterErrorArray(errorData.error);
            }
            throw new Error('The request to the server failed!. More details: ' + errorData.detail);
        }
    } catch (error) {
        console.error('Error fetching consultants:', error);
        return null;
    }
}

async function searchAllActiveConsultantsBySearchText(searchTextInput, hiddenInputForId, consultantNameInput, consultantEmailInput, userCategoryName) {
    if (searchTextInput.value.length > 100) {
        searchTextInput.value = searchTextInput.value.slice(0, 100);
    } else {
        let resultsContainer = document.getElementById('consultant-search-results');
        resultsContainer.innerHTML = '';
        resultsContainer.innerHTML = `<div class="text-center"><div class="spinner-border" role="status">
        <span class="sr-only" ></span>
                </div></div>`;
        let data = await getAllActiveConsultantsBySearchText(searchTextInput.value, userCategoryName);
        resultsContainer.innerHTML = '';
        resultsContainer.style.display = 'block';
        if (data.consultants.length > 0) {
            let resultList = document.createElement('ul');
            for (let item of data.consultants) {
                let listItem = document.createElement('li');
                listItem.innerHTML = '<strong>' + item.consultantName + '</strong> ' + (item.userCategoryName === "Administrative" ? '<span class="green-label">(' : '<span class="blue-label">(') + item.userCategoryName + ')</span>';
                listItem.onclick = function () {
                    document.getElementById(hiddenInputForId).value = item.consultantId;
                    document.getElementById(consultantNameInput).value = item.consultantName;
                    document.getElementById(consultantEmailInput).value = item.email;
                    hideConsultantResults();
                };
                resultList.appendChild(listItem);
            }
            resultsContainer.appendChild(resultList);
        } else {
            resultsContainer.innerHTML = '<div class="red-label text-center">No results found</div>';
        }
        document.addEventListener('click', function (event) {
            let isClickInside = resultsContainer.contains(event.target);
            if (!isClickInside) {
                hideConsultantResults();
            }
        });
        document.addEventListener('keydown', function (event) {
            if (event.key === "Escape") {
                hideConsultantResults();
            }
        });
    }
}
function hideConsultantResults() {
    let resultsContainer = document.getElementById('consultant-search-results');
    resultsContainer.style.display = 'none';
}


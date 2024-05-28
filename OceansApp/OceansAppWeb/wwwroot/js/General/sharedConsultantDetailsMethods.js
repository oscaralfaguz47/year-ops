
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
        validateSessionExpiration(error.message);
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
        validateSessionExpiration(error.message);
        console.error('Error fetching consultants:', error);
        return null;
    }
}


let selectedIndex = -1;

async function searchAllActiveConsultantsBySearchText(searchTextInput, hiddenInputForId, consultantNameInput, consultantEmailInput, userCategoryName) {
    if (searchTextInput.value.length > 100) {
        searchTextInput.value = searchTextInput.value.slice(0, 100);
    } else {
        let resultsContainer = document.getElementById('consultant-search-results');
        resultsContainer.innerHTML = '';
        resultsContainer.innerHTML = `<div class="text-center"><div class="spinner-border" role="status">
        <span class="sr-only"></span>
        </div></div>`;
        let data = await getAllActiveConsultantsBySearchText(searchTextInput.value, userCategoryName);
        resultsContainer.innerHTML = '';
        resultsContainer.style.display = 'block';
        if (data.consultants.length > 0) {
            let resultList = document.createElement('ul');
            resultList.id = 'search-result-list'; // Assign an ID to the results list container
            for (let item of data.consultants) {
                let listItem = document.createElement('li');
                listItem.innerHTML = '<strong>' + item.consultantName + '</strong> ' + (item.userCategoryName === "Administrative" ? '<span style="color:gray">(' : '<span class="blue-label">(') + item.userCategoryName + ')</span>';
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
    }
    document.addEventListener('keydown', keyboardNavigation);
}

// Function to update the active item in the results list
function updateActiveItem() {
    const listItems = document.querySelectorAll('#search-result-list li');
    // Removes the active class from all elements.
    listItems.forEach(item => {
        item.classList.remove('active');
    });
    // Adds the active class to the selected element.
    if (selectedIndex >= 0 && selectedIndex < listItems.length) {
        listItems[selectedIndex].classList.add('active');
        listItems[selectedIndex].scrollIntoView({ behavior: "smooth", block: "nearest" });
    }
}

function keyboardNavigation(event) {
    const resultsContainer = document.getElementById('consultant-search-results');
    const listItems = document.querySelectorAll('#search-result-list li');
    if (resultsContainer.style.display !== 'none') {
        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                if (selectedIndex < listItems.length - 1) {
                    selectedIndex++;
                    updateActiveItem();
                }
                break;
            case 'ArrowUp':
                event.preventDefault();
                if (selectedIndex > 0) {
                    selectedIndex--;
                    updateActiveItem();
                }
                break;
            case 'Enter':
                event.preventDefault();
                if (selectedIndex >= 0 && selectedIndex < listItems.length) {
                    listItems[selectedIndex].click();
                }
                break;
        }
    }
    if (event.key === 'Escape') {
        hideConsultantResults();
    }
}

function hideConsultantResults() {
    let resultsContainer = document.getElementById('consultant-search-results');
    resultsContainer.style.display = 'none';
    selectedIndex = -1; // Reset the selected index
    document.getElementById('search-consultant-input').value = null;
}

// Add a listener for clicks outside the results container to close the results when clicked outside.
document.addEventListener('click', function (event) {
    const searchContainer = document.getElementById('consultants-search-cont');
    if (!searchContainer.contains(event.target)) {
        hideConsultantResults();
    }
});

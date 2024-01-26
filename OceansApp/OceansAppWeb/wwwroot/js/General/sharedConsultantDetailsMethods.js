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
        return null; // Devuelve null en caso de error
    } finally {
    }
}


function hideConsultantResults() {
    let resultsContainer = document.getElementById('consultant-search-results');
    resultsContainer.style.display = 'none';
}

async function searchConsultantsBySearchText(searchText, hiddenInputForId, consultantNameInput, consultantEmailInput) {
    let resultsContainer = document.getElementById('consultant-search-results');
    resultsContainer.innerHTML = '';
    resultsContainer.innerHTML = `<div class="text-center"><div class="spinner-border" role="status">
        <span class="sr-only" ></span>
                </div></div>`;
    let data = await getConsultantsBySearchText(searchText);
    resultsContainer.innerHTML = '';
    resultsContainer.style.display = 'block';
    if (data.consultants.length > 0) {
        let resultList = document.createElement('ul');
        for (let item of data.consultants) {
            let listItem = document.createElement('li');
            listItem.innerHTML = '<strong>' + item.consultantName + '</strong>' + ' (' + item.email + ')';
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



async function getBenefitsList() {
    var url = "/AdminCenter/ConsultantBenefits/GetAllBenefitsListForSelect";
    return fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    displayToasterError(errorData.error);
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .catch(error => {
                validateSessionExpiration(error.message);
            displayToasterError("Internet connection failed");
            throw new Error('Network error or unable to reach the server. More details: ' + error.message);
        });
}

function fillBenefitsSelect(selectElement, firstOption) {
    if (selectElement.length > 1) {
        return;
    }
    selectElement.innerHTML = '<option value="loading">Loading options… (⏳)</option>';
    getBenefitsList()
        .then(data => {
            selectElement.innerHTML = '<option value="">-' + firstOption + '-</option>';
            data.benefits.forEach(obj => {
                selectElement.add(new Option(obj.text, obj.value));
            });
        })
        .catch(error => {
            console.error('Error fetching benefits:', error);
        });
}
//BENEFIT CATEGORIES
async function getBenefitCategoriesList(benefitId) {
    var url = "/AdminCenter/ConsultantBenefits/GetAllBenefitCategoriesListForSelect?benefitId=" + encodeURIComponent(benefitId);
    return fetch(url)
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                return response.json().then(errorData => {
                    displayToasterError(errorData.error);
                    throw new Error('The request to the server failed!. More details: ' + errorData.detail);
                });
            }
        })
        .catch(error => {
                validateSessionExpiration(error.message);
            displayToasterError("Internet connection failed");
            throw new Error('Network error or unable to reach the server. More details: ' + error.message);
        });
}
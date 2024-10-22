//Profile Photo
const uploadButton = getElementById('upload-button');
const saveButton = getElementById('save-button');
const fileInput = getElementById('profile-picture-input');
const profileImage = getElementById('profile-picture-image');
const messageElement = document.createElement('p'); 
messageElement.style.color = 'red';
messageElement.style.display = 'none';
uploadButton.insertAdjacentElement('afterend', messageElement); // Insert it just below the button

// Variables to store the previously selected image and the saved image
let previousImageSrc = profileImage.src;
let savedImageSrc = profileImage.src; // New variable for the saved image

// Initially hide the "Save" button
saveButton.style.display = 'none';

// When clicking the "Upload New Picture" button
uploadButton.addEventListener('click', function () {
    fileInput.click();
    messageElement.style.display = 'none'; 
});

// When the input changes (an image is selected or the picker is canceled)
fileInput.addEventListener('change', function (event) {
    const file = event.target.files[0];

    if (file) {
        // Check if the file is an image
        if (file.type.startsWith('image/')) {
            const reader = new FileReader();
            reader.onload = function (e) {
                // Only show the "Save" button if the image is different from the saved one
                if (profileImage.src !== e.target.result && savedImageSrc !== e.target.result) {
                    profileImage.src = e.target.result;
                    previousImageSrc = profileImage.src;
                    saveButton.style.display = 'inline-block'; 
                    saveButton.disabled = false; 
                    messageElement.style.display = 'none'; 
                }
            };
            reader.readAsDataURL(file);
        } else {
            // If the file is not an image, show a warning message
            saveButton.style.display = 'none'; 
            messageElement.textContent = 'Please select an image file (jpg, png, gif, etc.)';
            messageElement.style.display = 'block'; 
        }
    } else {
        // If canceled, keep the previously selected image
        profileImage.src = previousImageSrc;

        // Keep the "Save" button visible and functional if an image was previously selected
        if (previousImageSrc !== savedImageSrc) {
            saveButton.style.display = 'inline-block';
            saveButton.disabled = false; // Keep the "Save" button enabled if an image was already selected
        }
    }
});

saveButton.addEventListener('click', async function () {
    const file = fileInput.files[0]; // Only get the file if a new one is selected

    if (file) {
        const formData = new FormData();
        formData.append('file', file); // Append the selected file

        const token = $('[name="__RequestVerificationToken"]').val();
        displaySpinner();
        try {
            const response = await fetch('/Account/ChangeProfilePhoto', {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'RequestVerificationToken': token
                },
                body: formData
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw {
                    status: response.status,
                    message: `${errorData.message}`
                };
            }

            const data = await response.json();
            saveButton.style.display = 'none';
            saveButton.disabled = true;
            savedImageSrc = profileImage.src;
            displayToasterSuccess(data.message);
            return data;

        } catch (error) {
            validateSessionExpiration(error.message, error.status);
            console.error('Network or fetch error:', error);
            return null;
        } finally {
            hideSpinner();
        }
    } else {
        displayToasterError("Please select an image file before saving.");
    }
});

//Personal Info

let idValue = null;
const nameInputPI = getElementById('NamePI');
const lastNameInputPI = getElementById('LastNamePI');
const occupationInputPI = getElementById('OccupationPI');
const phoneNumberInputPI = getElementById('PhoneNumberPI');
const emailInputPI = getElementById('EmailPI');
const saveBtnPI = getElementById('SaveBtnPI');

saveBtnPI.addEventListener('click', async function () {
    await updateProfileInfo();
});


async function getPersonalInfo() {
    return (async () => {
        const url = `/Account/GetProfileInfo`;
        try {
            const response = await fetch(url);
            if (!response.ok) {
                bonuslyCont.innerHTML = cardErrorInfo('Error loading Bonusly info!', 'getBonuslyInfo()');
                const errorData = await response.json();
                throw {
                    status: response.status,
                    message: `${errorData.message}`
                };
            }
            const data = await response.json();
            return data;
        } catch (error) {
            validateSessionExpiration(error.message, error.status);
            throw new Error(`Network error or unable to reach the server. More details: ${error.message}`);
        }
    })();
}

document.addEventListener("DOMContentLoaded", async function () {
    // Bonusly
    displaySpinner();
    await getPersonalInfo()
        .then(data => {
            const profileInfo = data.profileInfo;

            idValue = profileInfo.id;
            nameInputPI.value = profileInfo.name;
            lastNameInputPI.value = profileInfo.lastName;
            occupationInputPI.value = profileInfo.occupation;
            emailInputPI.value = profileInfo.email;
            phoneNumberInputPI.value = profileInfo.phoneNumber;
            profileImage.src = !profileInfo.profileUrl ?
                '/icons/Shared/profile-user.svg' :
                profileInfo.profileUrl;
        })
        .catch(error => {
            console.error(`Failed to load personal info: ${error.message}`);
        }).finally(() => {
            hideSpinner();
        });;
});

async function updateProfileInfo() {
    const validateName = validateRequiredInput(nameInputPI, 'The name is required');
    const validateLastName = validateRequiredInput(lastNameInputPI, 'The last name is required');

    if (!validateName || !validateLastName) {
        return;
    }

    var token = $('[name="__RequestVerificationToken"]').val();

    var data = {
        Id: idValue,
        PhoneNumber: phoneNumberInputPI.value,
        Name: nameInputPI.value,
        LastName: lastNameInputPI.value,
        Occupation: occupationInputPI.value
    };
    displaySpinner();
    try {
        const response = await fetch('/Account/UpdateProfile', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: token
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorData = await response.json();
            switch (errorData.messageType) {
                case "Validation Error":
                    const allErrors = Object.values(errorData.errors).reduce((acc, current) => {
                        return acc.concat(current);
                    }, []);
                    displayToasterWarningArray(allErrors);
                    break;
                default:
                    displayToasterError('An unexpected error occurred: ' + errorData.error);
            }
            return null;
        }

        const dataFromApi = await response.json();
        displayToasterSuccess(dataFromApi.message);
        return dataFromApi;
    } catch (err) {
        validateSessionExpiration(err.message);
        console.error('Network or fetch error:', err);
        return null;
    } finally {
        hideSpinner();
    }
}

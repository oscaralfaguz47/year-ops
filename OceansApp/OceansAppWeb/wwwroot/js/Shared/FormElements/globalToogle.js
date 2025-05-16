
function initGlobalToggles() {
    const toggles = document.querySelectorAll('.global-toggle-container');

    toggles.forEach(toggle => {
        const option1 = toggle.querySelector('.global-toggle-opt1');
        const option2 = toggle.querySelector('.global-toggle-opt2');
        const hiddenInput = toggle.querySelector('.global-toggle-hidden-input');

        // Set default value based on initial active option without triggering events
        if (option1.classList.contains('active')) {
            hiddenInput.value = 1;
        } else if (option2.classList.contains('active')) {
            hiddenInput.value = 2;
        }

        toggle.addEventListener('click', () => {
            toggle.classList.toggle('active');

            if (toggle.classList.contains('active')) {
                option1.classList.remove('active');
                option2.classList.add('active');
                hiddenInput.value = 2;
            } else {
                option2.classList.remove('active');
                option1.classList.add('active');
                hiddenInput.value = 1;
            }

            // Trigger the change event manually
            hiddenInput.dispatchEvent(new Event('input'));
        });
    });
}



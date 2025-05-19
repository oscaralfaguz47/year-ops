function initGlobalToggles() {
    const toggles = document.querySelectorAll('.global-toggle-container');

    toggles.forEach(toggle => {
        const option1 = toggle.querySelector('.global-toggle-opt1');
        const option2 = toggle.querySelector('.global-toggle-opt2');
        const slider = toggle.querySelector('.global-toggle-slider');
        const hiddenInput = toggle.querySelector('.global-toggle-hidden-input');

        // Set default active state
        const activeOption = toggle.querySelector('.global-toggle-option.active');
        hiddenInput.value = activeOption.classList.contains('global-toggle-opt1') ? 1 : 2;

        // Ensure the slider is sized correctly on load, even if hidden
        adjustSliderWidth(toggle, slider);

        // Remove existing click event listener if present
        toggle.removeEventListener('click', toggle._clickListener);

        // Define and add the click event listener
        toggle._clickListener = () => {
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

            // Adjust slider after the transition
            adjustSliderWidth(toggle, slider);
            hiddenInput.dispatchEvent(new Event('input'));
        };

        toggle.addEventListener('click', toggle._clickListener);

        // Adjust slider width on window resize
        window.addEventListener('resize', () => adjustSliderWidth(toggle, slider));

        // Force adjustment if toggle becomes visible later
        const observer = new MutationObserver(() => {
            if (toggle.offsetParent !== null) {
                adjustSliderWidth(toggle, slider);
            }
        });

        observer.observe(toggle, { attributes: true, childList: true, subtree: true });
    });
}

function adjustSliderWidth(toggle, slider) {
    const activeOption = toggle.querySelector('.global-toggle-option.active');

    // Use requestAnimationFrame to ensure styles are fully applied
    requestAnimationFrame(() => {
        slider.style.width = `${activeOption.offsetWidth}px`;
        slider.style.left = `${activeOption.offsetLeft}px`;
    });
}
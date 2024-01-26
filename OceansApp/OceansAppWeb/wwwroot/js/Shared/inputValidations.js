function validateInputTypeNumber(inputElementId) {
        var input = document.getElementById(inputElementId);

        input.addEventListener('input', function (e) {
            var value = input.value;
            // Remove invalid chars
            if (/[^0-9.]+/.test(value)) {
                input.value = value.replace(/[^0-9.]+/, '');
            }

            // Adjust the decimals if they enter more than 2
            if (value.includes('.')) {
                var parts = value.split('.');
                if (parts[1].length > 2) {
                    input.value = parseFloat(value).toFixed(2);
                }
            }
        });

        input.addEventListener('keydown', function (e) {
            // Allow only number, dots and control keys
            if ((e.key >= '0' && e.key <= '9') ||
                e.key === '.' || e.key === 'Backspace' ||
                e.key === 'ArrowLeft' || e.key === 'ArrowRight' ||
                e.key === 'Tab') {
                return;
            } else {
                e.preventDefault();
            }
        });
}

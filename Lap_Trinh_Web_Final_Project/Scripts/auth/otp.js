// 6 ô nhập OTP: tự nhảy ô, xóa lùi, dán cả mã, rồi gộp lại vào ô ẩn khi gửi form.
(function () {
    var otpBoxes = document.querySelectorAll('#otpInputs input');
    var otpForm = document.getElementById('otpForm');

    otpBoxes.forEach(function (box, index) {
        box.addEventListener('input', function () {
            box.value = box.value.replace(/\D/g, '').slice(0, 1);

            if (box.value && index < otpBoxes.length - 1) {
                otpBoxes[index + 1].focus();
            }
        });

        box.addEventListener('keydown', function (event) {
            if (event.key === 'Backspace' && !box.value && index > 0) {
                otpBoxes[index - 1].focus();
            }
            if (event.key === 'ArrowLeft' && index > 0) {
                otpBoxes[index - 1].focus();
            }
            if (event.key === 'ArrowRight' && index < otpBoxes.length - 1) {
                otpBoxes[index + 1].focus();
            }
        });

        box.addEventListener('paste', function (event) {
            var code = event.clipboardData
                .getData('text')
                .replace(/\D/g, '')
                .slice(0, otpBoxes.length);

            if (!code) {
                return;
            }

            event.preventDefault();

            code.split('').forEach(function (digit, digitIndex) {
                otpBoxes[digitIndex].value = digit;
            });

            otpBoxes[Math.min(code.length, otpBoxes.length) - 1].focus();
        });
    });

    otpForm.addEventListener('submit', function () {
        var code = '';
        otpBoxes.forEach(function (box) { code += box.value; });
        document.getElementById('otpValue').value = code;
    });
})();
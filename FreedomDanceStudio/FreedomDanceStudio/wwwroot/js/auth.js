document.addEventListener('DOMContentLoaded', function() {
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');
    const tabs = document.querySelectorAll('.auth-tab');

    let currentForm = 'login';

    // Переключение между формами
    tabs.forEach(tab => {
        tab.addEventListener('click', function() {
            const tabType = this.getAttribute('data-tab');

            tabs.forEach(t => t.classList.remove('active'));
            this.classList.add('active');

            document.querySelectorAll('.auth-form').forEach(form => {
                form.classList.remove('active');
            });
            document.getElementById(`${tabType}Form`).classList.add('active');
            currentForm = tabType;
        });
    });

    // Валидация форм
    function validateForm(form) {
        let isValid = true;
        const inputs = form.querySelectorAll('input[required]');

        inputs.forEach(input => {
            if (!input.value.trim()) {
                input.classList.add('is-invalid');
                isValid = false;
            } else {
                input.classList.remove('is-invalid');
            }
        });

        // Дополнительная проверка для регистрации
        if (form.id === 'registerForm') {
            const password = document.getElementById('regPassword').value;
            const confirmPassword = document.getElementById('confirmPassword').value;

            if (password !== confirmPassword) {
                document.getElementById('confirmPassword')
                    .classList.add('is-invalid');
                isValid = false;
            }
        }

        return isValid;
    }

    // Обработчики отправки форм
    loginForm.addEventListener('submit', function(e) {
        if (!validateForm(this)) {
            e.preventDefault();
            document.body.classList.add('shake-error');
            setTimeout(() => document.body.classList.remove('shake-error'), 500);
        }
    });

    registerForm.addEventListener('submit', function(e) {
        if (!validateForm(this)) {
            e.preventDefault();
            document.body.classList.add('shake-error');
            setTimeout(() => document.body.classList.remove('shake-error'), 500);
        }
    });

    // Автофокус на первом поле
    document.querySelector('.auth-input').focus();
});

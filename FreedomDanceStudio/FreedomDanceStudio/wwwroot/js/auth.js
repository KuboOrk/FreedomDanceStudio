document.addEventListener('DOMContentLoaded', function() {
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');
    const tabs = document.querySelectorAll('.auth-tab');
    const phoneInput = document.getElementById('phone');
    const emailInput = document.getElementById('email'); // Получаем поле email

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
            const value = input.value.trim();
            if (!value) {
                input.classList.add('is-invalid');
                isValid = false;
            } else {
                input.classList.remove('is-invalid');
            }
        });

        // Дополнительная проверка для регистрации
        if (form.id === 'registerForm') {
            const password = document.getElementById('regPassword').value.trim();
            const confirmPassword = document.getElementById('confirmPassword').value.trim();

            if (password !== confirmPassword) {
                const confirmInput = document.getElementById('confirmPassword');
                confirmInput.classList.add('is-invalid');
                document.getElementById('confirmPasswordError').style.display = 'block';
                isValid = false;
            } else {
                document.getElementById('confirmPasswordError').style.display = 'none';
            }
        }

        return isValid;
    }

    // Обработчики отправки форм
    if (loginForm) {
        loginForm.addEventListener('submit', function(e) {
            if (!validateForm(this)) {
                e.preventDefault();
                document.body.classList.add('shake-error');
                setTimeout(() => document.body.classList.remove('shake-error'), 500);
            }
        });
    }

    if (registerForm) {
        registerForm.addEventListener('submit', function(e) {
            if (!validateForm(this)) {
                e.preventDefault();
                document.body.classList.add('shake-error');
                setTimeout(() => document.body.classList.remove('shake-error'), 500);
            }
        });
    }

    // Автофокус на первом поле
    const firstInput = document.querySelector('.auth-input');
    if (firstInput) {
        firstInput.focus();
    }

    // Мгновенная валидация при вводе (опционально)
    const requiredInputs = document.querySelectorAll('input[required]');
    requiredInputs.forEach(input => {
        input.addEventListener('input', () => {
            if (input.value.trim()) {
                input.classList.remove('is-invalid');
            }
        });
    });

    // Форматирование номера телефона
    function formatPhone(value) {
        // Удаляем все символы, кроме цифр
        const digits = value.replace(/\D/g, '');

        // Если цифр меньше 10, возвращаем очищенное значение
        if (digits.length < 10) return digits;

        // Формируем номер в нужном формате
        const formatted = '+7 (' +
            digits.substr(0, 3) + ') ' +
            digits.substr(3, 3) + '-' +
            digits.substr(6, 2) + '-' +
            digits.substr(8, 2);

        return formatted;
    }

    // Обработчик ввода для поля телефона
    if (phoneInput) {
        phoneInput.addEventListener('input', function(e) {
            const currentValue = this.value;
            const formattedValue = formatPhone(currentValue);

            // Обновляем значение поля, если длина строки увеличилась
            if (formattedValue.length > currentValue.length) {
                this.value = formattedValue;
            }
        });
    }

    // Мгновенная проверка email при вводе
    if (emailInput) {
        emailInput.addEventListener('input', function() {
            const value = this.value.trim();

            // Регулярное выражение для базовой проверки email
            const emailRegex = /^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$/i;
            const isValid = emailRegex.test(value);

            if (value && !isValid) {
                this.classList.add('is-invalid');
                // Можно добавить кастомное сообщение под полем (если есть контейнер)
            } else {
                this.classList.remove('is-invalid');
            }
        });
    }
});
/**
 * Обрабатывает отправку формы с проверкой валидности
 */
document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('abonnementForm');
    const maxVisitsInput = document.getElementById('@Html.IdFor(m => m.MaxVisits)');
    const validationModalEl = document.getElementById('validationModal');
    const validationMessageEl = document.getElementById('validationMessage');

    // Инициализация модального окна Bootstrap
    const validationModal = new bootstrap.Modal(validationModalEl);

    // Обработчик отправки формы
    form.addEventListener('submit', function(event) {
        // Проверяем общую валидность формы
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();

            // Проверяем конкретное поле MaxVisits
            if (!maxVisitsInput.value || maxVisitsInput.value.trim() === '') {
                validationMessageEl.textContent = 'Поле "Максимальное количество посещений" обязательно для заполнения.';
            } else {
                validationMessageEl.textContent = 'Пожалуйста, заполните все обязательные поля.';
            }

            // Показываем модальное окно
            validationModal.show();
        }

        // Добавляем стили для индикации ошибок
        form.classList.add('was-validated');
    });
});

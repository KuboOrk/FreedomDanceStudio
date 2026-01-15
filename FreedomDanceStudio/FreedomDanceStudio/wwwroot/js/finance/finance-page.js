// Обновление статистики при фильтрации
function refreshFinanceStats() {
    const startDate = document.querySelector('input[name="startDate"]').value;
    const endDate = document.querySelector('input[name="endDate"]').value;

    fetch(`/Finance?startDate=${startDate}&endDate=${endDate}`)
        .then(response => response.text())
        .then(html => {
            document.querySelector('.container.mt-4').innerHTML =
                new DOMParser().parseFromString(html, 'text/html')
                    .querySelector('.container.mt-4').innerHTML;
        });
}

// Автообновление при изменении фильтров
document.querySelectorAll('input[type="date"]').forEach(input => {
    input.addEventListener('change', refreshFinanceStats);
});

// Обработчик открытия модального окна
// Ждём полной загрузки DOM
document.addEventListener('DOMContentLoaded', function() {
    // Находим все кнопки удаления
    const deleteButtons = document.querySelectorAll('[data-bs-toggle="modal"]');

    const modal = new bootstrap.Modal(document.getElementById('deleteModal'));

    // Обработчик для каждой кнопки
    deleteButtons.forEach(button => {
        button.addEventListener('click', function() {
            try {
                // Получаем данные из атрибутов кнопки
                const transactionId = this.getAttribute('data-transaction-id');
                const amount = this.getAttribute('data-transaction-amount');
                const type = this.getAttribute('data-transaction-type');

                // Проверяем существование элементов перед заполнением
                const idElement = document.getElementById('modalTransactionId');
                const amountElement = document.getElementById('modalTransactionAmount');
                const typeElement = document.getElementById('modalTransactionType');
                const hiddenId = document.getElementById('hiddenTransactionId');

                if (idElement && amountElement && typeElement && hiddenId) {
                    // Заполняем ID и сумму
                    idElement.textContent = transactionId;
                    amountElement.textContent = amount;

                    // Заполняем тип с правильной стилизацией
                    typeElement.textContent = type;
                    typeElement.className = 'badge ' + (type === 'Income' ? 'bg-success' : 'bg-danger');

                    // Устанавливаем ID в скрытое поле формы
                    hiddenId.value = transactionId;

                    // Открываем модальное окно
                    modal.show();
                } else {
                    console.error('Не удалось найти элементы модального окна');
                }
            } catch (error) {
                console.error('Ошибка при открытии модального окна:', error);
            }
        });
    });

    // Очистка при закрытии модального окна
    document.getElementById('deleteModal').addEventListener('hidden.bs.modal', function () {
        const idElement = document.getElementById('modalTransactionId');
        const amountElement = document.getElementById('modalTransactionAmount');
        const typeElement = document.getElementById('modalTransactionType');
        const hiddenId = document.getElementById('hiddenTransactionId');

        if (idElement && amountElement && typeElement && hiddenId) {
            idElement.textContent = '-';
            amountElement.textContent = '-';
            typeElement.textContent = '';
            typeElement.className = 'badge';
            hiddenId.value = '';
        }
    });
});
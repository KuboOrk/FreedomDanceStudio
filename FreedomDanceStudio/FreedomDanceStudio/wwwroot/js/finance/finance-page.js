// Обновление статистики при фильтрации
function refreshFinanceStats() {
    const startDateInput = document.querySelector('input[name="startDate"]');
    const endDateInput = document.querySelector('input[name="endDate"]');

    let startDate, endDate;

    // Если поля пустые — используем текущий месяц в локальном времени
    if (!startDateInput.value) {
        const now = new Date();
        startDate = new Date(now.getFullYear(), now.getMonth(), 1); // Первый день месяца
    } else {
        startDate = new Date(startDateInput.value);
    }

    if (!endDateInput.value) {
        const now = new Date();
        const year = now.getFullYear();
        const month = now.getMonth();
        const daysInMonth = new Date(year, month + 1, 0).getDate();
        endDate = new Date(year, month, daysInMonth); // Последний день месяца
    } else {
        endDate = new Date(endDateInput.value);
    }

    // Конвертируем в UTC для передачи в запрос
    const utcStartDate = new Date(Date.UTC(
        startDate.getFullYear(),
        startDate.getMonth(),
        startDate.getDate()
    ));
    const utcEndDate = new Date(Date.UTC(
        endDate.getFullYear(),
        endDate.getMonth(),
        endDate.getDate(),
        23, 59, 59 // Включаем весь последний день
    ));

    // Формируем строку запроса с датами в формате ISO 8601 (UTC)
    fetch(`/Finance?startDate=${utcStartDate.toISOString().split('T')[0]}&endDate=${utcEndDate.toISOString().split('T')[0]}`)
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
    // Находим ТОЛЬКО кнопки удаления с классом .delete-button
    const deleteButtons = document.querySelectorAll('.delete-button');
    const modal = new bootstrap.Modal(document.getElementById('deleteModal'));

    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            // Гарантируем, что это именно кнопка удаления
            if (!this.classList.contains('delete-button')) return;

            e.preventDefault(); // Предотвращаем побочные эффекты

            try {
                const transactionId = this.getAttribute('data-transaction-id');
                const amount = this.getAttribute('data-transaction-amount');
                const type = this.getAttribute('data-transaction-type');

                // Заполняем поля модального окна
                document.getElementById('modalTransactionId').textContent = transactionId;
                document.getElementById('modalTransactionAmount').textContent = amount;
                const typeElement = document.getElementById('modalTransactionType');
                typeElement.textContent = type;
                typeElement.className = 'badge ' + (type === 'Income' ? 'bg-success' : 'bg-danger');
                document.getElementById('hiddenTransactionId').value = transactionId;

                // Открываем ТОЛЬКО окно удаления
                modal.show();
            } catch (error) {
                console.error('Ошибка при открытии модального окна:', error);
            }
        });
    });

    // Очистка при закрытии модального окна удаления
    document.getElementById('deleteModal').addEventListener('hidden.bs.modal', function () {
        document.getElementById('modalTransactionId').textContent = '-';
        document.getElementById('modalTransactionAmount').textContent = '-';
        const typeElement = document.getElementById('modalTransactionType');
        typeElement.textContent = '';
        typeElement.className = 'badge';
        document.getElementById('hiddenTransactionId').value = '';
    });
});
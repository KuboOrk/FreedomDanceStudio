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
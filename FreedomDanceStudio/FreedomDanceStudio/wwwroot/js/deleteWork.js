document.addEventListener('DOMContentLoaded', function() {
    const deleteButtons = document.querySelectorAll('[data-bs-toggle="modal"]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function() {
            // Получаем данные из атрибутов кнопки
            const id = this.getAttribute('data-id');
            const employee = this.getAttribute('data-employee');
            const date = this.getAttribute('data-date');

            // Заполняем модальное окно данными
            document.getElementById('modalId').value = id;
            document.getElementById('modalEmployee').textContent = employee;
            document.getElementById('modalDate').textContent = date;
        });
    });
});
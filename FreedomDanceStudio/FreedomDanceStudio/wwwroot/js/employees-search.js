/**
 * Реализует AJAX‑поиск сотрудников в реальном времени с обработкой состояний
 */
document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    const tableBody = document.getElementById('employeesTableBody');
    const noResultsMessage = document.getElementById('noResultsMessage');
    const loadingIndicator = document.createElement('div');
    let searchTimeout;

    // Настройка индикатора загрузки
    loadingIndicator.className = 'text-center text-muted my-3';
    loadingIndicator.textContent = 'Поиск...';
    loadingIndicator.style.display = 'none';
    tableBody.parentNode.insertBefore(loadingIndicator, tableBody);

    /**
     * Выполняет AJAX‑запрос для поиска сотрудников
     */
    async function performSearch(searchTerm) {
        // Показываем индикатор загрузки
        loadingIndicator.style.display = 'block';
        tableBody.style.opacity = '0.6';

        try {
            const url = `/Employees/Search?search=${encodeURIComponent(searchTerm)}`;
            const response = await fetch(url);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const employees = await response.json();
            updateTable(employees);
        } catch (error) {
            console.error('Ошибка при поиске:', error);
            showError('Не удалось выполнить поиск. Проверьте подключение к сети.');
            // В случае ошибки оставляем текущую таблицу без изменений
            tableBody.style.opacity = '1';
            loadingIndicator.style.display = 'none';
        } finally {
            // Скрываем индикатор в любом случае
            loadingIndicator.style.display = 'none';
            tableBody.style.opacity = '1';
        }
    }

    /**
     * Обновляет таблицу с результатами поиска
     */
    function updateTable(employees) {
        // Очищаем таблицу
        tableBody.innerHTML = '';

        if (employees.length === 0) {
            noResultsMessage.style.display = 'block';
            return;
        }

        noResultsMessage.style.display = 'none';

        // Заполняем таблицу новыми данными
        employees.forEach(employee => {
            const row = document.createElement('tr');

            row.innerHTML = `
                <td>${employee.firstName} ${employee.lastName}</td>
                <td>${employee.phone || '-'}</td>
                <td>${employee.email || '-'}</td>
                <td>${employee.salary} ₽</td>
                <td class="text-end">
                    <a href="/Employees/Edit/${employee.id}"
               class="btn btn-sm btn-outline-primary me-1">Редактировать</a>
            <form action="/Employees/Delete" method="post" style="display:inline;">
                <input type="hidden" name="id" value="${employee.id}"/>
            <button type="submit" class="btn btn-sm btn-outline-danger"
            onclick="return confirm('Удалить сотрудника ${employee.firstName} ${employee.lastName}?')">Удалить</button>
        </form>
    </td>`;


            tableBody.appendChild(row);
        });
    }

    /**
     * Показывает сообщение об ошибке
     */
    function showError(message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger';
        errorDiv.textContent = message;
        errorDiv.style.position = 'fixed';
        errorDiv.style.top = '20px';
        errorDiv.style.right = '20px';
        errorDiv.style.zIndex = '1050';

        document.body.appendChild(errorDiv);

        setTimeout(() => {
            errorDiv.remove();
        }, 5000);
    }

    // Обработчик ввода с задержкой для оптимизации
    searchInput.addEventListener('input', function(e) {
        const searchTerm = e.target.value.trim();

        // Очищаем предыдущий таймер
        clearTimeout(searchTimeout);

        // Запускаем поиск через 300 мс после последнего ввода
        searchTimeout = setTimeout(() => {
            performSearch(searchTerm);
        }, 300);
    });

    // Инициализация — показываем все строки при загрузке (если нет поиска)
    if (!searchInput.value) {
        performSearch('');
    }
});

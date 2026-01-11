/**
 * Реализует AJAX‑поиск клиентов без разрушения форм удаления
 */
document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    const tableBody = document.getElementById('clientsTableBody');
    const noResultsMessage = document.getElementById('noResultsMessage');
    const loadingIndicator = document.createElement('div');
    let searchTimeout;

    // Настройка индикатора загрузки
    loadingIndicator.className = 'text-center text-muted my-3';
    loadingIndicator.textContent = 'Поиск...';
    loadingIndicator.style.display = 'none';
    tableBody.parentNode.insertBefore(loadingIndicator, tableBody);

    /**
     * Выполняет AJAX‑запрос для поиска клиентов
     */
    async function performSearch(searchTerm) {
        loadingIndicator.style.display = 'block';
        tableBody.style.opacity = '0.6';

        try {
            const url = `/Clients/Search?search=${encodeURIComponent(searchTerm)}`;
            const response = await fetch(url);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const clients = await response.json();
            updateTableVisibility(clients);
        } catch (error) {
            console.error('Ошибка при поиске:', error);
            showError('Не удалось выполнить поиск. Проверьте подключение к сети.');
            tableBody.style.opacity = '1';
            loadingIndicator.style.display = 'none';
        } finally {
            loadingIndicator.style.display = 'none';
            tableBody.style.opacity = '1';
        }
    }

    /**
     * Обновляет видимость строк и текст в текстовых ячейках (не трогает формы)
     */
    function updateTableVisibility(clients) {
        const allRows = tableBody.querySelectorAll('tr');
        const clientIds = new Set(clients.map(c => c.id));

        // Скрываем все строки
        allRows.forEach(row => {
            row.style.display = 'none';
        });

        if (clients.length === 0) {
            noResultsMessage.style.display = 'block';
            return;
        }

        noResultsMessage.style.display = 'none';

        // Показываем совпадающие строки и обновляем текст ТОЛЬКО в текстовых ячейках
        clients.forEach(client => {
            const row = tableBody.querySelector(`tr[data-client-id="${client.id}"]`);
            if (row) {
                row.style.display = '';
                // Обновляем текст только в первых четырёх ячейках (без действий)
                const textCells = row.querySelectorAll('td:not(:last-child)');
                textCells[0].textContent = client.firstName;
                textCells[1].textContent = client.lastName;
                textCells[2].textContent = client.phone;
                textCells[3].textContent = client.email || '-';
            }
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
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            performSearch(searchTerm);
        }, 300);
    });

    // Инициализация — показываем все строки при загрузке (если нет поиска)
    if (!searchInput.value) {
        performSearch('');
    }
});

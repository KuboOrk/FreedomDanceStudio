/**
 * Реализует AJAX‑поиск абонементов без разрушения форм удаления
 */
document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    const tableBody = document.getElementById('servicesTableBody');
    const noResultsMessage = document.getElementById('noResultsMessage');
    const loadingIndicator = document.createElement('div');
    let searchTimeout;

    // Проверка существования обязательных элементов
    if (!searchInput || !tableBody || !noResultsMessage) {
        console.error('Критические элементы DOM не найдены');
        return;
    }

    // Настройка индикатора загрузки
    loadingIndicator.className = 'text-center text-muted my-3';
    loadingIndicator.textContent = 'Поиск...';
    loadingIndicator.style.display = 'none';
    tableBody.parentNode.insertBefore(loadingIndicator, tableBody);

    /**
     * Выполняет AJAX‑запрос для поиска абонементов
     */
    async function performSearch(searchTerm) {
        loadingIndicator.style.display = 'block';
        tableBody.style.opacity = '0.6';

        try {
            const url = `/Services/Search?search=${encodeURIComponent(searchTerm)}`;
            const response = await fetch(url);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const services = await response.json();
            updateTableVisibility(services);
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
    function updateTableVisibility(services) {
        const allRows = tableBody.querySelectorAll('tr');
        const serviceIds = new Set(services.map(s => s.id));

        // Скрываем все строки
        allRows.forEach(row => {
            row.style.display = 'none';
        });

        if (services.length === 0) {
            noResultsMessage.style.display = 'block';
            return;
        }

        noResultsMessage.style.display = 'none';

        // Показываем совпадающие строки и обновляем текст ТОЛЬКО в текстовых ячейках
        services.forEach(service => {
            const row = tableBody.querySelector(`tr[data-service-id="${service.id}"]`);
            if (row) {
                row.style.display = '';
                // Обновляем текст только в первых четырёх ячейках (без действий)
                const textCells = row.querySelectorAll('td:not(:last-child)');
                textCells[0].textContent = service.name;
                textCells[1].textContent = service.description || '-';
                textCells[2].textContent = `${service.price.toLocaleString('ru-RU')} ₽`;
                textCells[3].textContent = service.durationDays;
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

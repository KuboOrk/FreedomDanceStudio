/**
 * Реализует AJAX‑поиск продаж абонементов без разрушения форм и обработчиков
 */
document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    const tableBody = document.getElementById('salesTableBody');
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
     * Выполняет AJAX‑запрос для поиска продаж абонементов
     */
    async function performSearch(searchTerm) {
        loadingIndicator.style.display = 'block';
        tableBody.style.opacity = '0.6';

        try {
            const url = `${window.location.origin}/AbonnementSales/Search?search=${encodeURIComponent(searchTerm)}`;
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    throw new Error('Метод поиска не найден на сервере');
                } else if (response.status === 500) {
                    const errorData = await response.json();
                    throw new Error(`Ошибка сервера: ${errorData.error || 'Неизвестная ошибка'}`);
                }
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const sales = await response.json();
            updateTableVisibility(sales);
        } catch (error) {
            console.error('Ошибка поиска:', error);
            showError(`Не удалось выполнить поиск: ${error.message}`);
            tableBody.style.opacity = '1';
            loadingIndicator.style.display = 'none';
        } finally {
            loadingIndicator.style.display = 'none';
            tableBody.style.opacity = '1';
        }
    }

    /**
     * Обновляет видимость строк и текст в текстовых ячейках (не трогает формы и кнопки)
     */
    /**
     * Обновляет видимость строк и текст в текстовых ячейках (не трогает формы и кнопки)
     */
    function updateTableVisibility(sales) {
        const allRows = tableBody.querySelectorAll('tr');
        const saleIds = new Set(sales.map(s => s.id));

        // Скрываем все строки
        allRows.forEach(row => {
            row.style.display = 'none';
        });

        if (sales.length === 0) {
            noResultsMessage.style.display = 'block';
            return;
        }

        noResultsMessage.style.display = 'none';

        // Показываем совпадающие строки и обновляем текст ТОЛЬКО в текстовых ячейках
        sales.forEach(sale => {
            const row = tableBody.querySelector(`tr[data-sale-id="${sale.id}"]`);
            if (row) {
                row.style.display = '';
                // Обновляем текст начиная с первой ячейки (теперь это «Клиент»)
                const textCells = row.querySelectorAll('td:not(:nth-last-child(-n+2))');
                textCells[0].textContent = sale.clientName;     // Клиент (было textCells[1])
                textCells[1].textContent = sale.serviceName;   // Услуга (было textCells[2])
                textCells[2].textContent = sale.saleDate;     // Дата продажи (было textCells[3])
                textCells[3].textContent = sale.startDate;    // Начало действия (было textCells[4])
                textCells[4].textContent = sale.endDate;     // Окончание действия (было textCells[5])

                // Обновляем счётчик посещений в 6‑й ячейке (индекс 5)
                const visitBadge = row.querySelector('.badge.bg-primary');
                if (visitBadge) {
                    visitBadge.textContent = sale.visitCount;
                }

                // Сохраняем атрибуты данных для логики посещений
                row.setAttribute('data-max-visits', sale.maxVisits);
                row.setAttribute('data-visit-count', sale.visitCount);
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

        setTimeout(() => errorDiv.remove(), 5000);
    }

    // Обработчик ввода с задержкой для оптимизации
    searchInput.addEventListener('input', function(e) {
        const searchTerm = e.target.value.trim();
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => performSearch(searchTerm), 300);
    });

    // Инициализация — показываем все строки при загрузке (если нет поиска)
    if (!searchInput.value) {
        performSearch('');
    }
});

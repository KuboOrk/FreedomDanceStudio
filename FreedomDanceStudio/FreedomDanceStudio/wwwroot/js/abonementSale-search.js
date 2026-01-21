/**
 * Реализует AJAX‑поиск продаж абонементов без разрушения форм и обработчиков
 */
document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    const tableBody = document.getElementById('salesTableBody');
    const noResultsMessage = document.getElementById('noResultsMessage');
    let searchTimeout;

    // Проверка существования обязательных элементов
    if (!searchInput || !tableBody || !noResultsMessage) {
        console.error('Критические элементы DOM не найдены');
        return;
    }

    // Настройка индикатора загрузки
    const loadingIndicator = document.createElement('div');
    loadingIndicator.className = 'text-center text-muted my-3';
    loadingIndicator.textContent = 'Поиск...';
    loadingIndicator.style.display = 'none';
    // ИСПРАВЛЕНО: используем корректный метод вставки
    tableBody.parentElement.insertAdjacentElement('beforeBegin', loadingIndicator);


    /**
     * Выполняет AJAX‑запрос для поиска продаж абонементов
     */
    async function performSearch(searchTerm, page = 1) {
        loadingIndicator.style.display = 'block';
        tableBody.style.opacity = '0.6';

        try {
            const url = `${window.location.origin}/AbonnementSales/Search?search=${encodeURIComponent(searchTerm)}&page=${page}`;
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            // ИСПРАВЛЕНО: ключи в camelCase согласно ответу сервера
            if (!data || !Array.isArray(data.sales) || !data.pagination) {
                console.error('Некорректный формат ответа от сервера:', data);
                showError('Сервер вернул некорректные данные. Попробуйте позже.');
                return;
            }

            updateTableVisibility(data.sales, data.pagination); // camelCase
        } catch (error) {
            console.error('Ошибка поиска:', error);
            showError(`Не удалось выполнить поиск: ${error.message}`);
        } finally {
            loadingIndicator.style.display = 'none';
            tableBody.style.opacity = '1';
        }
    }

    /**
     * Обновляет видимость строк и текст в текстовых ячейках (не трогает формы и кнопки)
     */
    function updateTableVisibility(sales, pagination) {
        // ИСПРАВЛЕНО: защита от undefined/null
        if (!sales || !Array.isArray(sales)) {
            console.warn('Данные продаж отсутствуют или некорректны');
            tableBody.innerHTML = '';
            noResultsMessage.style.display = 'block';
            updatePagination(null);
            return;
        }

        // Очищаем таблицу
        tableBody.innerHTML = '';

        if (sales.length === 0) {
            noResultsMessage.style.display = 'block';
            updatePagination(pagination);
            return;
        }

        noResultsMessage.style.display = 'none';

        sales.forEach(sale => {
            const newRow = createTableRow(sale);
            tableBody.appendChild(newRow);
        });

        updatePagination(pagination);
    }

    function updatePagination(pagination) {
        const paginationContainer = document.querySelector('.pagination-container');
        if (!paginationContainer) {
            console.warn('.pagination-container не найден в DOM');
            return;
        }

        let html = '<ul class="pagination">';

        if (pagination && pagination.hasPreviousPage) { // camelCase
            html += `<li class="page-item"><a href="#" class="page-link" data-page="${pagination.currentPage - 1}">Назад</a></li>`;
        } else {
            html += '<li class="page-item disabled"><span class="page-link">Назад</span></li>';
        }

        if (pagination) {
            for (let i = 1; i <= pagination.totalPages; i++) { // camelCase
                if (i === pagination.currentPage) { // camelCase
                    html += `<li class="page-item active"><span class="page-link">${i}</span></li>`;
                } else {
                    html += `<li class="page-item"><a href="#" class="page-link" data-page="${i}">${i}</a></li>`;
                }
            }
        }

        if (pagination && pagination.hasNextPage) { // camelCase
            html += `<li class="page-item"><a href="#" class="page-link" data-page="${pagination.currentPage + 1}">Вперед</a></li>`;
        } else {
            html += '<li class="page-item disabled"><span class="page-link">Вперед</span></li>';
        }

        html += '</ul>';
        paginationContainer.innerHTML = html;

        document.querySelectorAll('.page-link').forEach(link => {
            link.addEventListener('click', function(e) {
                e.preventDefault();
                const page = parseInt(this.getAttribute('data-page'), 10);
                performSearch(searchInput.value.trim(), page);
            });
        });
    }


    /**
     * Создаёт новую строку таблицы для найденной продажи абонемента
     * @param {Object} sale - данные продажи из AJAX‑ответа
     * @returns {HTMLTableRowElement} - готовая строка таблицы
     */
    function createTableRow(sale) {
        const row = document.createElement('tr');
        row.setAttribute('data-sale-id', sale.id);
        row.setAttribute('data-max-visits', sale.maxVisits);
        row.setAttribute('data-visit-count', sale.visitCount);

        // Экранируем текст для защиты от XSS
        const safeClientName = escapeHtml(sale.clientName || 'Клиент не найден');
        const safeServiceName = escapeHtml(sale.serviceName || 'Услуга не найдена');

        row.innerHTML = `
        <td>${safeClientName}</td>
        <td>${safeServiceName}</td>
        <td>${escapeHtml(sale.saleDate)}</td>
        <td>${escapeHtml(sale.startDate)}</td>
        <td>${escapeHtml(sale.endDate)}</td>
        <td>
            <span class="badge bg-primary">${sale.visitCount}</span>
            <button type="button"
                    class="btn btn-sm btn-outline-info view-history-btn"
                    data-abonnement-id="${sale.id}"
            title="Посмотреть историю посещений">
                <i class="bi bi-clock-history"></i> История
            </button>
        </td>
        <td class="text-end">
            <a href="/AbonnementSales/Edit/${sale.id}"
               class="btn btn-sm btn-outline-primary me-1">Редактировать</a>
            <button type="button"
                    class="btn btn-sm btn-outline-success mark-visit-btn"
            onclick="markVisitAndRefresh(${sale.id})"
            data-abonnement-id="${sale.id}"
            title="Отметить посещение клиента">
                <i class="bi bi-calendar-check"></i> Отметить
            </button>
            <form action="/AbonnementSales/Delete" method="post" style="display:inline;">
                <input type="hidden" name="id" value="${sale.id}"/>
                <button type="submit" class="btn btn-sm btn-outline-danger"
                        onclick="return confirm('Удалить запись?')">Удалить</button>
            </form>
        </td>`;


        return row;
    }

    /**
     * Экранирует специальные символы HTML для безопасного вывода
     * @param {string} text - исходный текст
     * @returns {string} - экранированный текст
     */
    function escapeHtml(text) {
        if (typeof text !== 'string') return '';
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, m => map[m]);
    }

    /**
     * Показывает сообщение об ошибке
     * @param {string} message - текст ошибки для отображения
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
    searchInput.addEventListener('input', function (e) {
        const searchTerm = e.target.value.trim();
        clearTimeout(searchTimeout);

        if (searchTerm === '') {
            // Возвращаемся к стандартному представлению Index
            window.location.href = '/AbonnementSales';
        } else {
            searchTimeout = setTimeout(() => performSearch(searchTerm), 300);
        }
    });
});

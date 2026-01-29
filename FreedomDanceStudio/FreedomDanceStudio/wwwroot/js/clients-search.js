/**
 * Реализует AJAX‑поиск клиентов с пагинацией и корректной работой форм удаления
 */
document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    const tableBody = document.getElementById('clientsTableBody');
    const noResultsMessage = document.getElementById('noResultsMessage');
    const paginationContainer = document.querySelector('.pagination-container');
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
    tableBody.parentElement.insertAdjacentElement('beforeBegin', loadingIndicator);

    /**
     * Выполняет AJAX‑запрос для поиска клиентов
     */
    async function performSearch(searchTerm, page = 1) {
        loadingIndicator.style.display = 'block';
        tableBody.style.opacity = '0.6';

        try {
            const url = `${window.location.origin}/Clients/Search?search=${encodeURIComponent(searchTerm)}&page=${page}`;
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

            if (!data || !Array.isArray(data.clients) || !data.pagination) {
                console.error('Некорректный формат ответа от сервера:', data);
                showError('Сервер вернул некорректные данные. Попробуйте позже.');
                return;
            }

            updateTableVisibility(data.clients, data.pagination);
        } catch (error) {
            console.error('Ошибка поиска:', error);
            showError(`Не удалось выполнить поиск: ${error.message}`);
        } finally {
            loadingIndicator.style.display = 'none';
            tableBody.style.opacity = '1';
        }
    }

    /**
     * Обновляет видимость строк и перепривязывает обработчики
     */
    function updateTableVisibility(clients, pagination) {
        if (!clients || !Array.isArray(clients)) {
            console.warn('Данные клиентов отсутствуют или некорректны');
            tableBody.innerHTML = '';
            noResultsMessage.style.display = 'block';
            updatePagination(pagination);
            return;
        }

        tableBody.innerHTML = '';

        if (clients.length === 0) {
            noResultsMessage.style.display = 'block';
            updatePagination(pagination);
            return;
        }

        noResultsMessage.style.display = 'none';

        clients.forEach(client => {
            const newRow = createTableRow(client);
            tableBody.appendChild(newRow);
        });

        updatePagination(pagination);

        // Перепривязываем обработчики после обновления таблицы
        rebindEventHandlers();
    }

    function updatePagination(pagination) {
        const paginationContainer = document.querySelector('.pagination-container');
        const serverPagination = document.getElementById('serverPagination');
        
        if (!paginationContainer) {
            console.warn('.pagination-container не найден в DOM');
            return;
        }

        // Скрываем серверную пагинацию при AJAX-поиске
        if (serverPagination) {
            serverPagination.style.display = 'none';
        }

        let html = '<ul class="pagination">';

        if (pagination && pagination.hasPreviousPage) {
            html += `<li class="page-item"><a href="#" class="page-link" data-page="${pagination.currentPage - 1}">Назад</a></li>`;
        } else {
            html += '<li class="page-item disabled"><span class="page-link">Назад</span></li>';
        }

        if (pagination) {
            for (let i = 1; i <= pagination.totalPages; i++) {
                if (i === pagination.currentPage) {
                    html += `<li class="page-item active"><span class="page-link">${i}</span></li>`;
                } else {
                    html += `<li class="page-item"><a href="#" class="page-link" data-page="${i}">${i}</a></li>`;
                }
            }
        }

        if (pagination && pagination.hasNextPage) {
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
     * Создаёт новую строку таблицы для найденного клиента
     */
    function createTableRow(client) {
        const row = document.createElement('tr');
        row.setAttribute('data-client-id', client.id);

        const safeFirstName = escapeHtml(client.firstName || '');
        const safeLastName = escapeHtml(client.lastName || '');
        const safePhone = escapeHtml(client.phone || '');
        const safeEmail = escapeHtml(client.email || '-');

        row.innerHTML = `
            <td>${safeFirstName}</td>
            <td>${safeLastName}</td>
            <td>${safePhone}</td>
            <td>${safeEmail}</td>
            <td class="text-end">
                <a href="/Clients/Edit/${client.id}" class="btn btn-sm btn-outline-primary me-1">Редактировать</a>
                <form action="/Clients/Delete" method="post" style="display:inline;">
                    <input type="hidden" name="id" value="${client.id}"/>
                    <!-- Добавляем токен вручную -->
                    <input type="hidden" name="__RequestVerificationToken" 
                           value="${document.querySelector('input[name="__RequestVerificationToken"]').value}">
                    <button type="submit" class="btn btn-sm btn-outline-danger"
                            onclick="return confirm('Удалить запись?')">Удалить</button>
                </form>
            </td>`;

        return row;
    }

    /**
     * Перепривязывает обработчики событий к динамическим элементам
     */
    function rebindEventHandlers() {
        // Обработчики для кнопок «Удалить» (перепривязываем на случай обновления таблицы)
        document.querySelectorAll('form[action="/Clients/Delete"] button[type="submit"]').forEach(btn => {
            btn.removeEventListener('click', handleDeleteConfirmation);
            btn.addEventListener('click', handleDeleteConfirmation);
        });
    }

    /**
     * Обработчик подтверждения удаления
     */
    function handleDeleteConfirmation(e) {
        return confirm('Удалить запись?');
    }

    /**
     * Экранирует специальные символы HTML для безопасного вывода
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

        if (searchTerm === '') {
            window.location.href = '/Clients';
        } else {
            searchTimeout = setTimeout(() => performSearch(searchTerm), 300);
        }
    });

    // Инициализация при загрузке (если есть поисковый запрос в URL)
    const urlParams = new URLSearchParams(window.location.search);
    const initialSearch = urlParams.get('search') || '';
    const initialPage = parseInt(urlParams.get('page')) || 1;

    if (initialSearch) {
        searchInput.value = initialSearch;
        performSearch(initialSearch, initialPage);
    } else {
        performSearch('', 1);
    }
});
/**
 * Реализует AJAX‑поиск продаж абонементов с корректной работой модальных окон и кнопок
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

            if (!data || !Array.isArray(data.sales) || !data.pagination) {
                console.error('Некорректный формат ответа от сервера:', data);
                showError('Сервер вернул некорректные данные. Попробуйте позже.');
                return;
            }

            updateTableVisibility(data.sales, data.pagination);
            refreshVisitButtons(); // Обновляем состояние кнопок после поиска
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
    function updateTableVisibility(sales, pagination) {
        if (!sales || !Array.isArray(sales)) {
            console.warn('Данные продаж отсутствуют или некорректны');
            tableBody.innerHTML = '';
            noResultsMessage.style.display = 'block';
            updatePagination(pagination);
            return;
        }

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

        // Перепривязываем обработчики после обновления таблицы
        rebindEventHandlers();
    }

    function updatePagination(pagination) {
        const paginationContainer = document.querySelector('.pagination-container');
        if (!paginationContainer) {
            console.warn('.pagination-container не найден в DOM');
            return;
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
     * Создаёт новую строку таблицы для найденной продажи абонемента
     */
    function createTableRow(sale) {
        const row = document.createElement('tr');
        row.setAttribute('data-sale-id', sale.id);
        row.setAttribute('data-max-visits', sale.maxVisits);
        row.setAttribute('data-visit-count', sale.visitCount);

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
        <a href="/AbonnementSales/Edit/${sale.id}" class="btn btn-sm btn-outline-primary me-1">Редактировать</a>
        <button type="button"
                class="btn btn-sm btn-outline-success mark-visit-btn"
                data-abonnement-id="${sale.id}"
                title="Отметить посещение клиента">
            <i class="bi bi-calendar-check"></i> Отметить
        </button>
        <form action="/AbonnementSales/Delete" method="post" style="display:inline;">
            <input type="hidden" name="id" value="${sale.id}"/>
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
        // Обработчики для кнопки «История посещений»
        document.querySelectorAll('.view-history-btn').forEach(btn => {
            btn.removeEventListener('click', handleViewHistory); // Удаляем старые (если есть)
            btn.addEventListener('click', handleViewHistory);
        });

        // Обработчики для кнопки «Отметить посещение»
        document.querySelectorAll('.mark-visit-btn').forEach(btn => {
            btn.removeEventListener('click', handleMarkVisit); // Удаляем старые
            btn.addEventListener('click', handleMarkVisit);
        });
    }

    /**
     * Обработчик клика по кнопке «История посещений»
     */
    async function handleViewHistory() {
        const abonnementId = this.getAttribute('data-abonnement-id');

        try {
            const response = await fetch(`/ClientVisits/GetVisitHistory?abonnementSaleId=${abonnementId}`);

            if (response.ok) {
                const history = await response.json();
                showVisitHistoryModal(history, abonnementId);
            } else {
                showToast('Ошибка загрузки', 'HTTP ' + response.status, 'danger');
            }
        } catch (error) {
            console.error('Ошибка при загрузке истории посещений (ID: ' + abonnementId + '):', error);
            showToast('Ошибка', 'Не удалось загрузить историю посещений.', 'danger');
        }
    }

    /**
     * Обработчик клика по кнопке «Отметить посещение»
     */
    async function handleMarkVisit() {
        const abonnementId = this.getAttribute('data-abonnement-id');
        const row = this.closest('tr');
        const visitCountEl = row.querySelector('.badge');
        const maxVisits = parseInt(row.getAttribute('data-max-visits')) || 0;

        try {
            const response = await fetch('/ClientVisits/MarkVisit', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: `abonnementSaleId=${abonnementId}`
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    // Обновляем счётчик
                    visitCountEl.textContent = result.visitCount;
                    row.setAttribute('data-visit-count', result.visitCount);
                    refreshVisitButtons(); // Перепроверяем состояние кнопок
                    // ПОКАЗЫВАЕМ TOAST: успешная отметка
                    showToast('Успех', result.message, 'success');
                } else {
                    // Лимит исчерпан
                    visitCountEl.textContent = result.visitCount;
                    row.setAttribute('data-visit-count', result.visitCount);
                    refreshVisitButtons();
                    // ПОКАЗЫВАЕМ TOAST: лимит исчерпан
                    showToast('Внимание', result.message, 'warning');
                }
            } else {
                // ПОКАЗЫВАЕМ TOAST: ошибка сервера
                showToast('Ошибка', `HTTP ${response.status}`, 'danger');
            }
        } catch (error) {
            console.error('Ошибка при отметке посещения (ID: ' + abonnementId + '):', error);
            // ПОКАЗЫВАЕМ TOAST: сетевая ошибка
            showToast('Ошибка', 'Не удалось отметить посещение. Проверьте консоль (F12).', 'danger');
        }
    }

    /**
     * Обновляет состояние кнопок «Отметить посещение» на основе данных строки
     */
    function refreshVisitButtons() {
        document.querySelectorAll('.mark-visit-btn').forEach(btn => {
            const row = btn.closest('tr');
            const visitCount = parseInt(row.getAttribute('data-visit-count')) || 0;
            const maxVisits = parseInt(row.getAttribute('data-max-visits')) || 0;

            if (maxVisits > 0 && visitCount >= maxVisits) {
                btn.disabled = true;
                btn.textContent = 'Исчерпано';
                btn.classList.remove('btn-outline-success', 'btn-success');
                btn.classList.add('btn-danger');
                btn.title = 'Лимит посещений исчерпан';
            } else {
                btn.disabled = false;
                btn.textContent = 'Отметить';
                btn.classList.remove('btn-danger', 'btn-success');
                btn.classList.add('btn-outline-success');
                btn.title = 'Отметить посещение клиента';
            }
        });
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
    searchInput.addEventListener('input', function (e) {
        const searchTerm = e.target.value.trim();
        clearTimeout(searchTimeout);


        if (searchTerm === '') {
            window.location.href = '/AbonnementSales';
        } else {
            searchTimeout = setTimeout(() => performSearch(searchTerm), 300);
        }
    });

    // Инициализация при загрузке
    refreshVisitButtons();
});

/**
 * Отображает модальное окно с историей посещений (переиспользовано из history-modal.js)
 */
function showVisitHistoryModal(history, abonnementId) {
    const modalHtml = `
    <div class="modal fade" id="visitHistoryModal" tabindex="-1">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">История посещений (абонемент №${abonnementId})</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    ${history.length > 0
        ? `<table class="table table-striped">
                            <thead>
                                <tr>
                                    <th>Дата посещения</th>
                                    <th>Последнее изменение</th>
                                    <th class="text-end">Действия</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${history.map(visit => `
                                <tr data-visit-id="${visit.id}">
                                    <td>
                                        <span class="visit-date-display">${new Date(visit.visitDate).toLocaleDateString('ru-RU')}</span>
                                        <input type="date"
                               class="form-control visit-date-input"
                               value="${visit.visitDate}"
                               style="display:none;">
                                    </td>
                                    <td>${visit.modifiedAt
            ? new Date(visit.modifiedAt).toLocaleDateString('ru-RU')
            : 'Не изменялось'}</td>
                                    <td class="text-end">
                                        <button type="button"
                                class="btn btn-sm btn-outline-primary edit-date-btn"
                                data-visit-id="${visit.id}">
                            <i class="bi bi-pencil"></i> Редактировать
                        </button>
                        <button type="button"
                                class="btn btn-sm btn-success save-date-btn"
                                style="display:none;"
                                data-visit-id="${visit.id}">
                            <i class="bi bi-check"></i> Сохранить
                        </button>
                        <button type="button"
                                class="btn btn-sm btn-secondary cancel-edit-btn"
                                style="display:none;">
                            <i class="bi bi-x"></i> Отмена
                        </button>
                                    </td>
                                </tr>`).join('')}
                            </tbody>
                        </table>`
        : '<p class="text-muted">Посещения не найдены</p>'
    }
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Закрыть</button>
                </div>
            </div>
        </div>
    </div>`;

    document.body.insertAdjacentHTML('beforeend', modalHtml);
    const modal = new bootstrap.Modal(document.getElementById('visitHistoryModal'));
    modal.show();

    // Обработчики событий для кнопок внутри модального окна
    document.querySelectorAll('.edit-date-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const visitId = this.getAttribute('data-visit-id');
            const row = this.closest('tr');
            row.querySelector('.visit-date-display').style.display = 'none';
            row.querySelector('.visit-date-input').style.display = 'block';
            this.style.display = 'none';
            row.querySelector('.save-date-btn').style.display = 'inline-block';
            row.querySelector('.cancel-edit-btn').style.display = 'inline-block';
        });
    });

    document.querySelectorAll('.cancel-edit-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const row = this.closest('tr');
            row.querySelector('.visit-date-display').style.display = 'block';
            row.querySelector('.visit-date-input').style.display = 'none';
            row.querySelector('.edit-date-btn').style.display = 'inline-block';
            this.style.display = 'none';
            row.querySelector('.save-date-btn').style.display = 'none';
        });
    });

    document.querySelectorAll('.save-date-btn').forEach(btn => {
        btn.addEventListener('click', async function() {
            const visitId = this.getAttribute('data-visit-id');
            const row = this.closest('tr');
            const dateInput = row.querySelector('.visit-date-input');
            const newDateStr = dateInput.value; // Формат: "yyyy-MM-dd"

            if (!newDateStr) {
                showToast('Ошибка', 'Выберите дату', 'danger');
                return;
            }

            // Преобразуем в UTC дату без времени
            const newDate = new Date(newDateStr + 'T00:00:00Z');

            try {
                const response = await fetch(`/ClientVisits/EditVisitDate?id=${visitId}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(newDate)
                });

                if (response.ok) {
                    const result = await response.json();

                    // Обновляем отображение (только дата)
                    row.querySelector('.visit-date-display').textContent =
                        new Date(result.newDate).toLocaleDateString('ru-RU');
                    if (result.modifiedAt) {
                        row.cells[1].textContent =
                            new Date(result.modifiedAt).toLocaleDateString('ru-RU');
                    }

                    // Возвращаем в режим просмотра
                    dateInput.style.display = 'none';
                    row.querySelector('.visit-date-display').style.display = 'block';
                    this.style.display = 'none';
                    row.querySelector('.cancel-edit-btn').style.display = 'none';
                    row.querySelector('.edit-date-btn').style.display = 'inline-block';

                    showToast('Успех', 'Дата посещения обновлена', 'success');
                } else {
                    const error = await response.json();
                    showToast('Ошибка', error.message || 'Не удалось обновить дату', 'danger');
                }
            } catch (error) {
                console.error('Ошибка при сохранении даты:', error);
                showToast('Ошибка', 'Не удалось сохранить изменения', 'danger');
            }
        });
    });

    // Удаляем модальное окно из DOM после закрытия
    document.getElementById('visitHistoryModal').addEventListener('hidden.bs.modal', function () {
        this.remove();
    });
}

/**
 * Показывает уведомление (toast)
 * @param {string} title - Заголовок уведомления
 * @param {string} message - Текст сообщения
 * @param {string} type - Тип (success, danger, warning, info)
 */
function showToast(title, message, type = 'info') {
    const container = document.getElementById('liveToastContainer');

    if (!container) {
        console.error('Контейнер #liveToastContainer не найден в DOM!');
        return;
    }

    const toastId = 'toast-' + Date.now() + Math.random().toString(36).substr(2, 9);
    const typeClasses = {
        'success': 'bg-success text-white',
        'danger': 'bg-danger text-white',
        'warning': 'bg-warning text-dark',
        'info': 'bg-info text-white'
    };
    const currentTypeClass = typeClasses[type] || typeClasses['info'];


    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center ${currentTypeClass}" role="alert">
            <div class="d-flex">
                <div class="toast-body">${title}: ${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>`;

    container.insertAdjacentHTML('afterbegin', toastHtml);

    const toastEl = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastEl, { autohide: true, delay: 5000 });
    toast.show();

    toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
}
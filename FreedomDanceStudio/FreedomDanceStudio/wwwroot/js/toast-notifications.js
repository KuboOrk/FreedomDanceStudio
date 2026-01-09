/**
 * Показывает всплывающее уведомление (toast)
 * @param {string} title - Заголовок тоста
 * @param {string} message - Текст сообщения
 * @param {string} type - Тип: 'success', 'danger', 'warning', 'info'
 */
function showToast(title, message, type = 'info') {
    const container = document.getElementById('liveToastContainer');

    // Генерируем уникальный ID для тоста
    const toastId = 'toast-' + Date.now() + Math.random().toString(36).substr(2, 9);

    // Определяем стили по типу
    const typeClasses = {
        'success': 'bg-success text-white',
        'danger': 'bg-danger text-white',
        'warning': 'bg-warning text-dark',
        'info': 'bg-info text-white'
    };

    const headerClasses = {
        'success': 'bg-success-subtle',
        'danger': 'bg-danger-subtle',
        'warning': 'bg-warning-subtle',
        'info': 'bg-info-subtle'
    };

    const currentTypeClass = typeClasses[type] || typeClasses['info'];
    const currentHeaderClass = headerClasses[type] || headerClasses['info'];

    // Создаём HTML тоста
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white ${currentTypeClass}" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-header ${currentHeaderClass}">
                    <strong class="me-auto">${title}</strong>
                    <small>только что</small>
            <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
        <div class="toast-body">
            ${message}
        </div>
    </div>
</div>`;

    // Вставляем в контейнер
    container.insertAdjacentHTML('afterbegin', toastHtml);

    // Инициализируем и показываем
    const toastEl = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastEl, {
        autohide: true,
        delay: 5000 // Автоскрытие через 5 секунд
    });

    toast.show();

    // Удаляем элемент из DOM после скрытия
    toastEl.addEventListener('hidden.bs.toast', function () {
        this.remove();
    });
}

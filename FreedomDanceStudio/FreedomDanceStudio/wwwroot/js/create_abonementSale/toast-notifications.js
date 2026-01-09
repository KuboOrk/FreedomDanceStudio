/**
 * Показывает всплывающее уведомление (toast)
 * @param {string} title - Заголовок тоста
 * @param {string} message - Текст сообщения
 * @param {string} type - Тип: 'success', 'danger', 'warning', 'info'
 */
function showToast(title, message, type = 'info') {
    // Контейнер для тостов (создаём, если отсутствует)
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.setAttribute('aria-live', 'polite');
        toastContainer.setAttribute('aria-atomic', 'true');
        toastContainer.style.position = 'fixed';
        toastContainer.style.bottom = '20px';
        toastContainer.style.right = '20px';
        toastContainer.style.zIndex = '1070';
        document.body.appendChild(toastContainer);
    }

    // Генерируем уникальный ID для тоста
    const toastId = 'toast-' + Date.now() + Math.random().toString(36).substr(2, 9);

    // Определяем стили по типу
    const typeClasses = {
        'success': 'bg-success text-white',
        'danger': 'bg-danger text-white',
        'warning': 'bg-warning text-dark',
        'info': 'bg-info text-white'
    };

    const currentTypeClass = typeClasses[type] || typeClasses['info'];

    // Создаём HTML тоста
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center ${currentTypeClass}" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
        </div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
    </div>
</div>`;


    // Вставляем в контейнер
    toastContainer.insertAdjacentHTML('afterbegin', toastHtml);

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

/**
 * Инициализация страницы списка продаж абонементов
 */
document.addEventListener('DOMContentLoaded', function () {
    // Сначала устанавливаем начальные значения счётчиков в data‑атрибуты
    document.querySelectorAll('tr[data-max-visits]').forEach(row => {
        const visitCountEl = row.querySelector('.badge');
        const currentCount = parseInt(visitCountEl.textContent) || 0;
        row.setAttribute('data-visit-count', currentCount);
    });

    // Затем инициализируем состояние кнопок
    refreshVisitButtons();
});

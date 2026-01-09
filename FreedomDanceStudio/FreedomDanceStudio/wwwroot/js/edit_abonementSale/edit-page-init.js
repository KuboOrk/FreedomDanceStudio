/**
 * Инициализация страницы редактирования абонемента
 */
document.addEventListener('DOMContentLoaded', function() {
    // Обработчик изменения видимого поля даты
    const endDateDisplay = document.getElementById('EndDateDisplay');
    endDateDisplay.addEventListener('change', function() {
        if (validateEndDate()) {
            syncEndDateFields();
        }
    });

    // Инициализация при загрузке — показываем сохранённую дату из БД
    syncEndDateFields();
});

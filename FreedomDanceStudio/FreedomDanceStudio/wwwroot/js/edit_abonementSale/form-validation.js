/**
 * Синхронизирует видимое и скрытое поля даты окончания
 */
function syncEndDateFields() {
    const endDateDisplay = document.getElementById('EndDateDisplay');
    const endDateHidden = document.getElementById('EndDateHidden');

    const displayValue = endDateDisplay.value;
    if (displayValue) {
        endDateHidden.value = displayValue;
    }
}

/**
 * Проверяет валидность даты окончания относительно даты начала
 * @returns {boolean} true, если дата валидна
 */
function validateEndDate() {
    const originalStartDate = new Date('@Model.StartDate.ToString("yyyy-MM-dd")');
    const selectedEndDate = new Date(document.getElementById('EndDateDisplay').value);

    if (selectedEndDate < originalStartDate) {
        showToast('Ошибка', 'Дата окончания не может быть раньше начала действия абонемента!', 'danger');
        // Восстанавливаем исходную дату
        const originalEndDate = new Date('@Model.EndDate.ToString("yyyy-MM-dd")');
        document.getElementById('EndDateDisplay').value = originalEndDate.toISOString().split('T')[0];
        syncEndDateFields();
        return false;
    }
    return true;
}

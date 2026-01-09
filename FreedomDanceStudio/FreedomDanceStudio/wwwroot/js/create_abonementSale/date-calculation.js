/**
 * Пересчитывает дату окончания на основе выбранной услуги и даты начала
 */
function recalculateEndDate() {
    const serviceSelect = document.getElementById('ServiceSelect');
    const startDateInput = document.getElementById('StartDate');
    const endDateDisplay = document.getElementById('EndDateDisplay');
    const endDateHidden = document.getElementById('EndDateHidden');
    const serviceInfo = document.getElementById('ServiceInfo');
    const serviceNameDisplay = document.getElementById('ServiceNameDisplay');
    const priceDisplay = document.getElementById('PriceDisplay');
    const durationDisplay = document.getElementById('DurationDisplay');

    const startDateStr = startDateInput.value;
    const serviceId = serviceSelect.value;

    // Если нет данных, очищаем EndDate
    if (!startDateStr || !serviceId) {
        endDateDisplay.value = '';
        endDateHidden.value = '';
        if (serviceInfo) serviceInfo.style.display = 'none';
        return;
    }

    const url = `${window.location.origin}/AbonnementSales/GetServiceDuration?serviceId=${serviceId}`;

    fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            const startDate = new Date(startDateStr);
            const newEndDate = new Date(startDate);
            newEndDate.setDate(startDate.getDate() + data.durationDays);

            // Форматируем в ISO для скрытого поля
            const isoEndDate = newEndDate.toISOString().split('T')[0];
            endDateDisplay.value = isoEndDate;
            endDateHidden.value = isoEndDate;

            // Обновляем информацию об услуге
            if (serviceNameDisplay) serviceNameDisplay.textContent = data.serviceName;
            if (priceDisplay) priceDisplay.textContent = data.price;
            if (durationDisplay) durationDisplay.textContent = data.durationDays;
            if (serviceInfo) serviceInfo.style.display = 'block';
        })
        .catch(error => {
            console.error('Ошибка при получении данных:', error);
            alert(`Не удалось загрузить данные услуги: ${error.message}. Проверьте подключение к сети.`);

            // При ошибке очищаем EndDate
            endDateDisplay.value = '';
            endDateHidden.value = '';
            if (serviceInfo) serviceInfo.style.display = 'none';
        });
}

// Устанавливаем обработчики событий после загрузки DOM
document.addEventListener('DOMContentLoaded', function() {
    const serviceSelect = document.getElementById('ServiceSelect');
    const startDateInput = document.getElementById('StartDate');

    if (serviceSelect) serviceSelect.addEventListener('change', recalculateEndDate);
    if (startDateInput) startDateInput.addEventListener('change', recalculateEndDate);

    // Инициализация при загрузке (если элементы найдены и есть выбор)
    if (serviceSelect && startDateInput && serviceSelect.value && startDateInput.value) {
        recalculateEndDate();
    }
});

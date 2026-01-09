/**
 * Обрабатывает изменение выбранной услуги — загружает данные и пересчитывает дату окончания
 */
document.addEventListener('DOMContentLoaded', function() {
    const serviceSelect = document.getElementById('ServiceSelect');
    const endDateDisplay = document.getElementById('EndDateDisplay');
    const serviceInfo = document.getElementById('ServiceInfo');
    const serviceNameDisplay = document.getElementById('ServiceNameDisplay');
    const priceDisplay = document.getElementById('PriceDisplay');
    const durationDisplay = document.getElementById('DurationDisplay');

    // Исходная дата начала действия абонемента
    const originalStartDate = new Date('@Model.StartDate.ToString("yyyy-MM-dd")');

    serviceSelect.addEventListener('change', async function() {
        const serviceId = this.value;

        if (!serviceId) {
            serviceInfo.style.display = 'none';
            syncEndDateFields(); // Сохраняем текущую дату
            return;
        }

        try {
            const response = await fetch(`/AbonnementSales/GetServiceDuration?serviceId=${serviceId}`);
            if (!response.ok) throw new Error('Ошибка сети');

            const data = await response.json();

            // Заполняем информацию об услуге
            serviceNameDisplay.textContent = data.serviceName;
            priceDisplay.textContent = data.price;
            durationDisplay.textContent = data.durationDays;
            serviceInfo.style.display = 'block';

            // Пересчитываем дату окончания на основе исходного StartDate
            const newEndDate = new Date(originalStartDate);
            newEndDate.setDate(originalStartDate.getDate() + data.durationDays);

            // Устанавливаем в видимое поле (формат yyyy-MM-dd для input[type=date])
            const formattedDate = newEndDate.toISOString().split('T')[0];
            endDateDisplay.value = formattedDate;
            syncEndDateFields(); // Синхронизируем с скрытым полем
        } catch (error) {
            console.error('Ошибка при получении данных:', error);
            showToast('Ошибка', 'Не удалось загрузить данные услуги. Проверьте подключение к сети.', 'danger');
            // Возвращаем сохранённую дату при ошибке
            syncEndDateFields();
        }
    });
});

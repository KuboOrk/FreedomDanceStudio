/**
 * Инициализация страницы создания абонемента
 */
document.addEventListener('DOMContentLoaded', function() {
    // Устанавливаем текущую дату для поля SaleDate (только для чтения)
    const saleDateInput = document.getElementById('@Html.IdFor(m => m.SaleDate)');
    if (saleDateInput) {
        const today = new Date().toISOString().split('T')[0];
        saleDateInput.value = today;
    }
});

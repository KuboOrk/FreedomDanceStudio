$(document).ready(function() {
    $('.selectpicker').selectpicker({
        style: 'btn-secondary',
        size: 10, // количество видимых опций до прокрутки
        liveSearch: true, // активируем поиск
        liveSearchPlaceholder: 'Поиск клиента...', // текст-плейсхолдер для поля поиска
        dropdownAlignRight: false // выравнивание выпадающего списка
    });
});

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

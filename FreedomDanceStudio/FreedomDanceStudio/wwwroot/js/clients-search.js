/**
 * Реализует функционал поиска клиентов в реальном времени
 */
document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    const tableBody = document.getElementById('clientsTableBody');
    const noResultsMessage = document.getElementById('noResultsMessage');

    // Сохраняем все строки таблицы для фильтрации
    const allRows = Array.from(tableBody.getElementsByTagName('tr'));

    // Функция фильтрации таблицы
    function filterTable(searchTerm) {
        let visibleCount = 0;

        allRows.forEach(row => {
            const cells = row.getElementsByTagName('td');
            const firstName = (cells[0]?.textContent || '').toLowerCase();
            const lastName = (cells[1]?.textContent || '').toLowerCase();
            const phone = (cells[2]?.textContent || '').toLowerCase();
            const email = (cells[3]?.textContent || '').toLowerCase();

            // Проверяем совпадение в любом из полей
            const matches = searchTerm === '' ||
                firstName.includes(searchTerm) ||
                lastName.includes(searchTerm) ||
                phone.includes(searchTerm) ||
                email.includes(searchTerm);

            if (matches) {
                row.style.display = '';
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });

        // Показываем/скрываем сообщение об отсутствии результатов
        if (searchTerm && visibleCount === 0) {
            noResultsMessage.style.display = 'block';
        } else {
            noResultsMessage.style.display = 'none';
        }
    }

    // Обработчик ввода с задержкой для оптимизации
    let searchTimeout;
    searchInput.addEventListener('input', function(e) {
        const searchTerm = e.target.value.toLowerCase().trim();

        // Очищаем предыдущий таймер
        clearTimeout(searchTimeout);

        // Запускаем поиск через 300 мс после последнего ввода
        searchTimeout = setTimeout(() => {
            filterTable(searchTerm);
        }, 300);
    });

    // Инициализация — показываем все строки при загрузке
    filterTable('');
});

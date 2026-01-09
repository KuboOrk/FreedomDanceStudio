document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    const tableBody = document.getElementById('salesTableBody');
    const noResultsMessage = document.getElementById('noResultsMessage');
    const loadingIndicator = document.createElement('div');
    let searchTimeout;

    // Настройка индикатора загрузки
    loadingIndicator.className = 'text-center text-muted my-3';
    loadingIndicator.textContent = 'Поиск...';
    loadingIndicator.style.display = 'none';
    tableBody.parentNode.insertBefore(loadingIndicator, tableBody);

    async function performSearch(searchTerm) {
        loadingIndicator.style.display = 'block';
        tableBody.style.opacity = '0.6';

        try {
            const url = `${window.location.origin}/AbonnementSales/Search?search=${encodeURIComponent(searchTerm)}`;
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    throw new Error('Метод поиска не найден на сервере');
                } else if (response.status === 500) {
                    const errorData = await response.json();
                    throw new Error(`Ошибка сервера: ${errorData.error || 'Неизвестная ошибка'}`);
                }
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const sales = await response.json();
            updateTable(sales);
        } catch (error) {
            console.error('Ошибка поиска:', error);
            showError(`Не удалось выполнить поиск: ${error.message}`);
            tableBody.style.opacity = '1';
            loadingIndicator.style.display = 'none';
        } finally {
            loadingIndicator.style.display = 'none';
            tableBody.style.opacity = '1';
        }
    }

    function updateTable(sales) {
        tableBody.innerHTML = '';

        if (sales.length === 0) {
            noResultsMessage.style.display = 'block';
            return;
        }

        noResultsMessage.style.display = 'none';

        sales.forEach(sale => {
            const row = document.createElement('tr');
            row.setAttribute('data-max-visits', sale.maxVisits);
            row.setAttribute('data-visit-count', sale.visitCount);

            row.innerHTML = `
                <td>${sale.id}</td>
                <td>${sale.clientName}</td>
                <td>${sale.serviceName}</td>
                <td>${sale.saleDate}</td>
                <td>${sale.startDate}</td>
                <td>${sale.endDate}</td>
                <td>
                    <span class="badge bg-primary">${sale.visitCount}</span>
                    <button type="button"
                    class="btn btn-sm btn-outline-info view-history-btn"
            data-abonnement-id="${sale.id}"
            title="Посмотреть историю посещений">
                <i class="bi bi-clock-history"></i> История
            </button>
        </td>
        <td class="text-end">
            <a href="/AbonnementSales/Edit/${sale.id}"
               class="btn btn-sm btn-outline-primary me-1">Изменить</a>
            <button type="button"
                class="btn btn-sm btn-outline-success mark-visit-btn"
                data-abonnement-id="${sale.id}"
                title="Отметить посещение клиента">
                <i class="bi bi-calendar-check"></i> Отметить
            </button>
            <form action="/AbonnementSales/Delete" method="post" style="display:inline;">
                <input type="hidden" name="id" value="${sale.id}"/>
                <button type="submit" class="btn btn-sm btn-outline-danger"
                        onclick="return confirm('Удалить запись №${sale.id}?')">Удалить</button>
            </form>
        </td>`;

            tableBody.appendChild(row);
        });
    }

    function showError(message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger';
        errorDiv.textContent = message;
        errorDiv.style.position = 'fixed';
        errorDiv.style.top = '20px';
        errorDiv.style.right = '20px';
        errorDiv.style.zIndex = '1050';
        document.body.appendChild(errorDiv);

        setTimeout(() => errorDiv.remove(), 5000);
    }

    searchInput.addEventListener('input', function(e) {
        const searchTerm = e.target.value.trim();
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => performSearch(searchTerm), 300);
    });

    if (!searchInput.value) {
        performSearch('');
    }
});

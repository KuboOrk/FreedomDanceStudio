/**
 * Загружает и показывает историю посещений для абонемента
 */
document.querySelectorAll('.view-history-btn').forEach(btn => {
    btn.addEventListener('click', async function () {
        const abonnementId = this.getAttribute('data-abonnement-id');

        try {
            // Отправляем запрос на сервер за историей посещений
            const response = await fetch(`/ClientVisits/GetVisitHistory?abonnementSaleId=${abonnementId}`);

            if (response.ok) {
                const history = await response.json();
                showVisitHistoryModal(history, abonnementId);
            } else {
                showToast('Ошибка загрузки', 'HTTP ' + response.status, 'danger');
            }
        } catch (error) {
            console.error('Ошибка при загрузке истории посещений (ID: ' + abonnementId + '):', error);
            showToast('Ошибка', 'Не удалось загрузить историю посещений.', 'danger');
        }
    });
});

/**
 * Отображает модальное окно с историей посещений
 * @param {Array} history - Массив объектов посещений
 * @param {number} abonnementId - ID абонемента
 */
function showVisitHistoryModal(history, abonnementId) {
    // Создаём модальное окно динамически
    const modalHtml = `
<div class="modal fade" id="visitHistoryModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">История посещений (абонемент №${abonnementId})</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                ${history.length > 0
        ? `<table class="table table-striped">
                        <thead>
                            <tr>
                                <th>Дата посещения</th>
                        </tr>
                    </thead>
            <tbody>
                ${history.map(visit => `
                    <tr>
                <td>${new Date(visit.visitDate).toLocaleDateString('ru-RU')}</td>
            </tr>`).join('')}
            </tbody>
        </table>`
        : '<p class="text-muted">Посещения не найдены</p>'
    }
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Закрыть</button>
            </div>
        </div>
    </div>
</div>`;

    // Вставляем модальное окно в DOM
    document.body.insertAdjacentHTML('beforeend', modalHtml);

    // Инициализируем и показываем модальное окно (Bootstrap 5)
    const modal = new bootstrap.Modal(document.getElementById('visitHistoryModal'));
    modal.show();

    // Удаляем модальное окно из DOM после закрытия
    document.getElementById('visitHistoryModal').addEventListener('hidden.bs.modal', function () {
        this.remove();
    });
}

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
            <th>Последнее изменение</th>
            <th class="text-end">Действия</th>
                </tr>
            </thead>
            <tbody>
                ${history.map(visit => `
            <tr data-visit-id="${visit.id}">
                <td>
            <span class="visit-date-display">${new Date(visit.visitDate).toLocaleDateString('ru-RU')}</span>
            <input type="date"
                   class="form-control visit-date-input"
                   value="${visit.visitDate}"
                   style="display:none;">
                </td>
                <td>${visit.modifiedAt
            ? new Date(visit.modifiedAt).toLocaleDateString('ru-RU')
            : 'Не изменялось'}</td>
                <td class="text-end">
            <button type="button"
            class="btn btn-sm btn-outline-primary edit-date-btn"
            data-visit-id="${visit.id}">
                <i class="bi bi-pencil"></i> Редактировать
            </button>
            <button type="button"
            class="btn btn-sm btn-success save-date-btn"
            style="display:none;"
            data-visit-id="${visit.id}">
                <i class="bi bi-check"></i> Сохранить
            </button>
            <button type="button"
            class="btn btn-sm btn-secondary cancel-edit-btn"
            style="display:none;">
                <i class="bi bi-x"></i> Отмена
            </button>
                </td>
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

    document.body.insertAdjacentHTML('beforeend', modalHtml);
    const modal = new bootstrap.Modal(document.getElementById('visitHistoryModal'));
    modal.show();

    // Обработчики событий
    document.querySelectorAll('.edit-date-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const visitId = this.getAttribute('data-visit-id');
            const row = this.closest('tr');
            row.querySelector('.visit-date-display').style.display = 'none';
            row.querySelector('.visit-date-input').style.display = 'block';
            this.style.display = 'none';
            row.querySelector('.save-date-btn').style.display = 'inline-block';
            row.querySelector('.cancel-edit-btn').style.display = 'inline-block';
        });
    });

    document.querySelectorAll('.cancel-edit-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const row = this.closest('tr');
            row.querySelector('.visit-date-display').style.display = 'block';
            row.querySelector('.visit-date-input').style.display = 'none';
            row.querySelector('.edit-date-btn').style.display = 'inline-block';
            this.style.display = 'none';
            row.querySelector('.save-date-btn').style.display = 'none';
        });
    });

    document.querySelectorAll('.save-date-btn').forEach(btn => {
        btn.addEventListener('click', async function() {
            const visitId = this.getAttribute('data-visit-id');
            const row = this.closest('tr');
            const dateInput = row.querySelector('.visit-date-input');
            const newDateStr = dateInput.value; // Формат: "yyyy-MM-dd"

            if (!newDateStr) {
                showToast('Ошибка', 'Выберите дату', 'danger');
                return;
            }

            // Преобразуем в UTC дату без времени
            const newDate = new Date(newDateStr + 'T00:00:00Z');

            try {
                const response = await fetch(`/ClientVisits/EditVisitDate?id=${visitId}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(newDate)
                });

                if (response.ok) {
                    const result = await response.json();

                    // Обновляем отображение (только дата)
                    row.querySelector('.visit-date-display').textContent =
                        new Date(result.newDate).toLocaleDateString('ru-RU');
                    if (result.modifiedAt) {
                        row.cells[1].textContent =
                            new Date(result.modifiedAt).toLocaleDateString('ru-RU');
                    }

                    // Возвращаем в режим просмотра
                    dateInput.style.display = 'none';
                    row.querySelector('.visit-date-display').style.display = 'block';
                    this.style.display = 'none';
                    row.querySelector('.cancel-edit-btn').style.display = 'none';
                    row.querySelector('.edit-date-btn').style.display = 'inline-block';

                    showToast('Успех', 'Дата посещения обновлена', 'success');
                } else {
                    const error = await response.json();
                    showToast('Ошибка', error.message || 'Не удалось обновить дату', 'danger');
                }
            } catch (error) {
                console.error('Ошибка при сохранении даты:', error);
                showToast('Ошибка', 'Не удалось сохранить изменения', 'danger');
            }
        });
    });

// Удаляем модальное окно из DOM после закрытия
    document.getElementById('visitHistoryModal').addEventListener('hidden.bs.modal', function () {
        this.remove();
    });
}
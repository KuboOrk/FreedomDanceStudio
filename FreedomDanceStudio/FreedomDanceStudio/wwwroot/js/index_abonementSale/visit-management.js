/**
 * Обновляет состояние кнопок «Отметить посещение» на основе данных строки
 */
function refreshVisitButtons() {
    document.querySelectorAll('.mark-visit-btn').forEach(btn => {
        const row = btn.closest('tr');
        const visitCount = parseInt(row.getAttribute('data-visit-count')) || 0;
        const maxVisits = parseInt(row.getAttribute('data-max-visits')) || 0;

        if (maxVisits > 0 && visitCount >= maxVisits) {
            btn.disabled = true;
            btn.textContent = 'Исчерпано';
            btn.classList.remove('btn-outline-success', 'btn-success');
            btn.classList.add('btn-danger');
            btn.title = 'Лимит посещений исчерпан';
        } else {
            btn.disabled = false;
            btn.textContent = 'Отметить';
            btn.classList.remove('btn-danger', 'btn-success');
            btn.classList.add('btn-outline-success');
            btn.title = 'Отметить посещение клиента';
        }
    });
}

/**
 * Обрабатывает нажатие кнопки «Отметить посещение»
 */
document.querySelectorAll('.mark-visit-btn').forEach(btn => {
    btn.addEventListener('click', async function () {
        const abonnementId = this.getAttribute('data-abonnement-id');
        const row = this.closest('tr');
        const visitCountEl = row.querySelector('.badge');
        const maxVisits = parseInt(row.getAttribute('data-max-visits')) || 0;

        try {
            const response = await fetch('/ClientVisits/MarkVisit', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: `abonnementSaleId=${abonnementId}`
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    // Обновляем счётчик
                    visitCountEl.textContent = result.visitCount;
                    row.setAttribute('data-visit-count', result.visitCount);
                    refreshVisitButtons(); // Переинициализация кнопок
                    showToast('Успех', result.message, 'success');
                } else {
                    // Ошибка от сервера (лимит исчерпан)
                    visitCountEl.textContent = result.visitCount;
                    row.setAttribute('data-visit-count', result.visitCount);
                    refreshVisitButtons();
                    showToast('Ошибка', result.message, 'danger');
                }
            } else {
                showToast('Ошибка сервера', 'HTTP ' + response.status, 'danger');
            }
        } catch (error) {
            console.error('Ошибка при отметке посещения (ID: ' + abonnementId + '):', error);
            showToast('Ошибка', 'Не удалось отметить посещение. Проверьте консоль (F12).', 'danger');
        }
    });
});

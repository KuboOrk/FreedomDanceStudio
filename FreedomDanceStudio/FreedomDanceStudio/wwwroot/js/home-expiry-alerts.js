document.addEventListener('DOMContentLoaded', function() {
    refreshAllAbonnementAlerts(); // Первоначальная загрузка (для обновлений)
    setInterval(refreshAllAbonnementAlerts, 30000); // Автообновление каждые 30 сек
});

async function refreshAllAbonnementAlerts() {
    try {
        console.log('Fetching alerts from /Home/GetAllAbonnementAlerts...');
        const response = await fetch('/Home/GetAllAbonnementAlerts');

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const alerts = await response.json();
        console.log('Received alerts:', alerts); // Отладка: посмотрите структуру

        updateAllAbonnementAlertsDisplay(alerts);
    } catch (error) {
        console.error('Error refreshing all abonnement alerts:', error);
        showAllAbonnementErrorState();
    }
}

function updateAllAbonnementAlertsDisplay(alerts) {
    const container = document.getElementById('allAbonnementAlertsContainer');
    container.innerHTML = '';

    if (!alerts || alerts.length === 0) {
        container.innerHTML = `
            <div class="col-12">
                <div class="alert alert-info text-center">
                    Нет абонементов в системе.
                </div>
            </div>`;
        return;
    }

    alerts.forEach(alert => {
        // Валидация данных
        if (!alert.client || !alert.usagePercent) {
            console.warn('Invalid alert data:', alert);
            return; // Пропускаем некорректные записи
        }

        const card = document.createElement('div');
        card.className = 'col-md-6 col-lg-4 mb-3';
        card.innerHTML = `
            <div class="card alert-card ${alert.alertLevel?.toLowerCase() || 'normal'}">
                <div class="card-body">
                    <h5 class="card-title">${alert.client.firstName || ''} ${alert.client.lastName || ''}</h5>
                    <p class="card-text">
                Абонемент истекает: <strong>${formatDate(alert.expiryDate)}</strong>
            </p>
            <!-- Круговой индикатор -->
            <div class="circular-progress" data-percent="${alert.usagePercent}">
                <svg viewBox="0 0 36 36" class="circular-chart">
                    <path class="circle-bg"
                          d="M18 2.0845
               a 15.9155 15.9155 0 0 1 0 31.831
               a 15.9155 15.9155 0 0 1 0 -31.831" />
            <path class="circle"
                  stroke-dasharray="${(alert.usagePercent / 100) * 2 * Math.PI * 15.9155}, 100"
                  d="M18 2.0845
               a 15.9155 15.9155 0 0 1 0 31.831
               a 15.9155 15.9155 0 0 1 0 -31.831" />
                </svg>
                <div class="percentage">${alert.usagePercent}%</div>
            </div>

            <div class="mt-2">
                <small class="text-muted">
                    Использовано: <strong>${alert.usedVisits}</strong> из <strong>${alert.maxVisits}</strong> посещений
                </small>
            </div>

            <!-- Кнопка быстрого действия -->
            <a href="/AbonnementSales/Edit/${alert.abonnementSaleId}"
               class="btn btn-sm btn-outline-primary mt-2 w-100">
                Продлить абонемент
            </a>
        </div>
    </div>`;
        container.appendChild(card);
    });

    initializeProgressCircles(); // Переинициализация SVG‑индикаторов
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('ru-RU');
}

function initializeProgressCircles() {
    const progressElements = document.querySelectorAll('.circular-progress');
    progressElements.forEach(function(element) {
        const percentStr = element.getAttribute('data-percent');
        const percent = parseFloat(percentStr);

        if (isNaN(percent)) {
            console.error('Invalid percent value:', percentStr);
            return;
        }

        const circle = element.querySelector('.circle');
        if (!circle) {
            console.error('Circle path not found in:', element);
            return;
        }

        const radius = 15.9155;
        const circumference = 2 * Math.PI * radius;
        const dashArray = (percent / 100) * circumference;

        circle.style.strokeDasharray = `${dashArray} ${circumference}`;
        circle.style.transition = 'stroke-dasharray 0.3s ease-in-out'; // Плавная анимация
    });
}

function showAllAbonnementErrorState() {
    const container = document.getElementById('allAbonnementAlertsContainer');
    container.innerHTML = `
        <div class="col-12">
            <div class="alert alert-danger text-center">
                Ошибка загрузки данных. Попробуйте обновить страницу.
            </div>
        </div>`;
}

// Функция для отметки посещения и мгновенного обновления але́ртов
async function markVisitAndRefresh(abonnementSaleId) {
    try {
        const response = await fetch('/ClientVisits/MarkVisit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ abonnementSaleId: abonnementSaleId })
        });

        const result = await response.json();

        if (result.success) {
            await refreshAllAbonnementAlerts(); // Обновление индикаторов
            showSuccessMessage(result.message);
        } else {
            showErrorMessage(result.message);
        }
    } catch (error) {
        console.error('Error marking visit:', error);
        showErrorMessage('Ошибка при отметке посещения');
    }
}

// Вспомогательные функции для сообщений
function showSuccessMessage(message) {
    const alert = document.createElement('div');
    alert.className = 'alert alert-success';
    alert.textContent = message;
    document.body.appendChild(alert);
    setTimeout(() => alert.remove(), 3000);
}

function showErrorMessage(message) {
    const alert = document.createElement('div');
    alert.className = 'alert alert-danger';
    alert.textContent = message;
    document.body.appendChild(alert);
    setTimeout(() => alert.remove(), 5000);
}


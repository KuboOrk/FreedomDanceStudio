// Валидация формы добавления рабочего времени
document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('.needs-validation');
    if (form) {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    }
});

// Динамическое обновление поля «Часы» на основе посещений (опционально)
function calculateHours() {
    const visits = document.getElementById('VisitsCount').value;
    const hours = Math.min(visits * 0.5, 24); // 0.5 часа на посещение
    document.getElementById('HoursCount').value = hours.toFixed(2);
}

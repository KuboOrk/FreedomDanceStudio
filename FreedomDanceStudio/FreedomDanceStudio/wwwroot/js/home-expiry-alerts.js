document.addEventListener('DOMContentLoaded', function() {
    // Анимируем круговые индикаторы
    const progressElements = document.querySelectorAll('.circular-progress');

    progressElements.forEach(function(element) {
        const percent = parseFloat(element.getAttribute('data-percent'));
        const circle = element.querySelector('.circle');

        // Расчёт длины дуги
        const radius = 15.9155;
        const circumference = 2 * Math.PI * radius;
        const dashArray = (percent / 100) * circumference;
        circle.style.strokeDasharray = `${dashArray} ${circumference}`;
    });

    // Автообновление данных каждые 5 минут
    setInterval(function() {
        location.reload(); // Простое решение; можно заменить на AJAX
    }, 5 * 60 * 1000);
});

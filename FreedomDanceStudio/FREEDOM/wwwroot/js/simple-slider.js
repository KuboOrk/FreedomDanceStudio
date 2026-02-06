document.addEventListener('DOMContentLoaded', function() {
    const slides = document.querySelectorAll('.slide');
    let currentIndex = 0;
    const slideCount = slides.length;

    // Функция показа слайда с анимацией
    function showSlide(index) {
        slides.forEach((slide, i) => {
            slide.classList.remove('active', 'prev', 'next'); // Убираем все классы анимации

            if (i === index) {
                slide.classList.add('active'); // Текущий слайд — активный
            } else if (i < index) {
                slide.classList.add('prev'); // Предыдущие слайды — уходят влево
            } else {
                slide.classList.add('next'); // Следующие слайды — приходят справа
            }
        });
    }

    // Переход к следующему слайду
    function nextSlide() {
        currentIndex = (currentIndex + 1) % slideCount;
        showSlide(currentIndex);
    }

    // Автопрокрутка (каждые 3 секунды)
    let sliderInterval = setInterval(nextSlide, 3000);

    // Показ первого слайда при загрузке
    showSlide(currentIndex);

    // Опционально: остановка автопрокрутки при наведении на слайдер
    const slider = document.querySelector('.simple-slider');
    slider.addEventListener('mouseenter', () => clearInterval(sliderInterval));
    slider.addEventListener('mouseleave', () => {
        sliderInterval = setInterval(nextSlide, 3000); // Возобновляем автопрокрутку
    });
});
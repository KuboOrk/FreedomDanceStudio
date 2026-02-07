function initScrollAnimations() {
    const sections = document.querySelectorAll('.section');
    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting && !entry.target.classList.contains('animated')) {
                entry.target.classList.add('animated', 'fadeIn');
                entry.target.classList.add('animated', 'slideDown');
            }
        });
    }, { threshold: 0.1 });

    sections.forEach(section => observer.observe(section));

    // Анимация для карточек при прокрутке
    const cards = document.querySelectorAll('.card');
    const cardObserver = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('bounceInUp');
            }
        });
    }, { threshold: 0.3 });

    cards.forEach(card => cardObserver.observe(card));
}

// Кнопка "Наверх"
document.getElementById('scrollToTop').addEventListener('click', function () {
    window.scrollTo({
        top: 0,
        behavior: 'smooth' // плавная прокрутка
    });
});

// Показываем кнопку при прокрутке
window.addEventListener('scroll', function () {
    const scrollToTop = document.getElementById('scrollToTop');
    if (window.pageYOffset > 300) {
        scrollToTop.style.display = 'block';
    } else {
        scrollToTop.style.display = 'none';
    }
});


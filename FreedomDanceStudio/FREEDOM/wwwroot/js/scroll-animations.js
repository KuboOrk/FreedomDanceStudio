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

document.addEventListener('DOMContentLoaded', function() {
    // Инициализация скриптов
    initScrollAnimations();
});

document.addEventListener('DOMContentLoaded', function() {
    // Выбираем все ссылки в меню, которые ведут к блокам на странице
    const menuLinks = document.querySelectorAll('.navbar-nav .nav-link[href^="#"]');


    menuLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault(); // Отменяем стандартное поведение ссылки


            const targetId = this.getAttribute('href'); // Получаем id блока (например, "#directions")
            const targetElement = document.querySelector(targetId);


            if (targetElement) {
                // Плавная прокрутка к блоку
                targetElement.scrollIntoView({
                    behavior: 'smooth', // Плавность
                    block: 'start'     // Выравниваем по верху блока
                });
            }
        });
    });
});
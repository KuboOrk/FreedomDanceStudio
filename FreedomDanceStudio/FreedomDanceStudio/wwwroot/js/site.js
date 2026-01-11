document.addEventListener('DOMContentLoaded', function() {
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('mainContent');
    const toggleBtn = document.getElementById('sidebarToggle');

    // Переключение сайдбара
    toggleBtn.addEventListener('click', function() {
        sidebar.classList.toggle('open');
        mainContent.classList.toggle('shifted');
    });

    // Закрытие при клике вне сайдбара на мобильных
    document.addEventListener('click', function(e) {
        if (!sidebar.contains(e.target) && !toggleBtn.contains(e.target) && sidebar.classList.contains('open')) {
            sidebar.classList.remove('open');
            mainContent.classList.remove('shifted');
        }
    });

    // Подсветка активного пункта меню
    const currentUrl = window.location.pathname;
    const navLinks = document.querySelectorAll('.sidebar .nav-link');

    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentUrl) {
            link.classList.add('active');
        }
    });
});

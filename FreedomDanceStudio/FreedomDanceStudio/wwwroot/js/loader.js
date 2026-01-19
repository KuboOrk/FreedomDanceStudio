document.addEventListener('DOMContentLoaded', function() {
    const loader = document.getElementById('loader');
    const loginForm = document.getElementById('loginForm');

    // Показываем лоадер, скрываем форму
    if (loader) loader.style.display = 'flex';
    if (loginForm) loginForm.style.display = 'none';

    // Таймер на 7 секунд
    setTimeout(function() {
        // Скрываем лоадер
        if (loader) {
            loader.style.opacity = '0';
            setTimeout(function() {
                loader.style.display = 'none';

                // Показываем форму входа
                if (loginForm) {
                    loginForm.style.display = 'block';
                }
            }, 300);
        }
    }, 3000);
});

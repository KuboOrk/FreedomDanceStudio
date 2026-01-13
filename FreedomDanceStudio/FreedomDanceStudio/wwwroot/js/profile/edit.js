document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('form');
    form.style.opacity = '0';
    form.style.transform = 'translateY(20px)';

    setTimeout(() => {
        form.style.transition = 'all 0.5s ease';
        form.style.opacity = '1';
        form.style.transform = 'translateY(0)';
    }, 100);
});
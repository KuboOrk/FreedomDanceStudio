document.addEventListener('DOMContentLoaded', function() {
    const phrases = document.querySelectorAll('.phrase');
    const delayBetweenCycles = 2000; // Пауза между циклами (2 сек)
    const typeSpeed = 100;           // Скорость печати (мс на символ)
    const eraseSpeed = 50;          // Скорость стирания (мс на символ)
    const holdTime = 1500;         // Время показа полного текста (1.5 сек)

    let currentPhraseIndex = 0;

    function typeText(element, text, callback) {
        element.textContent = ''; // Очищаем перед печатью
        let i = 0;
        const chars = text.split('');

        function type() {
            if (i < chars.length) {
                element.textContent += chars[i];
                i++;
                setTimeout(type, typeSpeed);
            } else {
                callback();
            }
        }
        type();
    }

    function eraseText(element, callback) {
        const text = element.textContent;
        let i = text.length;

        function erase() {
            if (i > 0) {
                element.textContent = text.slice(0, i - 1);
                i--;
                setTimeout(erase, eraseSpeed);
            } else {
                callback();
            }
        }
        erase();
    }

    function showNextPhrase() {
        const currentPhrase = phrases[currentPhraseIndex];
        const nextIndex = (currentPhraseIndex + 1) % phrases.length;
        const nextPhrase = phrases[nextIndex];

        // 1. Печатаем текущий текст
        typeText(currentPhrase, currentPhrase.getAttribute('data-text'), function() {
            // 2. Держим текст
            setTimeout(function() {
                // 3. Стираем текст
                eraseText(currentPhrase, function() {
                    // 4. Переходим к следующей фразе
                    currentPhraseIndex = nextIndex;

                    // ВАЖНО: сбрасываем opacity для новой фразы
                    currentPhrase.style.opacity = 1; // Возвращаем видимость

                    showNextPhrase(); // Запускаем следующий цикл
                });
            }, holdTime);
        });
    }

    // Запуск анимации
    showNextPhrase();
});

ymaps.ready(function () {
    var myMap = new ymaps.Map('yandex-map', {
        center: [55.792837, 49.168793], // Замените на координаты вашего адреса
        zoom: 16 // Масштаб карты (16 — достаточно детальный)
    });

    // Добавляем метку с адресом
    var myPlacemark = new ymaps.Placemark([55.792837, 49.168793], {
        hintContent: 'FREEDOM',
        balloonContent: 'г. Казань, ул. Патриса Лумумбы, д. 4 (здание ДОСААФ), 5 этаж, кабинет 502'
    }, {
        iconLayout: 'default#image',
        iconImageHref: '/Content/лого без фона.png',
        iconImageSize: [40, 40],
        iconImageOffset: [-5, -5]
    });

    myMap.geoObjects.add(myPlacemark);
});

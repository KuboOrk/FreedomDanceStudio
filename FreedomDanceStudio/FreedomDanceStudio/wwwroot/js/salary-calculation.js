document.addEventListener('DOMContentLoaded', function() {
    // Получаем основные элементы (с проверкой на существование)
    const paymentTypeSelect = document.getElementById('paymentType');
    const percentageSection = document.getElementById('percentageSection');
    const form = document.getElementById('salaryCalculationForm');
    const employeeSelect = document.getElementById('employeeSelect');
    const modal = document.getElementById('salaryCalculationModal');

    // Защитное программирование: проверяем, что все ключевые элементы найдены
    if (!paymentTypeSelect || !percentageSection || !form || !employeeSelect || !modal) {
        console.error('Критические элементы DOM не найдены. Проверьте HTML.');
        return;
    }

    // 1. Показать/скрыть секцию процента в зависимости от типа оплаты
    paymentTypeSelect.addEventListener('change', function() {
        percentageSection.style.display = this.value === 'Percentage' ? 'block' : 'none';
        calculateAmount(); // Пересчитываем сумму при смене типа оплаты
    });

    // 2. Динамический расчёт суммы (на основе текущих значений полей)
    function calculateAmount() {
        const paymentType = paymentTypeSelect.value;
        const hourlyRate = parseFloat(document.getElementById('HourlyRate').value) || 0;
        // Теперь берём из редактируемых полей
        const totalHours = parseFloat(document.getElementById('TotalHours').value) || 0;
        const totalVisits = parseInt(document.getElementById('TotalVisits').value) || 0;

        let amount = 0;

        switch (paymentType) {
            case 'Hourly':
                amount = hourlyRate * totalHours;
                break;
            case 'PerVisit':
                const visitRate = hourlyRate * 0.1; // 10 % от часовой ставки
                amount = visitRate * totalVisits;
                break;
            case 'Percentage':
                const percentageRate = parseFloat(document.getElementById('PercentageRate').value) || 0;
                // Пример: 5 % от условного дохода студии (10 000 ₽) × процентная ставка
                amount = 0.05 * 10000 * percentageRate;
                break;
        }

        // Форматируем и выводим сумму в рублях
        document.getElementById('calculatedAmount').textContent = new Intl.NumberFormat('ru-RU', {
            style: 'currency',
            currency: 'RUB'
        }).format(amount);
    }

    // 3. Обработчики для мгновенного пересчёта при вводе
    document.getElementById('TotalHours').addEventListener('input', calculateAmount);
    document.getElementById('TotalVisits').addEventListener('input', calculateAmount);

    // 4. Валидация входных данных
    function validateInputs() {
        const totalHours = parseFloat(document.getElementById('TotalHours').value);
        const totalVisits = parseInt(document.getElementById('TotalVisits').value);

        if (totalHours < 0 || totalVisits < 0) {
            alert('Количество часов и посещений не может быть отрицательным!');
            return false;
        }
        return true;
    }

    // 5. Загрузка списка сотрудников из API
    async function loadEmployees() {
        try {
            // Очищаем список и показываем статус «Загрузка…»
            employeeSelect.innerHTML = '<option value="">Загрузка сотрудников...</option>';

            const response = await fetch('/Employees/GetAll');

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const employees = await response.json();

            // Очищаем перед заполнением
            employeeSelect.innerHTML = '<option value="">-- Выберите сотрудника --</option>';

            // Заполняем список, если сотрудники есть
            if (employees && employees.length > 0) {
                employees.forEach(emp => {
                    const option = document.createElement('option');
                    option.value = emp.id;
                    option.textContent = `${emp.firstName} ${emp.lastName}`;
                    option.setAttribute('data-salary', emp.salary);
                    employeeSelect.appendChild(option);
                });
            } else {
                employeeSelect.innerHTML = '<option value="">Сотрудники не найдены</option>';
            }
        } catch (error) {
            console.error('Ошибка загрузки сотрудников:', error);
            employeeSelect.innerHTML = '<option value="">Ошибка загрузки. Проверьте консоль.</option>';
        }
    }

    // 6. Обработчик изменения выбранного сотрудника
    employeeSelect.addEventListener('change', function() {
        const selectedOption = this.options[this.selectedIndex];
        const salary = selectedOption ? parseFloat(selectedOption.getAttribute('data-salary')) : 0;

        if (salary > 0) {
            document.getElementById('HourlyRate').value = salary.toFixed(2);
            calculateAmount(); // Автоматически пересчитываем сумму
        } else {
            document.getElementById('HourlyRate').value = '';
        }
    });

    // 7. Обработчик отправки формы расчёта зарплаты
    form.addEventListener('submit', async function(e) {
        e.preventDefault();

        if (!validateInputs()) return; // Прерываем отправку при ошибке валидации

        const formData = new FormData(form);

        try {
            const response = await fetch('/SalaryCalculation/Create', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                alert('Зарплата успешно рассчитана и добавлена в финансы!');
                location.reload(); // Обновляем страницу для отображения новой транзакции
            } else {
                alert('Ошибка: ' + (result.errors?.join(', ') || 'Неизвестная ошибка'));
            }
        } catch (error) {
            console.error('Ошибка при отправке формы:', error);
            alert('Произошла ошибка при сохранении. Проверьте консоль.');
        }
    });

    // 8. События модального окна Bootstrap
    modal.addEventListener('show.bs.modal', loadEmployees); // Загрузка сотрудников при открытии
    modal.addEventListener('shown.bs.modal', function() {
        calculateAmount(); // Пересчёт суммы после показа
        document.getElementById('TotalHours').focus(); // Автофокус на поле часов
    });
});

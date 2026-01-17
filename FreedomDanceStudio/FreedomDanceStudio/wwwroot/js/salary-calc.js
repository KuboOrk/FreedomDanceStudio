document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('salaryCalcForm');
    const employeeSelect = document.getElementById('employeeSelect');
    const startDateInput = document.getElementById('startDate');
    const endDateInput = document.getElementById('endDate');
    const hourlyRateInput = document.getElementById('hourlyRate');
    const totalHoursInput = document.getElementById('totalHours');
    const totalAmountInput = document.getElementById('totalAmount');

    // Установка текущих дат по умолчанию
    const now = new Date();
    const utcNow = new Date(now.toISOString());
    startDateInput.value = utcNow.toISOString().slice(0, 16);
    endDateInput.value = utcNow.toISOString().slice(0, 16);

    // Обработчик выбора сотрудника
    employeeSelect.addEventListener('change', function() {
        if (this.value) {
            fetch(`/Employees/GetEmployeeData/${this.value}`)
                .then(response => {
                    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
                    return response.json();
                })
                .then(data => {
                    hourlyRateInput.value = data.salary || 0;
                    updateHoursAndAmount();
                })
                .catch(error => {
                    console.error('Error fetching employee data:', error);
                    alert('Ошибка загрузки данных сотрудника. Проверьте консоль для деталей.');
                    hourlyRateInput.value = '0.00';
                    totalHoursInput.value = '0.00';
                    totalAmountInput.value = '0.00';
                });
        } else {
            hourlyRateInput.value = '0.00';
            totalHoursInput.value = '0.00';
            totalAmountInput.value = '0.00';
        }
    });

    // Обработчики изменения дат
    startDateInput.addEventListener('change', updateHoursAndAmount);
    endDateInput.addEventListener('change', updateHoursAndAmount);

    function updateHoursAndAmount() {

        if (!employeeSelect.value || !startDateInput.value || !endDateInput.value) {
            return;
        }

        const start = new Date(startDateInput.value);
        const end = new Date(endDateInput.value);

        fetch(`/EmployeeWorkHours/GetHoursForPeriod?employeeId=${employeeSelect.value}&startDate=${start.toISOString()}&endDate=${end.toISOString()}`)
            .then(response => response.json())
            .then(hours => {
                totalHoursInput.value = hours.toFixed(2);
                const rate = parseFloat(hourlyRateInput.value) || 0;
                totalAmountInput.value = (rate * hours).toFixed(2);
            })
            .catch(error => {
                totalHoursInput.value = '0.00';
                totalAmountInput.value = '0.00';
            });
    }

    // Обработчик отправки формы
    form.addEventListener('submit', function(e) {

        // Проверка выбора сотрудника
        if (!employeeSelect.value) {
            e.preventDefault();
            alert('Пожалуйста, выберите сотрудника.');
            return;
        }

        // Явное преобразование значений
        totalHoursInput.value = parseFloat(totalHoursInput.value).toFixed(2);
        totalAmountInput.value = parseFloat(totalAmountInput.value).toFixed(2);

        // Дополнительная проверка на NaN
        if (isNaN(parseFloat(totalHoursInput.value)) || isNaN(parseFloat(totalAmountInput.value))) {
            e.preventDefault();
            alert('Ошибка в расчётах. Проверьте введённые данные.');
            return;
        }
    });
});

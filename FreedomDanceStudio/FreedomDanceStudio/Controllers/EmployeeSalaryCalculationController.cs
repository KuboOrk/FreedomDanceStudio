using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Admin")]
public class EmployeeSalaryCalculationController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeeSalaryCalculationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /EmployeeSalaryCalculation
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var calculations = await _context.EmployeeSalaryCalculations
            .Include(c => c.Employee)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return View(calculations);
    }

    // GET: /EmployeeSalaryCalculation/Create?employeeId=1
    // Этот метод можно оставить для прямой навигации, но модальное окно его не использует
    [HttpGet]
    public async Task<IActionResult> Create(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return NotFound();

        // Предзаполняем ставку из Employee.Salary
        var model = new EmployeeSalaryCalculation
        {
            EmployeeId = employeeId,
            HourlyRate = employee.Salary,
            StartDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-30), DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc)
        };

        // Считаем часы за период
        var totalHours = await _context.EmployeeWorkHours
            .Where(wh => wh.EmployeeId == employeeId &&
                        wh.WorkDate >= model.StartDate &&
                        wh.WorkDate <= model.EndDate)
            .SumAsync(wh => (decimal?)wh.HoursCount) ?? 0;
        model.TotalHours = totalHours;
        model.TotalAmount = model.HourlyRate * model.TotalHours;

        ViewBag.EmployeeName = employee.FirstName;
        
        // ВАЖНО: даже если вы оставляете этот метод, он не должен рендерить представление,
        // т. к. мы отказались от Create.cshtml. Вместо этого — редирект на Finance/Index
        TempData["ErrorMessage"] = "Прямой доступ к форме расчёта ЗП запрещён. Используйте модальное окно на странице финансов.";
        return RedirectToAction("Index", "Finance");
    }

        // POST: /EmployeeSalaryCalculation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeSalaryCalculation model)
        {

            // Детальная проверка EmployeeId
            if (model.EmployeeId == 0)
            {
                TempData["ErrorMessage"] = "Сотрудник не выбран. Пожалуйста, выберите сотрудника из списка.";
                Console.WriteLine("Validation failed: EmployeeId is 0");
                return RedirectToAction("Index", "Finance");
            }

            // Проверка существования сотрудника и загрузка объекта Employee
            var employee = await _context.Employees.FindAsync(model.EmployeeId);
            if (employee == null) {
                // обработка ошибки
            }
            model.Employee = employee; // присваиваем уже загруженный объект

            // Дополнительная проверка диапазона дат на сервере
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Дата окончания не может быть раньше даты начала.");
                TempData["ErrorMessage"] = "Ошибка валидации: Дата окончания не может быть раньше даты начала.";
                return RedirectToAction("Index", "Finance");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();
                
                TempData["ErrorMessage"] = $"Ошибка валидации: {string.Join("; ", errors)}";
                return RedirectToAction("Index", "Finance");
            }

            try
            {
                // Нормализация DateTime
                if (model.StartDate.Kind == DateTimeKind.Unspecified)
                    model.StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc);
                if (model.EndDate.Kind == DateTimeKind.Unspecified)
                    model.EndDate = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc);

                model.CreatedAt = DateTime.UtcNow;
                _context.Add(model);
                await _context.SaveChangesAsync();

                // Создание связанной транзакции
                var financeTransaction = new FinancialTransaction
                {
                    TransactionType = "Expense",
                    Amount = model.TotalAmount,
                    Category = "Зарплата сотрудника",
                    Description = $"Зарплата {employee.FirstName} {employee.LastName} за {model.StartDate:dd.MM}–{model.EndDate:dd.MM}",
                    IsManual = true,
                    EmployeeSalaryCalculationId = model.Id,
                    TransactionDate = model.CreatedAt
                };
                _context.FinancialTransactions.Add(financeTransaction);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Расчёт зарплаты сохранён. Транзакция добавлена в финансы.";
                return RedirectToAction("Index", "Finance");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при сохранении: {ex.Message}";
                return RedirectToAction("Index", "Finance");
            }
        }

    // POST: /EmployeeSalaryCalculation/Delete/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var calculation = await _context.EmployeeSalaryCalculations.FindAsync(id);
        if (calculation == null) return NotFound();

        // Удаляем связанную транзакцию
        var financeTransaction = await _context.FinancialTransactions
            .FirstOrDefaultAsync(ft => ft.EmployeeSalaryCalculationId == id);
        if (financeTransaction != null)
            _context.FinancialTransactions.Remove(financeTransaction);

        _context.EmployeeSalaryCalculations.Remove(calculation);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Расчёт и связанная транзакция удалены.";
        return RedirectToAction(nameof(Index));
    }
}

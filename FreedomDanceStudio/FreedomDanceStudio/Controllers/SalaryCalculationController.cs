using Microsoft.AspNetCore.Mvc;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Admin")]
[Route("[controller]")]
public class SalaryCalculationController : Controller
{
    private readonly ApplicationDbContext _context;

    public SalaryCalculationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /SalaryCalculation/Calculate?employeeId=1
    [HttpGet("Calculate")]
    public async Task<IActionResult> Calculate(int employeeId, DateTime? startDate, DateTime? endDate)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return NotFound();

        // Предзаполняем даты (последний месяц)
        startDate ??= DateTime.UtcNow.AddMonths(-1);
        endDate ??= DateTime.UtcNow.Date;

        // Считаем часы и посещения за период
        var workHours = await _context.EmployeeWorkHours
            .Where(wh => wh.EmployeeId == employeeId &&
                           wh.WorkDate >= startDate &&
                           wh.WorkDate <= endDate)
            .ToListAsync();

        var totalHours = workHours.Sum(wh => wh.HoursCount);
        var totalVisits = workHours.Sum(wh => wh.VisitsCount);

        var model = new EmployeeSalaryCalculation
        {
            EmployeeId = employeeId,
            StartDate = startDate.Value,
            EndDate = endDate.Value,
            HourlyRate = employee.Salary,
            TotalHours = totalHours,
            TotalVisits = totalVisits,
            PaymentType = "Hourly" // по умолчанию
        };

        await CalculateAmount(model); // считаем сумму

        ViewBag.EmployeeName = employee.FirstName;
        return PartialView("_SalaryCalculationModal", model);
    }

    // POST: /SalaryCalculation/Create
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeSalaryCalculation model)
    {
        if (ModelState.IsValid)
        {
            await CalculateAmount(model);

            // Создаём транзакцию расхода
            var transaction = new FinancialTransaction
            {
                TransactionType = "Expense",
                Amount = model.CalculatedAmount,
                TransactionDate = DateTime.UtcNow.Date,
                Description = $"Зарплата {model.Employee?.FirstName} за {model.StartDate:dd.MM}–{model.EndDate:dd.MM}",
                IsManual = true
            };

            _context.FinancialTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            model.FinancialTransactionId = transaction.Id;
            _context.EmployeeSalaryCalculations.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Зарплата успешно рассчитана и добавлена в финансы.";
            return Json(new { success = true, transactionId = transaction.Id });
        }

        // Если валидация не прошла
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        return Json(new { success = false, errors });
    }

    private async Task CalculateAmount(EmployeeSalaryCalculation model)
    {
        switch (model.PaymentType)
        {
            case "Hourly":
                model.CalculatedAmount = model.HourlyRate * model.TotalHours;
                break;
            case "PerVisit":
                // Предположим, ставка за посещение = 10% от часовой ставки
                var visitRate = model.HourlyRate * 0.1m;
                model.CalculatedAmount = visitRate * model.TotalVisits;
                break;
            case "Percentage":
                if (model.PercentageRate.HasValue)
                {
                    // Здесь можно добавить логику расчёта от доходов студии за период
                    // Для примера: 5% от общего дохода за период
                    var studioIncome = await _context.FinancialTransactions
                        .Where(t => t.TransactionType == "Income" &&
                                    t.TransactionDate >= model.StartDate &&
                                    t.TransactionDate <= model.EndDate)
                        .SumAsync(t => t.Amount);
                    model.CalculatedAmount = studioIncome * model.PercentageRate.Value;
                }
                break;
        }
    }
}

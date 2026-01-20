using Microsoft.AspNetCore.Mvc;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace FreedomDanceStudio.Controllers;

[Authorize]
[Route("[controller]")]
public class FinanceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FinanceController> _logger; // ← поле для логгера

    // Конструктор с внедрением зависимостей
    public FinanceController(ApplicationDbContext context, ILogger<FinanceController> logger)
    {
        _context = context;
        _logger = logger; // ← инициализация логгера
    }

    // GET: /Finance
    [HttpGet]
public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
{
    // Загрузка списка сотрудников для модального окна
    var employees = await _context.Employees
        .Select(e => new Employee
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName
        })
        .ToListAsync();
    ViewBag.Employees = employees;

    var query = _context.FinancialTransactions
        .Include(t => t.AbonnementSale)
        .ThenInclude(a => a.Client)
        .Include(t => t.EmployeeSalaryCalculation)
        .AsQueryable();

    // Если пользователь не задал даты — фильтруем по текущему месяцу
    if (!startDate.HasValue && !endDate.HasValue)
    {
        var currentYear = DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;

        // Первый день месяца в UTC
        startDate = new DateTime(currentYear, currentMonth, 1, 0, 0, 0, DateTimeKind.Utc);

        // Последний день месяца в UTC (более надёжный расчёт)
        var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
        endDate = new DateTime(currentYear, currentMonth, daysInMonth, 23, 59, 59, DateTimeKind.Utc);
    }
    else
    {
        // Конвертируем переданные пользователем даты в UTC
        if (startDate.HasValue && startDate.Value.Kind != DateTimeKind.Utc)
            startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
        if (endDate.HasValue && endDate.Value.Kind != DateTimeKind.Utc)
            endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
    }

    // Фильтрация по периоду (уже в UTC)
    if (startDate.HasValue)
        query = query.Where(t => t.TransactionDate >= startDate.Value);
    if (endDate.HasValue)
        query = query.Where(t => t.TransactionDate <= endDate.Value);

    var transactions = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();

    // Сводная статистика (только по отфильтрованным транзакциям)
    var stats = new
    {
        TotalIncome = transactions.Where(t => t.TransactionType == "Income").Sum(t => t.Amount),
        TotalExpense = transactions.Where(t => t.TransactionType == "Expense").Sum(t => t.Amount),
        Balance = transactions
            .Where(t => t.TransactionType == "Income")
            .Sum(t => t.Amount) -
            transactions.Where(t => t.TransactionType == "Expense")
            .Sum(t => t.Amount)
    };

    ViewBag.Stats = stats;
    ViewBag.StartDate = startDate;
    ViewBag.EndDate = endDate;

    return View(transactions);
}

    // GET: /Finance/Create
    [HttpGet("Create")]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Finance/Create
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePost(FinancialTransaction transaction)
    {
        if (ModelState.IsValid)
        {
            // Преобразуем TransactionDate в UTC, если Kind = Unspecified
            if (transaction.TransactionDate.Kind == DateTimeKind.Unspecified)
            {
                transaction.TransactionDate = transaction.TransactionDate.ToUniversalTime();
            }
            else if (transaction.TransactionDate.Kind == DateTimeKind.Local)
            {
                transaction.TransactionDate = TimeZoneInfo.ConvertTimeToUtc(transaction.TransactionDate);
            }

            transaction.IsManual = true;
            _context.FinancialTransactions.Add(transaction);
            await _context.SaveChangesAsync(); // Теперь сработает без ошибки
            return RedirectToAction(nameof(Index));
        }
        return View(transaction);
    }
    
    // GET: /Finance/Delete/{id}
    [HttpGet("Delete/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var transaction = await _context.FinancialTransactions
            .Include(t => t.AbonnementSale)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null)
            return NotFound();

        return View(transaction);
    }

    // POST: /Finance/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [Route("Finance/DeleteConfirmed")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var transaction = await _context.FinancialTransactions.FindAsync(id);
            if (transaction == null)
                return NotFound();

            // Проверка: нельзя удалять транзакции, связанные с продажами абонементов
            if (transaction.AbonnementSale != null)
            {
                TempData["ErrorMessage"] = "Нельзя удалить транзакцию, связанную с продажей абонемента.";
                return RedirectToAction(nameof(Index));
            }

            _context.FinancialTransactions.Remove(transaction);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Транзакция №{id} успешно удалена.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении транзакции ID {Id}", id);
            TempData["ErrorMessage"] = "Не удалось удалить транзакцию. Попробуйте позже.";
            return RedirectToAction(nameof(Index));
        }
    }
}

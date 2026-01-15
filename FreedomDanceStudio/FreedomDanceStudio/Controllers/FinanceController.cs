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
        var query = _context.FinancialTransactions
            .Include(t => t.AbonnementSale)
            .AsQueryable();

        // Фильтрация по периоду
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        var transactions = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();

        // Сводная статистика
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
            transaction.IsManual = true;
            _context.FinancialTransactions.Add(transaction);
            await _context.SaveChangesAsync();
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
    
    // GET: /Finance/SalaryCalculations
    [HttpGet("SalaryCalculations")]
    public async Task<IActionResult> SalaryCalculations()
    {
        var calculations = await _context.EmployeeSalaryCalculations
            .Include(c => c.Employee)
            .Include(c => c.FinancialTransaction)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return View(calculations);
    }
    
    #region Предзаполнение МО данными о сотруднике
    [HttpGet("GetEmployeeData")]
    [Produces("application/json")]
    public async Task<IActionResult> GetEmployeeData(int employeeId)
    {
        var employee = await _context.Employees
            .Where(e => e.Id == employeeId)
            .Select(e => new
            {
                e.Id,
                e.FirstName,
                e.LastName,
                e.Salary
            })
            .FirstOrDefaultAsync();

        if (employee == null)
            return NotFound();

        return Json(employee);
    }
    #endregion
}

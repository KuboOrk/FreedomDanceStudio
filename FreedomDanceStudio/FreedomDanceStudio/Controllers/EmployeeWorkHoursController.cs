using FreedomDanceStudio.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Admin")]
public class EmployeeWorkHoursController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeeWorkHoursController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Вспомогательный метод: нормализация всех DateTime полей
    private void NormalizeDateTimeFields(EmployeeWorkHours model)
    {
        // WorkDate: если Kind не указан, устанавливаем как UTC
        if (model.WorkDate.Kind == DateTimeKind.Unspecified)
            model.WorkDate = DateTime.SpecifyKind(model.WorkDate, DateTimeKind.Utc);
        
        // CreatedAt: если Kind не указан, устанавливаем как UTC
        if (model.CreatedAt.Kind == DateTimeKind.Unspecified)
            model.CreatedAt = DateTime.SpecifyKind(model.CreatedAt, DateTimeKind.Utc);
        // UpdatedAt: если есть значение и Kind не указан, устанавливаем как UTC
        if (model.UpdatedAt.HasValue && model.UpdatedAt.Value.Kind == DateTimeKind.Unspecified)
            model.UpdatedAt = DateTime.SpecifyKind(model.UpdatedAt.Value, DateTimeKind.Utc);
    }

    // GET: /EmployeeWorkHours
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var workHours = await _context.EmployeeWorkHours
            .Include(e => e.Employee)
            .OrderByDescending(wh => wh.WorkDate)
            .ToListAsync();
        return View(workHours);
    }

    // GET: /EmployeeWorkHours/Create?employeeId=1
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public ActionResult Create(int employeeId)
    {
        var employee = _context.Employees.Find(employeeId);
        if (employee == null) return NotFound();

        var model = new EmployeeWorkHours
        {
            EmployeeId = employeeId,
            WorkDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc)
        };
        ViewBag.EmployeeName = employee.FirstName;
        return View(model);
    }

    // POST: /EmployeeWorkHours/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(EmployeeWorkHours model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => 
                    new { x.Key, Errors = string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage)) })
                .ToList();
            foreach (var error in errors)
            {
                Console.WriteLine($"Ошибка валидации: {error.Key} — {error.Errors}");
            }
            ViewBag.EmployeeName = _context.Employees
                .Find(model.EmployeeId)?.FirstName ?? "Неизвестный";
            return View(model);
        }

        try
        {
            // Нормализуем все DateTime поля ПЕРЕД сохранением
            NormalizeDateTimeFields(model);
            // Устанавливаем CreatedAt в UTC
            model.CreatedAt = DateTime.UtcNow;
            _context.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Рабочее время успешно добавлено.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка сохранения: {ex.Message}");
            ViewBag.EmployeeName = _context.Employees
                .Find(model.EmployeeId)?.FirstName ?? "Неизвестный";
            return View(model);
        }
    }

    // GET: /EmployeeWorkHours/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var workHours = await _context.EmployeeWorkHours
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (workHours == null) return NotFound();
        return View(workHours);
    }

    // POST: /EmployeeWorkHours/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, EmployeeWorkHours model)
    {
        if (id != model.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            try
            {
                // Нормализуем все DateTime поля ПЕРЕД обновлением
                NormalizeDateTimeFields(model);
                // Устанавливаем UpdatedAt в UTC
                model.UpdatedAt = DateTime.UtcNow;
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Данные успешно обновлены.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeWorkHoursExists(model.Id))
                    return NotFound();
                else
            throw;
            }
        }
        return View(model);
    }

    // GET: /EmployeeWorkHours/Delete/5 — показываем модальное окно подтверждения
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var workHours = await _context.EmployeeWorkHours
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(m => m.Id == id);
    
        if (workHours == null) return NotFound();
        return View(workHours);
    }

    // POST: /EmployeeWorkHours/Delete/5 — обрабатываем удаление
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var workHours = await _context.EmployeeWorkHours.FindAsync(id);
            if (workHours != null)
            {
                _context.EmployeeWorkHours.Remove(workHours);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Запись успешно удалена.";
            }
            else
            {
                TempData["ErrorMessage"] = "Запись не найдена.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Ошибка удаления: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }


    private bool EmployeeWorkHoursExists(int id) =>
        _context.EmployeeWorkHours.Any(e => e.Id == id);
    
    // GET: /EmployeeWorkHours/GetHoursForPeriod?employeeId=1&startDate=...&endDate=...
    [HttpGet]
    [Produces("application/json")]
    public async Task<IActionResult> GetHoursForPeriod(int employeeId, DateTime startDate, DateTime endDate)
    {
        var totalHours = await _context.EmployeeWorkHours
            .Where(wh => wh.EmployeeId == employeeId &&
                         wh.WorkDate >= startDate &&
                         wh.WorkDate <= endDate)
            .SumAsync(wh => (decimal?)wh.HoursCount) ?? 0;

        return Json(totalHours);
    }
}

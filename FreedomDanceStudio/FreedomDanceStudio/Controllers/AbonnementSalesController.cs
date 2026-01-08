using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FreedomDanceStudio.Controllers;

[Authorize]
public class AbonnementSalesController: Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AbonnementSalesController> _logger;

    #region Конструктор
    public AbonnementSalesController(ApplicationDbContext context,ILogger<AbonnementSalesController> logger)
    {
        _context = context;
        _logger = logger;
    }
    #endregion

    // GET: /AbonnementSales
    [HttpGet]
    #region Список всех продаж абонементов
    public IActionResult Index()
    {
        var sales = _context.AbonnementSales
            .Include(s => s.Client)
            .Include(s => s.Service)
            .ToList();
        return View(sales);
    }
    #endregion

    // GET: /AbonnementSales/Create
    [HttpGet]
    [Authorize(Roles = "Admin")]
    #region Форма создания новой продажи абонемента
    public IActionResult Create()
    {
        // Заполняем выпадающие списки для выбора клиента и услуги
        ViewBag.Clients = _context.Clients
            .Select(client => new SelectListItem
            {
                Value = client.Id.ToString(),
                Text = $"{client.FirstName} {client.LastName} ({client.Phone})"
            })
            .ToList();

        ViewBag.Services = _context.Services
            .Select(service => new SelectListItem
            {
                Value = service.Id.ToString(),
                Text = $"{service.Name} — {service.Price} ₽ ({service.DurationDays} дней)"
            })
            .ToList();

        return View();
    }
    #endregion

    // POST: /AbonnementSales/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    #region Обработка отправки формы продажи абонемента
    public async Task<IActionResult> Create(AbonnementSale sale)
    {
        if (ModelState.IsValid)
        {
            var service = await _context.Services.FindAsync(sale.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("ServiceId", "Выбранная услуга не найдена в системе!");
                return View(await ReloadViewData(sale));
            }

            // Явно устанавливаем даты на основе логики приложения
            sale.StartDate = DateTime.UtcNow.Date;
            sale.EndDate = sale.StartDate.AddDays(service.DurationDays);
            sale.SaleDate = DateTime.UtcNow.Date;

            _context.AbonnementSales.Add(sale);
        
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Ошибка при сохранении в базу данных: {ex.Message}");
                return View(await ReloadViewData(sale));
            }
        }

        // Если ModelState невалиден — перезагружаем списки и возвращаем форму с ошибками
        return View(await ReloadViewData(sale));
    }
    #endregion
    
    private async Task<AbonnementSale> ReloadViewData(AbonnementSale sale)
    {
        ViewBag.Clients = await _context.Clients
            .Select(client => new SelectListItem
            {
                Value = client.Id.ToString(),
                Text = $"{client.FirstName} {client.LastName} ({client.Phone})"
            })
            .ToListAsync();

        ViewBag.Services = await _context.Services
            .Select(service => new SelectListItem
            {
                Value = service.Id.ToString(),
                Text = $"{service.Name} — {service.Price} ₽ ({service.DurationDays} дней)"
            })
            .ToListAsync();

        return sale;
    }
    // GET: /AbonnementSales/GetServiceDuration?serviceId=1
    [HttpGet]
    [Authorize]
    #region API-метод для получения срока действия услуги
    public IActionResult GetServiceDuration(int serviceId)
    {
        var service = _context.Services.Find(serviceId);
        if (service == null)
            return NotFound(new { error = "Услуга не найдена" });

        return Json(new
        {
            durationDays = service.DurationDays,
            serviceName = service.Name,
            price = service.Price
        });
    }
    #endregion
    
    // GET: /AbonnementSales/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    #region Форма редактирования продажи абонемента
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || id == 0)
            return NotFound();

        var sale = await _context.AbonnementSales
            .Include(s => s.Client)
            .Include(s => s.Service)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null)
            return NotFound();

        // Заполняем списки для выбора
        ViewBag.Clients = _context.Clients.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.FirstName} {c.LastName} ({c.Phone})"
        }).ToList();

        ViewBag.Services = _context.Services.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = $"{s.Name} — {s.Price} ₽ ({s.DurationDays} дней)"
        }).ToList();

        return View(sale);
    }
    #endregion

    // POST: /AbonnementSales/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    #region Обработка сохранения изменений
    public async Task<IActionResult> Edit(int id, AbonnementSale sale)
    {
if (id != sale.Id)
        return NotFound();

    if (ModelState.IsValid)
    {
        try
        {
            var existingSale = await _context.AbonnementSales
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingSale == null)
                return NotFound();

            // Сохраняем неизменные поля
            existingSale.ClientId = sale.ClientId;
            existingSale.ServiceId = sale.ServiceId;
            existingSale.SaleDate = sale.SaleDate;

            var entry = _context.Entry(existingSale);

            // Если услуга изменилась — пересчитываем EndDate на основе исходного StartDate
            if (entry.Property(e => e.ServiceId).IsModified)
            {
                var service = await _context.Services.FindAsync(sale.ServiceId);
                if (service != null)
                {
                    // Сохраняем исходный StartDate, пересчитываем EndDate
                    existingSale.EndDate = existingSale.StartDate.AddDays(service.DurationDays);
                }
                else
                {
                    ModelState.AddModelError("ServiceId", "Выбранная услуга не найдена!");
                    return View(await ReloadViewData(sale));
                }
            }
            else
            {
                // Если услуга не менялась, сохраняем старые даты
                existingSale.StartDate = sale.StartDate;
                existingSale.EndDate = sale.EndDate;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            ModelState.AddModelError("", $"Ошибка параллельного изменения: {ex.Message}");
            _logger.LogWarning(ex, "Конфликт параллельного изменения при редактировании продажи ID: {Id}", id);
            return View(await ReloadViewData(sale));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
            _logger.LogError(ex, "Ошибка при редактировании продажи абонемента ID: {Id}", id);
            return View(await ReloadViewData(sale));
        }
    }

    // Если ModelState невалиден, перезагружаем списки и возвращаем форму с ошибками
    return View(await ReloadViewData(sale));
}
#endregion

// GET: /AbonnementSales/Delete/5
[HttpGet]
[Authorize(Roles = "Admin")]
#region Форма подтверждения удаления
public async Task<IActionResult> Delete(int? id)
{
    if (id == null || id == 0)
        return NotFound();

    var sale = await _context.AbonnementSales
        .Include(s => s.Client)
        .Include(s => s.Service)
        .FirstOrDefaultAsync(s => s.Id == id);

    if (sale == null)
        return NotFound();

    return View(sale);
}
#endregion


// POST: /AbonnementSales/Delete/5
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
#region Обработка удаления записи
public async Task<IActionResult> DeletePost(int? id)
{
    var sale = await _context.AbonnementSales.FindAsync(id);
    if (sale == null)
        return NotFound();

    _context.AbonnementSales.Remove(sale);
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
#endregion
}
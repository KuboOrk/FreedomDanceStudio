using FreedomDanceStudio.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FreedomDanceStudio.Controllers;

[Authorize]
public class AbonnementSalesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AbonnementSalesController> _logger;
    private readonly IExpiryAlertService _alertService;

    #region Конструктор

    public AbonnementSalesController(ApplicationDbContext context, ILogger<AbonnementSalesController> logger,IExpiryAlertService alertService)
    {
        _context = context;
        _logger = logger;
        _alertService = alertService;
    }

    #endregion

    #region Список всех продаж абонементов

    // GET: /AbonnementSales
    [HttpGet]
    [ActionName("Index")]
    public async Task<IActionResult> Index(string search, int page = 1)
    {
        const int pageSize = 10;

        var sales = _context.AbonnementSales
            .Include(s => s.Client)
            .Include(s => s.Service)
            .Include(s => s.Visits)
            .AsQueryable();

        // Поиск
        if (!string.IsNullOrEmpty(search))
        {
            sales = sales.Where(s =>
                (s.Client != null &&
                 ((s.Client.FirstName != null && s.Client.FirstName.Contains(search)) ||
                  (s.Client.LastName != null && s.Client.LastName.Contains(search)))) ||
                (s.Service != null && s.Service.Name != null && s.Service.Name.Contains(search)));
        }

        // Пагинация
        var pagedSales = await PagedList<AbonnementSale>.CreateAsync(sales, page, pageSize);

        return View(pagedSales);
    }

    #endregion
    
    [HttpGet]
    [ActionName("Search")]
    [Produces("application/json")]
    public async Task<IActionResult> Search(string search = "")
    {
        try
        {
            var sales = _context.AbonnementSales
                .Include(s => s.Client)
                .Include(s => s.Service)
                .Include(s => s.Visits)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                sales = sales.Where(s =>
                    (s.Client != null &&
                     ((s.Client.FirstName != null && s.Client.FirstName.ToLower().Contains(search)) ||
                      (s.Client.LastName != null && s.Client.LastName.ToLower().Contains(search)))) ||
                    (s.Service != null && s.Service.Name != null && s.Service.Name.ToLower().Contains(search)));
            }

            var result = await sales.Select(s => new
            {
                Id = s.Id,
                ClientName = s.Client != null
                    ? $"{s.Client.FirstName ?? ""} {s.Client.LastName ?? ""}".Trim()
                    : "Клиент не найден",
                ServiceName = s.Service != null ? s.Service.Name : "Услуга не найдена",
                SaleDate = s.SaleDate.ToString("dd.MM.yyyy"),
                StartDate = s.StartDate.ToString("dd.MM.yyyy"),
                EndDate = s.EndDate.ToString("dd.MM.yyyy"),
                VisitCount = s.Visits.Count(),
                MaxVisits = s.MaxVisits
            }).ToListAsync();

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в методе Search");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера", details = ex.Message });
        }
    }

    #region Форма создания новой продажи абонемента

    // GET: /AbonnementSales/Create
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        // Инициализируем модель с текущей датой по умолчанию
        var newSale = new AbonnementSale
        {
            StartDate = DateTime.UtcNow.Date // Текущая дата по умолчанию
        };

        // Заполняем выпадающие списки
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

        return View(newSale);
    }

    #endregion

    #region Обработка отправки формы продажи абонемента

    // POST: /AbonnementSales/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [Bind("Id,ClientId,ServiceId,SaleDate,StartDate,EndDate,MaxVisits")] AbonnementSale sale)
    {
        if (ModelState.IsValid)
        {
            var service = await _context.Services.FindAsync(sale.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("ServiceId", "Выбранная услуга не найдена в системе!");
                return View(await ReloadViewData(sale));
            }

            // Валидация MaxVisits
            if (sale.MaxVisits < 0)
            {
                ModelState.AddModelError("MaxVisits", "Количество посещений не может быть отрицательным");
                return View(await ReloadViewData(sale));
            }

            // Берём StartDate из модели или используем текущую дату
            sale.StartDate = sale.StartDate == default ? DateTime.UtcNow.Date : sale.StartDate;

            // Пересчитываем EndDate после всех проверок
            sale.EndDate = sale.StartDate.AddDays(service.DurationDays);

            // SaleDate всегда текущая дата
            sale.SaleDate = DateTime.UtcNow.Date;

            _context.AbonnementSales.Add(sale);

            try
            {
                await _context.SaveChangesAsync();
                // Создаём запись о доходе от продажи абонемента
                var incomeTransaction = new FinancialTransaction
                {
                    TransactionType = "Income",
                    Amount = service.Price,
                    Description = $"Продажа абонемента: {service.Name}",
                    TransactionDate = sale.SaleDate,
                    AbonnementSaleId = sale.Id,
                    IsManual = false
                };
                _context.FinancialTransactions.Add(incomeTransaction);
                await _context.SaveChangesAsync(); // Сохраняем транзакцию
                // Пересчитываем индикатор для этого абонемента
                await _alertService.UpdateExpiryAlertForSaleAsync(sale.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Ошибка при сохранении в базу данных: {ex.Message}");
                return View(await ReloadViewData(sale));
            }
        }

        // Если ModelState невалиден — возвращаем форму с ошибками
        return View(await ReloadViewData(sale));
    }

    #endregion

    private async Task<AbonnementSale> ReloadViewData(AbonnementSale? sale)
    {
        if (sale == null)
            sale = new AbonnementSale();

        // Явно инициализируем StartDate, если он не задан
        if (sale.StartDate == default)
            sale.StartDate = DateTime.UtcNow.Date;

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

    #region API-метод для получения срока действия услуги

    // GET: /AbonnementSales/GetServiceDuration?serviceId=1
    [HttpGet]
    [Authorize]
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

    #region Форма редактирования продажи абонемента

    // GET: /AbonnementSales/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || id == 0)
            return NotFound();

        var sale = await _context.AbonnementSales
            .Include(s => s.Client)
            .Include(s => s.Service)
            .Include(s => s.Visits) // ВАЖНО: загружаем посещения для проверки лимита
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null)
            return NotFound();

        // ПРОВЕРКА: если лимит посещений исчерпан, запрещаем редактирование
        if (sale.MaxVisits > 0 && sale.Visits.Count >= sale.MaxVisits)
        {
            TempData["EditError"] = "Редактирование запрещено: лимит посещений исчерпан.";
            _logger.LogWarning("Попытка редактирования абонемента ID {Id} с исчерпанным лимитом посещений", id);
            return RedirectToAction(nameof(Index));
        }

        // Заполняем списки для выбора
        ViewBag.Clients = _context.Clients.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.FirstName} {c.LastName} ({c.Phone})"
            })
            .ToList();

        ViewBag.Services = _context.Services.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Name} — {s.Price} ₽ ({s.DurationDays} дней)"
            })
            .ToList();

        return View(sale);
    }

    #endregion

    #region Обработка сохранения изменений

    // POST: /AbonnementSales/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
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

                // Валидация MaxVisits
                if (sale.MaxVisits < 0)
                {
                    ModelState.AddModelError("MaxVisits", "Количество посещений не может быть отрицательным");
                    return View(await ReloadViewData(sale));
                }

                // Сохраняем неизменные поля
                existingSale.ClientId = sale.ClientId;
                existingSale.ServiceId = sale.ServiceId;
                existingSale.MaxVisits = sale.MaxVisits;

                var entry = _context.Entry(existingSale);

                // Если услуга изменилась — пересчитываем EndDate на основе StartDate из модели
                if (entry.Property(e => e.ServiceId).IsModified)
                {
                    var service = await _context.Services.FindAsync(sale.ServiceId);
                    if (service != null)
                    {
                        // Берём StartDate из модели (может быть изменён пользователем)
                        existingSale.StartDate = sale.StartDate;

                        // Пересчитываем EndDate
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
                    // Если услуга не менялась, сохраняем даты из модели
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

    #region Форма подтверждения удаления

    // GET: /AbonnementSales/Delete/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
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

    #region Обработка удаления записи

    // POST: /AbonnementSales/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
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
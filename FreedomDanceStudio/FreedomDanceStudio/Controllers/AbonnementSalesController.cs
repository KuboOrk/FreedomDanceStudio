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
        _logger.LogInformation("POST Create called. Model: ClientId={ClientId}, ServiceId={ServiceId}",
        sale.ClientId, sale.ServiceId);

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

        // Преобразование дат в UTC
        sale.StartDate = DateTime.SpecifyKind(
            (sale.StartDate == default ? DateTime.UtcNow.Date : sale.StartDate),
            DateTimeKind.Utc);
        
        sale.EndDate = DateTime.SpecifyKind(sale.StartDate.AddDays(service.DurationDays), DateTimeKind.Utc);
        sale.SaleDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

        _context.AbonnementSales.Add(sale);

        try
        {
            await _context.SaveChangesAsync();

            var client = await _context.Clients.FindAsync(sale.ClientId);
            if (client == null)
            {
                ModelState.AddModelError("ClientId", "Клиент не найден!");
                return View(await ReloadViewData(sale));
            }

            var incomeTransaction = new FinancialTransaction
            {
                TransactionType = "Income",
                Amount = service.Price,
                Description = $"Продажа абонемента: {service.Name} (клиент: {client.FirstName} {client.LastName})", // Подставляем реальные значения
                TransactionDate = sale.SaleDate,
                AbonnementSaleId = sale.Id,
                IsManual = false
            };
            _context.FinancialTransactions.Add(incomeTransaction);
            await _context.SaveChangesAsync();

            await _alertService.UpdateExpiryAlertForSaleAsync(sale.Id);
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("", $"Ошибка БД: {ex.Message}");
            _logger.LogError(ex, "Ошибка сохранения AbonnementSale ID={SaleId}", sale.Id);
            return View(await ReloadViewData(sale));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Неожиданная ошибка: {ex.Message}");
            _logger.LogError(ex, "Критическая ошибка при создании продажи");
            return View(await ReloadViewData(sale));
        }
    }

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
public async Task<IActionResult> Edit
    (int id, [Bind("Id,ClientId,ServiceId,SaleDate,StartDate,EndDate")] AbonnementSale sale)
{
    if (id != sale.Id)
        return NotFound();

    if (ModelState.IsValid)
    {
        try
        {
            var existingSale = await _context.AbonnementSales
                .Include(s => s.FinancialTransactions)
                .Include(s => s.Visits)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingSale == null)
                return NotFound();

            // Валидация MaxVisits
            if (sale.MaxVisits != existingSale.MaxVisits && sale.MaxVisits < existingSale.Visits.Count)
            {
                ModelState.AddModelError("MaxVisits", 
                    $"Нельзя установить лимит {sale.MaxVisits} — уже использовано {existingSale.Visits.Count} посещений");
            }
            
            // Если MaxVisits не передан (null), устанавливаем значение из БД
            if (sale.MaxVisits == 0) // или null, если тип nullable int
            {
                var existing = await _context.AbonnementSales.FindAsync(sale.Id);
                sale.MaxVisits = existing.MaxVisits;
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            bool success = false;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Обновляем поля из модели
                    existingSale.ClientId = sale.ClientId;
                    existingSale.ServiceId = sale.ServiceId;
                    existingSale.MaxVisits = sale.MaxVisits;

                    // Конвертируем даты в UTC
                    existingSale.SaleDate = DateTime.SpecifyKind(sale.SaleDate, DateTimeKind.Utc);
                    existingSale.StartDate = DateTime.SpecifyKind(sale.StartDate, DateTimeKind.Utc);
                    existingSale.EndDate = DateTime.SpecifyKind(sale.EndDate, DateTimeKind.Utc);

                    var entry = _context.Entry(existingSale);

                    // Если услуга изменилась — пересчитываем EndDate
                    if (entry.Property(e => e.ServiceId).IsModified)
                    {
                        var service = await _context.Services.FindAsync(sale.ServiceId);
                        if (service != null)
                        {
                            existingSale.EndDate = DateTime.SpecifyKind(
                                existingSale.StartDate.AddDays(service.DurationDays),
                                DateTimeKind.Utc);
                        }
                        else
                        {
                            ModelState.AddModelError("ServiceId", "Выбранная услуга не найдена!");
                            throw new InvalidOperationException("Услуга не найдена");
                        }
                    }

                    // Сохраняем изменения продажи
                    await _context.SaveChangesAsync();

                    // Синхронизация транзакции дохода — ДОРАБОТАННЫЙ БЛОК
                    var incomeTransaction = existingSale.FinancialTransactions
                        .FirstOrDefault(t => t.TransactionType == "Income");

                    if (incomeTransaction != null)
                    {
                        // Явно загружаем данные клиента по ID
                        var client = await _context.Clients.FindAsync(existingSale.ClientId);
                        if (client == null)
                        {
                            ModelState.AddModelError("", "Клиент не найден — невозможно обновить описание транзакции.");
                            throw new Exception("Клиент не найден");
                        }

                        var currentService = await _context.Services.FindAsync(existingSale.ServiceId);
                        if (currentService != null)
                        {
                            bool shouldUpdate = false;

                            if (incomeTransaction.TransactionDate != existingSale.SaleDate)
                            {
                                incomeTransaction.TransactionDate = existingSale.SaleDate;
                                shouldUpdate = true;
                            }

                            if (incomeTransaction.Amount != currentService.Price)
                            {
                                incomeTransaction.Amount = currentService.Price;
                                shouldUpdate = true;
                            }

                            // Формируем полное описание с именем клиента
                            incomeTransaction.Description = $"Продажа абонемента: {currentService.Name} (клиент: {client.FirstName} {client.LastName})";
                            shouldUpdate = true;

                            if (shouldUpdate)
                            {
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                    else
                    {
                        // Если транзакции нет — создаём новую
                        var service = await _context.Services.FindAsync(existingSale.ServiceId);
                        var client = await _context.Clients.FindAsync(existingSale.ClientId); // Загружаем клиента

                        if (service != null && client != null)
                        {
                            var newIncomeTransaction = new FinancialTransaction
                            {
                                TransactionType = "Income",
                                Amount = service.Price,
                                Description = $"Продажа абонемента: {service.Name} (клиент: {client.FirstName} {client.LastName})", // Описание с именем клиента
                                TransactionDate = existingSale.SaleDate,
                                AbonnementSaleId = existingSale.Id,
                                IsManual = false
                            };
                            _context.FinancialTransactions.Add(newIncomeTransaction);
                            await _context.SaveChangesAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    success = true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            if (success)
            {
                // ДОБАВЛЕН ВЫЗОВ ОБНОВЛЕНИЯ АЛЕРТОВ ПОСЛЕ СОХРАНЕНИЯ ИЗМЕНЕНИЙ
                await _alertService.UpdateExpiryAlertForSaleAsync(existingSale.Id);

                TempData["SuccessMessage"] = "Изменения сохранены успешно";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(await ReloadViewData(sale));
            }
        }
        catch (InvalidOperationException ex) when (ex.Message == "Услуга не найдена")
        {
            return View(await ReloadViewData(sale));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
            _logger.LogError("Ошибка при редактировании продажи абонемента ID: {Id}", id);
            return View(await ReloadViewData(sale));
        }
    }

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
using FreedomDanceStudio.Attributes;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreedomDanceStudio.Controllers;

[Authorize(Roles = "Admin")]
public class ClientVisitsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientVisitsController> _logger;
    private readonly IExpiryAlertService _alertService; // добавлено

    public ClientVisitsController(ApplicationDbContext context, ILogger<ClientVisitsController> logger,IExpiryAlertService alertService)
    {
        _context = context;
        _logger = logger;
        _alertService = alertService;
    }

    #region Отметить посещение клиента

    // POST: Отметить посещение
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> MarkVisit(int abonnementSaleId)
{
    try
    {
        var abonnementSale = await _context.AbonnementSales
            .Include(s => s.Visits)
            .FirstOrDefaultAsync(s => s.Id == abonnementSaleId);

        if (abonnementSale == null)
            return NotFound(new
            {
                success = false,
                message = "Абонемент не найден"
            });

        // Проверка даты действия абонемента (в UTC)
        if (abonnementSale.EndDate < DateTime.UtcNow.Date)
            return BadRequest(new
            {
                success = false,
                message = "Абонемент истёк!"
            });

        // Проверка лимита посещений
        var currentVisitCount = abonnementSale.Visits?.Count ?? 0;
        bool isVisitAllowed = true;

        if (abonnementSale.MaxVisits > 0 && currentVisitCount >= abonnementSale.MaxVisits)
        {
            isVisitAllowed = false;
            return BadRequest(new
            {
                success = false,
                message = $"Лимит посещений ({abonnementSale.MaxVisits}) исчерпан! Осталось посещений: {abonnementSale.MaxVisits - currentVisitCount}",
                isVisitAllowed = isVisitAllowed
            });
        }

        // Создание записи о посещении (UTC)
        var visit = new ClientVisit
        {
            AbonnementSaleId = abonnementSaleId,
            VisitDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
        };

        _context.ClientVisits.Add(visit);
        await _context.SaveChangesAsync();

        // ПЕРЕСЧЁТ АЛЕ́РТОВ ДЛЯ ЭТОГО АБОНЕМЕНТА
        await _alertService.UpdateExpiryAlertForSaleAsync(abonnementSaleId);

        // Получаем все але́рты и фильтруем в памяти
        var allAlerts = await _alertService.GetAllAbonnementAlertsAsync();
        var updatedAlert = allAlerts.FirstOrDefault(a => a.AbonnementSaleId == abonnementSaleId);

        return Json(new
        {
            success = true,
            message = "Посещение отмечено!",
            visitCount = updatedAlert?.UsedVisits ?? (currentVisitCount + 1),
            remainingVisits = abonnementSale.MaxVisits - (updatedAlert?.UsedVisits ?? (currentVisitCount + 1)),
            alertData = updatedAlert
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Ошибка при отметке посещения для абонемента ID: {AbonnementSaleId}", abonnementSaleId);
        return StatusCode(500, new
        {
            success = false,
            message = "Ошибка сервера"
        });
    }
}

    #endregion

    #region Получить историю посещений

    // GET: История посещений для конкретного абонемента
    [HttpGet]
    public async Task<IActionResult> GetVisitHistory(int abonnementSaleId)
    {
        var visits = await _context.ClientVisits
            .Where(v => v.AbonnementSaleId == abonnementSaleId)
            .OrderByDescending(v => v.VisitDate)
            .Select(v => new
            {
                VisitDate = v.VisitDate.ToString("yyyy-MM-ddTHH:mm:ss") // ISO-формат
            })
            .ToListAsync();

        return Json(visits);
    }

    #endregion
}
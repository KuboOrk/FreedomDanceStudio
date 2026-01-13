using FreedomDanceStudio.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace FreedomDanceStudio.Controllers;

public class HomeController : Controller
{
    private readonly IExpiryAlertService _alertService;

    public HomeController(IExpiryAlertService alertService)
    {
        _alertService = alertService;
    }

    #region Главная страница с индикаторами

    [HttpGet]
    [ActionName("Index")]
    public async Task<IActionResult> Index()
    {
        // Обновляем оповещения (можно вынести в фоновое задание)
        await _alertService.UpdateExpiryAlertsAsync();
        
        // Получаем ближайшие оповещения
        var alerts = await _alertService.GetUpcomingExpiryAlertsAsync(30);
        ViewBag.ExpiryAlerts = alerts;
        return View();
    }

    #endregion
}
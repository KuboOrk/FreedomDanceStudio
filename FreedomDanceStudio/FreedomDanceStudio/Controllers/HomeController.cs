using FreedomDanceStudio.Attributes;
using Microsoft.AspNetCore.Mvc;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;

namespace FreedomDanceStudio.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IExpiryAlertService _alertService;

    public HomeController(IExpiryAlertService alertService)
    {
        _alertService = alertService;
    }

    #region Главная страница (серверная загрузка)

    [HttpGet]
    [ActionName("Index")]
    public async Task<IActionResult> Index()
    {
        var alerts = await _alertService.GetAllAbonnementAlertsAsync();
        var model = new HomeViewModel
        {
            AllAbonnementAlerts = alerts
        };
        return View(model);
    }

    #endregion

    #region API для AJAX‑обновлений

    [HttpGet]
    [Produces("application/json")]
    [Route("Home/GetAllAbonnementAlerts")]
    public async Task<IActionResult> GetAllAbonnementAlerts()
    {
        var alerts = await _alertService.GetAllAbonnementAlertsAsync();
        return Json(alerts);
    }

    #endregion
}
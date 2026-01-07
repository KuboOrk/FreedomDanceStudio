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

    #region Конструктор
    public AbonnementSalesController(ApplicationDbContext context)
    {
        _context = context;
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
            // Получаем выбранную услугу для расчёта срока действия
            var service = await _context.Services.FindAsync(sale.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("", "Услуга не найдена!");
                return View(sale);
            }

            // Рассчитываем даты начала и окончания действия абонемента
            sale.StartDate = DateTime.UtcNow.Date; // Абонемент начинается сегодня
            sale.EndDate = sale.StartDate.AddDays(service.DurationDays);
            sale.SaleDate = DateTime.UtcNow.Date;

            _context.AbonnementSales.Add(sale);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Если ModelState невалиден, повторно заполняем списки
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

        return View(sale);
    }
    #endregion
}
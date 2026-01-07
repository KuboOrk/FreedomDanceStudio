    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using FreedomDanceStudio.Data;
    using FreedomDanceStudio.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;

    namespace FreedomDanceStudio.Controllers
    {
        public class ClientSubscriptionsController : Controller
        {
            private readonly ApplicationDbContext _context;

            public ClientSubscriptionsController(ApplicationDbContext context)
            {
                _context = context;
            }

            // GET: /ClientSubscriptions — список абонементов
            public IActionResult Index()
            {
                var subscriptions = _context.ClientSubscriptions
                    .Include(cs => cs.Client)
                    .Include(cs => cs.Service)
                    .Where(cs => cs.IsActive)
                    .ToList();
                return View(subscriptions);
            }

            // GET: /ClientSubscriptions/Create — форма создания
            public IActionResult Create()
            {
                ViewBag.Clients = _context.Clients
                    .Where(c => c.IsActive)
                    .ToList();
                
                ViewBag.Services = _context.Services
                    .Where(s => s.IsActive)
                    .ToList();
                return View();
            }

            // POST: /ClientSubscriptions/Create — обработка отправки формы
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(ClientSubscription subscription)
            {
                if (ModelState.IsValid)
                {
                    var service = await _context.Services.FindAsync(subscription.ServiceId);
                    if (service != null)
                    {
                        subscription.EndDate = subscription.StartDate.AddDays(service.DurationDays);
                    }
                    subscription.PaymentDate = DateTime.Now;
                    _context.Add(subscription);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                // Повторное заполнение списков при ошибке валидации
                ViewBag.Clients = _context.Clients
                    .Where(c => c.IsActive)
                    .ToList();
                ViewBag.Services = _context.Services
                    .Where(s => s.IsActive)
                    .ToList();
                return View(subscription);
            }
            
            // AJAX: /ClientSubscriptions/CalculateEndDate
            [HttpGet]
            public JsonResult CalculateEndDate(int serviceId, DateTime startDate)
            {
                var service = _context.Services.Find(serviceId);
                if (service != null)
                {
                    var endDate = startDate.AddDays(service.DurationDays);
                    return Json(new { endDate = endDate.ToString("dd.MM.yyyy") });
                }
                return Json(null);
            }

            // GET: /ClientSubscriptions/Edit/5 — форма редактирования
            public async Task<IActionResult> Edit(int id)
            {
                var subscription = await _context.ClientSubscriptions
                    .Include(cs => cs.Client)
                    .Include(cs => cs.Service)
                    .FirstOrDefaultAsync(cs => cs.Id == id);

                if (subscription == null) return NotFound();

                ViewBag.Clients = _context.Clients
                    .Where(c => c.IsActive)
                    .ToList();
                ViewBag.Services = _context.Services
                    .Where(s => s.IsActive)
                    .ToList();
                return View(subscription);
            }

            // POST: /ClientSubscriptions/Edit/5 — сохранение изменений
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, ClientSubscription subscription)
            {
                if (id != subscription.Id) return BadRequest();

                if (ModelState.IsValid)
                {
                    try
                    {
                        // Перерасчёт даты окончания при изменении сервиса или даты начала
                var service = await _context.Services.FindAsync(subscription.ServiceId);
                if (service != null)
                {
                    subscription.EndDate = subscription.StartDate.AddDays(service.DurationDays);
                }

                _context.Update(subscription);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientSubscriptionExists(subscription.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // Повторное заполнение списков при ошибке валидации
        ViewBag.Clients = _context.Clients
            .Where(c => c.IsActive)
            .ToList();
        ViewBag.Services = _context.Services
            .Where(s => s.IsActive)
            .ToList();
        return View(subscription);
    }

    // GET: /ClientSubscriptions/Delete/5 — подтверждение удаления
    public async Task<IActionResult> Delete(int id)
    {
        var subscription = await _context.ClientSubscriptions
            .Include(cs => cs.Client)
            .Include(cs => cs.Service)
            .FirstOrDefaultAsync(cs => cs.Id == id);

        if (subscription == null) return NotFound();
        return View(subscription);
    }

    // POST: /ClientSubscriptions/Delete/5 — фактическое удаление
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var subscription = await _context.ClientSubscriptions.FindAsync(id);
        _context.ClientSubscriptions.Remove(subscription);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ClientSubscriptionExists(int id) =>
        _context.ClientSubscriptions.Any(cs => cs.Id == id);
    } 
    }

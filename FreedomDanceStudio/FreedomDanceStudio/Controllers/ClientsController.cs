using Microsoft.AspNetCore.Mvc;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace FreedomDanceStudio.Controllers;

[Authorize]
public class ClientsController: Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(ApplicationDbContext context,
        ILogger<ClientsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Поиск клиентов
    
    // GET: /Clients
    [HttpGet]
    [ActionName("Index")]
    public async Task<IActionResult> Index(string search, int page = 1)
    {
        const int pageSize = 10;
        
        _logger.LogInformation("Вход в метод Index. Параметры: search='{Search}', page={Page}", search, page);

        var clients = _context.Clients.AsQueryable();

        // Поиск
        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            clients = clients.Where(c =>
                (c.FirstName != null && c.FirstName.ToLower().Contains(search)) ||
                (c.LastName != null && c.LastName.ToLower().Contains(search)) ||
                (c.Phone != null && c.Phone.ToLower().Contains(search)) ||
                (c.Email != null && c.Email.ToLower().Contains(search)));
            
            _logger.LogInformation("Выполнен поиск по запросу: '{Search}'", search);
        }

        // Пагинация
        var pagedClients = await PagedList<Client>.CreateAsync(clients, page, pageSize);

        _logger.LogInformation(
            "Пагинация: текущая страница={CurrentPage}, всего страниц={TotalPages}, размер страницы={PageSize}",
            pagedClients.CurrentPage, pagedClients.TotalPages, pageSize);

        return View(pagedClients);
    }

    // AJAX: /Clients/Search
    [HttpGet]
    [ActionName("Search")]
    [Produces("application/json")]
    public async Task<IActionResult> Search(string search = "", int page = 1)
    {
        try
        {
            const int pageSize = 10;
            
            _logger.LogInformation("Вход в метод Search. Параметры: search='{Search}', page={Page}", search, page);

            IQueryable<Client> query = _context.Clients;

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    (c.FirstName != null && c.FirstName.ToLower().Contains(search)) ||
                    (c.LastName != null && c.LastName.ToLower().Contains(search)) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(search)) ||
                    (c.Email != null && c.Email.ToLower().Contains(search)));
                
                _logger.LogInformation("Фильтрация по поисковому запросу: '{Search}'", search);
            }

            // Создаём пагинированный список
            var pagedClients = await PagedList<Client>.CreateAsync(query, page, pageSize);
            
            _logger.LogInformation("Пагинация выполнена: текущая страница={CurrentPage}, всего страниц={TotalPages}",
                pagedClients.CurrentPage, pagedClients.TotalPages);

            // Явно указываем типы в Select
            var result = pagedClients.Select(client => new
            {
                Id = client.Id,
                FirstName = client.FirstName ?? "",
                LastName = client.LastName ?? "",
                Phone = client.Phone ?? "",
                Email = client.Email ?? "-"
            }).ToList();
            
            _logger.LogInformation("Сформирован ответ: {Count} записей, параметры пагинации: CurrentPage={CurrentPage}, TotalPages={TotalPages}",
                result.Count, pagedClients.CurrentPage, pagedClients.TotalPages);

            return Json(new
            {
                Clients = result,
                Pagination = new
                {
                    CurrentPage = pagedClients.CurrentPage,
                    TotalPages = pagedClients.TotalPages,
                    HasPreviousPage = pagedClients.HasPreviousPage,
                    HasNextPage = pagedClients.HasNextPage,
                    PageSize = pageSize
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Ошибка в методе Search: {Message}\nStackTrace: {StackTrace}", ex.Message, ex.StackTrace);
            return StatusCode(500, new { error = "Внутренняя ошибка сервера", details = ex.Message });
        }
    }
        #endregion

    #region Обработчик создания абонементов
    
    // GET: /Clients/Create
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ActionName("Create")]
    public IActionResult Create()
    {
        _logger.LogInformation("Вход в метод Create (GET)");
        return View();
    }

    // POST: /Clients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [ActionName("Create")]
    public async Task<IActionResult> CreatePost(Client client)
    {
        _logger.LogInformation("Вход в метод CreatePost. ClientId={ClientId}, Name={FirstName} {LastName}",
            client.Id, client.FirstName, client.LastName);
        
        if (ModelState.IsValid)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Клиент создан успешно. ClientId={ClientId}", client.Id);
            return RedirectToAction(nameof(Index));
        }
        
        _logger.LogWarning("Модель невалидна при создании клиента. ClientId={ClientId}", client.Id);
        return View(client);
    }
    #endregion

    #region Обработчик редактирования абонементов
    
    // GET: /Clients/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ActionName("Edit")]
    public IActionResult Edit(int? id)
    {
        _logger.LogInformation("Вход в метод Edit (GET). Id={Id}", id);

        if (id == null || id == 0)
        {
            _logger.LogWarning("Некорректный Id при вызове Edit: {Id}", id);
            return NotFound();
        }

        var client = _context.Clients.Find(id);
        if (client == null)
        {
            _logger.LogWarning("Клиент не найден при вызове Edit. Id={Id}", id);
            return NotFound();
        }

        return View(client);
    }

    // POST: /Clients/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(int id, Client client)
    {
        _logger.LogInformation("Вход в метод EditPost. Id={Id}, ClientId={ClientId}", id, client.Id);

        if (id != client.Id)
        {
            _logger.LogWarning("Id из маршрута не совпадает с Id модели. RouteId={Id}, ModelId={ClientId}", id, client.Id);
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _context.Update(client);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Клиент обновлён успешно. ClientId={ClientId}", client.Id);
            return RedirectToAction(nameof(Index));
        }
        
        _logger.LogWarning("Модель невалидна при обновлении клиента. ClientId={ClientId}", client.Id);
        return View(client);
    }
    #endregion

    #region Удаление
    
    // POST: /Clients/Delete/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int? id)
    {
        _logger.LogInformation("Вход в метод DeletePost. Id={Id}", id);

        if (id == null || id == 0)
        {
            _logger.LogWarning("Некорректный Id при вызове DeletePost: {Id}", id);
            return NotFound();
        }

        var client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            _logger.LogWarning("Клиент не найден при удалении. Id={Id}", id);
            return NotFound();
        }

        try
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Клиент удалён успешно. ClientId={ClientId}", client.Id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError("Ошибка при удалении клиента Id={ClientId}: {Message}\nStackTrace: {StackTrace}",
                client.Id, ex.Message, ex.StackTrace);

            ModelState.AddModelError("", "Произошла ошибка при удалении клиента. Попробуйте снова.");
            return View("Index", client);
        }
    }
     #endregion
     
     #region Вспомогательные методы
     
     private bool ClientExists(int id)
     {
         var exists = _context.Clients.Any(e => e.Id == id);
         _logger.LogDebug("Проверка существования клиента Id={Id}: {Exists}", id, exists);
         return exists;
     }
     #endregion
}
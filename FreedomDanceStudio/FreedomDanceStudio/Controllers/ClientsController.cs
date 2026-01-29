using Microsoft.AspNetCore.Mvc;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FreedomDanceStudio.Controllers;

[Authorize]
public class ClientsController: Controller
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Clients
    [HttpGet]
    [ActionName("Index")]
    public async Task<IActionResult> Index(string search, int page = 1)
    {
        const int pageSize = 10;

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
        }

        // Пагинация
        var pagedClients = await PagedList<Client>.CreateAsync(clients, page, pageSize);

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

            IQueryable<Client> query = _context.Clients;

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    (c.FirstName != null && c.FirstName.ToLower().Contains(search)) ||
                    (c.LastName != null && c.LastName.ToLower().Contains(search)) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(search)) ||
                    (c.Email != null && c.Email.ToLower().Contains(search)));
            }

            // Создаём пагинированный список
            var pagedClients = await PagedList<Client>.CreateAsync(query, page, pageSize);

            // Явно указываем типы в Select
            var result = pagedClients.Select(client => new
            {
                Id = client.Id,
                FirstName = client.FirstName ?? "",
                LastName = client.LastName ?? "",
                Phone = client.Phone ?? "",
                Email = client.Email ?? "-"
            }).ToList();

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
            return StatusCode(500, new { error = "Внутренняя ошибка сервера", details = ex.Message });
        }
    }

    // GET: /Clients/Create
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ActionName("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Clients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [ActionName("Create")]
    public async Task<IActionResult> CreatePost(Client client)
    {
        if (ModelState.IsValid)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(client);
    }

    // GET: /Clients/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ActionName("Edit")]
    public IActionResult Edit(int? id)
    {
        if (id == null || id == 0)
            return NotFound();

        var client = _context.Clients.Find(id);
        if (client == null)
            return NotFound();

        return View(client);
    }

    // POST: /Clients/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(int id, Client client)
    {
        if (id != client.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(client);
    }

    // POST: /Clients/Delete/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int? id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null)
            return NotFound();

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
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
    public IActionResult Index()
    {
        return View(_context.Clients.ToList());
    }
    
    // AJAX: /Clients/Search
    [HttpGet]
    [ActionName("Search")]
    [Produces("application/json")]
    public async Task<IActionResult> Search(string search = "")
    {
        var clients = _context.Clients.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            clients = clients.Where(c =>
                c.FirstName.ToLower().Contains(search) ||
                c.LastName.ToLower().Contains(search) ||
                c.Phone.ToLower().Contains(search) ||
                (c.Email != null && c.Email.ToLower().Contains(search)));
        }

        var result = await clients.Select(c => new
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Phone = c.Phone,
            Email = c.Email ?? "-"
        }).ToListAsync();

        return Json(result);
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
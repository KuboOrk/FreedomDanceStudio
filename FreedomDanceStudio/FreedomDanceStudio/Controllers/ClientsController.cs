using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;

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
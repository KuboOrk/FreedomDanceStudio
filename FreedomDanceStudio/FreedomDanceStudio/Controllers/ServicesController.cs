using Microsoft.AspNetCore.Mvc;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FreedomDanceStudio.Controllers;

[Authorize]
public class ServicesController: Controller
{
    private readonly ApplicationDbContext _context;

    public ServicesController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // GET: /Services
    [HttpGet]
    public IActionResult Index()
    {
        return View(_context.Services.ToList());
    }
    
    // AJAX: /Services/Search
    [HttpGet]
    [ActionName("Search")]
    [Produces("application/json")]
    public async Task<IActionResult> Search(string search = "")
    {
        var services = _context.Services.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            services = services.Where(s =>
                s.Name.ToLower().Contains(search) ||
                (s.Description != null && s.Description.ToLower().Contains(search)));
        }

        var result = await services.Select(s => new
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description ?? "-",
            Price = s.Price,
            DurationDays = s.DurationDays
        }).ToListAsync();

        return Json(result);
    }
    
    // GET: /Services/Create
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ActionName("Create")]
    public IActionResult Create()
    {
        return View();
    }
    
    // POST: /Services/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [ActionName("Create")]
    public async Task<IActionResult> CreatePost(Service service)
    {
        if (ModelState.IsValid)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(service);
    }
    
    // GET: /Services/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ActionName("Edit")]
    public IActionResult Edit(int? id)
    {
        if (id == null || id == 0)
            return NotFound();

        var service = _context.Services.Find(id);
        if (service == null)
            return NotFound();

        return View(service);
    }
    
    // POST: /Services/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(int id, Service service)
    {
        if (id != service.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(service);
    }

    // POST: /Services/Delete/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int? id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
            return NotFound();

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
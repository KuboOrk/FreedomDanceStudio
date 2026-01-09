using Microsoft.AspNetCore.Mvc;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;

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
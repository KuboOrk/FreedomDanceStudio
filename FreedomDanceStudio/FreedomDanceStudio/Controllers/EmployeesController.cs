using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;

namespace FreedomDanceStudio.Controllers;

[Authorize]
public class EmployeesController: Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Employees
    [HttpGet]
    public IActionResult Index()
    {
        return View(_context.Employees.ToList());
    }

    // GET: /Employees/Create
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ActionName("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Employees/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [ActionName("Create")]
    public async Task<IActionResult> CreatePost(Employee employee)
    {
        if (ModelState.IsValid)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(employee);
    }

    // GET: /Employees/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ActionName("Edit")]
    public IActionResult Edit(int? id)
    {
        if (id == null || id == 0)
            return NotFound();

        var employee = _context.Employees.Find(id);
        if (employee == null)
            return NotFound();

        return View(employee);
    }

    // POST: /Employees/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(int id, Employee employee)
    {
        if (id != employee.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(employee);
    }

    // POST: /Employees/Delete/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int? id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
using Microsoft.AspNetCore.Mvc;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
    
    // AJAX: /Employees/Search
    [HttpGet]
    [ActionName("Search")]
    [Produces("application/json")]
    public async Task<IActionResult> Search(string search = "")
    {
        var employees = _context.Employees.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            employees = employees.Where(e =>
                e.FirstName.ToLower().Contains(search) ||
                e.LastName.ToLower().Contains(search) ||
                (e.Phone != null && e.Phone.ToLower().Contains(search)) ||
                (e.Email != null && e.Email.ToLower().Contains(search)));
        }

        var result = await employees.Select(e => new
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Phone = e.Phone ?? "-",
            Email = e.Email ?? "-",
            Salary = e.Salary
        }).ToListAsync();

        return Json(result);
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
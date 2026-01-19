using FreedomDanceStudio.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FreedomDanceStudio.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Admin/Users — список всех пользователей
    [HttpGet]
    public IActionResult Users()
    {
        var users = _context.Users.ToList();
        return View(users);
    }

    // GET: /Admin/EditRole/{id} — форма изменения роли
    [HttpGet]
    public IActionResult EditRole(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
            return NotFound();

        return View(user);
    }

    // POST: /Admin/EditRole — обработка изменения роли
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(int id, string newRole)
    {
        if (string.IsNullOrEmpty(newRole) || (newRole != "User" && newRole != "Admin"))
        {
            ModelState.AddModelError("", "Некорректная роль");
            return View(_context.Users.Find(id));
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.Role = newRole;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Роль пользователя {user.Username} изменена на {newRole}";
        return RedirectToAction("Users");
    }
}
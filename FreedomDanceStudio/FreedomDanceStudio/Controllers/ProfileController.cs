using FreedomDanceStudio.Data;
using Microsoft.AspNetCore.Mvc;
using FreedomDanceStudio.Models;
using Microsoft.AspNetCore.Authorization;

namespace FreedomDanceStudio.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Profile/Index — просмотр профиля
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = await GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
            return RedirectToAction("Login", "Account");

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userId = await GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
            return RedirectToAction("Login", "Account");

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(User model)
    {
        if (!ModelState.IsValid)
            return View(model);
    
        var userId = await GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account");
    
        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
            return RedirectToAction("Login", "Account");
    
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.Phone = model.Phone;
    
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Профиль успешно обновлён";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Ошибка при сохранении изменений");
            return View(model);
        }
    }

    /// <summary>
    /// Получает ID текущего пользователя из куки
    /// </summary>
    private async Task<int?> GetCurrentUserId()
    {
        // Получаем UserId из Claims
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null)
            return null;

        if (!int.TryParse(userIdClaim.Value, out int userId))
            return null;

        return userId;
    }
}

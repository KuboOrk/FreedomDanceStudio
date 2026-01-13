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
    public IActionResult Index()
    {
        var userId = GetCurrentUserIdFromCookie();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var user = _context.Users.Find(userId.Value);
        if (user == null)
            return RedirectToAction("Login", "Account");

        return View(user);
    }

    // GET: /Profile/Edit — форма редактирования
    [HttpGet]
    public IActionResult Edit()
    {
        var userId = GetCurrentUserIdFromCookie();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var user = _context.Users.Find(userId.Value);
        if (user == null)
            return RedirectToAction("Login", "Account");

        return View(user);
    }

    // POST: /Profile/Edit — обработка формы
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(User model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetCurrentUserIdFromCookie();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var existingUser = _context.Users.Find(userId.Value);
        if (existingUser == null)
            return RedirectToAction("Login", "Account");

        // Обновляем только изменяемые поля
        existingUser.FirstName = model.FirstName;
        existingUser.LastName = model.LastName;
        existingUser.Email = model.Email;
        existingUser.Phone = model.Phone;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Профиль успешно обновлён";
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Получает ID текущего пользователя из куки
    /// </summary>
    private int? GetCurrentUserIdFromCookie()
    {
        var cookieValue = HttpContext.Request.Cookies["UserId"];
        if (string.IsNullOrEmpty(cookieValue))
            return null;

        if (int.TryParse(cookieValue, out int userId))
            return userId;

        return null;
    }
}

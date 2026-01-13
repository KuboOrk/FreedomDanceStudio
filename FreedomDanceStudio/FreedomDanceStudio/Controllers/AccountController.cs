using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using BCrypt.Net;

namespace FreedomDanceStudio.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Account или /Account/Login
    [HttpGet]
    [Route("")]
    [Route("Login")]
    public IActionResult Login()
    {
        // Проверяем авторизацию через куки
        if (IsUserAuthenticated())
            return RedirectToAction("Index", "Home");

        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (!ModelState.IsValid)
            return View();

        var user = _context.Users.FirstOrDefault(u => u.Username == username && u.IsActive);

        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // ОБЯЗАТЕЛЬНО: устанавливаем куку UserId для _Layout
            SetUserIdCookie(user.Id);

            user.LastLogin = DateTime.UtcNow;
            _context.Update(user);
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                ModelState.AddModelError("", "Ошибка при сохранении данных пользователя");
                return View();
            }

            TempData["Success"] = "Вы успешно вошли в систему.";
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Неверный логин или пароль");
        return View();
    }

    // GET: /Account/Register
    [HttpGet]
    [Route("Register")]
    public IActionResult Register()
    {
        // Проверяем авторизацию через куки
        if (IsUserAuthenticated())
            return RedirectToAction("Index", "Home");

        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Register")]
    [ActionName("Register")]
    public async Task<IActionResult> Register(User user, string confirmPassword)
    {
        // Валидация совпадения паролей
        if (user.PasswordHash != confirmPassword)
        {
            ModelState.AddModelError("confirmPassword", "Пароли не совпадают");
            return View(user);
        }

        // Проверка уникальности логина
        if (_context.Users.Any(u => u.Username == user.Username))
        {
            ModelState.AddModelError("Username", "Пользователь с таким логином уже существует");
            return View(user);
        }

        // Хеширование пароля
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.CreatedAt = DateTime.UtcNow;
        user.Role = "User";
        user.IsActive = true;

        _context.Users.Add(user);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Логируем ошибку
            ModelState.AddModelError("", "Ошибка при регистрации пользователя");
            return View(user);
        }

        TempData["Success"] = "Регистрация прошла успешно! Теперь войдите в систему.";
        return RedirectToAction("Login");
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Logout")]
    [ActionName("Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // ОБЯЗАТЕЛЬНО: удаляем куку UserId
        RemoveUserIdCookie();
        
        TempData["Success"] = "Вы вышли из системы.";
        return RedirectToAction("Login");
    }

    /// <summary>
    /// Проверяет авторизацию через куки UserId
    /// </summary>
    private bool IsUserAuthenticated()
    {
        var userIdCookie = HttpContext.Request.Cookies["UserId"];
        return !string.IsNullOrEmpty(userIdCookie);
    }

    /// <summary>
    /// Устанавливает куку UserId с настройками безопасности
    /// </summary>
    private void SetUserIdCookie(int userId)
    {
        HttpContext.Response.Cookies.Append("UserId", userId.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // true в продакшене с HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddDays(7),
                Path = "/"
            });
    }

    /// <summary>
    /// Удаляет куку UserId при выходе
    /// </summary>
    private void RemoveUserIdCookie()
    {
        HttpContext.Response.Cookies.Delete("UserId",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });
    }
    
    private User GetCurrentUser()
    {
        var userIdCookie = HttpContext.Request.Cookies["UserId"];
        if (string.IsNullOrEmpty(userIdCookie) || !int.TryParse(userIdCookie, out int userId))
            return null;

        return _context.Users.Find(userId);
    }
}

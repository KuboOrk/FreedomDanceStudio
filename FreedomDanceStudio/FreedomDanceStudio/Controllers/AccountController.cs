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
        var referrer = HttpContext.Request.Headers["Referer"].ToString();
    
        // Если пришли с той же страницы — не редиректим
        if (referrer.Contains("/Account/Login"))
            return View(); // Просто показываем форму

        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Проверка на пустые значения
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Логин и пароль обязательны");
            return View();
        }

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

            user.LastLogin = DateTime.UtcNow;
            _context.Update(user);
            await _context.SaveChangesAsync();

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
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Register")]
    public async Task<IActionResult> Register(User user, string confirmPassword)
    {
        // 1. Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(user.Username) ||
            string.IsNullOrWhiteSpace(user.PasswordHash) ||
            string.IsNullOrWhiteSpace(confirmPassword) ||
            string.IsNullOrWhiteSpace(user.Email))
        {
            ModelState.AddModelError("", "Все поля обязательны");
            return View(user);
        }

        // 2. Проверка совпадения паролей
        if (user.PasswordHash != confirmPassword)
        {
            ModelState.AddModelError("confirmPassword", "Пароли не совпадают");
            return View(user);
        }

        // 3. Валидация формата email (на сервере)
        if (!IsValidEmail(user.Email))
        {
            ModelState.AddModelError("Email", "Некорректный формат email");
            return View(user);
        }

        // 4. Проверка уникальности логина
        if (_context.Users.Any(u => u.Username == user.Username))
        {
            ModelState.AddModelError("Username", "Пользователь с таким логином уже существует");
            return View(user);
        }

        // 5. Проверка уникальности email
        if (_context.Users.Any(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Email уже зарегистрирован");
            return View(user);
        }

        // 6. Дополнительная валидация модели
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        // 7. Хеширование пароля и сохранение
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.CreatedAt = DateTime.UtcNow;
        user.Role = "User";
        user.IsActive = true;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Регистрация прошла успешно! Теперь войдите в систему.";
        return RedirectToAction("Login");
    }

    // Вспомогательный метод для проверки email
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Вы вышли из системы.";
        return RedirectToAction("Login");
    }
}

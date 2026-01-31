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
    private readonly ILogger<AccountController> _logger;

    public AccountController(ApplicationDbContext context,
        ILogger<AccountController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Обработчик авторизации пользователя

    // GET: /Account или /Account/Login
    [HttpGet]
    [Route("")]
    [Route("Login")]
    public IActionResult Login()
    {
        var referrer = HttpContext.Request.Headers["Referer"].ToString();
        if (referrer.Contains("/Account/Login"))
            return View();

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
        // 1.Проверка на пустые значения
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Логин и пароль обязательны");
            return View();
        }
        
        // 2.Валидация формата и длины
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 50)
        {
            ModelState.AddModelError("username", "Логин должен быть от 3 до 50 символов.");
            return View();
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            ModelState.AddModelError("password", "Пароль должен быть не менее 6 символов.");
            return View();
        }
        
        
        try
        {
            // 3. Защита от timing-атак: задержка при отсутствии пользователя
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
            
            if (user == null)
            {
                await Task.Delay(100); // Имитация времени проверки пароля
                _logger.LogWarning("Неудачная попытка входа: пользователь не найден ({Username})", username);
                ModelState.AddModelError("", "Неверный логин или пароль.");
                return View();
            }

            // 4. Проверка пароля (BCrypt устойчив к SQL-инъекциям — хеширование происходит вне SQL)
            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
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
            else
            {
                _logger.LogWarning("Неудачная попытка входа: неверный пароль для пользователя {Username}", username);
                ModelState.AddModelError("", "Неверный логин или пароль.");
                return View();
            }
        }
        catch (Exception ex)
        {
            // 5. Обработка исключений БД
            _logger.LogError(ex, "Ошибка при входе пользователя {Username}", username);
            ModelState.AddModelError("", "Произошла внутренняя ошибка. Попробуйте снова.");
            return View();
        }
    }

    #endregion

    #region Обработчик регистрации пользователя

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
        
        // 2. Валидация длины и формата
        if (string.IsNullOrWhiteSpace(user.Username) || user.Username.Length < 3 || user.Username.Length > 50)
        {
            ModelState.AddModelError("Username", "Логин должен быть от 3 до 50 символов.");
            return View(user);
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash) || user.PasswordHash.Length < 6)
        {
            ModelState.AddModelError("PasswordHash", "Пароль должен быть не менее 6 символов.");
            return View(user);
        }

        if (string.IsNullOrWhiteSpace(confirmPassword) || confirmPassword.Length < 6)
        {
            ModelState.AddModelError("confirmPassword", "Подтверждение пароля должно быть не менее 6 символов.");
            return View(user);
        }

        // 3. Проверка совпадения паролей
        if (user.PasswordHash != confirmPassword)
        {
            ModelState.AddModelError("confirmPassword", "Пароли не совпадают");
            return View(user);
        }

        // 4. Валидация формата email (на сервере)
        if (!IsValidEmail(user.Email))
        {
            ModelState.AddModelError("Email", "Некорректный формат email");
            return View(user);
        }

        // 5. Проверка уникальности логина
        if (_context.Users.Any(u => u.Username == user.Username))
        {
            ModelState.AddModelError("Username", "Пользователь с таким логином уже существует");
            return View(user);
        }

        // 6. Проверка уникальности email
        if (_context.Users.Any(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Email уже зарегистрирован");
            return View(user);
        }

        // 7. Дополнительная валидация модели
        if (!ModelState.IsValid)
            return View(user);

        try
        {
            // 6. Хеширование и сохранение
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;
            user.Role = "User";
            user.IsActive = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Регистрация прошла успешно! Теперь войдите в систему.";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации пользователя {Username}", user.Username);
            ModelState.AddModelError("", "Произошла ошибка при сохранении данных. Попробуйте снова.");
            return View(user);
        }
    }

    #endregion
    
    #region Выход из системы
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
    #endregion
    
    
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
}

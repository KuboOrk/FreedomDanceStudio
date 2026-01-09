using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FreedomDanceStudio.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        ViewData["Title"] = "Вход в систему";
        return View();
    }

    #region Вход в систему

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        // В реальном приложении — проверка в БД
        if (username == "admin" && password == "123") // тестовые данные
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["Success"] = "Вы успешно вошли в систему.";
            return RedirectToAction("Index", "Home");
        }

        ViewData["Error"] = "Неверный логин или пароль";
        return View();
    }

    #endregion

    #region Выход из системы

    /// <summary>
    /// Выполняет выход пользователя из системы.
    /// </summary>
    /// <param name="returnUrl">URL для перенаправления после выхода (если локальный)</param>
    /// <returns>Перенаправление на главную или указанный URL</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        // Проверяем, авторизован ли пользователь
        if (User.Identity?.IsAuthenticated == true)
        {
            // Очищаем все аутентификационные куки
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //Очищаем сессию (если используется)
            HttpContext.Session.Clear();

            //Логируем выход (опционально)
            //_logger.LogInformation("User {UserName} logged out.", User.Identity.Name);
        }

        //Безопасное перенаправление
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        TempData["Success"] = "Вы успешно вышли из системы.";
        return RedirectToAction("Index", "Home");
    }

    #endregion
}
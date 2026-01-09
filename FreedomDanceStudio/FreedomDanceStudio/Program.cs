using Microsoft.EntityFrameworkCore;
using FreedomDanceStudio.Data;

//using FreedomDanceStudio.Interfaces;
//using FreedomDanceStudio.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Контекст БД
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        opt => opt.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null)
    )
);

// 2. Сессии — добавляем ДО аутентификации
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Время жизни сессии без активности
    options.Cookie.HttpOnly = true; // Защита от JS‑доступа
    options.Cookie.IsEssential = true; // Обязательно для работы
});

// 3. Аутентификация
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // путь к странице входа
        options.LogoutPath = "/Account/Logout"; // путь к выходу
        options.AccessDeniedPath = "/Account/AccessDenied"; // при отказе в доступе
    });

// 4. MVC + Razor Pages (раскомментируйте нужное)
builder.Services.AddControllersWithViews(); // Для контроллеров

// builder.Services.AddRazorPages(); // Для Razor Pages

// Регистрация сервиса VisitService
//builder.Services.AddScoped<IVisitService, VisitService>();

var app = builder.Build();

// === КОНФИГУРАЦИЯ MIDDLEWARE ===
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseSession(); // ← Добавляем сразу после UseAuthentication!
app.UseAuthorization();

// === МАРШРУТИЗАЦИЯ (современный способ) ===
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Если используете Razor Pages, раскомментируйте:
// app.MapRazorPages();

app.Run();
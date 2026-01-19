using System.Globalization;
using System.Text.Json.Serialization;
using FreedomDanceStudio.Attributes;
using Microsoft.EntityFrameworkCore;
using FreedomDanceStudio.Data;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// 1. Контекст БД
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        opt => opt.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null)
    )
);

builder.Services.AddScoped<IExpiryAlertService, ExpiryAlertService>();

var cultureInfo = new CultureInfo("ru-RU");
cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(cultureInfo);
    options.SupportedCultures = new List<CultureInfo> { cultureInfo };
    options.SupportedUICultures = new List<CultureInfo> { cultureInfo };
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Или ReferenceHandler.Preserve — см. примечание ниже
        options.JsonSerializerOptions.WriteIndented = false; // Убрать отступы в продакшене
    });


// 2. Сессии — добавляем ДО аутентификации
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Аутентификация
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("InstructorOrAdmin", policy =>
        policy.RequireRole("Instructor", "Admin"));
});

// 4. MVC + Razor Pages
builder.Services.AddControllersWithViews();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

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
app.UseSession();
app.UseAuthorization();

// === МАРШРУТИЗАЦИЯ ===

// Перенаправление с корня на форму входа
app.MapGet("/", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
        return Results.Redirect("/Account/Login");
    return Results.Redirect("/Home/Index"); // Для авторизованных — главная
});


// Дефолтные маршруты для остальных случаев
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseHealthChecks("/health");

// Если используете Razor Pages, раскомментируйте:
// app.MapRazorPages();

app.Run();

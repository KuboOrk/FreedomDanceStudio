using Microsoft.EntityFrameworkCore;
using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
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

app.Use(async (context, next) =>
{
    var userIdCookie = context.Request.Cookies["UserId"];
    User currentUser = null;
    if (!string.IsNullOrEmpty(userIdCookie) && int.TryParse(userIdCookie, out int userId))
    {
        // Здесь можно внедрить контекст БД или сервис
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        currentUser = dbContext.Users.Find(userId);
    }
    context.Items["CurrentUser"] = currentUser;
    await next();
});


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


app.Run();

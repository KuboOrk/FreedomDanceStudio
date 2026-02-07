var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Основной маршрут MVC (обрабатывает /Home/Rules, /Home/Offer и др.)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Дополнительные маршруты: перенаправляем на действия контроллера
app.MapGet("/rules", () => Results.Redirect("/Home/Rules"));
app.MapGet("/offer", () => Results.Redirect("/Home/Offer"));

app.Run();
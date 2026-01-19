using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.EntityFrameworkCore;

namespace FreedomDanceStudio.Attributes;

public interface IExpiryAlertService
{
    Task<List<AbonnementExpiryAlert>> GetUpcomingExpiryAlertsAsync(int days = 30);
    Task UpdateExpiryAlertForSaleAsync(int saleId);
    Task<List<AbonnementExpiryAlert>> GetAllAbonnementAlertsAsync(); // Новый метод
}

    public class ExpiryAlertService: IExpiryAlertService
{
    private readonly ApplicationDbContext _context;

    public ExpiryAlertService(ApplicationDbContext context)
    {
        _context = context;
    }

    private string GetAlertLevelByUsage(decimal usagePercent)
    {
        if (usagePercent >= 80) return "Critical";   // 80–100 % — критично
        if (usagePercent >= 50) return "Warning";  // 50–79 % — предупреждение
        return "Normal";                            // 0–49 % — нормально
    }


    public async Task<List<AbonnementExpiryAlert>> GetUpcomingExpiryAlertsAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.Date.AddDays(days);
        return await _context.AbonnementExpiryAlerts
            .Where(a =>
                a.ExpiryDate <= cutoffDate &&
                a.UsagePercent >= 50m) // Только абонементы с заполнением ≥ 50 %
            .OrderByDescending(a => a.UsagePercent) // Сортировка по убыванию процента
            .Take(10)
            .ToListAsync();
    }
    
    public async Task UpdateExpiryAlertForSaleAsync(int saleId)
    {
        var sale = await _context.AbonnementSales
            .Include(s => s.Client)
            .Include(s => s.Visits)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale == null) return;

        // Нормализация даты окончания в UTC
        var endDateUtc = sale.EndDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(sale.EndDate, DateTimeKind.Utc)
            : sale.EndDate.ToUniversalTime();

        // Расчёт использования
        var usedVisits = sale.Visits?.Count ?? 0;
        var maxVisits = sale.MaxVisits;
        decimal usagePercent = maxVisits > 0
            ? Math.Round((decimal)usedVisits / maxVisits * 100m, 2)
            : 0m;

        var alertLevel = GetAlertLevelByUsage(usagePercent);

        var existingAlert = await _context.AbonnementExpiryAlerts
            .FirstOrDefaultAsync(a => a.AbonnementSaleId == saleId);

        if (existingAlert != null)
        {
            existingAlert.UsedVisits = usedVisits;
            existingAlert.MaxVisits = maxVisits;
            existingAlert.UsagePercent = usagePercent;
            existingAlert.AlertLevel = alertLevel;
            existingAlert.ExpiryDate = endDateUtc; // Явное обновление даты
            existingAlert.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.AbonnementExpiryAlerts.Add(new AbonnementExpiryAlert
            {
                ClientId = sale.ClientId,
                AbonnementSaleId = sale.Id,
                ExpiryDate = endDateUtc,
                UsedVisits = usedVisits,
                MaxVisits = maxVisits,
                UsagePercent = usagePercent,
                AlertLevel = alertLevel,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }
    public async Task<List<AbonnementExpiryAlert>> GetAllAbonnementAlertsAsync()
    {
        return await _context.AbonnementExpiryAlerts
            .Include(a => a.Client)
            .Include(a => a.AbonnementSale) // обязательно загружаем продажу
            .Where(a =>
                a.AbonnementSale != null &&        // продажа существует
                !a.AbonnementSale.IsDeleted)     // продажа не удалена (если есть такое поле)
            .OrderByDescending(a => a.UsagePercent)
            .ThenByDescending(a => a.ExpiryDate)
            .ToListAsync();
    }
}
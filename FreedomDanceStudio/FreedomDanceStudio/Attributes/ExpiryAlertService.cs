using FreedomDanceStudio.Data;
using FreedomDanceStudio.Models;
using Microsoft.EntityFrameworkCore;

namespace FreedomDanceStudio.Attributes;

public interface IExpiryAlertService
{
    Task UpdateExpiryAlertsAsync();
    Task<List<AbonnementExpiryAlert>> GetUpcomingExpiryAlertsAsync(int days = 30);
}

public class ExpiryAlertService: IExpiryAlertService
{
    private readonly ApplicationDbContext _context;

    public ExpiryAlertService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task UpdateExpiryAlertsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var sales = await _context.AbonnementSales
            .Include(s => s.Client)
            .ToListAsync();

        foreach (var sale in sales)
        {
            // Нормализуем EndDate к UTC
            var endDateUtc = sale.EndDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(sale.EndDate, DateTimeKind.Utc)
                : sale.EndDate.ToUniversalTime();

            var daysRemaining = (endDateUtc - today).Days;
            var alertLevel = GetAlertLevel(daysRemaining);

            var existingAlert = await _context.AbonnementExpiryAlerts
                .FirstOrDefaultAsync(a => a.AbonnementSaleId == sale.Id);

            if (existingAlert != null)
            {
                existingAlert.DaysRemaining = daysRemaining;
                existingAlert.AlertLevel = alertLevel;
                existingAlert.UpdatedAt = DateTime.UtcNow; // Уже UTC
            }
            else
            {
                _context.AbonnementExpiryAlerts.Add(new AbonnementExpiryAlert
                {
                    ClientId = sale.ClientId,
                    AbonnementSaleId = sale.Id,
                    ExpiryDate = endDateUtc, // Явно UTC
                    DaysRemaining = daysRemaining,
                    AlertLevel = alertLevel,
                    CreatedAt = DateTime.UtcNow, // UTC
                    UpdatedAt = DateTime.UtcNow  // UTC
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private string GetAlertLevel(int daysRemaining)
    {
        if (daysRemaining < 15) return "Critical";
        if (daysRemaining < 30) return "Warning";
        return "Normal";
    }

    public async Task<List<AbonnementExpiryAlert>> GetUpcomingExpiryAlertsAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.Date.AddDays(days);
        return await _context.AbonnementExpiryAlerts
            .Where(a => a.ExpiryDate <= cutoffDate && a.DaysRemaining > 0)
            .OrderBy(a => a.DaysRemaining)
            .Take(10) // Топ‑10 ближайших
            .ToListAsync();
    }
}
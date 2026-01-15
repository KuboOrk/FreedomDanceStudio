using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using FreedomDanceStudio.Models;

namespace FreedomDanceStudio.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Service> Services { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<AbonnementSale> AbonnementSales { get; set; }

    public DbSet<ClientVisit> ClientVisits { get; set; } = null!;
    public DbSet<User> Users { get; set; }
    public DbSet<AbonnementExpiryAlert> AbonnementExpiryAlerts { get; set; }
    [AllowNull]
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
    public DbSet<EmployeeWorkHours> EmployeeWorkHours { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Не добавляйте конвертеры для DateTime, если тип столбца — date
    }
}
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
    public DbSet<EmployeeSalaryCalculation> EmployeeSalaryCalculations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Не добавляйте конвертеры для DateTime, если тип столбца — date
        // 1. Связь EmployeeSalaryCalculation → Employee (один ко многим)
        modelBuilder.Entity<EmployeeSalaryCalculation>()
            .HasOne(esc => esc.Employee)
            .WithMany(e => e.SalaryCalculations)  // Предполагаем, что в Employee есть коллекция
            .HasForeignKey(esc => esc.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);  // Не удаляем сотрудника при удалении расчёта

        // 2. Связь FinancialTransaction → EmployeeSalaryCalculation (один к одному/опционально)
        modelBuilder.Entity<FinancialTransaction>()
            .HasOne<EmployeeSalaryCalculation>(ft => ft.EmployeeSalaryCalculation)
            .WithMany(esc => esc.FinancialTransactions) // добавьте коллекцию в EmployeeSalaryCalculation
            .HasForeignKey(ft => ft.EmployeeSalaryCalculationId)
            .OnDelete(DeleteBehavior.Cascade);

        // 3. Индексы для производительности
        // Индекс по EmployeeId для быстрого поиска расчётов сотрудника
        modelBuilder.Entity<EmployeeSalaryCalculation>()
            .HasIndex(esc => esc.EmployeeId);

        // Составной индекс по датам для запросов за период
        modelBuilder.Entity<EmployeeSalaryCalculation>()
            .HasIndex(esc => new { esc.StartDate, esc.EndDate });

        // Индекс по EmployeeSalaryCalculationId в FinancialTransaction
        modelBuilder.Entity<FinancialTransaction>()
            .HasIndex(ft => ft.EmployeeSalaryCalculationId);

        // 4. Настройка типов данных для PostgreSQL
        // Явное указание типов для столбцов с десятичными числами
        modelBuilder.Entity<EmployeeSalaryCalculation>()
            .Property(esc => esc.HourlyRate)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<EmployeeSalaryCalculation>()
            .Property(esc => esc.TotalHours)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<EmployeeSalaryCalculation>()
            .Property(esc => esc.TotalAmount)
            .HasColumnType("decimal(18,2)");

        // 5. Настройка DateTime для UTC
        // Для всех сущностей с DateTime полями
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) ||
                    property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp with time zone"); // PostgreSQL
                }
            }
        }

        // 6. Настройка длины строк
        modelBuilder.Entity<EmployeeSalaryCalculation>()
            .Property(esc => esc.PaymentType)
            .HasMaxLength(50);

        // 7. Каскадное удаление для Employee → EmployeeWorkHours
        modelBuilder.Entity<EmployeeWorkHours>()
            .HasOne(ewh => ewh.Employee)
            .WithMany(e => e.WorkHours)
            .HasForeignKey(ewh => ewh.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
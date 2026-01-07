using Microsoft.EntityFrameworkCore;
using FreedomDanceStudio.Models;

namespace FreedomDanceStudio.Data;

public class ApplicationDbContext: DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Service> Services { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<AbonnementSale> AbonnementSales { get; set; }
    public DbSet<SalaryCalculation> SalaryCalculations { get; set; }
}
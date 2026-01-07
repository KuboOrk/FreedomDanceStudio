using Microsoft.EntityFrameworkCore;
using FreedomDanceStudio.Models;

namespace FreedomDanceStudio.Data;

public class ApplicationDbContext: DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    DbSet<Service> Services { get; set; }
    DbSet<Employee> Employees { get; set; }
    DbSet<Client> Clients { get; set; }
    DbSet<AbonnementSale> AbonnementSales { get; set; }
    DbSet<SalaryCalculation> SalaryCalculations { get; set; }
}
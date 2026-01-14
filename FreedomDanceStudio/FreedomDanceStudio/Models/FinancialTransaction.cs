using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreedomDanceStudio.Models;

public class FinancialTransaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string TransactionType { get; set; } = null!; // "Income" или "Expense"

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [Required]
    [Column(TypeName = "date")]
    [Display(Name = "Дата транзакции")]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow.Date;

    [Required]
    [Display(Name = "Создано")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Связь с продажей абонемента (для автоматических доходов)
    public int? AbonnementSaleId { get; set; }
    public virtual AbonnementSale? AbonnementSale { get; set; }

    // Флаг ручной транзакции
    [Display(Name = "Ручная транзакция")]
    public bool IsManual { get; set; }
}
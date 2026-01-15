using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreedomDanceStudio.Models;

public class EmployeeSalaryCalculation
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Сотрудник")]
    public int EmployeeId { get; set; }
    
    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Начало периода")]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Конец периода")]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime EndDate { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Тип оплаты")]
    public string PaymentType { get; set; } = "Hourly"; // Hourly, PerVisit, Percentage

    [Range(0, double.MaxValue)]
    [Display(Name = "Ставка за час")]
    public decimal HourlyRate { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Всего часов")]
    public decimal TotalHours { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Всего посещений")]
    public int TotalVisits { get; set; }

    [Range(0, 1)]
    [Display(Name = "Процентная ставка")]
    public decimal? PercentageRate { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [Display(Name = "Рассчитанная сумма")]
    public decimal CalculatedAmount { get; set; }

    [Display(Name = "ID транзакции")]
    public int? FinancialTransactionId { get; set; }
    [ForeignKey("FinancialTransactionId")]
    public virtual FinancialTransaction? FinancialTransaction { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    [Column(TypeName = "timestamp with time zone")]
    [Display(Name = "Создано")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [DataType(DataType.DateTime)]
    [Column(TypeName = "timestamp with time zone")]
    [Display(Name = "Обновлено")]
    public DateTime? UpdatedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreedomDanceStudio.Models;

public class EmployeeSalaryCalculation
{
    [Key] public int Id { get; set; }

    [Required] public int EmployeeId { get; set; }
    
    [ForeignKey("EmployeeId")]
    public virtual Employee? Employee { get; set; }

    [Display(Name = "Дата начала")]
    [DataType(DataType.DateTime)]
    [Required]
    public DateTime StartDate { get; set; }

    [Display(Name = "Дата окончания")]
    [DataType(DataType.DateTime)]
    [Required]
    public DateTime EndDate { get; set; }

    [Required] [StringLength(50)] public string PaymentType { get; set; } = "Hourly";

    [Range(0, 999999.99, ErrorMessage = "Ставка за час должна быть положительной")]
    [Display(Name = "Ставка за час")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal HourlyRate { get; set; }

    [Range(0, 999999.99, ErrorMessage = "Количество часов должно быть положительным")]
    [Display(Name = "Всего часов")]
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal TotalHours { get; set; }

    [Display(Name = "Итого к выплате")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Метод валидации
    public ValidationResult ValidateDateRange(DateTime endDate)
    {
        if (EndDate < StartDate)
            return new ValidationResult("Дата окончания не может быть раньше даты начала.");
        return ValidationResult.Success;
    }
    
    public ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();
}
using System.ComponentModel.DataAnnotations;

namespace FreedomDanceStudio.Models;

public class EmployeeWorkHours
{
    public int Id { get; set; }
        
    public int EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }
        
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Дата работы")]
    public DateTime WorkDate { get; set; }
        
    [Range(0, 100, ErrorMessage = "Количество посещений должно быть от 0 до 100.")]
    [Display(Name = "Посещения")]
    public int VisitsCount { get; set; } = 0;
        
    [Range(0.0, 24.0, ErrorMessage = "Часы должны быть от 0 до 24.")]
    [Display(Name = "Часы")]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public decimal HoursCount { get; set; } = 0.00m;
        
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
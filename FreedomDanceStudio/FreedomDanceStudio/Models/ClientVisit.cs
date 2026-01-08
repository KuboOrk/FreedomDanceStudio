using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreedomDanceStudio.Models;

public class ClientVisit
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Продажа абонемента")]
    public int AbonnementSaleId { get; set; }
    
    public virtual AbonnementSale? AbonnementSale { get; set; }

    [Required]
    [Display(Name = "Дата посещения")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
    public DateTime VisitDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Создано")]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FreedomDanceStudio.Attributes;

namespace FreedomDanceStudio.Models;

public class AbonnementSale
{
    /// <summary>
    /// Уникальный идентификатор продажи
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Ссылка на клиента, купившего абонемент
    /// </summary>
    [Required]
    [Display(Name = "Клиент")]
    public int ClientId { get; set; }

    /// <summary>
    /// Навигация к объекту клиента (для удобства работы)
    /// </summary>
    public virtual Client? Client { get; set; }

    /// <summary>
    /// Ссылка на тип абонемента (услугу)
    /// </summary>
    [Required]
    [Display(Name = "Тип абонемента")]
    public int ServiceId { get; set; }

    /// <summary>
    /// Навигация к услуге
    /// </summary>
    public virtual Service? Service { get; set; }

    /// <summary>
    /// Дата продажи абонемента
    /// </summary>
    [Required(ErrorMessage = "Дата продажи обязательна")]
    [Column(TypeName = "date")]
    [Display(Name = "Дата продажи")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime SaleDate { get; set; } = DateTime.UtcNow.Date;

    /// <summary>
    /// Дата начала действия абонемента
    /// </summary>
    [Column(TypeName = "date")]
    [Display(Name = "Начало действия")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Required(ErrorMessage = "Укажите дату начала действия абонемента")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Дата окончания действия абонемента (рассчитывается автоматически)
    /// </summary>
    [Column(TypeName = "date")]
    [Display(Name = "Окончание действия")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Required(ErrorMessage = "Дата окончания рассчитывается автоматически")]
    [FutureDate(ErrorMessage = "Дата окончания не может быть в прошлом")]
    public DateTime EndDate { get; set; }

    public virtual ICollection<ClientVisit> Visits { get; set; } = new List<ClientVisit>();

    /// <summary>
    /// Максимальное количество посещений по абонементу
    /// </summary>
    [Display(Name = "Макс. посещений")]
    public int MaxVisits { get; set; } = 0;
    
    /// <summary>
    /// Флаг удаления продажи абонемента
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
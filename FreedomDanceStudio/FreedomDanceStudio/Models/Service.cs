using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreedomDanceStudio.Models;

public class Service
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } // Название абонемента

    [Required]
    [Range(0, 100000)]
   decimal Price { get; set; } // Цена

    [Required]
    public int DurationDays { get; set; } // Срок действия в днях

    public bool IsActive { get; set; } = true; // Активен ли абонемент
}
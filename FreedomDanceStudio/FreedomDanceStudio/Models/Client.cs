using System.ComponentModel.DataAnnotations;

namespace FreedomDanceStudio.Models;

public class Client
{
    /// <summary>
    /// Уникальный идентификатор клиента (первичный ключ)
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Имя клиента (обязательное поле)
    /// </summary>
    [Required(ErrorMessage = "Имя обязательно для заполнения")]
    [StringLength(50, ErrorMessage = "Имя не может превышать 50 символов")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Фамилия клиента (обязательное поле)
    /// </summary>
    [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
    [StringLength(50, ErrorMessage = "Фамилия не может превышать 50 символов")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Номер телефона (обязателен для связи)
    /// </summary>
    [Required(ErrorMessage = "Телефон обязателен для заполнения")]
    [Phone(ErrorMessage = "Введите корректный номер телефона")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = null!;

    /// <summary>
    /// Email клиента (опционально)
    /// </summary>
    [EmailAddress(ErrorMessage = "Введите корректный email")]
    [Display(Name = "Email")]
    public string? Email { get; set; }
    
    /// <summary>
    /// Флаг удаления клиента (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
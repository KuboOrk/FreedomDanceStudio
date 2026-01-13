using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FreedomDanceStudio.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Логин")]
    public string Username { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string PasswordHash { get; set; } = null!;

    [StringLength(100)]
    [Display(Name = "Имя")]
    public string? FirstName { get; set; }

    [StringLength(100)]
    [Display(Name = "Фамилия")]
    public string? LastName { get; set; }

    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Phone]
    [Display(Name = "Телефон")]
    public string? Phone { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Роль")]
    public string Role { get; set; } = "User"; // Admin, Instructor, User

    [Display(Name = "Дата создания")]
    public DateTime CreatedAt { get; set; }=DateTime.UtcNow;

    [Display(Name = "Последний вход")]
    public DateTime? LastLogin { get; set; }

    [Display(Name = "Активен")]
    public bool IsActive { get; set; }
}
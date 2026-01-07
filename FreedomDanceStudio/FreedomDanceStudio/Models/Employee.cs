using System.ComponentModel.DataAnnotations;

namespace FreedomDanceStudio.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [Phone]
    string? Phone { get; set; }

    [EmailAddress]
    string? Email { get; set; }

    [Range(0, 999999.99)]
    decimal Salary { get; set; }
}
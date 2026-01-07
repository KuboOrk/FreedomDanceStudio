using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreedomDanceStudio.Models;

public class Client
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Phone]
    public string Phone { get; set; }
    [EmailAddress]
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
}
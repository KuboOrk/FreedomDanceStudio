using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreedomDanceStudio.Models;

public class ClientSubscription
{
    [Key]
    public int Id { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; }
    public int ServiceId { get; set; }
    public Service Service { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
}
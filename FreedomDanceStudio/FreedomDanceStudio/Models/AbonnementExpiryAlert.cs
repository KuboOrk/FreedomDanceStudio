using System.ComponentModel.DataAnnotations;

namespace FreedomDanceStudio.Models;

public class AbonnementExpiryAlert
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int AbonnementSaleId { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime ExpiryDate { get; set; }
    
    public int DaysRemaining { get; set; }
    public string AlertLevel { get; set; } // 'Normal', 'Warning', 'Critical'
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Навигация
    public virtual Client Client { get; set; }
    public virtual AbonnementSale AbonnementSale { get; set; }
}
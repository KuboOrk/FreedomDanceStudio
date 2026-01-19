using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreedomDanceStudio.Models;

public class AbonnementExpiryAlert
{
    [Key]
    public int Id { get; set; }
    
    public int ClientId { get; set; }
    public int AbonnementSaleId { get; set; }
    
    [DataType(DataType.Date)]
    [Column(TypeName = "date")]
    public DateTime ExpiryDate { get; set; }
    
    public int DaysRemaining { get; set; }
    [MaxLength(20)]
    public string AlertLevel { get; set; } // 'Normal', 'Warning', 'Critical'
    
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
    [DataType(DataType.DateTime)]
    public DateTime UpdatedAt { get; set; }
    
    public int UsedVisits { get; set; }
    public int MaxVisits { get; set; }
    
    [Column(TypeName = "numeric(5,2)")]
    public decimal UsagePercent { get; set; } // 0.00–100.00

    // Навигация
    public virtual Client Client { get; set; }
    public virtual AbonnementSale AbonnementSale { get; set; }
}
namespace ReceiptSystem.API.Models;

public class RouteMerchant
{
    public int RouteMerchantId { get; set; }
    public int RouteId { get; set; }
    public int MerchantId { get; set; }
    public int PositionOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public int AddedByUserId { get; set; }

    // Navigation
    public Route Route { get; set; } = null!;
    public Merchant Merchant { get; set; } = null!;
    public User AddedByUser { get; set; } = null!;
    public ICollection<DayInvoice> DayInvoices { get; set; } = new List<DayInvoice>();
}

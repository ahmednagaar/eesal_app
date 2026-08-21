namespace ReceiptSystem.API.Models;

public class DayInvoice
{
    public int DayInvoiceId { get; set; }
    public int DeliveryDayId { get; set; }
    public int RouteMerchantId { get; set; }
    public int MerchantId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public string? Notes { get; set; }
    public int? ManualPositionOverride { get; set; }
    public int EnteredByUserId { get; set; }
    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public DeliveryDay DeliveryDay { get; set; } = null!;
    public RouteMerchant RouteMerchant { get; set; } = null!;
    public Merchant Merchant { get; set; } = null!;
    public User EnteredByUser { get; set; } = null!;
}

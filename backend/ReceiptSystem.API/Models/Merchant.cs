namespace ReceiptSystem.API.Models;

public class Merchant
{
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    public ICollection<RouteMerchant> RouteMerchants { get; set; } = new List<RouteMerchant>();
    public ICollection<DayInvoice> DayInvoices { get; set; } = new List<DayInvoice>();
}

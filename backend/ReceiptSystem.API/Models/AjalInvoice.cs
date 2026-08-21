namespace ReceiptSystem.API.Models;

public class AjalInvoice
{
    public int AjalInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int MerchantId { get; set; }
    public int? RouteId { get; set; }
    public string? CallCenterEmployeeName { get; set; }
    public decimal Amount { get; set; }
    public DateTime SessionDate { get; set; }
    public string InvoiceStatus { get; set; } = "Active"; // Active | Cancelled | Modified
    public decimal? OriginalAmount { get; set; }
    public string? ModificationNote { get; set; }
    public string? Notes { get; set; }
    public string ImportSource { get; set; } = "Manual"; // Manual | Excel
    public int EnteredByUserId { get; set; }
    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Merchant Merchant { get; set; } = null!;
    public Route? Route { get; set; }
    public User EnteredByUser { get; set; } = null!;
}

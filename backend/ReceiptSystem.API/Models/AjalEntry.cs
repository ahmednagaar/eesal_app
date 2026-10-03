namespace ReceiptSystem.API.Models;

public class AjalEntry
{
    public int EntryId { get; set; }
    public int SessionId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;  // ERP invoice number (6-digit)
    public int MerchantId { get; set; }
    public decimal? Amount { get; set; }                        // Reference only from printed invoice
    public string? Notes { get; set; }
    public int SortOrder { get; set; }                          // Original entry position

    // Review (مراجعة)
    public bool IsReviewed { get; set; } = false;
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public int EnteredByUserId { get; set; }
    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AjalSession Session { get; set; } = null!;
    public Merchant Merchant { get; set; } = null!;
    public User EnteredByUser { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
}

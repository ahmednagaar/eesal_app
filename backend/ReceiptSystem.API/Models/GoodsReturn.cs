namespace ReceiptSystem.API.Models;

/// <summary>
/// A goods return record (مرتجع). Each return uses a receipt number from a ReturnReceiptBook.
/// Contains header info; items are in GoodsReturnItem.
/// </summary>
public class GoodsReturn
{
    public int ReturnId { get; set; }
    public int ReturnReceiptNumber { get; set; }
    public int? BookId { get; set; }
    public int DriverId { get; set; }
    public int ReasonId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string Status { get; set; } = "Open"; // Open | UnderInvestigation | Verified | Resolved | Cancelled
    public string? CustomerName { get; set; }
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public int EnteredByUserId { get; set; }
    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ReturnReceiptBook? Book { get; set; }
    public Driver Driver { get; set; } = null!;
    public ReturnReasonCategory Reason { get; set; } = null!;
    public User EnteredByUser { get; set; } = null!;
    public ICollection<GoodsReturnItem> Items { get; set; } = new List<GoodsReturnItem>();
}

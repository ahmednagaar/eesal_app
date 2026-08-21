 namespace ReceiptSystem.API.Models;

public class Receipt
{
    public int ReceiptId { get; set; }
    public int ReceiptNumber { get; set; }
    public int? SeriesId { get; set; }
    public int? BookId { get; set; }
    public int SessionId { get; set; }
    public int DriverId { get; set; }
    public int MerchantId { get; set; }
    public DateTime CollectionDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPartialPayment { get; set; } = false;
    public string? Notes { get; set; }
    public int EnteredByUserId { get; set; }
    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

    // Excel Import fields
    public string ImportSource { get; set; } = "Manual"; // Manual | Excel
    public bool IsWithoutReceipt { get; set; } = false;
    public string? DesktopSystemUser { get; set; }

    // Navigation
    public BookSeries? Series { get; set; }
    public ReceiptBook? Book { get; set; }
    public CollectionSession Session { get; set; } = null!;
    public Driver Driver { get; set; } = null!;
    public Merchant Merchant { get; set; } = null!;
    public User EnteredByUser { get; set; } = null!;
}

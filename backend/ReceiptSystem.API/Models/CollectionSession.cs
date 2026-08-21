namespace ReceiptSystem.API.Models;

public class CollectionSession
{
    public int SessionId { get; set; }
    public int DriverId { get; set; }
    public DateTime SessionDate { get; set; }
    public string RouteArea { get; set; } = string.Empty;
    public int FirstReceiptNumber { get; set; }
    public int LastReceiptNumber { get; set; }
    public int TotalReceiptsCount { get; set; }
    public decimal TotalAmountCollected { get; set; }
    public bool HasGaps { get; set; } = false;
    public string? Notes { get; set; }
    public int EnteredByUserId { get; set; }
    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

    // Excel Import fields
    public string ImportSource { get; set; } = "Manual"; // Manual | Excel
    public string? OriginalFileName { get; set; }

    // Cash Reconciliation
    public decimal? ActualCashCounted { get; set; }
    public decimal? CashDifference { get; set; }
    public bool CashReconciled { get; set; } = false;
    public DateTime? ReconciledAt { get; set; }
    public int? ReconciledByUserId { get; set; }

    // Session Confirmation / Locking
    public bool IsConfirmed { get; set; } = false;
    public DateTime? ConfirmedAt { get; set; }
    public int? ConfirmedByUserId { get; set; }

    // Navigation
    public Driver Driver { get; set; } = null!;
    public User EnteredByUser { get; set; } = null!;
    public User? ReconciledByUser { get; set; }
    public User? ConfirmedByUser { get; set; }
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    public ICollection<ReceiptGap> DetectedGaps { get; set; } = new List<ReceiptGap>();
}

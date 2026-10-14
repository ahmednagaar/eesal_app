namespace ReceiptSystem.API.Models;

public class ReceiptGap
{
    public int GapId { get; set; }
    public int MissingReceiptNumber { get; set; }
    public int? DetectedInSessionId { get; set; }
    public int DriverId { get; set; }
    public int? BookId { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Open"; // Open | UnderInvestigation | Resolved | Explained | Cancelled
    public string? Resolution { get; set; }
    public string? ReasonCategory { get; set; } // Lost | Skipped | IncompleteData | OutOfSequence | UnderInvestigation | Cancelled_BookArchive | Cancelled_Damaged | Cancelled_Other
    public DateTime? ResolvedAt { get; set; }
    public int? ResolvedByUserId { get; set; }

    // Cancellation (لاغي) fields
    public string? CancelledByReason { get; set; }
    public int? CancelledByUserId { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navigation
    public CollectionSession? DetectedInSession { get; set; }
    public Driver Driver { get; set; } = null!;
    public ReceiptBook? Book { get; set; }
    public User? ResolvedByUser { get; set; }
    public User? CancelledByUser { get; set; }
}

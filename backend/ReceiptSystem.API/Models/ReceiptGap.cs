namespace ReceiptSystem.API.Models;

public class ReceiptGap
{
    public int GapId { get; set; }
    public int MissingReceiptNumber { get; set; }
    public int DetectedInSessionId { get; set; }
    public int DriverId { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Open"; // Open | UnderInvestigation | Resolved | Explained
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? ResolvedByUserId { get; set; }

    // Navigation
    public CollectionSession DetectedInSession { get; set; } = null!;
    public Driver Driver { get; set; } = null!;
    public User? ResolvedByUser { get; set; }
}

namespace ReceiptSystem.API.Models;

/// <summary>
/// A camera audit session — reviewing loading footage for a specific driver/trip.
/// </summary>
public class LoadingAudit
{
    public int AuditId { get; set; }
    public DateTime AuditDate { get; set; }
    public int DriverId { get; set; }
    public string? TripReference { get; set; } // optional ERP trip/manifest number
    public string Status { get; set; } = "Draft"; // Draft | Completed | HasDiscrepancies
    public int ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Driver Driver { get; set; } = null!;
    public User ReviewedByUser { get; set; } = null!;
    public ICollection<LoadingDiscrepancy> Discrepancies { get; set; } = new List<LoadingDiscrepancy>();
}

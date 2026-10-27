namespace ReceiptSystem.API.Models;

/// <summary>
/// A discrepancy found during camera audit — shortage, surplus, damage, wrong product.
/// Follows a case lifecycle: Open → UnderInvestigation → Resolved/Closed.
/// </summary>
public class LoadingDiscrepancy
{
    public int DiscrepancyId { get; set; }
    public int AuditId { get; set; }
    public int ProductId { get; set; }
    public int ProductUnitId { get; set; }
    public decimal ExpectedQty { get; set; }
    public decimal ActualQty { get; set; }
    public decimal DifferenceQty { get; set; } // ActualQty - ExpectedQty (negative = shortage)
    public string DiscrepancyType { get; set; } = "Shortage"; // Shortage | Surplus | WrongProduct | Damaged
    public string Status { get; set; } = "Open"; // Open | UnderInvestigation | Resolved | Closed
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? ResolvedByUserId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public LoadingAudit Audit { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductUnit ProductUnit { get; set; } = null!;
    public User? ResolvedByUser { get; set; }
    public ICollection<CaseNote> CaseNotes { get; set; } = new List<CaseNote>();
}

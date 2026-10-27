namespace ReceiptSystem.API.Models;

/// <summary>
/// Investigation notes on a discrepancy case — tracks the conversation/progress of resolving a case.
/// </summary>
public class CaseNote
{
    public int NoteId { get; set; }
    public int DiscrepancyId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string? StatusChange { get; set; } // If this note changed the status, record old→new e.g. "Open→UnderInvestigation"
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public LoadingDiscrepancy Discrepancy { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}

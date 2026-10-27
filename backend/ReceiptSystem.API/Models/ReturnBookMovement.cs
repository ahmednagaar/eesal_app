namespace ReceiptSystem.API.Models;

/// <summary>
/// Tracks movements of return receipt books: deliver to driver, return, transfer, verify.
/// </summary>
public class ReturnBookMovement
{
    public int MovementId { get; set; }
    public int BookId { get; set; }
    public string ActionType { get; set; } = string.Empty; // Delivered | Returned | Transferred | Verified | Closed
    public int? FromDriverId { get; set; }
    public int? ToDriverId { get; set; }
    public int PerformedByUserId { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Navigation
    public ReturnReceiptBook Book { get; set; } = null!;
    public Driver? FromDriver { get; set; }
    public Driver? ToDriver { get; set; }
    public User PerformedByUser { get; set; } = null!;
}

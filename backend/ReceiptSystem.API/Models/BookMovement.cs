namespace ReceiptSystem.API.Models;

public class BookMovement
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
    public ReceiptBook Book { get; set; } = null!;
    public Driver? FromDriver { get; set; }
    public Driver? ToDriver { get; set; }
    public User PerformedByUser { get; set; } = null!;
}

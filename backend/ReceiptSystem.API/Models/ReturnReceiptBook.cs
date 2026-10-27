namespace ReceiptSystem.API.Models;

/// <summary>
/// A physical return receipt book (دفتر مرتجعات).
/// Assigned to a driver or kept at the warehouse for returns recording.
/// </summary>
public class ReturnReceiptBook
{
    public int BookId { get; set; }
    public int? SeriesId { get; set; }
    public int BookNumber { get; set; }
    public int StartReceiptNumber { get; set; }
    public int EndReceiptNumber { get; set; }
    public int? AssignedToDriverId { get; set; }
    public DateTime? AssignedDate { get; set; }
    public string Status { get; set; } = "Available"; // Available | Assigned | InProgress | Completed | Returned | Deactivated
    public int? AssignedByUserId { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Return tracking
    public DateTime? ReturnedDate { get; set; }
    public int? ReturnedToUserId { get; set; }

    // Verification
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public int? VerifiedByUserId { get; set; }

    // Navigation
    public ReturnBookSeries? Series { get; set; }
    public Driver? AssignedToDriver { get; set; }
    public User? AssignedByUser { get; set; }
    public User? ReturnedToUser { get; set; }
    public User? VerifiedByUser { get; set; }
    public ICollection<ReturnBookMovement> Movements { get; set; } = new List<ReturnBookMovement>();
    public ICollection<GoodsReturn> GoodsReturns { get; set; } = new List<GoodsReturn>();
}

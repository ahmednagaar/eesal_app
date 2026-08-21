namespace ReceiptSystem.API.Models;

public class ReceiptBook
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
    public BookSeries? Series { get; set; }
    public Driver? AssignedToDriver { get; set; }
    public User? AssignedByUser { get; set; }
    public User? ReturnedToUser { get; set; }
    public User? VerifiedByUser { get; set; }
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}

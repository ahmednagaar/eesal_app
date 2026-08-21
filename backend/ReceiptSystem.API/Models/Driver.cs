namespace ReceiptSystem.API.Models;

public class Driver
{
    public int DriverId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }

    // Navigation
    public User CreatedByUser { get; set; } = null!;
    public ICollection<ReceiptBook> AssignedBooks { get; set; } = new List<ReceiptBook>();
    public ICollection<CollectionSession> Sessions { get; set; } = new List<CollectionSession>();
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    public ICollection<ReceiptGap> Gaps { get; set; } = new List<ReceiptGap>();
}

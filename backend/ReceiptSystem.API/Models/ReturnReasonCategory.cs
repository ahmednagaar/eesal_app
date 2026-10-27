namespace ReceiptSystem.API.Models;

public class ReturnReasonCategory
{
    public int ReasonId { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public string? ReasonNameEn { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystemDefault { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }

    // Navigation
    public User CreatedByUser { get; set; } = null!;
}

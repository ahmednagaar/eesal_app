namespace ReceiptSystem.API.Models;

/// <summary>
/// A series (بادئة/سلسلة) of return receipt books.
/// Example: Series "R-A" contains 200 books of 50 receipts each.
/// </summary>
public class ReturnBookSeries
{
    public int SeriesId { get; set; }
    public string SeriesCode { get; set; } = string.Empty; // "R-A", "R-B"...
    public int TotalBooks { get; set; } = 200;
    public int ReceiptsPerBook { get; set; } = 50;
    public int StartReceiptNumber { get; set; } = 1;
    public int EndReceiptNumber { get; set; } = 10000;
    public string Status { get; set; } = "Active"; // Active | Completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public User CreatedByUser { get; set; } = null!;
    public ICollection<ReturnReceiptBook> Books { get; set; } = new List<ReturnReceiptBook>();
}

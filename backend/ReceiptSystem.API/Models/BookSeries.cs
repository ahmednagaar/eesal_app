namespace ReceiptSystem.API.Models;

public class BookSeries
{
    public int SeriesId { get; set; }
    public string SeriesCode { get; set; } = string.Empty; // "A", "B", "C"...
    public int TotalBooks { get; set; } = 500;
    public int ReceiptsPerBook { get; set; } = 50;
    public int StartReceiptNumber { get; set; } = 1;
    public int EndReceiptNumber { get; set; } = 25000;
    public string Status { get; set; } = "Active"; // Active | Completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public User CreatedByUser { get; set; } = null!;
    public ICollection<ReceiptBook> Books { get; set; } = new List<ReceiptBook>();
}

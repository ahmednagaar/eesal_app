namespace ReceiptSystem.API.Models;

public class ExcelImportBatch
{
    public int BatchId { get; set; }
    public string UploadedFileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int UploadedByUserId { get; set; }
    public int TotalRowsCount { get; set; }
    public int AssignedRowsCount { get; set; } = 0;
    public string Status { get; set; } = "InProgress"; // InProgress | Completed | Discarded
    public decimal? LedgerTotalAmount { get; set; }
    public decimal? ActualCashTotal { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public User UploadedByUser { get; set; } = null!;
    public ICollection<ExcelImportRow> Rows { get; set; } = new List<ExcelImportRow>();
}

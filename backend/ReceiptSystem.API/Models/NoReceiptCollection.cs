namespace ReceiptSystem.API.Models;

/// <summary>
/// Records a cash collection event where no physical receipt was issued.
/// Exists as a separate entity from Receipt because it has no receipt number
/// and must not consume or corrupt the receipt numbering sequence.
/// </summary>
public class NoReceiptCollection
{
    public int Id { get; set; }
    public int? SessionId { get; set; }
    public int? BatchId { get; set; }
    public int? ImportRowId { get; set; }
    public int? DriverId { get; set; }
    public int? MerchantId { get; set; }
    public string? MerchantNameRaw { get; set; }
    public decimal Amount { get; set; }
    public DateTime CollectionDate { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public int EnteredByUserId { get; set; }
    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public CollectionSession? Session { get; set; }
    public ExcelImportBatch? Batch { get; set; }
    public ExcelImportRow? ImportRow { get; set; }
    public Driver? Driver { get; set; }
    public Merchant? Merchant { get; set; }
    public User EnteredByUser { get; set; } = null!;
}

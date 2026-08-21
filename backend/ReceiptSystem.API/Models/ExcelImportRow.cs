namespace ReceiptSystem.API.Models;

public class ExcelImportRow
{
    public int RowId { get; set; }
    public int BatchId { get; set; }
    public int RowIndex { get; set; }
    public string MerchantNameRaw { get; set; } = string.Empty;
    public int? MatchedMerchantId { get; set; }
    public bool IsNewMerchant { get; set; } = false;
    public decimal Amount { get; set; }
    public bool IsAssigned { get; set; } = false;
    public int? AssignedReceiptId { get; set; }
    public DateTime? AssignedAt { get; set; }

    // Extra ERP data
    public string? ErpInvoiceNumber { get; set; }
    public string? ErpUser { get; set; }
    public string? Branch { get; set; }
    public string? ErpEntryTime { get; set; }
    public string? Treasury { get; set; }
    public string? ErpId { get; set; }

    // Navigation
    public ExcelImportBatch Batch { get; set; } = null!;
    public Merchant? MatchedMerchant { get; set; }
    public Receipt? AssignedReceipt { get; set; }
}

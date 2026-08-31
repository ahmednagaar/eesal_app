using System.ComponentModel.DataAnnotations;

namespace ReceiptSystem.API.DTOs;

// ── Preview ──
public class ErpImportPreviewDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int RowCount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ErpImportPreviewRowDto> Rows { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class ErpImportPreviewRowDto
{
    public int RowIndex { get; set; }
    public string MerchantNameRaw { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int? MatchedMerchantId { get; set; }
    
    public string? MatchedMerchantName { get; set; }
    public bool IsNewMerchant { get; set; }

    // Extra ERP columns
    public string? ErpInvoiceNumber { get; set; }
    public string? ErpUser { get; set; }
    public string? Branch { get; set; }
    public string? ErpEntryTime { get; set; }
    public string? Treasury { get; set; }
    public string? ErpId { get; set; }
}

// ── Create Batch ──
public class CreateBatchDto
{
    [Required(ErrorMessage = "اسم الملف مطلوب")]
    public string FileName { get; set; } = string.Empty;
    public decimal? LedgerTotalAmount { get; set; }
    public string? Notes { get; set; }
    [Required(ErrorMessage = "بيانات الصفوف مطلوبة")]
    public List<CreateBatchRowDto> Rows { get; set; } = new();
}

public class CreateBatchRowDto
{
    public int RowIndex { get; set; }
    public string MerchantNameRaw { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int? MatchedMerchantId { get; set; }
    public bool IsNewMerchant { get; set; }
    public string? NewMerchantName { get; set; }

    // Extra ERP data
    public string? ErpInvoiceNumber { get; set; }
    public string? ErpUser { get; set; }
    public string? Branch { get; set; }
    public string? ErpEntryTime { get; set; }
    public string? Treasury { get; set; }
    public string? ErpId { get; set; }
}

// ── Assign Block ──
public class AssignBlockDto
{
    [Required] public int BatchId { get; set; }
    [Required(ErrorMessage = "يجب تحديد الصفوف")]
    public List<int> RowIds { get; set; } = new();
    [Required(ErrorMessage = "يجب تحديد السائق")]
    public int DriverId { get; set; }
    [Required(ErrorMessage = "تاريخ الجلسة مطلوب")]
    public DateTime SessionDate { get; set; }
    [Required(ErrorMessage = "رقم الإيصال الأول مطلوب")]
    public int StartReceiptNumber { get; set; }
    public string? RouteArea { get; set; }
}

public class AssignBlockResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int SessionId { get; set; }
    public int ReceiptsCreated { get; set; }
    public List<int> DetectedGaps { get; set; } = new();
    public bool HasGaps { get; set; }
}

// ── Assign Single (orphan) ──
public class AssignSingleDto
{
    [Required] public int BatchId { get; set; }
    [Required] public int RowId { get; set; }
    [Required(ErrorMessage = "يجب تحديد الجلسة")]
    public int TargetSessionId { get; set; }
    [Required(ErrorMessage = "رقم الإيصال مطلوب")]
    public int ReceiptNumber { get; set; }
}

public class AssignSingleResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int ReceiptId { get; set; }
    public int? ResolvedGapId { get; set; }
    public string? ResolvedGapMessage { get; set; }
}

// ── Add Manual Row ──
public class AddManualRowDto
{
    [Required] public int BatchId { get; set; }
    public int? MerchantId { get; set; }
    public string? NewMerchantName { get; set; }
    [Required(ErrorMessage = "المبلغ مطلوب")]
    public decimal Amount { get; set; }
}

// ── Batch Listing / Detail ──
public class BatchSummaryDto
{
    public int BatchId { get; set; }
    public string UploadedFileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public int TotalRowsCount { get; set; }
    public int AssignedRowsCount { get; set; }
    public int RemainingRowsCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class BatchDetailDto : BatchSummaryDto
{
    public decimal? LedgerTotalAmount { get; set; }
    public decimal? ActualCashTotal { get; set; }
    public decimal? Difference { get; set; }
    public string? Notes { get; set; }
    public List<BatchRowDto> Rows { get; set; } = new();
}

public class BatchRowDto
{
    public int RowId { get; set; }
    public int RowIndex { get; set; }
    public string MerchantNameRaw { get; set; } = string.Empty;
    public int? MatchedMerchantId { get; set; }
    public string? MatchedMerchantName { get; set; }
    public decimal Amount { get; set; }
    public bool IsAssigned { get; set; }
    public int? AssignedReceiptId { get; set; }
    public int? AssignedReceiptNumber { get; set; }
    public DateTime? AssignedAt { get; set; }
}

// ── Reconcile ──
public class ReconcileBatchDto
{
    [Required(ErrorMessage = "إجمالي الدفتر مطلوب")]
    public decimal LedgerTotalAmount { get; set; }
    [Required(ErrorMessage = "المبلغ الفعلي مطلوب")]
    public decimal ActualCashTotal { get; set; }
}

public class ReconcileBatchResultDto
{
    public decimal LedgerTotal { get; set; }
    public decimal ActualCash { get; set; }
    public decimal Difference { get; set; }
    public string Status { get; set; } = string.Empty; // match | surplus | shortage
    public string Message { get; set; } = string.Empty;
}

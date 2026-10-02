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
    [Required(ErrorMessage = "رقم الإيصال الأخير مطلوب")]
    public int EndReceiptNumber { get; set; }
    public string? RouteArea { get; set; }
    /// <summary>
    /// Explicit list of receipt numbers the user has identified as missing/lost.
    /// These positions are skipped during assignment and recorded as ReceiptGaps.
    /// Replaces the old dangerous ForceAssign flag.
    /// </summary>
    public List<int> SkippedReceiptNumbers { get; set; } = new();
}

public class AssignBlockResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int SessionId { get; set; }
    public int ReceiptsCreated { get; set; }
    public List<int> DetectedGaps { get; set; } = new();
    public bool HasGaps { get; set; }
    // Book breakdown
    public List<BookBreakdownDto> BookBreakdown { get; set; } = new();
    public bool SpansTwoBooks { get; set; }
}

public class BookBreakdownDto
{
    public int BookId { get; set; }
    public int BookNumber { get; set; }
    public int FirstReceipt { get; set; }
    public int LastReceipt { get; set; }
    public int ReceiptCount { get; set; }
}

// ── Preview Block (before confirming) ──
public class PreviewBlockDto
{
    [Required] public int BatchId { get; set; }
    [Required] public List<int> RowIds { get; set; } = new();
    [Required] public int StartReceiptNumber { get; set; }
    [Required] public int EndReceiptNumber { get; set; }
    public int? DriverId { get; set; }  // Optional — for book ownership validation
}

public class PreviewBlockResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public bool HasMismatch { get; set; }
    public int ExpectedCount { get; set; }
    public int ActualRowCount { get; set; }
    public string? MismatchMessage { get; set; }
    public bool SpansTwoBooks { get; set; }
    public List<BookBreakdownDto> BookBreakdown { get; set; } = new();
    public List<PreviewReceiptMappingDto> Mappings { get; set; } = new();
}

public class PreviewReceiptMappingDto
{
    public int RowId { get; set; }
    public int ReceiptNumber { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int? BookId { get; set; }
    public int? BookNumber { get; set; }
}

// ── Undo Block Assignment ──
public class UndoBlockResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int ReceiptsDeleted { get; set; }
    public int RowsUnassigned { get; set; }
}

// ── Undo Single Receipt ──
public class UndoSingleResultDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int ReceiptId { get; set; }
    public int RowId { get; set; }
    public int? ReopenedGapId { get; set; }
}

// ── Session Summary (for Undo confirmation) ──
public class SessionSummaryDto
{
    public int SessionId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public string RouteArea { get; set; } = string.Empty;
    public int FirstReceiptNumber { get; set; }
    public int LastReceiptNumber { get; set; }
    public int TotalReceiptsCount { get; set; }
    public decimal TotalAmountCollected { get; set; }
    public bool IsConfirmed { get; set; }
    public bool HasGaps { get; set; }
    public string ImportSource { get; set; } = string.Empty;
}

// ── Driver's Current Books ──
public class DriverBooksDto
{
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public List<DriverBookInfoDto> Books { get; set; } = new();
}

public class DriverBookInfoDto
{
    public int BookId { get; set; }
    public int BookNumber { get; set; }
    public int StartReceiptNumber { get; set; }
    public int EndReceiptNumber { get; set; }
    public int UsedCount { get; set; }
    public int RemainingCount { get; set; }
    public string Status { get; set; } = string.Empty;
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

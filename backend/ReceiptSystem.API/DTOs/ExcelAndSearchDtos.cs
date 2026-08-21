using System.ComponentModel.DataAnnotations;

namespace ReceiptSystem.API.DTOs;

// ══════════════════════════════════
// EXCEL IMPORT DTOs
// ══════════════════════════════════

public class ExcelPreviewRowDto
{
    public int RowIndex { get; set; }
    public string MerchantNameRaw { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsWithoutReceipt { get; set; }
    public string? DesktopSystemUser { get; set; }
    public int? MatchedMerchantId { get; set; }
    public string? MatchedMerchantName { get; set; }
    public bool IsNewMerchant { get; set; }
    public string? NewMerchantName { get; set; }
}

public class ExcelPreviewResponseDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? MerchantNameColumn { get; set; }
    public string? AmountColumn { get; set; }
    public int RowCount { get; set; }
    public decimal TotalAmount { get; set; }
    public int WithoutReceiptCount { get; set; }
    public List<ExcelPreviewRowDto> Rows { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class SaveExcelRowDto
{
    public int RowIndex { get; set; }
    public string MerchantNameRaw { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsWithoutReceipt { get; set; }
    public string? DesktopSystemUser { get; set; }
    public int? MatchedMerchantId { get; set; }
    public bool IsNewMerchant { get; set; }
    public string? NewMerchantName { get; set; }
}

public class SaveExcelSessionDto
{
    [Required(ErrorMessage = "اختر السائق")]
    public int DriverId { get; set; }
    [Required(ErrorMessage = "التاريخ مطلوب")]
    public DateTime SessionDate { get; set; }
    [Required(ErrorMessage = "المنطقة مطلوبة")]
    public string RouteArea { get; set; } = string.Empty;
    [Required(ErrorMessage = "رقم أول إيصال مطلوب")]
    public int StartReceiptNumber { get; set; }
    [Required(ErrorMessage = "رقم آخر إيصال مطلوب")]
    public int EndReceiptNumber { get; set; }
    public string? OriginalFileName { get; set; }
    public string? Notes { get; set; }
    [Required(ErrorMessage = "يجب إدخال صف واحد على الأقل")]
    [MinLength(1)]
    public List<SaveExcelRowDto> Rows { get; set; } = new();
}

public class SaveExcelResponseDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int SessionId { get; set; }
    public int ReceiptsCreated { get; set; }
    public int NewMerchantsCreated { get; set; }
    public List<int> DetectedGaps { get; set; } = new();
    public bool HasGaps { get; set; }
}

// ══════════════════════════════════
// SEARCH DTOs
// ══════════════════════════════════

public class SearchReceiptsFilterDto
{
    public string? MerchantName { get; set; }
    public int? DriverId { get; set; }
    public string? RouteArea { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? ReceiptNumber { get; set; }
    public decimal? AmountMin { get; set; }
    public decimal? AmountMax { get; set; }
    public string? ImportSource { get; set; }
    public bool? WithoutReceipt { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class SearchReceiptResultDto
{
    public int ReceiptId { get; set; }
    public int ReceiptNumber { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string RouteArea { get; set; } = string.Empty;
    public DateTime CollectionDate { get; set; }
    public decimal Amount { get; set; }
    public string ImportSource { get; set; } = string.Empty;
    public bool IsWithoutReceipt { get; set; }
    public int SessionId { get; set; }
}

public class SearchResultTotalsDto
{
    public decimal TotalAmount { get; set; }
    public int ReceiptCount { get; set; }
}

public class SearchReceiptsResponseDto
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public List<SearchReceiptResultDto> Results { get; set; } = new();
    public SearchResultTotalsDto Totals { get; set; } = new();
}

public class MerchantPaymentHistoryDto
{
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public decimal TotalCollected { get; set; }
    public int TotalReceipts { get; set; }
    public List<MerchantPaymentEntryDto> History { get; set; } = new();
}

public class MerchantPaymentEntryDto
{
    public DateTime Date { get; set; }
    public int ReceiptNumber { get; set; }
    public decimal Amount { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string RouteArea { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public string ImportSource { get; set; } = string.Empty;
}

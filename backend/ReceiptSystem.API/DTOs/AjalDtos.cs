namespace ReceiptSystem.API.DTOs;

// ══════════════════════════════════
// AJAL REGISTER DTOs
// ══════════════════════════════════

// ─── Daily Register ───
public class AjalDailyResponseDto
{
    public DateTime SessionDate { get; set; }
    public int TotalInvoices { get; set; }
    public int ActiveInvoices { get; set; }
    public int CancelledInvoices { get; set; }
    public decimal TotalAmount { get; set; }
    public List<AjalRouteGroupDto> Routes { get; set; } = new();
}

public class AjalRouteGroupDto
{
    public int? RouteId { get; set; }
    public string RouteName { get; set; } = "غير محدد";
    public int InvoiceCount { get; set; }
    public decimal RouteTotal { get; set; }
    public List<AjalInvoiceDto> Invoices { get; set; } = new();
}

public class AjalInvoiceDto
{
    public int AjalInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? MerchantPhone { get; set; }
    public string? MerchantCity { get; set; }
    public decimal Amount { get; set; }
    public decimal? OriginalAmount { get; set; }
    public string? CallCenterEmployeeName { get; set; }
    public string InvoiceStatus { get; set; } = "Active";
    public string? ModificationNote { get; set; }
    public string? Notes { get; set; }
    public string ImportSource { get; set; } = "Manual";
    public string EnteredByUserName { get; set; } = string.Empty;
    public DateTime EnteredAt { get; set; }
}

// ─── Create ───
public class CreateAjalInvoicesDto
{
    public DateTime SessionDate { get; set; }
    public int? RouteId { get; set; }
    public List<CreateAjalInvoiceRowDto> Invoices { get; set; } = new();
}

public class CreateAjalInvoiceRowDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int MerchantId { get; set; }
    public string? CallCenterEmployeeName { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class CreateAjalResponseDto
{
    public bool Success { get; set; }
    public int Saved { get; set; }
    public List<string> Errors { get; set; } = new();
}

// ─── Edit ───
public class EditAjalInvoiceDto
{
    public decimal Amount { get; set; }
    public string? CallCenterEmployeeName { get; set; }
    public string? InvoiceStatus { get; set; }
    public string? ModificationNote { get; set; }
    public int? RouteId { get; set; }
    public string? Notes { get; set; }
}

public class CancelAjalInvoiceDto
{
    public string Reason { get; set; } = string.Empty;
}

// ─── Merchant History ───
public class AjalMerchantHistoryDto
{
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int TotalInvoices { get; set; }
    public decimal ActiveTotal { get; set; }
    public List<AjalMerchantHistoryEntryDto> Invoices { get; set; } = new();
}

public class AjalMerchantHistoryEntryDto
{
    public DateTime SessionDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? RouteName { get; set; }
    public string? CallCenterEmployeeName { get; set; }
    public string InvoiceStatus { get; set; } = "Active";
}

// ─── Employee Performance ───
public class AjalEmployeePerformanceResponseDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int GrandTotalInvoices { get; set; }
    public decimal GrandTotalAmount { get; set; }
    public List<AjalEmployeeDto> Employees { get; set; } = new();
}

public class AjalEmployeeDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageInvoice { get; set; }
    public List<AjalEmployeeDailyDto> DailyBreakdown { get; set; } = new();
}

public class AjalEmployeeDailyDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

// ─── Search ───
public class AjalSearchFilterDto
{
    public string? MerchantName { get; set; }
    public string? InvoiceNumber { get; set; }
    public int? RouteId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class AjalSearchResponseDto
{
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public int Page { get; set; }
    public List<AjalInvoiceDto> Results { get; set; } = new();
}

// ─── Excel Import ───
public class AjalExcelPreviewRowDto
{
    public int RowIndex { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string MerchantNameRaw { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? CallCenterEmployeeName { get; set; }
    public int? MatchedMerchantId { get; set; }
    public string? MatchedMerchantName { get; set; }
    public bool IsNewMerchant { get; set; }
    public bool IsDuplicate { get; set; }
}

public class AjalExcelPreviewResponseDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int RowCount { get; set; }
    public decimal TotalAmount { get; set; }
    public int DuplicateCount { get; set; }
    public List<AjalExcelPreviewRowDto> Rows { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class SaveAjalExcelDto
{
    public DateTime SessionDate { get; set; }
    public int? RouteId { get; set; }
    public List<SaveAjalExcelRowDto> Rows { get; set; } = new();
}

public class SaveAjalExcelRowDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int? MerchantId { get; set; }
    public bool IsNewMerchant { get; set; }
    public string? NewMerchantName { get; set; }
    public decimal Amount { get; set; }
    public string? CallCenterEmployeeName { get; set; }
}

public class SaveAjalExcelResponseDto
{
    public bool Success { get; set; }
    public int Saved { get; set; }
    public int Skipped { get; set; }
    public int NewMerchantsCreated { get; set; }
}

// ─── Settings ───
public class AjalPrefixSettingsDto
{
    public string Prefix { get; set; } = string.Empty;
    public int WarningThreshold { get; set; }
    public int TotalDigits { get; set; }
    public string NextPrefix { get; set; } = string.Empty;
}

public class UpdatePrefixDto
{
    public string NewPrefix { get; set; } = string.Empty;
}

// ─── Dashboard ───
public class AjalDashboardSummaryDto
{
    public DateTime TodaySessionDate { get; set; }
    public int InvoiceCount { get; set; }
    public decimal TotalAmount { get; set; }
    public int RouteCount { get; set; }
    public int CancelledCount { get; set; }
}

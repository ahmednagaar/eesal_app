namespace ReceiptSystem.API.DTOs;

// ══════════════════════════════════
// AJAL REGISTER DTOs (Redesigned)
// ══════════════════════════════════

// ─── Create Session with Entries ───
public class CreateAjalSessionDto
{
    public DateTime SessionDate { get; set; }
    public int RouteId { get; set; }
    public int? DriverId { get; set; }
    public string? Notes { get; set; }
    public List<CreateAjalEntryDto> Entries { get; set; } = new();
}

public class CreateAjalEntryDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int MerchantId { get; set; }
    public decimal? Amount { get; set; }
    public string? Notes { get; set; }
}

public class CreateAjalSessionResponseDto
{
    public bool Success { get; set; }
    public int SessionId { get; set; }
    public int EntriesSaved { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

// ─── Update Session ───
public class UpdateAjalSessionDto
{
    public int? DriverId { get; set; }
    public string? Notes { get; set; }
}

// ─── Add Entries to Existing Session ───
public class AddAjalEntriesDto
{
    public int SessionId { get; set; }
    public List<CreateAjalEntryDto> Entries { get; set; } = new();
}

// ─── Update Entry ───
public class UpdateAjalEntryDto
{
    public string? InvoiceNumber { get; set; }
    public int? MerchantId { get; set; }
    public decimal? Amount { get; set; }
    public string? Notes { get; set; }
}

// ─── Review ───
public class ReviewBatchDto
{
    public List<int> EntryIds { get; set; } = new();
}

// ─── Daily View (All invoices for a day, sorted ascending by InvoiceNumber) ───
public class AjalDailyViewDto
{
    public DateTime SessionDate { get; set; }
    public int TotalEntries { get; set; }
    public int ReviewedCount { get; set; }
    public int PendingCount { get; set; }
    public int RouteCount { get; set; }
    public List<AjalDailyEntryDto> Entries { get; set; } = new();
}

public class AjalDailyEntryDto
{
    public int EntryId { get; set; }
    public int SessionId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? MerchantCity { get; set; }
    public decimal? Amount { get; set; }
    public int RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public int? DriverId { get; set; }
    public string? DriverName { get; set; }
    public bool IsReviewed { get; set; }
    public string? ReviewedByUserName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime EnteredAt { get; set; }
}

// ─── Session Detail (Route/Driver grouped view) ───
public class AjalSessionDetailDto
{
    public int SessionId { get; set; }
    public DateTime SessionDate { get; set; }
    public int RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public int? DriverId { get; set; }
    public string? DriverName { get; set; }
    public string? Notes { get; set; }
    public int EntryCount { get; set; }
    public int ReviewedCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string EnteredByUserName { get; set; } = string.Empty;
    public DateTime EnteredAt { get; set; }
    public List<AjalEntryDto> Entries { get; set; } = new();
}

public class AjalEntryDto
{
    public int EntryId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? MerchantCity { get; set; }
    public decimal? Amount { get; set; }
    public int SortOrder { get; set; }
    public bool IsReviewed { get; set; }
    public string? ReviewedByUserName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime EnteredAt { get; set; }
}

// ─── Sessions List (for a date) ───
public class AjalSessionSummaryDto
{
    public int SessionId { get; set; }
    public DateTime SessionDate { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string? DriverName { get; set; }
    public int EntryCount { get; set; }
    public int ReviewedCount { get; set; }
    public decimal TotalAmount { get; set; }
}

// ─── Search ───
public class AjalSearchFilterDto
{
    public string? InvoiceNumber { get; set; }
    public string? MerchantName { get; set; }
    public int? RouteId { get; set; }
    public int? DriverId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? ReviewStatus { get; set; }  // "all" | "reviewed" | "pending"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class AjalSearchResponseDto
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public List<AjalDailyEntryDto> Results { get; set; } = new();
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

// ─── Duplicate Check ───
public class DuplicateCheckResultDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public bool IsDuplicate { get; set; }
    public DateTime? ExistingDate { get; set; }
    public string? ExistingRoute { get; set; }
}

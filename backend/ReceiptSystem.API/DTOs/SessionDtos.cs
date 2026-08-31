using System.ComponentModel.DataAnnotations;

namespace ReceiptSystem.API.DTOs;

// ── Sessions ──
public class SessionDto
{
    public int SessionId { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public string RouteArea { get; set; } = string.Empty;
    public int FirstReceiptNumber { get; set; }
    public int LastReceiptNumber { get; set; }
    public int TotalReceiptsCount { get; set; }
    public decimal TotalAmountCollected { get; set; }
    public bool HasGaps { get; set; }
    public string? Notes { get; set; }
    public string EnteredByName { get; set; } = string.Empty;
    public DateTime EnteredAt { get; set; }
    public string ImportSource { get; set; } = "Manual";
    public string? OriginalFileName { get; set; }
}

public class SessionWithReceiptsDto : SessionDto
{
    public List<ReceiptDto> Receipts { get; set; } = new();
    public List<GapDto> Gaps { get; set; } = new();
}

public class CreateSessionDto
{
    [Required(ErrorMessage = "اختر السائق")]
    public int DriverId { get; set; }
    [Required(ErrorMessage = "التاريخ مطلوب")]
    public DateTime SessionDate { get; set; }
    [Required(ErrorMessage = "المنطقة مطلوبة")]
    [MaxLength(200)]
    public string RouteArea { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string? Notes { get; set; }
    [Required(ErrorMessage = "يجب إدخال إيصال واحد على الأقل")]
    [MinLength(1, ErrorMessage = "يجب إدخال إيصال واحد على الأقل")]
    public List<CreateReceiptDto> Receipts { get; set; } = new();
}

// ── Receipts ──
public class ReceiptDto
{
    public int ReceiptId { get; set; }
    public int ReceiptNumber { get; set; }
    public int? BookId { get; set; }
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public DateTime CollectionDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPartialPayment { get; set; }
    public string? Notes { get; set; }
    public string ImportSource { get; set; } = "Manual";
    public bool IsWithoutReceipt { get; set; }
    public string? DesktopSystemUser { get; set; }
}

public class CreateReceiptDto
{
    [Required] public int ReceiptNumber { get; set; }
    [Required] public int BookId { get; set; }
    [Required] public int MerchantId { get; set; }
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
    public decimal Amount { get; set; }
    public bool IsPartialPayment { get; set; } = false;
    public string? Notes { get; set; }
}

// ── Gaps ──
public class GapDto
{
    public int GapId { get; set; }
    public int MissingReceiptNumber { get; set; }
    public int DetectedInSessionId { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByName { get; set; }
}

public class ResolveGapDto
{
    [Required(ErrorMessage = "الحالة مطلوبة")]
    public string Status { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string? Resolution { get; set; }
}

public class GapSummaryDto
{
    public int OpenCount { get; set; }
    public int UnderInvestigationCount { get; set; }
    public int ResolvedCount { get; set; }
    public int TotalCount { get; set; }
}

// ── Reports ──
public class DailyReportDto
{
    public DateTime ReportDate { get; set; }
    public string PreparedBy { get; set; } = string.Empty;
    public int TotalDrivers { get; set; }
    public int TotalReceipts { get; set; }
    public decimal TotalAmount { get; set; }
    public int MissingReceiptsCount { get; set; }
    public List<DriverDailyReportDto> DriverReports { get; set; } = new();
}

public class DriverDailyReportDto
{
    public string DriverName { get; set; } = string.Empty;
    public string RouteArea { get; set; } = string.Empty;
    public int ReceiptCount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ReceiptLineDto> Receipts { get; set; } = new();
    public List<int> MissingReceipts { get; set; } = new();
}

public class ReceiptLineDto
{
    public int ReceiptNumber { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// ── Driver Performance ──
public class DriverPerformanceDto
{
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int TotalReceipts { get; set; }
    public decimal TotalAmountCollected { get; set; }
    public int TotalGapsDetected { get; set; }
    public int TotalGapsResolved { get; set; }
    public int TotalGapsOpen { get; set; }
    public decimal GapRatePercent { get; set; }
}

// ── Missing Driver Alert ──
public class MissingDriverDto
{
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? LastSessionDate { get; set; }
}


// ── Dashboard ──
public class DashboardDto
{
    public decimal TotalAmountToday { get; set; }
    public int DriversToday { get; set; }
    public int OpenGapsCount { get; set; }
    public int ReceiptsToday { get; set; }
    public List<SessionDto> TodaySessions { get; set; } = new();
}

// ── Session Reconciliation ──
public class ReconcileSessionDto
{
    [Required(ErrorMessage = "المبلغ الفعلي مطلوب")]
    public decimal ActualCashCounted { get; set; }
}

public class UnlockSessionDto
{
    [Required(ErrorMessage = "سبب فتح القفل مطلوب")]
    public string Reason { get; set; } = string.Empty;
}

public class ReconcileResponseDto
{
    public string Status { get; set; } = string.Empty; // match | surplus | shortage
    public decimal Difference { get; set; }
    public string Message { get; set; } = string.Empty;
}

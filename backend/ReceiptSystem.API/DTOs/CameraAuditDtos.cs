namespace ReceiptSystem.API.DTOs;

// ══════════════════════════════════════
// Loading Audit DTOs
// ══════════════════════════════════════

public class LoadingAuditListDto
{
    public int AuditId { get; set; }
    public DateTime AuditDate { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string? TripReference { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReviewedByUserName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int DiscrepancyCount { get; set; }
    public int OpenDiscrepancyCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoadingAuditDto
{
    public int AuditId { get; set; }
    public DateTime AuditDate { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string? TripReference { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReviewedByUserName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<LoadingDiscrepancyDto> Discrepancies { get; set; } = new();
}

public class CreateLoadingAuditDto
{
    public DateTime AuditDate { get; set; }
    public int DriverId { get; set; }
    public string? TripReference { get; set; }
    public string? Notes { get; set; }
    public List<CreateDiscrepancyDto>? Discrepancies { get; set; }
}

// ══════════════════════════════════════
// Loading Discrepancy DTOs
// ══════════════════════════════════════

public class LoadingDiscrepancyDto
{
    public int DiscrepancyId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int ProductUnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal ExpectedQty { get; set; }
    public decimal ActualQty { get; set; }
    public decimal DifferenceQty { get; set; }
    public string DiscrepancyType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByUserName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CaseNoteCount { get; set; }
}

public class CreateDiscrepancyDto
{
    public int ProductId { get; set; }
    public int ProductUnitId { get; set; }
    public decimal ExpectedQty { get; set; }
    public decimal ActualQty { get; set; }
    public string DiscrepancyType { get; set; } = "Shortage";
    public string? Notes { get; set; }
}

public class UpdateDiscrepancyStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public string? NoteText { get; set; } // Optional note to add when changing status
}

// ══════════════════════════════════════
// Case Note DTOs
// ══════════════════════════════════════

public class CaseNoteDto
{
    public int NoteId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string? StatusChange { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateCaseNoteDto
{
    public string NoteText { get; set; } = string.Empty;
}

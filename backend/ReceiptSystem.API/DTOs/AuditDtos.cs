using System.ComponentModel.DataAnnotations;

namespace ReceiptSystem.API.DTOs;

// ══════════════════════════════════
// AUDIT LOG DTOs
// ══════════════════════════════════

public class AuditLogDto
{
    public int AuditLogId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuditLogFilterDto
{
    public int? UserId { get; set; }
    public string? EntityType { get; set; }
    public string? Action { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class AuditLogResponseDto
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public List<AuditLogDto> Results { get; set; } = new();
}

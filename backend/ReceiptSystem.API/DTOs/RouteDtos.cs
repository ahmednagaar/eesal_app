using System.ComponentModel.DataAnnotations;

namespace ReceiptSystem.API.DTOs;

// ── Routes ──
public class RouteDto
{
    public int RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MerchantCount { get; set; }
}

public class CreateRouteDto
{
    [Required(ErrorMessage = "اسم الخط مطلوب")]
    [MaxLength(200)]
    public string RouteName { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Notes { get; set; }
}

// ── Route Merchants ──
public class RouteMerchantDto
{
    public int RouteMerchantId { get; set; }
    public int RouteId { get; set; }
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? City { get; set; }
    public int PositionOrder { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class AddMerchantToRouteDto
{
    [Required(ErrorMessage = "اختر التاجر")]
    public int MerchantId { get; set; }
    [Required(ErrorMessage = "حدد الترتيب")]
    [Range(1, int.MaxValue)]
    public int Position { get; set; }
    [MaxLength(300)]
    public string? Notes { get; set; }
}

public class UpdatePositionDto
{
    [Required]
    public int NewPosition { get; set; }
}

// ── Delivery Days ──
public class DeliveryDayDto
{
    public int DeliveryDayId { get; set; }
    public int RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public string? AssignedDriver { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int InvoiceCount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CreateDeliveryDayDto
{
    [Required(ErrorMessage = "اختر الخط")]
    public int RouteId { get; set; }
    [Required(ErrorMessage = "التاريخ مطلوب")]
    public DateTime DeliveryDate { get; set; }
    [MaxLength(100)]
    public string? AssignedDriver { get; set; }
    [MaxLength(500)]
    public string? Notes { get; set; }
}

// ── Day Invoices ──
public class DayInvoiceDto
{
    public int DayInvoiceId { get; set; }
    public int DeliveryDayId { get; set; }
    public int RouteMerchantId { get; set; }
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? City { get; set; }
    public int PositionOrder { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public string? Notes { get; set; }
    public int? ManualPositionOverride { get; set; }
}

public class CreateDayInvoiceDto
{
    [Required] public int RouteMerchantId { get; set; }
    [Required] public int MerchantId { get; set; }
    [MaxLength(50)] public string? InvoiceNumber { get; set; }
    [MaxLength(200)] public string? Quantity { get; set; }
    public decimal? Amount { get; set; }
    [MaxLength(300)] public string? Notes { get; set; }
}

public class UpdateDayInvoiceDto
{
    [MaxLength(50)] public string? InvoiceNumber { get; set; }
    [MaxLength(200)] public string? Quantity { get; set; }
    public decimal? Amount { get; set; }
    [MaxLength(300)] public string? Notes { get; set; }
}

public class ReorderInvoiceDto
{
    [Required] public int InvoiceId { get; set; }
    [Required] public int NewPosition { get; set; }
}

// ── Print Sheets ──
public class SheetDto
{
    public string RouteName { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public string? AssignedDriver { get; set; }
    public string PreparedBy { get; set; } = string.Empty;
    public int TotalMerchants { get; set; }
    public decimal TotalAmount { get; set; }
    public List<SheetLineDto> Lines { get; set; } = new();
}

public class SheetLineDto
{
    public int SequenceNumber { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? Quantity { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal? Amount { get; set; }
    public string? Notes { get; set; }
}

public class UnlockDayDto
{
    public string Reason { get; set; } = string.Empty;
}

namespace ReceiptSystem.API.DTOs;

// ══════════════════════════════════════
// Return Book Series DTOs
// ══════════════════════════════════════

public class ReturnBookSeriesDto
{
    public int SeriesId { get; set; }
    public string SeriesCode { get; set; } = string.Empty;
    public int TotalBooks { get; set; }
    public int ReceiptsPerBook { get; set; }
    public int StartReceiptNumber { get; set; }
    public int EndReceiptNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedByUserName { get; set; }
    public string? Notes { get; set; }
    public int BookCount { get; set; }
    public int AssignedBookCount { get; set; }
}

public class CreateReturnBookSeriesDto
{
    public string SeriesCode { get; set; } = string.Empty;
    public int TotalBooks { get; set; } = 200;
    public int ReceiptsPerBook { get; set; } = 50;
    public int StartReceiptNumber { get; set; } = 1;
    public string? Notes { get; set; }
}

// ══════════════════════════════════════
// Return Receipt Book DTOs
// ══════════════════════════════════════

public class ReturnReceiptBookDto
{
    public int BookId { get; set; }
    public int? SeriesId { get; set; }
    public string? SeriesCode { get; set; }
    public int BookNumber { get; set; }
    public int StartReceiptNumber { get; set; }
    public int EndReceiptNumber { get; set; }
    public int? AssignedToDriverId { get; set; }
    public string? AssignedToDriverName { get; set; }
    public DateTime? AssignedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedByUserName { get; set; }
    public int ReturnsCount { get; set; }
}

public class AssignReturnBookDto
{
    public int DriverId { get; set; }
    public string? Notes { get; set; }
}

public class ReturnBookActionDto
{
    public string? Notes { get; set; }
}

// ══════════════════════════════════════
// Goods Return DTOs
// ══════════════════════════════════════

public class GoodsReturnListDto
{
    public int ReturnId { get; set; }
    public int ReturnReceiptNumber { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string ReasonName { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime EnteredAt { get; set; }
    public string? EnteredByUserName { get; set; }
}

public class GoodsReturnDto
{
    public int ReturnId { get; set; }
    public int ReturnReceiptNumber { get; set; }
    public int? BookId { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int ReasonId { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime EnteredAt { get; set; }
    public string? EnteredByUserName { get; set; }
    public List<GoodsReturnItemDto> Items { get; set; } = new();
}

public class CreateGoodsReturnDto
{
    public int ReturnReceiptNumber { get; set; }
    public int? BookId { get; set; }
    public int DriverId { get; set; }
    public int ReasonId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string? CustomerName { get; set; }
    public string? Notes { get; set; }
    public List<CreateGoodsReturnItemDto> Items { get; set; } = new();
}

public class UpdateGoodsReturnDto
{
    public int ReasonId { get; set; }
    public string? CustomerName { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Open";
}

// ══════════════════════════════════════
// Goods Return Item DTOs
// ══════════════════════════════════════

public class GoodsReturnItemDto
{
    public int ItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int ProductUnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
}

public class CreateGoodsReturnItemDto
{
    public int ProductId { get; set; }
    public int ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
}

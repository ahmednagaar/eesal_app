namespace ReceiptSystem.API.Models;

public class ProductUnit
{
    public int ProductUnitId { get; set; }
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public bool IsDefault { get; set; } = false;
    public decimal? ConversionToBaseQty { get; set; }
    public int? BaseUnitId { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public UnitOfMeasure Unit { get; set; } = null!;
    public UnitOfMeasure? BaseUnit { get; set; }
}

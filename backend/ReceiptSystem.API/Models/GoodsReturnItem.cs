namespace ReceiptSystem.API.Models;

/// <summary>
/// A line item on a goods return — which product, in what unit, how many, at what price.
/// </summary>
public class GoodsReturnItem
{
    public int ItemId { get; set; }
    public int ReturnId { get; set; }
    public int ProductId { get; set; }
    public int ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public GoodsReturn Return { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductUnit ProductUnit { get; set; } = null!;
}

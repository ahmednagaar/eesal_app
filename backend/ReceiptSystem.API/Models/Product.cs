namespace ReceiptSystem.API.Models;

public class Product
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }

    // Navigation
    public ProductCategory Category { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<ProductUnit> ProductUnits { get; set; } = new List<ProductUnit>();
}

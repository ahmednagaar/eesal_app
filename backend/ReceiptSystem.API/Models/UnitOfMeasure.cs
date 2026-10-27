namespace ReceiptSystem.API.Models;

public class UnitOfMeasure
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string? UnitAbbreviation { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

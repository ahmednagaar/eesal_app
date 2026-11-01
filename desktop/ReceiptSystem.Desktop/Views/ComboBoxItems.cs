namespace ReceiptSystem.Desktop.Views;

/// <summary>
/// Strongly-typed combo box item classes to avoid WPF anonymous type binding issues.
/// WPF on .NET Core cannot bind to anonymous type properties via DisplayMemberPath/SelectedValuePath.
/// </summary>

public class DriverComboItem
{
    public int driverId { get; set; }
    public string fullName { get; set; } = string.Empty;
    public override string ToString() => fullName;
}

public class BookComboItem
{
    public int bookId { get; set; }
    public string display { get; set; } = string.Empty;
    public override string ToString() => display;
}

public class MerchantComboItem
{
    public int merchantId { get; set; }
    public string merchantName { get; set; } = string.Empty;
    public override string ToString() => merchantName;
}

public class BatchComboItem
{
    public int batchId { get; set; }
    public string display { get; set; } = string.Empty;
    public override string ToString() => display;
}

public class CategoryComboItem
{
    public int categoryId { get; set; }
    public string categoryName { get; set; } = string.Empty;
    public override string ToString() => categoryName;
}

public class UnitComboItem
{
    public int unitId { get; set; }
    public string unitName { get; set; } = string.Empty;
    public override string ToString() => unitName;
}

public class ReasonComboItem
{
    public int reasonId { get; set; }
    public string reasonName { get; set; } = string.Empty;
    public override string ToString() => reasonName;
}

public class ReturnBookComboItem
{
    public int bookId { get; set; }
    public string bookLabel { get; set; } = string.Empty;
    public override string ToString() => bookLabel;
}

public class ProductComboItem
{
    public int productId { get; set; }
    public string productName { get; set; } = string.Empty;
    public string? categoryName { get; set; }
    public string? productCode { get; set; }
    public List<ProductUnitComboItem> units { get; set; } = new();
    public override string ToString() => productName;
}

public class ProductUnitComboItem
{
    public int productUnitId { get; set; }
    public int unitId { get; set; }
    public string unitName { get; set; } = string.Empty;
    public bool isDefault { get; set; }
    public override string ToString() => unitName;
}

public class SeriesComboItem
{
    public int seriesId { get; set; }
    public string seriesCode { get; set; } = string.Empty;
    public override string ToString() => seriesCode;
}

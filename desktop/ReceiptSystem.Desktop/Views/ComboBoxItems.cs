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

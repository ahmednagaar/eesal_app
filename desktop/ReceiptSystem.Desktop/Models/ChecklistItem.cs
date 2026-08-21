using System.ComponentModel;

namespace ReceiptSystem.Desktop.Models;

/// <summary>
/// Represents a single merchant in the daily checklist.
/// Implements INotifyPropertyChanged so the UI updates when values change.
/// </summary>
public class ChecklistItem : INotifyPropertyChanged
{
    public int RouteMerchantId { get; set; }
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = "";
    public string? City { get; set; }
    public int PositionOrder { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
    }

    private string _invoiceNumber = "";
    public string InvoiceNumber
    {
        get => _invoiceNumber;
        set { _invoiceNumber = value; OnPropertyChanged(nameof(InvoiceNumber)); }
    }

    private string _quantity = "";
    public string Quantity
    {
        get => _quantity;
        set { _quantity = value; OnPropertyChanged(nameof(Quantity)); }
    }

    private decimal? _amount;
    public decimal? Amount
    {
        get => _amount;
        set { _amount = value; OnPropertyChanged(nameof(Amount)); }
    }

    private string _notes = "";
    public string Notes
    {
        get => _notes;
        set { _notes = value; OnPropertyChanged(nameof(Notes)); }
    }

    public int? DayInvoiceId { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

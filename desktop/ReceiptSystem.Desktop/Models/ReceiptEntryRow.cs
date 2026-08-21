using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReceiptSystem.Desktop.Models;

/// <summary>
/// Represents one receipt row in the quick-add session entry form.
/// </summary>
public class ReceiptEntryRow : INotifyPropertyChanged
{
    private int _receiptNumber;
    private int _merchantId;
    private string _merchantDisplayName = string.Empty;
    private decimal _amount;
    private bool _isPartialPayment;
    private string? _notes;
    private bool _isDuplicate;
    private string? _duplicateWarning;

    public int ReceiptNumber
    {
        get => _receiptNumber;
        set { _receiptNumber = value; OnPropertyChanged(); }
    }

    public int MerchantId
    {
        get => _merchantId;
        set { _merchantId = value; OnPropertyChanged(); }
    }

    public string MerchantDisplayName
    {
        get => _merchantDisplayName;
        set { _merchantDisplayName = value; OnPropertyChanged(); }
    }

    public decimal Amount
    {
        get => _amount;
        set { _amount = value; OnPropertyChanged(); }
    }

    public bool IsPartialPayment
    {
        get => _isPartialPayment;
        set { _isPartialPayment = value; OnPropertyChanged(); }
    }

    public string? Notes
    {
        get => _notes;
        set { _notes = value; OnPropertyChanged(); }
    }

    public bool IsDuplicate
    {
        get => _isDuplicate;
        set { _isDuplicate = value; OnPropertyChanged(); }
    }

    public string? DuplicateWarning
    {
        get => _duplicateWarning;
        set { _duplicateWarning = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

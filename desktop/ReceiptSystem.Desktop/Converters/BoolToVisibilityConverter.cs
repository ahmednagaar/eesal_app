using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReceiptSystem.Desktop.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool bValue = false;
        if (value is bool b)
        {
            bValue = b;
        }

        if (Invert)
        {
            bValue = !bValue;
        }

        return bValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

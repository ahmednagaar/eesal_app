using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReceiptSystem.Desktop.Converters;

/// <summary>
/// Converts null or empty string to Visibility.Collapsed, otherwise Visibility.Visible.
/// Used for hiding UI elements when optional data (like City) is not present.
/// </summary>
public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
            return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

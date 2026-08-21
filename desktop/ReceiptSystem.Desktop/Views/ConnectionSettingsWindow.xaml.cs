using System.Windows;
using System.Windows.Input;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class ConnectionSettingsWindow : Window
{
    private readonly ApiClient _api;

    public ConnectionSettingsWindow(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        UrlBox.Text = AppSettings.ApiBaseUrl;
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var url = UrlBox.Text.Trim();
        if (string.IsNullOrEmpty(url)) return;

        AppSettings.ApiBaseUrl = url;
        AppSettings.Save();
        
        _api.BaseUrl = url;
        
        MessageBox.Show("تم حفظ الإعدادات بنجاح.", "إعدادات الاتصال", MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }
}

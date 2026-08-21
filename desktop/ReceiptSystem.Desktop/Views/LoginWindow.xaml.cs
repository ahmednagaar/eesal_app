using System.Linq;
using System.Windows;
using System.Windows.Input;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly ApiClient _api = new();

    public LoginWindow()
    {
        InitializeComponent();
        AppSettings.Load();

        // First-run: if no server has been configured, force the user to set one
        if (!AppSettings.HasConfiguredServer())
        {
            var settingsWindow = new ConnectionSettingsWindow(_api);
            settingsWindow.ShowDialog();
        }

        // Sync API client with loaded/configured URL
        _api.BaseUrl = AppSettings.ApiBaseUrl;
        ServerUrlBox.Text = AppSettings.ApiBaseUrl;
        UsernameBox.Focus();
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            Login_Click(sender, e);
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        string username = "";
        string password = "";
        string serverUrl = "";
        
        try
        {
            username = UsernameBox?.Text?.Trim() ?? "";
            password = PasswordBox?.Password ?? "";
            serverUrl = ServerUrlBox?.Text?.Trim() ?? "";
        }
        catch (Exception ex)
        {
            ShowError(ErrorMessageHelper.GetArabicMessage(ex));
            return;
        }

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("يرجى إدخال اسم المستخدم وكلمة المرور");
            return;
        }

        if (string.IsNullOrEmpty(serverUrl))
        {
            ShowError("يرجى إدخال عنوان السيرفر");
            return;
        }

        // Update API base URL and save
        try
        {
            AppSettings.ApiBaseUrl = serverUrl;
            AppSettings.Save();
            _api.BaseUrl = serverUrl;
        }
        catch (Exception ex)
        {
            ShowError(ErrorMessageHelper.GetArabicMessage(ex));
            return;
        }

        // Show loading
        LoginBtn.IsEnabled = false;
        LoadingText.Visibility = Visibility.Visible;
        ErrorText.Visibility = Visibility.Collapsed;

        try
        {
            var success = await _api.LoginAsync(username, password);
            if (success)
            {
                var mainWindow = new MainWindow(_api);
                mainWindow.Show();
                Close();
            }
            else
            {
                ShowError("اسم المستخدم أو كلمة المرور غير صحيحة");
            }
        }
        catch (Exception ex)
        {
            ErrorMessageHelper.LogError(ex);
            ShowError(ErrorMessageHelper.GetArabicMessage(ex));
        }
        finally
        {
            LoginBtn.IsEnabled = true;
            LoadingText.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}

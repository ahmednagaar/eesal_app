using System;
using System.Windows;
using System.Windows.Input;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class ChangePasswordWindow : Window
{
    private readonly ApiClient _api;

    public ChangePasswordWindow(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }

    private async void Change_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = "";

        string current = CurrentPwd.Password;
        string newPwd = NewPwd.Password;
        string confirm = ConfirmPwd.Password;

        if (string.IsNullOrWhiteSpace(current))
        {
            ErrorText.Text = "أدخل كلمة المرور الحالية";
            return;
        }
        if (newPwd.Length < 8)
        {
            ErrorText.Text = "كلمة المرور الجديدة يجب أن تكون 8 أحرف على الأقل";
            return;
        }
        if (newPwd != confirm)
        {
            ErrorText.Text = "كلمة المرور الجديدة وتأكيدها غير متطابقتين";
            return;
        }

        ChangeBtn.IsEnabled = false;
        try
        {
            bool ok = await _api.ChangePasswordAsync(current, newPwd, confirm);
            if (ok)
            {
                MessageBox.Show("✓ تم تغيير كلمة المرور بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                ErrorText.Text = "فشل تغيير كلمة المرور";
            }
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;
        }
        finally
        {
            ChangeBtn.IsEnabled = true;
        }
    }
}

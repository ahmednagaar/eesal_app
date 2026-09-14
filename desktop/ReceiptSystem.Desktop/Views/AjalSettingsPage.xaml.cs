using System;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AjalSettingsPage : UserControl
{
    private readonly ApiClient _api;

    public AjalSettingsPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        Loaded += async (_, _) => await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            var json = await _api.GetAjalPrefixSettingsJsonAsync();
            if (json == null) return;

            var data = JObject.Parse(json);
            CurrentPrefixText.Text = data["prefix"]?.ToString() ?? "—";
            ThresholdText.Text = (data["warningThreshold"]?.Value<int>() ?? 0).ToString();
            TotalDigitsText.Text = (data["totalDigits"]?.Value<int>() ?? 0).ToString();
            NextPrefixText.Text = data["nextPrefix"]?.ToString() ?? "—";
            NewPrefixBox.Text = "";
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex)); }
        finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
    }

    private async void UpdatePrefix_Click(object sender, RoutedEventArgs e)
    {
        string newPrefix = NewPrefixBox.Text.Trim();
        if (string.IsNullOrEmpty(newPrefix))
        {
            ToastHelper.ShowError(RootGrid, "أدخل البادئة الجديدة");
            return;
        }

        var confirm = MessageBox.Show(
            $"هل تريد تغيير البادئة من \"{CurrentPrefixText.Text}\" إلى \"{newPrefix}\"؟",
            "تأكيد تغيير البادئة",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            bool ok = await _api.UpdateAjalPrefixAsync(newPrefix);
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, $"✓ تم تحديث البادئة إلى: {newPrefix}");
                await LoadSettingsAsync();
            }
            else
            {
                ToastHelper.ShowError(RootGrid, "فشل تحديث البادئة");
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
    }
}

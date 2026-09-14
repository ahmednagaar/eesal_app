using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AjalMerchantHistoryPage : UserControl
{
    private readonly ApiClient _api;

    public AjalMerchantHistoryPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    private void MerchantSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) SearchMerchant_Click(sender, e);
    }

    private async void SearchMerchant_Click(object sender, RoutedEventArgs e)
    {
        string query = MerchantSearchBox.Text.Trim();
        if (query.Length < 2) { ToastHelper.ShowError(RootGrid, "اكتب حرفين على الأقل"); return; }

        LoadingOverlay.Visibility = Visibility.Visible;
        MerchantInfoPanel.Visibility = Visibility.Collapsed;
        InvoicesPanel.Visibility = Visibility.Collapsed;
        MerchantPickerPanel.Visibility = Visibility.Collapsed;

        try
        {
            var results = await _api.SearchMerchantsForRouteAsync(query);
            if (results.Count == 0)
            {
                ToastHelper.ShowError(RootGrid, "لم يتم العثور على تاجر بهذا الاسم");
                return;
            }

            if (results.Count == 1)
            {
                await LoadMerchantHistory(results[0].MerchantId);
            }
            else
            {
                // Show picker
                MerchantPickerList.Items.Clear();
                foreach (var m in results)
                {
                    MerchantPickerList.Items.Add(new ListBoxItem
                    {
                        Content = m.MerchantName,
                        Tag = m.MerchantId
                    });
                }
                MerchantPickerPanel.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex)); }
        finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
    }

    private async void MerchantPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MerchantPickerList.SelectedItem is ListBoxItem item && item.Tag is int merchantId)
        {
            MerchantPickerPanel.Visibility = Visibility.Collapsed;
            LoadingOverlay.Visibility = Visibility.Visible;
            try { await LoadMerchantHistory(merchantId); }
            catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
            finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
        }
    }

    private async Task LoadMerchantHistory(int merchantId)
    {
        var json = await _api.GetAjalMerchantHistoryJsonAsync(merchantId);
        if (json == null) { ToastHelper.ShowError(RootGrid, "لا توجد بيانات"); return; }

        var data = JObject.Parse(json);

        // Merchant info
        MerchantNameText.Text = data["merchantName"]?.ToString() ?? "";
        string phone = data["phone"]?.ToString() ?? "";
        string city = data["city"]?.ToString() ?? "";
        MerchantDetailsText.Text = $"{city} — {phone}".Trim(' ', '—');

        decimal activeTotal = data["activeTotal"]?.Value<decimal>() ?? 0;
        int totalInvoices = data["totalInvoices"]?.Value<int>() ?? 0;
        ActiveTotalText.Text = $"{activeTotal:N2} جنيه";
        InvoiceCountText.Text = $"{totalInvoices} فاتورة";

        MerchantInfoPanel.Visibility = Visibility.Visible;

        // Invoices
        var invoices = data["invoices"] as JArray;
        if (invoices != null)
        {
            HistoryGrid.ItemsSource = invoices.Select(inv => new
            {
                date = inv["sessionDate"]?.Value<DateTime>().ToString("dd/MM/yyyy") ?? "",
                invoiceNumber = inv["invoiceNumber"]?.ToString() ?? "",
                amount = (inv["amount"]?.Value<decimal>() ?? 0).ToString("N2"),
                routeName = inv["routeName"]?.ToString() ?? "—",
                employeeName = inv["callCenterEmployeeName"]?.ToString() ?? "—",
                status = inv["invoiceStatus"]?.ToString() == "Cancelled" ? "ملغاة" :
                         inv["invoiceStatus"]?.ToString() == "Modified" ? "معدّلة" : "نشطة"
            }).ToList();
        }

        InvoicesPanel.Visibility = Visibility.Visible;
    }
}

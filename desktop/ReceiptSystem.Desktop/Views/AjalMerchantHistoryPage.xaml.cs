using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AjalMerchantHistoryPage : UserControl
{
    private readonly ApiClient _api;

    private ICollectionView? _merchantsView;

    public AjalMerchantHistoryPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        Loaded += async (_, _) => await LoadMerchantsAsync();
    }

    private async Task LoadMerchantsAsync()
    {
        try
        {
            var json = await _api.GetMerchantsJsonAsync(1, 10000);
            if (json == null) return;
            var obj = JObject.Parse(json);
            var merchants = obj["data"] as JArray;
            if (merchants == null) return;

            var list = merchants.Select(m => new MerchantComboItem
            {
                merchantId = m["merchantId"]?.Value<int>() ?? 0,
                merchantName = m["merchantName"]?.ToString() ?? ""
            }).ToList();
            
            _merchantsView = CollectionViewSource.GetDefaultView(list);
            _merchantsView.Filter = (obj) =>
            {
                if (string.IsNullOrWhiteSpace(MerchantComboBox.Text)) return true;
                return ((MerchantComboItem)obj).merchantName.Contains(MerchantComboBox.Text, StringComparison.OrdinalIgnoreCase);
            };
            
            MerchantComboBox.ItemsSource = _merchantsView;
        }
        catch { }
    }

    private void MerchantComboBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Escape) return;
        _merchantsView?.Refresh();
        MerchantComboBox.IsDropDownOpen = true;
    }

    private async void MerchantComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MerchantComboBox.SelectedItem is MerchantComboItem item)
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            try { await LoadMerchantHistory(item.merchantId); }
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

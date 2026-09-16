using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;



public partial class ReceiptSearchPage : UserControl
{
    private readonly ApiClient _api;
    private int _currentPage = 1;
    private const int PageSize = 50;

    private ICollectionView? _driversView;
    private ICollectionView? _merchantsView;

    public ReceiptSearchPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            await LoadDriversAsync();
            await LoadMerchantsAsync();
        };
    }

    private async Task LoadDriversAsync()
    {
        try
        {
            var json = await _api.GetDriversJsonAsync();
            if (json == null) return;
            var drivers = JArray.Parse(json);
            var list = drivers.Select(d => new DriverComboItem
            {
                driverId = d["driverId"]?.Value<int>() ?? 0,
                fullName = d["fullName"]?.ToString() ?? ""
            }).ToList();
            list.Insert(0, new DriverComboItem { driverId = 0, fullName = "— الكل —" });
            
            _driversView = CollectionViewSource.GetDefaultView(list);
            _driversView.Filter = (obj) =>
            {
                if (string.IsNullOrWhiteSpace(FilterDriver.Text)) return true;
                return ((DriverComboItem)obj).fullName.Contains(FilterDriver.Text, StringComparison.OrdinalIgnoreCase);
            };
            
            FilterDriver.ItemsSource = _driversView;
            FilterDriver.SelectedIndex = 0;
        }
        catch { }
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
                if (string.IsNullOrWhiteSpace(FilterMerchant.Text)) return true;
                return ((MerchantComboItem)obj).merchantName.Contains(FilterMerchant.Text, StringComparison.OrdinalIgnoreCase);
            };
            
            FilterMerchant.ItemsSource = _merchantsView;
        }
        catch { }
    }

    private void FilterMerchant_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Escape) return;
        _merchantsView?.Refresh();
        FilterMerchant.IsDropDownOpen = true;
    }

    private void FilterDriver_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Escape) return;
        _driversView?.Refresh();
        FilterDriver.IsDropDownOpen = true;
    }

    private async Task SearchAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        EmptyState.Visibility = Visibility.Collapsed;
        try
        {
            string? merchantName = string.IsNullOrWhiteSpace(FilterMerchant.Text) ? null : FilterMerchant.Text.Trim();
            int? driverId = FilterDriver.SelectedValue is int did && did > 0 ? did : null;
            string? dateFrom = FilterDateFrom.SelectedDate?.ToString("yyyy-MM-dd");
            string? dateTo = FilterDateTo.SelectedDate?.ToString("yyyy-MM-dd");
            int? receiptNum = int.TryParse(FilterReceiptNum.Text, out int rn) ? rn : null;
            decimal? amountMin = decimal.TryParse(FilterAmountMin.Text, out decimal amin) ? amin : null;
            decimal? amountMax = decimal.TryParse(FilterAmountMax.Text, out decimal amax) ? amax : null;

            var json = await _api.SearchReceiptsJsonAsync(
                merchantName, driverId, dateFrom, dateTo, receiptNum, amountMin, amountMax,
                page: _currentPage, pageSize: PageSize);

            if (json != null)
            {
                var result = JObject.Parse(json);
                var items = result["items"] as JArray ?? result["receipts"] as JArray;
                var total = result["totalCount"]?.Value<int>() ?? result["total"]?.Value<int>() ?? 0;
                var totalAmt = result["totalAmount"]?.Value<decimal>() ?? 0;

                if (items != null && items.Count > 0)
                {
                    var rows = items.Select(r => new
                    {
                        receiptNumber = r["receiptNumber"]?.Value<int>() ?? 0,
                        merchantName = r["merchantName"]?.ToString() ?? "",
                        driverName = r["driverName"]?.ToString() ?? "",
                        amount = (r["amount"]?.Value<decimal>() ?? 0).ToString("N2"),
                        date = r["sessionDate"]?.ToString()?.Substring(0, 10) ?? "",
                        source = r["importSource"]?.ToString() ?? "يدوي",
                        sessionId = r["sessionId"]?.Value<int>() ?? 0
                    }).ToList();
                    ResultsGrid.ItemsSource = rows;

                    TotalCount.Text = total.ToString();
                    TotalAmount.Text = totalAmt.ToString("N2");

                    int totalPages = (int)Math.Ceiling((double)total / PageSize);
                    PageInfo.Text = $"صفحة {_currentPage} من {totalPages}";
                    PrevBtn.IsEnabled = _currentPage > 1;
                    NextBtn.IsEnabled = _currentPage < totalPages;
                    ExportBtn.IsEnabled = true;
                }
                else
                {
                    ResultsGrid.ItemsSource = null;
                    EmptyState.Text = "لم يتم العثور على نتائج";
                    EmptyState.Visibility = Visibility.Visible;
                    TotalCount.Text = "0"; TotalAmount.Text = "0.00";
                    ExportBtn.IsEnabled = false;
                }
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex));
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        _currentPage = 1;
        await SearchAsync();
    }

    private void ClearFilters_Click(object sender, RoutedEventArgs e)
    {
        FilterMerchant.Text = "";
        _merchantsView?.Refresh();
        
        if (FilterDriver.Items.Count > 0) FilterDriver.SelectedIndex = 0;
        _driversView?.Refresh();
        
        FilterDateFrom.SelectedDate = null;
        FilterDateTo.SelectedDate = null;
        FilterReceiptNum.Text = "";
        FilterAmountMin.Text = "";
        FilterAmountMax.Text = "";
        ResultsGrid.ItemsSource = null;
        EmptyState.Text = "ابدأ بالبحث أعلاه";
        EmptyState.Visibility = Visibility.Visible;
        TotalCount.Text = "0"; TotalAmount.Text = "0.00";
    }

    private async void Prev_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1) { _currentPage--; await SearchAsync(); }
    }
    private async void Next_Click(object sender, RoutedEventArgs e)
    {
        _currentPage++; await SearchAsync();
    }

    // ═══ EXPORT ═══
    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            FileName = $"receipts_export_{DateTime.Now:yyyyMMdd}.xlsx",
            Title = "حفظ تصدير الإيصالات"
        };
        if (dlg.ShowDialog() != true) return;

        ExportBtn.IsEnabled = false;
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            string? merchantName = string.IsNullOrWhiteSpace(FilterMerchant.Text) ? null : FilterMerchant.Text.Trim();
            int? driverId = FilterDriver.SelectedValue is int did && did > 0 ? did : null;
            string? dateFrom = FilterDateFrom.SelectedDate?.ToString("yyyy-MM-dd");
            string? dateTo = FilterDateTo.SelectedDate?.ToString("yyyy-MM-dd");
            int? receiptNum = int.TryParse(FilterReceiptNum.Text, out int rn) ? rn : null;
            decimal? amountMin = decimal.TryParse(FilterAmountMin.Text, out decimal amin) ? amin : null;
            decimal? amountMax = decimal.TryParse(FilterAmountMax.Text, out decimal amax) ? amax : null;

            var bytes = await _api.ExportReceiptsAsync(merchantName, driverId, dateFrom, dateTo, receiptNum, amountMin, amountMax);
            if (bytes != null)
            {
                await File.WriteAllBytesAsync(dlg.FileName, bytes);
                ToastHelper.ShowSuccess(RootGrid, $"✓ تم التصدير: {Path.GetFileName(dlg.FileName)}");
            }
            else
            {
                ToastHelper.ShowError(RootGrid, "فشل التصدير — لا توجد بيانات");
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, ex.Message);
        }
        finally
        {
            ExportBtn.IsEnabled = true;
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }
}

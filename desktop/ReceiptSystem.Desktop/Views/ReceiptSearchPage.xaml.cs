using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

    // Full lists for search
    private List<MerchantComboItem> _allMerchants = new();
    private List<DriverComboItem> _allDrivers = new();

    // Selected items
    private MerchantComboItem? _selectedMerchant;
    private DriverComboItem? _selectedDriver;

    // Prevent recursive TextChanged events during programmatic text updates
    private bool _suppressMerchantSearch;
    private bool _suppressDriverSearch;

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

    // ══════════════════════════════════════
    // LOAD DATA
    // ══════════════════════════════════════

    private async Task LoadDriversAsync()
    {
        try
        {
            var json = await _api.GetDriversJsonAsync();
            if (json == null) return;
            var drivers = JArray.Parse(json);
            _allDrivers = drivers.Select(d => new DriverComboItem
            {
                driverId = d["driverId"]?.Value<int>() ?? 0,
                fullName = d["fullName"]?.ToString() ?? ""
            }).ToList();
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

            _allMerchants = merchants.Select(m => new MerchantComboItem
            {
                merchantId = m["merchantId"]?.Value<int>() ?? 0,
                merchantName = m["merchantName"]?.ToString() ?? ""
            }).ToList();
        }
        catch { }
    }

    // ══════════════════════════════════════
    // MERCHANT SEARCH
    // ══════════════════════════════════════

    private void MerchantSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressMerchantSearch) return;

        var query = MerchantSearchBox.Text.Trim();

        // If text was cleared, deselect
        if (string.IsNullOrEmpty(query))
        {
            _selectedMerchant = null;
            ClearMerchantBtn.Visibility = Visibility.Collapsed;
            MerchantPopup.IsOpen = false;
            return;
        }

        // If text matches selected, don't re-search
        if (_selectedMerchant != null && query == _selectedMerchant.merchantName)
            return;

        // Clear selection since user is typing something new
        _selectedMerchant = null;

        // Filter merchants
        var filtered = _allMerchants
            .Where(m => m.merchantName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .ToList();

        if (filtered.Count > 0)
        {
            MerchantResults.ItemsSource = filtered;
            MerchantPopup.IsOpen = true;
        }
        else
        {
            MerchantPopup.IsOpen = false;
        }
    }

    private void MerchantSearch_GotFocus(object sender, RoutedEventArgs e)
    {
        // Show all if empty, or show current filtered results
        if (string.IsNullOrWhiteSpace(MerchantSearchBox.Text) && _allMerchants.Count > 0)
        {
            MerchantResults.ItemsSource = _allMerchants.Take(50).ToList();
            MerchantPopup.IsOpen = true;
        }
    }

    private void MerchantResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MerchantResults.SelectedItem is MerchantComboItem item)
        {
            _selectedMerchant = item;
            _suppressMerchantSearch = true;
            MerchantSearchBox.Text = item.merchantName;
            _suppressMerchantSearch = false;
            ClearMerchantBtn.Visibility = Visibility.Visible;
            MerchantPopup.IsOpen = false;
            MerchantResults.SelectedItem = null; // Reset for next time
        }
    }

    private void ClearMerchant_Click(object sender, RoutedEventArgs e)
    {
        _selectedMerchant = null;
        _suppressMerchantSearch = true;
        MerchantSearchBox.Text = "";
        _suppressMerchantSearch = false;
        ClearMerchantBtn.Visibility = Visibility.Collapsed;
        MerchantSearchBox.Focus();
    }

    // ══════════════════════════════════════
    // DRIVER SEARCH
    // ══════════════════════════════════════

    private void DriverSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressDriverSearch) return;

        var query = DriverSearchBox.Text.Trim();

        if (string.IsNullOrEmpty(query))
        {
            _selectedDriver = null;
            ClearDriverBtn.Visibility = Visibility.Collapsed;
            DriverPopup.IsOpen = false;
            return;
        }

        if (_selectedDriver != null && query == _selectedDriver.fullName)
            return;

        _selectedDriver = null;

        var filtered = _allDrivers
            .Where(d => d.fullName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .ToList();

        if (filtered.Count > 0)
        {
            DriverResults.ItemsSource = filtered;
            DriverPopup.IsOpen = true;
        }
        else
        {
            DriverPopup.IsOpen = false;
        }
    }

    private void DriverSearch_GotFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DriverSearchBox.Text) && _allDrivers.Count > 0)
        {
            DriverResults.ItemsSource = _allDrivers.Take(50).ToList();
            DriverPopup.IsOpen = true;
        }
    }

    private void DriverResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DriverResults.SelectedItem is DriverComboItem item)
        {
            _selectedDriver = item;
            _suppressDriverSearch = true;
            DriverSearchBox.Text = item.fullName;
            _suppressDriverSearch = false;
            ClearDriverBtn.Visibility = Visibility.Visible;
            DriverPopup.IsOpen = false;
            DriverResults.SelectedItem = null;
        }
    }

    private void ClearDriver_Click(object sender, RoutedEventArgs e)
    {
        _selectedDriver = null;
        _suppressDriverSearch = true;
        DriverSearchBox.Text = "";
        _suppressDriverSearch = false;
        ClearDriverBtn.Visibility = Visibility.Collapsed;
        DriverSearchBox.Focus();
    }

    // ══════════════════════════════════════
    // SEARCH
    // ══════════════════════════════════════

    private async Task SearchAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        EmptyState.Visibility = Visibility.Collapsed;
        try
        {
            // Use selected item ID if available, otherwise use text for merchant name search
            string? merchantName = _selectedMerchant != null
                ? _selectedMerchant.merchantName
                : (string.IsNullOrWhiteSpace(MerchantSearchBox.Text) ? null : MerchantSearchBox.Text.Trim());
            int? driverId = _selectedDriver?.driverId > 0 ? _selectedDriver.driverId : null;
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
                var items = result["results"] as JArray ?? result["items"] as JArray ?? result["receipts"] as JArray;
                var total = result["totalCount"]?.Value<int>() ?? result["total"]?.Value<int>() ?? 0;
                var totalAmt = result["totals"]?["totalAmount"]?.Value<decimal>() ?? result["totalAmount"]?.Value<decimal>() ?? 0;

                if (items != null && items.Count > 0)
                {
                    var rows = items.Select(r => new
                    {
                        receiptNumber = r["receiptNumber"]?.Value<int>() ?? 0,
                        merchantName = r["merchantName"]?.ToString() ?? "",
                        driverName = r["driverName"]?.ToString() ?? "",
                        amount = (r["amount"]?.Value<decimal>() ?? 0).ToString("N2"),
                        date = (r["collectionDate"]?.ToString() ?? r["sessionDate"]?.ToString() ?? "").Length >= 10
                            ? (r["collectionDate"]?.ToString() ?? r["sessionDate"]?.ToString() ?? "").Substring(0, 10)
                            : "",
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
        // Clear merchant
        _selectedMerchant = null;
        _suppressMerchantSearch = true;
        MerchantSearchBox.Text = "";
        _suppressMerchantSearch = false;
        ClearMerchantBtn.Visibility = Visibility.Collapsed;

        // Clear driver
        _selectedDriver = null;
        _suppressDriverSearch = true;
        DriverSearchBox.Text = "";
        _suppressDriverSearch = false;
        ClearDriverBtn.Visibility = Visibility.Collapsed;

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
            string? merchantName = _selectedMerchant?.merchantName
                ?? (string.IsNullOrWhiteSpace(MerchantSearchBox.Text) ? null : MerchantSearchBox.Text.Trim());
            int? driverId = _selectedDriver?.driverId > 0 ? _selectedDriver.driverId : null;
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

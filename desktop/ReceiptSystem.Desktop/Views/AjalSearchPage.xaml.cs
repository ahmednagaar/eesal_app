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

public partial class AjalSearchPage : UserControl
{
    private readonly ApiClient _api;
    private int _currentPage = 1;
    private int _totalCount = 0;
    private const int PAGE_SIZE = 50;

    private List<MerchantComboItem> _allMerchants = new();
    private MerchantComboItem? _selectedMerchant;
    private bool _suppressMerchantSearch;

    public AjalSearchPage(ApiClient api)
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

        if (string.IsNullOrEmpty(query))
        {
            _selectedMerchant = null;
            ClearMerchantBtn.Visibility = Visibility.Collapsed;
            MerchantPopup.IsOpen = false;
            return;
        }

        if (_selectedMerchant != null && query == _selectedMerchant.merchantName)
            return;

        _selectedMerchant = null;

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
            MerchantResults.SelectedItem = null;
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
    // SEARCH
    // ══════════════════════════════════════

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        _currentPage = 1;
        await SearchAsync();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _selectedMerchant = null;
        _suppressMerchantSearch = true;
        MerchantSearchBox.Text = "";
        _suppressMerchantSearch = false;
        ClearMerchantBtn.Visibility = Visibility.Collapsed;

        FilterInvoiceNum.Text = "";
        FilterEmployee.Text = ""; FilterStatus.SelectedIndex = 0;
        FilterDateFrom.SelectedDate = null; FilterDateTo.SelectedDate = null;
        ResultsGrid.ItemsSource = null; ResultSummary.Text = "";
        ExportBtn.IsEnabled = false;
    }

    private async Task SearchAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            string? reviewStatus = null;
            if (FilterStatus.SelectedItem is ComboBoxItem si && si.Tag is string st) reviewStatus = st;

            string? merchantName = _selectedMerchant?.merchantName
                ?? (string.IsNullOrWhiteSpace(MerchantSearchBox.Text) ? null : MerchantSearchBox.Text.Trim());

            var json = await _api.SearchAjalJsonAsync(
                merchantName: merchantName,
                invoiceNumber: string.IsNullOrWhiteSpace(FilterInvoiceNum.Text) ? null : FilterInvoiceNum.Text.Trim(),
                reviewStatus: reviewStatus,
                dateFrom: FilterDateFrom.SelectedDate?.ToString("yyyy-MM-dd"),
                dateTo: FilterDateTo.SelectedDate?.ToString("yyyy-MM-dd"),
                page: _currentPage,
                pageSize: PAGE_SIZE
            );

            if (json == null) { ResultsGrid.ItemsSource = null; return; }

            var data = JObject.Parse(json);
            _totalCount = data["totalCount"]?.Value<int>() ?? 0;
            decimal totalAmt = data["totalAmount"]?.Value<decimal>() ?? 0;

            ResultSummary.Text = $"النتائج: {_totalCount} فاتورة — الإجمالي: {totalAmt:N2} جنيه";

            var results = data["results"] as JArray;
            if (results != null)
            {
                ResultsGrid.ItemsSource = results.Select(inv => new
                {
                    invoiceNumber = inv["invoiceNumber"]?.ToString() ?? "",
                    merchantName = inv["merchantName"]?.ToString() ?? "",
                    amount = (inv["amount"]?.Value<decimal>() ?? 0).ToString("N2"),
                    employeeName = inv["callCenterEmployeeName"]?.ToString() ?? "—",
                    status = inv["invoiceStatus"]?.ToString() == "Cancelled" ? "ملغاة" :
                             inv["invoiceStatus"]?.ToString() == "Modified" ? "معدّلة" : "نشطة",
                    date = inv["enteredAt"]?.Value<DateTime>().ToString("dd/MM/yyyy") ?? "",
                    source = inv["importSource"]?.ToString() == "Excel" ? "إكسل" : "يدوي"
                }).ToList();
            }

            ExportBtn.IsEnabled = _totalCount > 0;
            UpdatePagination();
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex)); }
        finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
    }

    private void UpdatePagination()
    {
        int totalPages = Math.Max(1, (int)Math.Ceiling(_totalCount / (double)PAGE_SIZE));
        PageInfo.Text = $"صفحة {_currentPage} من {totalPages}";
        PrevBtn.IsEnabled = _currentPage > 1;
        NextBtn.IsEnabled = _currentPage < totalPages;
    }

    private async void Prev_Click(object sender, RoutedEventArgs e) { _currentPage--; await SearchAsync(); }
    private async void Next_Click(object sender, RoutedEventArgs e) { _currentPage++; await SearchAsync(); }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string? reviewStatus = null;
            if (FilterStatus.SelectedItem is ComboBoxItem si2 && si2.Tag is string st2) reviewStatus = st2;

            string? merchantName = _selectedMerchant?.merchantName
                ?? (string.IsNullOrWhiteSpace(MerchantSearchBox.Text) ? null : MerchantSearchBox.Text.Trim());

            var bytes = await _api.ExportAjalSearchAsync(
                merchantName: merchantName,
                invoiceNumber: string.IsNullOrWhiteSpace(FilterInvoiceNum.Text) ? null : FilterInvoiceNum.Text.Trim(),
                reviewStatus: reviewStatus,
                dateFrom: FilterDateFrom.SelectedDate?.ToString("yyyy-MM-dd"),
                dateTo: FilterDateTo.SelectedDate?.ToString("yyyy-MM-dd")
            );

            if (bytes == null || bytes.Length == 0) { ToastHelper.ShowError(RootGrid, "فشل التصدير"); return; }

            var dlg = new SaveFileDialog { FileName = $"ajal_search_{DateTime.Now:yyyyMMdd}.xlsx", Filter = "Excel Files|*.xlsx" };
            if (dlg.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(dlg.FileName, bytes);
                ToastHelper.ShowSuccess(RootGrid, $"✓ تم التصدير: {Path.GetFileName(dlg.FileName)}");
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
    }
}

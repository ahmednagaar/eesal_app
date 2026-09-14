using System;
using System.IO;
using System.Linq;
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

    public AjalSearchPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        _currentPage = 1;
        await SearchAsync();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        FilterMerchant.Text = ""; FilterInvoiceNum.Text = "";
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
            string? status = null;
            if (FilterStatus.SelectedItem is ComboBoxItem si && si.Tag is string st) status = st;

            var json = await _api.SearchAjalJsonAsync(
                merchantName: string.IsNullOrWhiteSpace(FilterMerchant.Text) ? null : FilterMerchant.Text.Trim(),
                invoiceNumber: string.IsNullOrWhiteSpace(FilterInvoiceNum.Text) ? null : FilterInvoiceNum.Text.Trim(),
                employeeName: string.IsNullOrWhiteSpace(FilterEmployee.Text) ? null : FilterEmployee.Text.Trim(),
                status: status,
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
            string? status = null;
            if (FilterStatus.SelectedItem is ComboBoxItem si && si.Tag is string st) status = st;

            var bytes = await _api.ExportAjalSearchAsync(
                merchantName: string.IsNullOrWhiteSpace(FilterMerchant.Text) ? null : FilterMerchant.Text.Trim(),
                invoiceNumber: string.IsNullOrWhiteSpace(FilterInvoiceNum.Text) ? null : FilterInvoiceNum.Text.Trim(),
                employeeName: string.IsNullOrWhiteSpace(FilterEmployee.Text) ? null : FilterEmployee.Text.Trim(),
                status: status,
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

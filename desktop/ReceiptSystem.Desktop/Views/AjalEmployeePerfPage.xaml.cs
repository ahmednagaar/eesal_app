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

public partial class AjalEmployeePerfPage : UserControl
{
    private readonly ApiClient _api;

    public AjalEmployeePerfPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        DateFrom.SelectedDate = DateTime.Today;
        DateTo.SelectedDate = DateTime.Today;
    }

    private void Today_Click(object sender, RoutedEventArgs e)
    {
        DateFrom.SelectedDate = DateTime.Today;
        DateTo.SelectedDate = DateTime.Today;
        _ = LoadAsync();
    }

    private void Week_Click(object sender, RoutedEventArgs e)
    {
        var today = DateTime.Today;
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Saturday)) % 7;
        DateFrom.SelectedDate = today.AddDays(-diff);
        DateTo.SelectedDate = today;
        _ = LoadAsync();
    }

    private void Month_Click(object sender, RoutedEventArgs e)
    {
        DateFrom.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        DateTo.SelectedDate = DateTime.Today;
        _ = LoadAsync();
    }

    private async void Load_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        if (DateFrom.SelectedDate == null || DateTo.SelectedDate == null) return;
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            var json = await _api.GetAjalEmployeePerformanceJsonAsync(DateFrom.SelectedDate.Value, DateTo.SelectedDate.Value);
            if (json == null) { EmployeeGrid.ItemsSource = null; return; }

            var data = JObject.Parse(json);

            GrandInvoices.Text = (data["grandTotalInvoices"]?.Value<int>() ?? 0).ToString();
            GrandAmount.Text = (data["grandTotalAmount"]?.Value<decimal>() ?? 0).ToString("N2");

            var employees = data["employees"] as JArray;
            if (employees != null)
            {
                EmployeeCount.Text = employees.Count.ToString();
                int rank = 0;
                EmployeeGrid.ItemsSource = employees
                    .OrderByDescending(e => e["totalAmount"]?.Value<decimal>() ?? 0)
                    .Select(emp => new
                    {
                        rank = ++rank,
                        name = emp["employeeName"]?.ToString() ?? "—",
                        count = emp["invoiceCount"]?.Value<int>() ?? 0,
                        total = (emp["totalAmount"]?.Value<decimal>() ?? 0).ToString("N2"),
                        avg = (emp["averageInvoice"]?.Value<decimal>() ?? 0).ToString("N2")
                    }).ToList();
            }

            ExportBtn.IsEnabled = (employees?.Count ?? 0) > 0;
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex)); }
        finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        if (DateFrom.SelectedDate == null || DateTo.SelectedDate == null) return;
        try
        {
            var bytes = await _api.ExportAjalEmployeesAsync(DateFrom.SelectedDate.Value, DateTo.SelectedDate.Value);
            if (bytes == null || bytes.Length == 0) { ToastHelper.ShowError(RootGrid, "فشل التصدير"); return; }

            var dlg = new SaveFileDialog
            {
                FileName = $"ajal_employees_{DateFrom.SelectedDate.Value:yyyyMMdd}_{DateTo.SelectedDate.Value:yyyyMMdd}.xlsx",
                Filter = "Excel Files|*.xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(dlg.FileName, bytes);
                ToastHelper.ShowSuccess(RootGrid, $"✓ تم التصدير: {Path.GetFileName(dlg.FileName)}");
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
    }
}

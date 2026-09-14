using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AjalExcelImportPage : UserControl
{
    private readonly ApiClient _api;
    private JArray? _previewRows;
    private string? _selectedFilePath;

    public AjalExcelImportPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        ImportDate.SelectedDate = DateTime.Today;
        Loaded += async (_, _) => await LoadRoutesAsync();
    }

    private async Task LoadRoutesAsync()
    {
        var routes = await _api.GetRoutesAsync();
        var routeList = new List<object> { new { RouteId = (int?)null, RouteName = "— بدون خط —" } };
        routeList.AddRange(routes.Select(r => (object)new { RouteId = (int?)r.RouteId, RouteName = r.RouteName }));
        ImportRouteCombo.ItemsSource = routeList;
        ImportRouteCombo.SelectedIndex = 0;
    }

    private async void PickFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Excel Files|*.xlsx",
            Title = "اختر ملف Excel لاستيراد الفواتير"
        };
        if (dlg.ShowDialog() != true) return;

        _selectedFilePath = dlg.FileName;
        FileNameText.Text = System.IO.Path.GetFileName(_selectedFilePath);

        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            var result = await _api.PreviewAjalExcelAsync(_selectedFilePath);
            if (result == null)
            {
                ToastHelper.ShowError(RootGrid, "فشل معالجة الملف");
                return;
            }

            bool success = result["success"]?.Value<bool>() ?? result["Success"]?.Value<bool>() ?? false;
            if (!success)
            {
                string error = result["error"]?.ToString() ?? result["Error"]?.ToString() ?? "خطأ غير معروف";
                ToastHelper.ShowError(RootGrid, error);
                return;
            }

            // Summary
            int rowCount = result["rowCount"]?.Value<int>() ?? result["RowCount"]?.Value<int>() ?? 0;
            decimal totalAmount = result["totalAmount"]?.Value<decimal>() ?? result["TotalAmount"]?.Value<decimal>() ?? 0;
            int dupCount = result["duplicateCount"]?.Value<int>() ?? result["DuplicateCount"]?.Value<int>() ?? 0;

            RowCountText.Text = rowCount.ToString();
            TotalAmountText.Text = totalAmount.ToString("N2");
            DuplicateCountText.Text = dupCount.ToString();

            // Preview rows
            _previewRows = (result["rows"] ?? result["Rows"]) as JArray;
            if (_previewRows != null)
            {
                PreviewGrid.ItemsSource = _previewRows.Select(r => new
                {
                    rowIndex = r["rowIndex"]?.Value<int>() ?? 0,
                    invoiceNumber = r["invoiceNumber"]?.ToString() ?? "",
                    merchantRaw = r["merchantNameRaw"]?.ToString() ?? "",
                    matchedMerchant = r["matchedMerchantName"]?.ToString() ?? (r["isNewMerchant"]?.Value<bool>() == true ? "⭐ جديد" : "—"),
                    amount = (r["amount"]?.Value<decimal>() ?? 0).ToString("N2"),
                    statusText = r["isDuplicate"]?.Value<bool>() == true ? "🔴 مكرر" :
                                 r["isNewMerchant"]?.Value<bool>() == true ? "🟡 تاجر جديد" : "✅ مطابق"
                }).ToList();
            }

            // Warnings
            var warnings = (result["warnings"] ?? result["Warnings"]) as JArray;
            if (warnings != null && warnings.Count > 0)
                WarningsText.Text = "⚠️ " + string.Join(" | ", warnings.Select(w => w.ToString()));
            else
                WarningsText.Text = "";

            PreviewSummary.Visibility = Visibility.Visible;
            PreviewPanel.Visibility = Visibility.Visible;
            SaveBar.Visibility = Visibility.Visible;
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex)); }
        finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_previewRows == null || _previewRows.Count == 0)
        {
            ToastHelper.ShowError(RootGrid, "لا توجد بيانات للحفظ");
            return;
        }
        if (ImportDate.SelectedDate == null)
        {
            ToastHelper.ShowError(RootGrid, "اختر التاريخ");
            return;
        }

        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            int? routeId = null;
            if (ImportRouteCombo.SelectedItem != null)
            {
                dynamic sel = ImportRouteCombo.SelectedItem;
                routeId = sel.RouteId;
            }

            var rows = _previewRows.Where(r => r["isDuplicate"]?.Value<bool>() != true).Select(r => new
            {
                InvoiceNumber = r["invoiceNumber"]?.ToString() ?? "",
                MerchantId = r["matchedMerchantId"]?.Value<int?>(),
                IsNewMerchant = r["isNewMerchant"]?.Value<bool>() ?? false,
                NewMerchantName = r["merchantNameRaw"]?.ToString(),
                Amount = r["amount"]?.Value<decimal>() ?? 0,
                CallCenterEmployeeName = r["callCenterEmployeeName"]?.ToString()
            }).ToList();

            var result = await _api.SaveAjalExcelAsync(ImportDate.SelectedDate.Value, routeId, rows);
            if (result == null)
            {
                ToastHelper.ShowError(RootGrid, "فشل الحفظ");
                return;
            }

            bool success = result["success"]?.Value<bool>() ?? result["Success"]?.Value<bool>() ?? false;
            if (success)
            {
                int saved = result["saved"]?.Value<int>() ?? result["Saved"]?.Value<int>() ?? 0;
                int skipped = result["skipped"]?.Value<int>() ?? result["Skipped"]?.Value<int>() ?? 0;
                int newMerchants = result["newMerchantsCreated"]?.Value<int>() ?? result["NewMerchantsCreated"]?.Value<int>() ?? 0;
                ToastHelper.ShowSuccess(RootGrid, $"✓ تم الحفظ: {saved} فاتورة — تخطي: {skipped} — تجار جدد: {newMerchants}");

                // Reset
                _previewRows = null;
                _selectedFilePath = null;
                FileNameText.Text = "لم يتم اختيار ملف";
                PreviewSummary.Visibility = Visibility.Collapsed;
                PreviewPanel.Visibility = Visibility.Collapsed;
                SaveBar.Visibility = Visibility.Collapsed;
                PreviewGrid.ItemsSource = null;
            }
            else
            {
                ToastHelper.ShowError(RootGrid, "فشل الحفظ");
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex)); }
        finally { LoadingOverlay.Visibility = Visibility.Collapsed; }
    }
}

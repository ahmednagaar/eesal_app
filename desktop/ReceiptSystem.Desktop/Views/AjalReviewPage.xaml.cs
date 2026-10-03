using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AjalReviewPage : UserControl
{
    private readonly ApiClient _api;
    private List<ReviewEntry> _allEntries = new();
    private List<ReviewEntry> _filteredEntries = new();

    public AjalReviewPage(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        dpDate.SelectedDate = DateTime.Today;
    }

    // ═══════════════════════════════════════
    // DATA MODEL
    // ═══════════════════════════════════════
    private class ReviewEntry
    {
        public int EntryId { get; set; }
        public int SessionId { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public string MerchantName { get; set; } = "";
        public string? MerchantCity { get; set; }
        public string RouteName { get; set; } = "";
        public string? DriverName { get; set; }
        public decimal? Amount { get; set; }
        public bool IsReviewed { get; set; }
        public string? Notes { get; set; }
        public string ReviewStatusDisplay => IsReviewed ? "✅" : "⏳";
    }

    // ═══════════════════════════════════════
    // LOAD DATA
    // ═══════════════════════════════════════
    private async void DpDate_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        await LoadData();
    }

    private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        if (dpDate.SelectedDate == null) return;

        try
        {
            var json = await _api.GetAjalDailyJsonAsync(dpDate.SelectedDate.Value);
            if (json == null) return;

            var data = JObject.Parse(json);
            _allEntries.Clear();

            var entries = data["entries"] as JArray;
            if (entries != null)
            {
                foreach (var e in entries)
                {
                    _allEntries.Add(new ReviewEntry
                    {
                        EntryId = (int)e["entryId"]!,
                        SessionId = (int)e["sessionId"]!,
                        InvoiceNumber = e["invoiceNumber"]?.ToString() ?? "",
                        MerchantName = e["merchantName"]?.ToString() ?? "",
                        MerchantCity = e["merchantCity"]?.ToString(),
                        RouteName = e["routeName"]?.ToString() ?? "",
                        DriverName = e["driverName"]?.ToString(),
                        Amount = e["amount"]?.Type == JTokenType.Null ? null : (decimal?)e["amount"],
                        IsReviewed = (bool)(e["isReviewed"] ?? false),
                        Notes = e["notes"]?.ToString()
                    });
                }
            }

            // Populate route filter
            var routes = _allEntries.Select(e => new { e.RouteName }).Distinct().ToList();
            var routeList = new List<dynamic> { new { RouteName = "كل الخطوط", RouteId = 0 } };
            int fakeId = 1;
            foreach (var r in routes)
                routeList.Add(new { r.RouteName, RouteId = fakeId++ });
            cmbRouteFilter.ItemsSource = routeList;
            cmbRouteFilter.SelectedIndex = 0;

            ApplyFilters();
            UpdateStats();
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, $"خطأ في تحميل البيانات: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════
    // FILTERS
    // ═══════════════════════════════════════
    private void CmbFilter_Changed(object sender, SelectionChangedEventArgs e) => ApplyFilters();
    private void TxtSearch_Changed(object sender, TextChangedEventArgs e) => ApplyFilters();

    private void ApplyFilters()
    {
        _filteredEntries = _allEntries.ToList();

        // Status filter
        if (cmbFilter.SelectedItem is ComboBoxItem item)
        {
            string tag = item.Tag?.ToString() ?? "all";
            if (tag == "pending") _filteredEntries = _filteredEntries.Where(e => !e.IsReviewed).ToList();
            else if (tag == "reviewed") _filteredEntries = _filteredEntries.Where(e => e.IsReviewed).ToList();
        }

        // Route filter
        if (cmbRouteFilter.SelectedIndex > 0)
        {
            var selected = cmbRouteFilter.SelectedItem;
            if (selected != null)
            {
                string routeName = ((dynamic)selected).RouteName;
                _filteredEntries = _filteredEntries.Where(e => e.RouteName == routeName).ToList();
            }
        }

        // Invoice number search
        string search = txtSearchInvoice.Text.Trim();
        if (!string.IsNullOrEmpty(search) && search != "بحث برقم الفاتورة...")
            _filteredEntries = _filteredEntries.Where(e => e.InvoiceNumber.Contains(search)).ToList();

        // Always sorted ascending by invoice number
        _filteredEntries = _filteredEntries.OrderBy(e => e.InvoiceNumber).ToList();

        dgEntries.ItemsSource = null;
        dgEntries.ItemsSource = _filteredEntries;
    }

    private void UpdateStats()
    {
        int total = _allEntries.Count;
        int reviewed = _allEntries.Count(e => e.IsReviewed);
        int pending = total - reviewed;
        double pct = total > 0 ? (double)reviewed / total * 100 : 0;

        txtTotal.Text = total.ToString();
        txtReviewed.Text = reviewed.ToString();
        txtPending.Text = pending.ToString();
        pbProgress.Value = pct;
        txtPercent.Text = $"{pct:F0}%";
    }

    // ═══════════════════════════════════════
    // REVIEW ACTIONS
    // ═══════════════════════════════════════
    private async Task ReviewEntryAsync(ReviewEntry entry)
    {
        if (entry.IsReviewed) return;

        bool ok = await _api.ReviewAjalEntryAsync(entry.EntryId);
        if (ok)
        {
            entry.IsReviewed = true;
            // Update the master list too
            var master = _allEntries.FirstOrDefault(e => e.EntryId == entry.EntryId);
            if (master != null) master.IsReviewed = true;

            ApplyFilters();
            UpdateStats();
        }
    }

    private async void BtnReviewSelected_Click(object sender, RoutedEventArgs e)
    {
        var selected = dgEntries.SelectedItems.Cast<ReviewEntry>()
            .Where(en => !en.IsReviewed).ToList();

        if (selected.Count == 0)
        {
            ToastHelper.ShowInfo(RootGrid, "لا توجد فواتير محددة للمراجعة");
            return;
        }

        var ids = selected.Select(en => en.EntryId).ToList();
        var result = await _api.ReviewAjalBatchAsync(ids);
        if (result != null)
        {
            foreach (var en in selected)
            {
                en.IsReviewed = true;
                var master = _allEntries.FirstOrDefault(a => a.EntryId == en.EntryId);
                if (master != null) master.IsReviewed = true;
            }
            ApplyFilters();
            UpdateStats();
            ToastHelper.ShowSuccess(RootGrid, $"تم مراجعة {selected.Count} فاتورة ✅");
        }
    }

    private async void BtnReviewAll_Click(object sender, RoutedEventArgs e)
    {
        var pending = _allEntries.Where(en => !en.IsReviewed).ToList();
        if (pending.Count == 0)
        {
            ToastHelper.ShowInfo(RootGrid, "تمت مراجعة كل الفواتير بالفعل ✅");
            return;
        }

        var confirm = MessageBox.Show(
            $"هل تريد تأكيد مراجعة كل {pending.Count} فاتورة؟",
            "تأكيد المراجعة", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var ids = pending.Select(en => en.EntryId).ToList();
        var result = await _api.ReviewAjalBatchAsync(ids);
        if (result != null)
        {
            foreach (var en in pending) en.IsReviewed = true;
            ApplyFilters();
            UpdateStats();
            ToastHelper.ShowSuccess(RootGrid, $"تم مراجعة {pending.Count} فاتورة ✅");
        }
    }

    // ═══════════════════════════════════════
    // KEYBOARD: Space = review current, Enter = next
    // ═══════════════════════════════════════
    private async void DgEntries_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true;
            if (dgEntries.SelectedItem is ReviewEntry entry)
            {
                await ReviewEntryAsync(entry);
                // Move to next row
                int idx = _filteredEntries.IndexOf(entry);
                if (idx < _filteredEntries.Count - 1)
                {
                    dgEntries.SelectedItem = _filteredEntries[idx + 1];
                    dgEntries.ScrollIntoView(dgEntries.SelectedItem);
                }
            }
        }
    }

    private async void DgEntries_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (dgEntries.SelectedItem is ReviewEntry entry)
            await ReviewEntryAsync(entry);
    }

    // ═══════════════════════════════════════
    // EXPORT
    // ═══════════════════════════════════════
    private async void BtnExport_Click(object sender, RoutedEventArgs e)
    {
        if (dpDate.SelectedDate == null) return;

        var dlg = new SaveFileDialog
        {
            Filter = "Excel|*.xlsx",
            FileName = $"ajal_review_{dpDate.SelectedDate:yyyyMMdd}.xlsx"
        };
        if (dlg.ShowDialog() != true) return;

        try
        {
            var bytes = await _api.ExportAjalDailyAsync(dpDate.SelectedDate.Value);
            if (bytes != null)
            {
                await System.IO.File.WriteAllBytesAsync(dlg.FileName, bytes);
                ToastHelper.ShowSuccess(RootGrid, "تم التصدير بنجاح ✅");
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, $"خطأ في التصدير: {ex.Message}");
        }
    }
}

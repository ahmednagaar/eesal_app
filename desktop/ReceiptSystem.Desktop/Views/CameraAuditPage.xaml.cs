
using System;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class CameraAuditPage : UserControl
{
    private readonly ApiClient _api;
    private bool _isLoaded = false;
    private List<DriverComboItem> _allDrivers = new();
    private DriverComboItem? _selectedFilterDriver;
    private bool _suppressFilterDriverSearch;
    private List<DriverComboItem> _dialogDrivers = new();
    private DriverComboItem? _selectedDialogDriver;
    private bool _suppressDialogDriverSearch;

    public CameraAuditPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDriversAsync();
        await LoadAuditsAsync();
        _isLoaded = true;
    }

    private async Task LoadDriversAsync()
    {
        try
        {
            var json = await _api.GetDriversJsonAsync();
            if (json != null)
            {
                var drivers = JArray.Parse(json);
                var list = drivers.Select(d => new DriverComboItem
                {
                    driverId = d["driverId"]?.Value<int>() ?? 0,
                    fullName = d["fullName"]?.ToString() ?? ""
                }).ToList();
                list.Insert(0, new DriverComboItem { driverId = 0, fullName = "كل السائقين" });
                _allDrivers = list;
            }
        }
        catch { }
    }

    private async Task LoadAuditsAsync()
    {
        try
        {
            int? driverId = _selectedFilterDriver?.driverId > 0 ? _selectedFilterDriver.driverId : null;
            string? status = null;
            if (AuditStatusFilter.SelectedItem is ComboBoxItem item && item.Tag is string tag && !string.IsNullOrEmpty(tag))
                status = tag;
            string? from = AuditFromDate.SelectedDate?.ToString("yyyy-MM-dd");
            string? to = AuditToDate.SelectedDate?.ToString("yyyy-MM-dd");

            var json = await _api.GetCameraAuditsJsonAsync(driverId, status, from, to);
            if (json != null)
            {
                var audits = JsonConvert.DeserializeObject<JArray>(json);
                AuditsGrid.ItemsSource = audits;
            }
        }
        catch (ApiUnauthorizedException) { MessageBox.Show("انتهت صلاحية الجلسة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void Filter_Changed(object sender, object e) { if (_isLoaded) await LoadAuditsAsync(); }
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAuditsAsync();

    private void AuditsGrid_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (AuditsGrid.SelectedItem is JObject audit)
            ShowAuditDetail(audit["auditId"]!.Value<int>());
    }

    // ═══ CREATE AUDIT ═══
    private async void AddAudit_Click(object sender, RoutedEventArgs e)
    {
        AuditDatePicker.SelectedDate = DateTime.Today;
        TripRefBox.Text = "";
        AuditNotesBox.Text = "";
        AuditDialogError.Visibility = Visibility.Collapsed;

        try
        {
            var driversJson = await _api.GetDriversJsonAsync();
            if (driversJson != null)
            {
                var drivers = JArray.Parse(driversJson);
                _dialogDrivers = drivers.Select(d => new DriverComboItem
                {
                    driverId = d["driverId"]?.Value<int>() ?? 0,
                    fullName = d["fullName"]?.ToString() ?? ""
                }).ToList();
                _selectedDialogDriver = null;
                _suppressDialogDriverSearch = true;
                AuditDialogDriverSearchBox.Text = "";
                _suppressDialogDriverSearch = false;
                ClearAuditDialogDriverBtn.Visibility = Visibility.Collapsed;
            }
        }
        catch { }

        AuditDialog.Visibility = Visibility.Visible;
    }

    private async void CreateAudit_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedDialogDriver == null)
        {
            AuditDialogError.Text = "اختر السائق";
            AuditDialogError.Visibility = Visibility.Visible;
            return;
        }

        try
        {
            var data = new
            {
                AuditDate = AuditDatePicker.SelectedDate ?? DateTime.Today,
                DriverId = _selectedDialogDriver!.driverId,
                TripReference = TripRefBox.Text.Trim(),
                Notes = AuditNotesBox.Text.Trim()
            };

            var result = await _api.CreateCameraAuditAsync(data);
            if (result != null)
            {
                AuditDialog.Visibility = Visibility.Collapsed;
                await LoadAuditsAsync();
            }
        }
        catch (Exception ex)
        {
            AuditDialogError.Text = ex.Message;
            AuditDialogError.Visibility = Visibility.Visible;
        }
    }

    private void CancelAuditDialog_Click(object sender, RoutedEventArgs e) => AuditDialog.Visibility = Visibility.Collapsed;

    // ═══ VIEW DETAIL ═══
    private void ViewAudit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is JObject audit)
            ShowAuditDetail(audit["auditId"]!.Value<int>());
    }

    private async void ShowAuditDetail(int auditId)
    {
        try
        {
            var json = await _api.GetCameraAuditJsonAsync(auditId);
            if (json != null)
            {
                var audit = JsonConvert.DeserializeObject<JObject>(json);
                if (audit != null)
                {
                    DetailTitle.Text = $"مراجعة {audit["driverName"]} — {audit["auditDate"]?.Value<DateTime>():yyyy-MM-dd}";
                    var discrepancies = audit["discrepancies"] as JArray;
                    DiscrepanciesGrid.ItemsSource = discrepancies;
                    DetailView.Visibility = Visibility.Visible;
                }
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void CloseDetail_Click(object sender, RoutedEventArgs e) => DetailView.Visibility = Visibility.Collapsed;

    // ═══ UPDATE DISCREPANCY STATUS ═══
    private async void UpdateStatus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not JObject disc) return;

        int discId = disc["discrepancyId"]!.Value<int>();
        string currentStatus = disc["status"]?.ToString() ?? "Open";

        // Simple status progression
        string nextStatus = currentStatus switch
        {
            "Open" => "UnderInvestigation",
            "UnderInvestigation" => "Resolved",
            _ => currentStatus
        };

        if (nextStatus == currentStatus)
        {
            MessageBox.Show("هذه الحالة لا يمكن تغييرها", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string? resolution = null;
        if (nextStatus == "Resolved")
        {
            var confirmResolve = MessageBox.Show("هل تم حل هذا الفرق؟", "تأكيد الحل", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmResolve != MessageBoxResult.Yes) return;
            resolution = "تم الحل بعد المراجعة";
        }

        try
        {
            var result = await _api.UpdateDiscrepancyStatusAsync(discId, nextStatus, resolution, $"تغيير الحالة إلى {nextStatus}");
            if (result)
            {
                MessageBox.Show("تم تحديث الحالة", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                // Refresh the detail view
                var auditJson = DiscrepanciesGrid.ItemsSource;
                await LoadAuditsAsync();
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    // ═══ DELETE AUDIT ═══
    private async void DeleteAudit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not JObject audit) return;
        var confirm = MessageBox.Show("هل تريد حذف هذه المراجعة؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            int id = audit["auditId"]!.Value<int>();
            bool deleted = await _api.DeleteCameraAuditAsync(id);
            if (deleted) await LoadAuditsAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    // ══════════════════════════════════════
    // FILTER DRIVER SEARCH
    // ══════════════════════════════════════
    private void AuditDriverFilterSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressFilterDriverSearch) return;
        var q = AuditDriverFilterSearchBox.Text.Trim();
        if (string.IsNullOrEmpty(q)) { _selectedFilterDriver = null; ClearAuditDriverFilterBtn.Visibility = Visibility.Collapsed; AuditDriverFilterPopup.IsOpen = false; return; }
        if (_selectedFilterDriver != null && q == _selectedFilterDriver.fullName) return;
        _selectedFilterDriver = null;
        var f = _allDrivers.Where(d => d.fullName.Contains(q, StringComparison.OrdinalIgnoreCase)).Take(50).ToList();
        AuditDriverFilterResults.ItemsSource = f; AuditDriverFilterPopup.IsOpen = f.Count > 0;
    }
    private void AuditDriverFilterSearch_GotFocus(object sender, RoutedEventArgs e)
    { if (string.IsNullOrWhiteSpace(AuditDriverFilterSearchBox.Text) && _allDrivers.Count > 0) { AuditDriverFilterResults.ItemsSource = _allDrivers.Take(50).ToList(); AuditDriverFilterPopup.IsOpen = true; } }
    private void AuditDriverFilterResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AuditDriverFilterResults.SelectedItem is DriverComboItem item)
        { _selectedFilterDriver = item; _suppressFilterDriverSearch = true; AuditDriverFilterSearchBox.Text = item.fullName; _suppressFilterDriverSearch = false; ClearAuditDriverFilterBtn.Visibility = Visibility.Visible; AuditDriverFilterPopup.IsOpen = false; AuditDriverFilterResults.SelectedItem = null; }
    }
    private void ClearAuditDriverFilter_Click(object sender, RoutedEventArgs e)
    { _selectedFilterDriver = null; _suppressFilterDriverSearch = true; AuditDriverFilterSearchBox.Text = ""; _suppressFilterDriverSearch = false; ClearAuditDriverFilterBtn.Visibility = Visibility.Collapsed; }

    // ══════════════════════════════════════
    // DIALOG DRIVER SEARCH
    // ══════════════════════════════════════
    private void AuditDialogDriverSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressDialogDriverSearch) return;
        var q = AuditDialogDriverSearchBox.Text.Trim();
        if (string.IsNullOrEmpty(q)) { _selectedDialogDriver = null; ClearAuditDialogDriverBtn.Visibility = Visibility.Collapsed; AuditDialogDriverPopup.IsOpen = false; return; }
        if (_selectedDialogDriver != null && q == _selectedDialogDriver.fullName) return;
        _selectedDialogDriver = null;
        var f = _dialogDrivers.Where(d => d.fullName.Contains(q, StringComparison.OrdinalIgnoreCase)).Take(50).ToList();
        AuditDialogDriverResults.ItemsSource = f; AuditDialogDriverPopup.IsOpen = f.Count > 0;
    }
    private void AuditDialogDriverSearch_GotFocus(object sender, RoutedEventArgs e)
    { if (string.IsNullOrWhiteSpace(AuditDialogDriverSearchBox.Text) && _dialogDrivers.Count > 0) { AuditDialogDriverResults.ItemsSource = _dialogDrivers.Take(50).ToList(); AuditDialogDriverPopup.IsOpen = true; } }
    private void AuditDialogDriverResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AuditDialogDriverResults.SelectedItem is DriverComboItem item)
        { _selectedDialogDriver = item; _suppressDialogDriverSearch = true; AuditDialogDriverSearchBox.Text = item.fullName; _suppressDialogDriverSearch = false; ClearAuditDialogDriverBtn.Visibility = Visibility.Visible; AuditDialogDriverPopup.IsOpen = false; AuditDialogDriverResults.SelectedItem = null; }
    }
    private void ClearAuditDialogDriver_Click(object sender, RoutedEventArgs e)
    { _selectedDialogDriver = null; _suppressDialogDriverSearch = true; AuditDialogDriverSearchBox.Text = ""; _suppressDialogDriverSearch = false; ClearAuditDialogDriverBtn.Visibility = Visibility.Collapsed; }
}

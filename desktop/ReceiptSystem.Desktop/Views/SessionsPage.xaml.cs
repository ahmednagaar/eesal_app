using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class SessionsPage : UserControl
{
    private readonly ApiClient _api;
    private int _currentPage = 1;
    private const int PageSize = 20;
    private int _selectedSessionId;
    private decimal _selectedSessionAmount;

    public SessionsPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        FilterDate.SelectedDate = DateTime.Today;
        Loaded += async (_, _) =>
        {
            await LoadDriversAsync();
            await LoadSessionsAsync();
        };
    }

    private async Task LoadDriversAsync()
    {
        try
        {
            var json = await _api.GetDriversJsonAsync();
            if (json == null) return;
            var drivers = JArray.Parse(json);
            var list = drivers.Select(d => new
            {
                driverId = d["driverId"]?.Value<int>() ?? 0,
                fullName = d["fullName"]?.ToString() ?? ""
            }).ToList();
            list.Insert(0, new { driverId = 0, fullName = "— الكل —" });
            FilterDriver.ItemsSource = list;
            FilterDriver.SelectedIndex = 0;
        }
        catch { }
    }

    private async Task LoadSessionsAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        DetailPanel.Visibility = Visibility.Collapsed;
        try
        {
            string? date = FilterDate.SelectedDate?.ToString("yyyy-MM-dd");
            int? driverId = FilterDriver.SelectedValue is int did && did > 0 ? did : null;

            var json = await _api.GetSessionsJsonAsync(_currentPage, PageSize, date, driverId);
            if (json != null)
            {
                var result = JObject.Parse(json);
                var sessions = result["items"] as JArray ?? result["sessions"] as JArray;
                var total = result["totalCount"]?.Value<int>() ?? result["total"]?.Value<int>() ?? 0;

                if (sessions != null && sessions.Count > 0)
                {
                    var rows = sessions.Select(s => new
                    {
                        sessionId = s["sessionId"]?.Value<int>() ?? 0,
                        sessionDate = s["sessionDate"]?.ToString()?.Substring(0, 10) ?? "",
                        driverName = s["driverName"]?.ToString() ?? "",
                        routeArea = s["routeArea"]?.ToString() ?? "",
                        receiptRange = $"{s["firstReceiptNumber"]}–{s["lastReceiptNumber"]}",
                        count = s["totalReceiptsCount"]?.Value<int>() ?? 0,
                        amount = (s["totalAmountCollected"]?.Value<decimal>() ?? 0).ToString("N2"),
                        rawAmount = s["totalAmountCollected"]?.Value<decimal>() ?? 0,
                        gaps = (s["hasGaps"]?.Value<bool>() ?? false) ? "⚠️" : "✓",
                        status = (s["isConfirmed"]?.Value<bool>() ?? false) ? "🔒 مؤكد" : "مفتوح",
                        isConfirmed = s["isConfirmed"]?.Value<bool>() ?? false,
                        source = s["importSource"]?.ToString() ?? "يدوي"
                    }).ToList();

                    SessionsGrid.ItemsSource = rows;
                    EmptyState.Visibility = Visibility.Collapsed;

                    int totalPages = (int)Math.Ceiling((double)total / PageSize);
                    PageInfo.Text = $"صفحة {_currentPage} من {totalPages}";
                    PrevBtn.IsEnabled = _currentPage > 1;
                    NextBtn.IsEnabled = _currentPage < totalPages;
                }
                else
                {
                    SessionsGrid.ItemsSource = null;
                    EmptyState.Visibility = Visibility.Visible;
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

    // ── Filters ──
    private void Filter_Changed(object sender, EventArgs e) { }
    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        _currentPage = 1;
        await LoadSessionsAsync();
    }
    private async void ClearFilters_Click(object sender, RoutedEventArgs e)
    {
        FilterDate.SelectedDate = null;
        if (FilterDriver.Items.Count > 0) FilterDriver.SelectedIndex = 0;
        _currentPage = 1;
        await LoadSessionsAsync();
    }

    // ── Pagination ──
    private async void Prev_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1) { _currentPage--; await LoadSessionsAsync(); }
    }
    private async void Next_Click(object sender, RoutedEventArgs e)
    {
        _currentPage++; await LoadSessionsAsync();
    }

    // ── Selection ──
    private void SessionsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SessionsGrid.SelectedItem == null) { DetailPanel.Visibility = Visibility.Collapsed; return; }

        dynamic row = SessionsGrid.SelectedItem;
        _selectedSessionId = (int)row.sessionId;
        _selectedSessionAmount = (decimal)row.rawAmount;
        DetailTitle.Text = $"جلسة {row.driverName} — {row.sessionDate}";
        DetailPanel.Visibility = Visibility.Visible;
        ReceiptsDetail.Visibility = Visibility.Collapsed;

        bool isConfirmed = (bool)row.isConfirmed;
        ConfirmBtn.Visibility = isConfirmed ? Visibility.Collapsed : Visibility.Visible;
        ReconcileBtn.Visibility = Visibility.Visible;

        // Admin-only unlock
        bool isAdmin = _api.CurrentUserRole?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
        UnlockBtn.Visibility = isConfirmed && isAdmin ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Confirm ──
    private async void Confirm_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            bool ok = await _api.ConfirmSessionAsync(_selectedSessionId);
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم تأكيد الجلسة");
                // Rebuild receipts detail if open so Edit buttons reflect confirmed state
                bool detailWasOpen = ReceiptsDetail.Visibility == Visibility.Visible;
                if (detailWasOpen) ReceiptsDetail.Visibility = Visibility.Collapsed;
                await LoadSessionsAsync();
                if (detailWasOpen) ViewDetails_Click(sender, e);
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

    // ── Reconcile ──
    private void Reconcile_Click(object sender, RoutedEventArgs e)
    {
        ReconcileSessionInfo.Text = $"المبلغ المسجل: {_selectedSessionAmount:N2} جنيه";
        ActualCashBox.Text = "";
        ReconcileResult.Visibility = Visibility.Collapsed;
        ReconcileDialog.Visibility = Visibility.Visible;
        ActualCashBox.Focus();
    }
    private async void ReconcileConfirm_Click(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(ActualCashBox.Text, out decimal actual))
        {
            ToastHelper.ShowError(RootGrid, "يرجى إدخال مبلغ صحيح");
            return;
        }
        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            var result = await _api.ReconcileSessionAsync(_selectedSessionId, actual);
            if (result != null)
            {
                string status = result["status"]?.ToString() ?? "";
                decimal diff = result["difference"]?.Value<decimal>() ?? 0;
                var color = status == "Match" ? "AccentSuccessBrush"
                    : status == "Surplus" ? "AccentWarningBrush" : "AccentDangerBrush";
                var label = status == "Match" ? "✓ متطابق"
                    : status == "Surplus" ? $"فائض: {diff:N2}" : $"عجز: {Math.Abs(diff):N2}";
                ReconcileResult.Text = label;
                ReconcileResult.Foreground = (System.Windows.Media.Brush)FindResource(color);
                ReconcileResult.Visibility = Visibility.Visible;
                ToastHelper.ShowSuccess(RootGrid, "تمت التسوية");
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }
    private void ReconcileCancel_Click(object sender, RoutedEventArgs e) => ReconcileDialog.Visibility = Visibility.Collapsed;

    // ── Unlock ──
    private void Unlock_Click(object sender, RoutedEventArgs e)
    {
        UnlockReasonBox.Text = "";
        UnlockDialog.Visibility = Visibility.Visible;
        UnlockReasonBox.Focus();
    }
    private async void UnlockConfirm_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UnlockReasonBox.Text))
        {
            ToastHelper.ShowError(RootGrid, "يرجى إدخال سبب فتح القفل");
            return;
        }
        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            var result = await _api.UnlockSessionAsync(_selectedSessionId, UnlockReasonBox.Text.Trim());
            if (result != null)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم فتح قفل الجلسة");
                UnlockDialog.Visibility = Visibility.Collapsed;
                // Rebuild receipts detail if open so Edit buttons reflect unlocked state
                bool detailWasOpen = ReceiptsDetail.Visibility == Visibility.Visible;
                if (detailWasOpen) ReceiptsDetail.Visibility = Visibility.Collapsed;
                await LoadSessionsAsync();
                if (detailWasOpen) ViewDetails_Click(sender, e);
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }
    private void UnlockCancel_Click(object sender, RoutedEventArgs e) => UnlockDialog.Visibility = Visibility.Collapsed;

    // ── View Details (receipts + gaps) ──
    private bool _selectedSessionConfirmed;
    private bool _detailIsAdmin;
    private bool _detailIsConfirmed;

    private async void ViewDetails_Click(object sender, RoutedEventArgs e)
    {
        if (ReceiptsDetail.Visibility == Visibility.Visible)
        {
            ReceiptsDetail.Visibility = Visibility.Collapsed;
            ReceiptsGrid.LoadingRow -= ReceiptsGrid_LoadingRow;
            return;
        }

        try
        {
            var json = await _api.GetSessionDetailJsonAsync(_selectedSessionId);
            if (json == null) return;
            var detail = JObject.Parse(json);

            _detailIsAdmin = _api.CurrentUserRole?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
            _detailIsConfirmed = detail["isConfirmed"]?.Value<bool>() ?? _selectedSessionConfirmed;
            _selectedSessionConfirmed = _detailIsConfirmed;

            // Receipts
            var receipts = detail["receipts"] as JArray;
            if (receipts != null)
            {
                var rows = receipts.Select(r => new
                {
                    receiptId = r["receiptId"]?.Value<int>() ?? r["id"]?.Value<int>() ?? 0,
                    receiptNumber = r["receiptNumber"]?.Value<int>() ?? 0,
                    merchantName = r["merchantName"]?.ToString() ?? "",
                    amount = (r["amount"]?.Value<decimal>() ?? 0).ToString("N2"),
                    partial = (r["isPartialPayment"]?.Value<bool>() ?? false) ? "✓" : "",
                    notes = r["notes"]?.ToString() ?? "",
                    canEdit = _detailIsAdmin || !_detailIsConfirmed,
                    canDelete = _detailIsAdmin
                }).ToList();
                ReceiptsGrid.ItemsSource = rows;
            }

            // Gaps
            var gaps = detail["gaps"] as JArray;
            if (gaps != null && gaps.Count > 0)
            {
                var gapRows = gaps.Select(g =>
                {
                    var st = g["status"]?.ToString() ?? "Open";
                    return new
                    {
                        gapNumber = g["receiptNumber"]?.Value<int>() ?? g["gapNumber"]?.Value<int>() ?? 0,
                        statusDisplay = st switch
                        {
                            "Open" => "مفتوح",
                            "Investigating" => "قيد التحقيق",
                            "Resolved" => "تم الحل",
                            "Dismissed" => "مُلغى",
                            _ => st
                        }
                    };
                }).ToList();
                GapsList.ItemsSource = gapRows;
                GapsEmptyLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                GapsList.ItemsSource = null;
                GapsEmptyLabel.Visibility = Visibility.Visible;
            }

            ReceiptsDetail.Visibility = Visibility.Visible;

            // Wire LoadingRow for virtualization-safe role gating
            // (fires per row, including rows created by scrolling)
            ReceiptsGrid.LoadingRow -= ReceiptsGrid_LoadingRow; // prevent double-wire
            ReceiptsGrid.LoadingRow += ReceiptsGrid_LoadingRow;
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
    }

    /// <summary>
    /// Fired per DataGrid row (including newly-virtualized rows on scroll).
    /// Applies role gating to Edit/Delete buttons in each row.
    /// </summary>
    private void ReceiptsGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
    {
        // Defer to after the row's template is applied
        e.Row.Loaded += (_, _) =>
        {
            var deleteButtons = FindVisualChildren<Button>(e.Row).Where(b => b.ToolTip?.ToString() == "حذف الإيصال");
            var editButtons = FindVisualChildren<Button>(e.Row).Where(b => b.ToolTip?.ToString() == "تعديل الإيصال");

            foreach (var btn in deleteButtons)
                btn.Visibility = _detailIsAdmin ? Visibility.Visible : Visibility.Collapsed;

            foreach (var btn in editButtons)
                btn.IsEnabled = _detailIsAdmin || !_detailIsConfirmed;
        };
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T t) yield return t;
            foreach (var sub in FindVisualChildren<T>(child)) yield return sub;
        }
    }

    // ── Edit Receipt ──
    private async void EditReceipt_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        dynamic? row = ((FrameworkElement)btn).DataContext;
        if (row == null) return;

        int receiptId = (int)row.receiptId;
        // For now, allow editing notes inline — open a simple InputBox
        string currentNotes = (string)row.notes;
        var dlg = new System.Windows.Controls.TextBox { Text = currentNotes, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 80 };
        var result = MessageBox.Show($"تعديل ملاحظات الإيصال {row.receiptNumber}?\n\nاستخدم الحقل في النافذة التالية.",
            "تعديل", MessageBoxButton.OKCancel, MessageBoxImage.Question);
        if (result != MessageBoxResult.OK) return;

        btn.IsEnabled = false;
        try
        {
            bool ok = await _api.UpdateReceiptAsync(receiptId, new { notes = currentNotes });
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم تحديث الإيصال");
                // Refresh detail
                ReceiptsDetail.Visibility = Visibility.Collapsed;
                ViewDetails_Click(sender, e);
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

    // ── Delete Receipt (Admin-only — backend returns 403 for non-Admin) ──
    private async void DeleteReceipt_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        dynamic? row = ((FrameworkElement)btn).DataContext;
        if (row == null) return;

        int receiptId = (int)row.receiptId;
        var confirm = MessageBox.Show($"هل أنت متأكد من حذف الإيصال رقم {row.receiptNumber}؟",
            "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        btn.IsEnabled = false;
        try
        {
            bool ok = await _api.DeleteReceiptAsync(receiptId);
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم حذف الإيصال");
                ReceiptsDetail.Visibility = Visibility.Collapsed;
                await LoadSessionsAsync();
            }
        }
        catch (Exception ex)
        {
            // Show the API's Arabic 403 message verbatim
            ToastHelper.ShowError(RootGrid, ex.Message);
        }
        finally { btn.IsEnabled = true; }
    }
}

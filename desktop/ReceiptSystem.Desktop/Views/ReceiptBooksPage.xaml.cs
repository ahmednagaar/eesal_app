using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class ReceiptBooksPage : UserControl
{
    private readonly ApiClient _api;
    private int _selectedSeriesId;
    private int _selectedBookId;
    private string _selectedBookStatus = "";
    private List<dynamic> _seriesData = new();
    private List<dynamic> _allBooks = new();

    public ReceiptBooksPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        AssignDate.SelectedDate = DateTime.Today;
        Loaded += async (_, _) =>
        {
            await LoadDriversAsync();
            await LoadSeriesAsync();
            await LoadAlertsAsync();
        };

        // Live summary update in create dialog
        NewSeriesTotalBooks.TextChanged += UpdateSeriesSummary;
        NewSeriesReceiptsPerBook.TextChanged += UpdateSeriesSummary;
        NewSeriesStartNumber.TextChanged += UpdateSeriesSummary;
    }

    // ══════════════════════════════════════
    // Load Data
    // ══════════════════════════════════════

    private async Task LoadDriversAsync()
    {
        try
        {
            var json = await _api.GetDriversJsonAsync();
            if (json == null) return;
            var drivers = JArray.Parse(json);
            AssignDriverCombo.ItemsSource = drivers.Select(d => new DriverComboItem
            {
                driverId = d["driverId"]?.Value<int>() ?? 0,
                fullName = d["fullName"]?.ToString() ?? ""
            }).ToList();
        }
        catch { }
    }

    private async Task LoadSeriesAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            var json = await _api.GetSeriesListJsonAsync();
            if (json == null) { SeriesEmpty.Visibility = Visibility.Visible; return; }

            var series = JArray.Parse(json);
            if (series.Count == 0) { SeriesList.ItemsSource = null; SeriesEmpty.Visibility = Visibility.Visible; return; }

            SeriesEmpty.Visibility = Visibility.Collapsed;
            _seriesData = series.Select(s =>
            {
                var status = s["status"]?.ToString() ?? "Active";
                var books = s["books"] as JArray;
                int available = 0, assigned = 0, completed = 0;

                if (books != null)
                {
                    foreach (var b in books)
                    {
                        var bs = b["status"]?.ToString() ?? "";
                        if (bs == "Available") available++;
                        else if (bs == "Assigned" || bs == "InProgress") assigned++;
                        else if (bs == "Returned" || bs == "Depleted" || bs == "Completed") completed++;
                    }
                }

                return (dynamic)new
                {
                    seriesId = s["seriesId"]?.Value<int>() ?? 0,
                    seriesCode = s["seriesCode"]?.ToString() ?? "",
                    totalBooks = s["totalBooks"]?.Value<int>() ?? 0,
                    receiptsPerBook = s["receiptsPerBook"]?.Value<int>() ?? 50,
                    status = status,
                    statusDisplay = status == "Active" ? "نشطة" : "مكتملة",
                    statusColor = status == "Active"
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
                    availableBooks = available,
                    assignedBooks = assigned,
                    completedBooks = completed,
                    returnedBooks = s["returnedBooks"]?.Value<int>() ?? 0,
                    usagePercent = s["usagePercent"]?.Value<double>() ?? 0,
                    progressText = $"{s["usagePercent"]?.Value<double>() ?? 0:F0}% مستخدم",
                    notes = s["notes"]?.ToString() ?? ""
                };
            }).ToList();

            SeriesList.ItemsSource = _seriesData;
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

    private async Task LoadAlertsAsync()
    {
        try
        {
            var json = await _api.GetSeriesAlertsJsonAsync();
            if (json == null) { AlertsBar.Visibility = Visibility.Collapsed; return; }

            var alerts = JArray.Parse(json);
            if (alerts.Count == 0) { AlertsBar.Visibility = Visibility.Collapsed; return; }

            AlertsList.ItemsSource = alerts.Select(a => new
            {
                message = a["message"]?.ToString() ?? a.ToString()
            }).ToList();
            AlertsBar.Visibility = Visibility.Visible;
        }
        catch { AlertsBar.Visibility = Visibility.Collapsed; }
    }

    // ══════════════════════════════════════
    // Series Card Click → Load Books
    // ══════════════════════════════════════

    private async void SeriesCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is object ctx)
        {
            dynamic series = ctx;
            _selectedSeriesId = (int)series.seriesId;

            // Update visual selection
            BooksTitle.Text = $"دورة {series.seriesCode}";
            BooksSubtitle.Text = $"{series.totalBooks} دفتر";
            BooksEmpty.Visibility = Visibility.Collapsed;
            FilterBar.Visibility = Visibility.Visible;

            // Reset filters
            FilterStatus.SelectedIndex = 0;
            FilterBookNumber.Text = "";
            FilterDriverName.Text = "";

            // Show/hide complete & delete buttons
            bool isActive = ((string)series.status) == "Active";
            bool isAdmin = _api.CurrentUserRole?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
            CompleteSeriesBtn.Visibility = isActive && isAdmin ? Visibility.Visible : Visibility.Collapsed;

            // Delete only allowed if ALL books are still available (none assigned/returned)
            bool canDelete = isActive && isAdmin && ((int)series.assignedBooks == 0) && ((int)series.completedBooks == 0);
            DeleteSeriesBtn.Visibility = canDelete ? Visibility.Visible : Visibility.Collapsed;

            // Show Quick Assign Next button for active series
            AssignNextBtn.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;

            // Reset book selection
            BookActionsPanel.Visibility = Visibility.Collapsed;
            _selectedBookId = 0;

            await LoadBooksForSeriesAsync(_selectedSeriesId);
        }
    }

    private async Task LoadBooksForSeriesAsync(int seriesId)
    {
        try
        {
            var json = await _api.GetSeriesDetailJsonAsync(seriesId);
            if (json == null) return;

            var detail = JObject.Parse(json);
            var books = detail["books"] as JArray;
            if (books == null) { BooksGrid.ItemsSource = null; return; }

            var rows = books.Select(b =>
            {
                var st = b["status"]?.ToString() ?? "Available";
                int start = b["startReceiptNumber"]?.Value<int>() ?? 0;
                int end = b["endReceiptNumber"]?.Value<int>() ?? 0;
                int usedCount = b["receipts"] is JArray receipts ? receipts.Count : 0;
                int total = end - start + 1;

                return new
                {
                    bookId = b["bookId"]?.Value<int>() ?? 0,
                    bookNumber = b["bookNumber"]?.Value<int>() ?? 0,
                    displayName = $"{detail["seriesCode"]}-{b["bookNumber"]}",
                    range = $"{start}–{end}",
                    startReceipt = start,
                    endReceipt = end,
                    statusDisplay = st switch
                    {
                        "Available" => "متاح",
                        "Assigned" => "مُعيّن",
                        "InProgress" => "قيد الاستخدام",
                        "Returned" => "تم الإرجاع",
                        "Depleted" => "مُنتهي",
                        "Completed" => "مكتمل",
                        "Deactivated" => "مُلغى",
                        _ => st
                    },
                    rawStatus = st,
                    driverName = b["assignedToDriver"]?["fullName"]?.ToString()
                                 ?? b["driverName"]?.ToString() ?? "—",
                    assignedDate = b["assignedDate"]?.ToString()?.Substring(0, Math.Min(10, b["assignedDate"]?.ToString()?.Length ?? 0)) ?? "—",
                    usedReceipts = usedCount,
                    remainingReceipts = total - usedCount,
                    isVerified = b["isVerified"]?.Value<bool>() ?? false,
                    verifiedDisplay = (b["isVerified"]?.Value<bool>() ?? false) ? "✓" : "—"
                };
            }).OrderBy(b => b.bookNumber).ToList();

            _allBooks = rows.Cast<dynamic>().ToList();
            ApplyFilters();
            FilterBar.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex));
        }
    }

    // ══════════════════════════════════════
    // Filters
    // ══════════════════════════════════════

    private void Filter_Changed(object sender, SelectionChangedEventArgs e) => ApplyFilters();
    private void Filter_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

    private void ApplyFilters()
    {
        if (!IsLoaded || BooksGrid == null) return;
        if (_allBooks == null || _allBooks.Count == 0) { BooksGrid.ItemsSource = null; return; }

        var filtered = _allBooks.AsEnumerable();

        // Status filter
        int statusIndex = FilterStatus?.SelectedIndex ?? 0;
        if (statusIndex > 0)
        {
            string statusFilter = statusIndex switch
            {
                1 => "Available",
                2 => "Assigned",
                3 => "Returned",
                4 => "Verified",
                5 => "Deactivated",
                _ => ""
            };
            if (statusFilter == "Verified")
                filtered = filtered.Where(b => (bool)b.isVerified);
            else if (!string.IsNullOrEmpty(statusFilter))
                filtered = filtered.Where(b => (string)b.rawStatus == statusFilter);
        }

        // Book number filter
        string bookNumText = FilterBookNumber?.Text?.Trim() ?? "";
        if (!string.IsNullOrEmpty(bookNumText))
        {
            if (int.TryParse(bookNumText, out int bookNum))
                filtered = filtered.Where(b => (int)b.bookNumber == bookNum);
            else
                filtered = filtered.Where(b => ((int)b.bookNumber).ToString().Contains(bookNumText));
        }

        // Driver name filter
        string driverText = FilterDriverName?.Text?.Trim() ?? "";
        if (!string.IsNullOrEmpty(driverText))
            filtered = filtered.Where(b => ((string)b.driverName).Contains(driverText, StringComparison.OrdinalIgnoreCase));

        BooksGrid.ItemsSource = filtered.ToList();
    }

    // ══════════════════════════════════════
    // Book Selection → Show Actions
    // ══════════════════════════════════════

    private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BooksGrid.SelectedItem == null) { BookActionsPanel.Visibility = Visibility.Collapsed; return; }

        var selectedItems = BooksGrid.SelectedItems;
        int count = selectedItems.Count;

        // Show/hide action buttons based on selection
        bool isAdmin = _api.CurrentUserRole?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
        bool isTreasury = _api.CurrentUserRole?.Equals("Treasury", StringComparison.OrdinalIgnoreCase) == true;
        bool canManageBooks = isAdmin || isTreasury;

        if (count > 1)
        {
            // Multi-select: show bulk actions only
            var allAvailable = selectedItems.Cast<dynamic>().All(b => (string)b.rawStatus == "Available");
            BookActionTitle.Text = $"تم تحديد {count} دفتر";
            BookActionsPanel.Visibility = Visibility.Visible;
            AssignSection.Visibility = allAvailable && canManageBooks ? Visibility.Visible : Visibility.Collapsed;
            AssignBtn.Visibility = Visibility.Collapsed;
            AssignBatchBtn.Visibility = allAvailable && canManageBooks ? Visibility.Visible : Visibility.Collapsed;
            ReturnBtn.Visibility = Visibility.Collapsed;
            VerifyBtn.Visibility = Visibility.Collapsed;
            return;
        }

        // Single select
        dynamic row = BooksGrid.SelectedItem;
        _selectedBookId = (int)row.bookId;
        _selectedBookStatus = (string)row.rawStatus;
        BookActionTitle.Text = $"دفتر {row.displayName} ({row.range})";
        BookActionsPanel.Visibility = Visibility.Visible;

        // Assign: for "Available" or "Returned" books (reassign)
        bool canAssign = (_selectedBookStatus == "Available" || _selectedBookStatus == "Returned") && canManageBooks;
        AssignSection.Visibility = canAssign ? Visibility.Visible : Visibility.Collapsed;
        AssignBtn.Visibility = canAssign ? Visibility.Visible : Visibility.Collapsed;
        AssignBatchBtn.Visibility = Visibility.Collapsed;

        // Return: only for "Assigned" or "InProgress" books
        ReturnBtn.Visibility = (_selectedBookStatus == "Assigned" || _selectedBookStatus == "InProgress") && canManageBooks
            ? Visibility.Visible : Visibility.Collapsed;

        // Verify: only for "Returned" books, Admin/Treasury only
        VerifyBtn.Visibility = _selectedBookStatus == "Returned" && canManageBooks
            ? Visibility.Visible : Visibility.Collapsed;
    }

    // ══════════════════════════════════════
    // Actions
    // ══════════════════════════════════════

    private async void Assign_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBookId <= 0) { ToastHelper.ShowError(RootGrid, "اختر دفتر أولاً"); return; }
        if (AssignDriverCombo.SelectedValue is not int driverId || driverId <= 0)
        { ToastHelper.ShowError(RootGrid, "اختر السائق"); return; }

        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            bool ok = await _api.AssignBookAsync(_selectedBookId, driverId, AssignDate.SelectedDate);
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم تسليم الدفتر للسائق");
                await LoadBooksForSeriesAsync(_selectedSeriesId);
                await LoadSeriesAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

    private async void AssignBatch_Click(object sender, RoutedEventArgs e)
    {
        if (BooksGrid.SelectedItems.Count <= 1) { ToastHelper.ShowError(RootGrid, "اختر أكثر من دفتر"); return; }
        if (AssignDriverCombo.SelectedValue is not int driverId || driverId <= 0)
        { ToastHelper.ShowError(RootGrid, "اختر السائق"); return; }

        var bookIds = BooksGrid.SelectedItems.Cast<dynamic>().Select(b => (int)b.bookId).ToArray();
        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            var result = await _api.AssignBooksBatchAsync(bookIds, driverId, AssignDate.SelectedDate);
            if (result != null)
            {
                ToastHelper.ShowSuccess(RootGrid, $"✓ تم تسليم {bookIds.Length} دفتر للسائق");
                await LoadBooksForSeriesAsync(_selectedSeriesId);
                await LoadSeriesAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

    private async void Return_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBookId <= 0) return;
        var confirm = MessageBox.Show("هل تم إرجاع هذا الدفتر من السائق؟", "تأكيد الإرجاع",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            bool ok = await _api.ReturnBookAsync(_selectedBookId);
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم تسجيل إرجاع الدفتر");
                await LoadBooksForSeriesAsync(_selectedSeriesId);
                await LoadSeriesAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

    private async void Verify_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBookId <= 0) return;
        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            bool ok = await _api.VerifyBookAsync(_selectedBookId);
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم التحقق من الدفتر بنجاح");
                await LoadBooksForSeriesAsync(_selectedSeriesId);
                await LoadSeriesAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

    private async void ShowHistory_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBookId <= 0) return;

        // Toggle visibility
        if (HistoryList.Visibility == Visibility.Visible)
        {
            HistoryList.Visibility = Visibility.Collapsed;
            return;
        }

        try
        {
            var json = await _api.GetBookHistoryJsonAsync(_selectedBookId);
            if (json == null) { ToastHelper.ShowError(RootGrid, "لا يوجد سجل"); return; }

            var logs = JArray.Parse(json);
            HistoryList.ItemsSource = logs.Select(l => new
            {
                action = l["action"]?.ToString() ?? "",
                userName = l["userName"]?.ToString() ?? "",
                dateDisplay = l["date"]?.ToString()?.Substring(0, Math.Min(16, l["date"]?.ToString()?.Length ?? 0)) ?? ""
            }).ToList();
            HistoryList.Visibility = Visibility.Visible;
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
    }

    // ══════════════════════════════════════
    // Complete Series
    // ══════════════════════════════════════

    private async void CompleteSeries_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedSeriesId <= 0) return;
        var confirm = MessageBox.Show("هل أنت متأكد من إغلاق هذه الدورة؟\nلا يمكن التراجع عن هذا الإجراء.",
            "تأكيد إغلاق الدورة", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            bool ok = await _api.CompleteSeriesAsync(_selectedSeriesId);
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم إغلاق الدورة");
                CompleteSeriesBtn.Visibility = Visibility.Collapsed;
                await LoadSeriesAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }
    // ══════════════════════════════════════
    // Delete Series
    // ══════════════════════════════════════

    private async void DeleteSeries_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedSeriesId <= 0) return;
        var confirm = MessageBox.Show("هل أنت متأكد من حذف هذه الدورة وجميع دفاترها؟\nلا يمكن التراجع عن هذا الإجراء.",
            "تأكيد حذف الدورة", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            bool ok = await _api.DeleteSeriesAsync(_selectedSeriesId);
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم حذف الدورة وجميع دفاترها");
                _selectedSeriesId = 0;
                DeleteSeriesBtn.Visibility = Visibility.Collapsed;
                CompleteSeriesBtn.Visibility = Visibility.Collapsed;
                BooksGrid.ItemsSource = null;
                BooksTitle.Text = "اختر دورة لعرض الدفاتر";
                BooksSubtitle.Text = "";
                BooksEmpty.Visibility = Visibility.Visible;
                BookActionsPanel.Visibility = Visibility.Collapsed;
                await LoadSeriesAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

    // ══════════════════════════════════════
    // Quick Assign Next
    // ══════════════════════════════════════

    private void AssignNext_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedSeriesId <= 0) { ToastHelper.ShowError(RootGrid, "اختر دورة أولاً"); return; }

        // Find selected series info
        var series = _seriesData.FirstOrDefault(s => (int)s.seriesId == _selectedSeriesId);
        if (series != null)
            AssignNextSeriesLabel.Text = $"الدورة: {series.seriesCode} — متاح: {series.availableBooks} دفتر";

        AssignNextDriverCombo.ItemsSource = AssignDriverCombo.ItemsSource;
        AssignNextDriverCombo.SelectedIndex = -1;
        AssignNextDialog.Visibility = Visibility.Visible;
    }

    private void AssignNextCancel_Click(object sender, RoutedEventArgs e) => AssignNextDialog.Visibility = Visibility.Collapsed;

    private async void AssignNextConfirm_Click(object sender, RoutedEventArgs e)
    {
        if (AssignNextDriverCombo.SelectedValue is not int driverId || driverId <= 0)
        { ToastHelper.ShowError(RootGrid, "اختر السائق"); return; }

        AssignNextConfirmBtn.IsEnabled = false;
        try
        {
            var result = await _api.AssignNextBookAsync(_selectedSeriesId, driverId);
            if (result != null)
            {
                string msg = result["message"]?.ToString() ?? "✓ تم التسليم";
                ToastHelper.ShowSuccess(RootGrid, msg);
                AssignNextDialog.Visibility = Visibility.Collapsed;
                await LoadBooksForSeriesAsync(_selectedSeriesId);
                await LoadSeriesAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { AssignNextConfirmBtn.IsEnabled = true; }
    }

    // ══════════════════════════════════════
    // Create Series Dialog
    // ══════════════════════════════════════

    private void CreateSeries_Click(object sender, RoutedEventArgs e)
    {
        NewSeriesCode.Text = "";
        NewSeriesTotalBooks.Text = "500";
        NewSeriesReceiptsPerBook.Text = "50";
        NewSeriesStartNumber.Text = "1";
        NewSeriesNotes.Text = "";
        UpdateSeriesSummary(null, null);
        CreateSeriesDialog.Visibility = Visibility.Visible;
        NewSeriesCode.Focus();
    }

    private void UpdateSeriesSummary(object? sender, TextChangedEventArgs? e)
    {
        if (NewSeriesTotalBooks == null || NewSeriesReceiptsPerBook == null || NewSeriesStartNumber == null || SeriesSummary == null) return;

        if (int.TryParse(NewSeriesTotalBooks.Text, out int totalBooks) &&
            int.TryParse(NewSeriesReceiptsPerBook.Text, out int perBook) &&
            int.TryParse(NewSeriesStartNumber.Text, out int start))
        {
            int totalReceipts = totalBooks * perBook;
            int endNumber = start + totalReceipts - 1;
            SeriesSummary.Text = $"سيتم إنشاء {totalBooks} دفتر × {perBook} إيصال = {totalReceipts:N0} إيصال إجمالي\n" +
                                 $"من إيصال رقم {start} إلى {endNumber}";
        }
        else
        {
            SeriesSummary.Text = "أدخل الأرقام لعرض الملخص";
        }
    }

    private async void CreateSeriesConfirm_Click(object sender, RoutedEventArgs e)
    {
        string code = NewSeriesCode.Text.Trim().ToUpper();
        if (string.IsNullOrEmpty(code))
        { ToastHelper.ShowError(RootGrid, "رمز الدورة مطلوب (مثال: A, B, C)"); return; }

        if (!int.TryParse(NewSeriesTotalBooks.Text, out int totalBooks) || totalBooks <= 0)
        { ToastHelper.ShowError(RootGrid, "عدد الدفاتر غير صحيح"); return; }

        if (!int.TryParse(NewSeriesReceiptsPerBook.Text, out int perBook) || perBook <= 0)
        { ToastHelper.ShowError(RootGrid, "عدد الإيصالات لكل دفتر غير صحيح"); return; }

        if (!int.TryParse(NewSeriesStartNumber.Text, out int startNumber) || startNumber < 0)
        { ToastHelper.ShowError(RootGrid, "رقم البداية غير صحيح"); return; }

        CreateSeriesConfirmBtn.IsEnabled = false;
        try
        {
            var result = await _api.CreateSeriesAsync(code, totalBooks, perBook, startNumber, NewSeriesNotes.Text.Trim());
            if (result != null)
            {
                ToastHelper.ShowSuccess(RootGrid, $"✓ تم إنشاء دورة {code} — {totalBooks} دفتر");
                CreateSeriesDialog.Visibility = Visibility.Collapsed;
                await LoadSeriesAsync();
                await LoadAlertsAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { CreateSeriesConfirmBtn.IsEnabled = true; }
    }

    private void CreateSeriesCancel_Click(object sender, RoutedEventArgs e) => CreateSeriesDialog.Visibility = Visibility.Collapsed;

    // ══════════════════════════════════════
    // Refresh
    // ══════════════════════════════════════

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadSeriesAsync();
        await LoadAlertsAsync();
        if (_selectedSeriesId > 0)
            await LoadBooksForSeriesAsync(_selectedSeriesId);
    }
}

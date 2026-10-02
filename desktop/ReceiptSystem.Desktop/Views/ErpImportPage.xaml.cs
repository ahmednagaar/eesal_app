using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

/// <summary>
/// Row model with checkbox support for the batch detail grid.
/// </summary>
public class ErpBatchRow : INotifyPropertyChanged
{
    private bool _isSelected;
    public bool isSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }
    public int rowId { get; set; }
    public int rowIndex { get; set; }
    public string merchantName { get; set; } = "";
    public string amount { get; set; } = "";
    public decimal rawAmount { get; set; }
    public string assignedDisplay { get; set; } = "";
    public bool isAssigned { get; set; }
    public string assignedReceipt { get; set; } = "";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class ErpImportPage : UserControl
{
    private readonly ApiClient _api;
    private string? _selectedFilePath;
    private dynamic? _previewData;
    private int _selectedBatchId;
    private List<ErpBatchRow> _batchRows = new();
    private List<DriverComboItem> _allDrivers = new();
    private List<Models.Route> _allRoutes = new();
    private bool _isFilteringCombo = false;

    public ErpImportPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        BlockSessionDate.SelectedDate = DateTime.Today;
        Loaded += async (_, _) => await LoadBatchesListAsync();
    }

    // ══════════════════════════════════════
    // TAB SWITCHING
    // ══════════════════════════════════════

    private void Tab_Changed(object sender, RoutedEventArgs e)
    {
        // Fires during InitializeComponent before panels exist
        if (UploadPanel == null || BatchesPanel == null) return;

        if (TabUpload?.IsChecked == true)
        {
            UploadPanel.Visibility = Visibility.Visible;
            BatchesPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            UploadPanel.Visibility = Visibility.Collapsed;
            BatchesPanel.Visibility = Visibility.Visible;
        }
    }

    // ══════════════════════════════════════
    // TAB 1: UPLOAD & PREVIEW
    // ══════════════════════════════════════

    private void PickFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            Title = "اختر ملف ERP Excel"
        };
        if (dlg.ShowDialog() == true)
        {
            _selectedFilePath = dlg.FileName;
            SelectedFileName.Text = Path.GetFileName(dlg.FileName);
            UploadPreview();
        }
    }

    private async void UploadPreview()
    {
        if (string.IsNullOrEmpty(_selectedFilePath)) return;
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            _previewData = await _api.UploadErpPreviewAsync(_selectedFilePath);
            if (_previewData != null)
            {
                var rows = _previewData["rows"] as JArray;
                if (rows != null)
                {
                    var previewRows = rows.Select(r => new
                    {
                        rowIndex = r["rowIndex"]?.Value<int>() ?? 0,
                        merchantNameRaw = r["merchantNameRaw"]?.ToString() ?? "",
                        amount = (r["amount"]?.Value<decimal>() ?? 0).ToString("N2"),
                        matchedName = r["matchedMerchantName"]?.ToString() ?? "—",
                        matchStatus = (r["isNewMerchant"]?.Value<bool>() ?? false) ? "🆕 جديد" : "✓ مطابق"
                    }).ToList();
                    PreviewGrid.ItemsSource = previewRows;

                    // Show warnings for unmatched
                    var unmatched = rows.Where(r => r["isNewMerchant"]?.Value<bool>() == true).ToList();
                    if (unmatched.Count > 0)
                    {
                        PreviewWarningsText.Text = $"⚠️ {unmatched.Count} تاجر غير مطابق — سيتم إنشاؤهم تلقائياً عند تأكيد الدُفعة";
                        PreviewWarnings.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        PreviewWarnings.Visibility = Visibility.Collapsed;
                    }

                    CommitPreviewBtn.Visibility = Visibility.Visible;
                }
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, ex.Message);
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private async void CommitPreview_Click(object sender, RoutedEventArgs e)
    {
        if (_previewData == null) return;
        CommitPreviewBtn.IsEnabled = false;
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            // Map preview data to CreateBatchDto format expected by the API
            var createDto = new
            {
                fileName = Path.GetFileName(_selectedFilePath ?? "Unknown.xlsx"),
                ledgerTotalAmount = _previewData["totalAmount"],
                notes = "تم الاستيراد من ملف Excel",
                rows = _previewData["rows"]
            };

            var result = await _api.CreateErpBatchAsync(createDto);
            if (result != null)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم إنشاء الدُفعة بنجاح");
                PreviewGrid.ItemsSource = null;
                CommitPreviewBtn.Visibility = Visibility.Collapsed;
                PreviewWarnings.Visibility = Visibility.Collapsed;
                SelectedFileName.Text = "لم يتم اختيار ملف";
                _previewData = null;
                _selectedFilePath = null;

                // Switch to batches tab
                TabBatches.IsChecked = true;
                await LoadBatchesListAsync();
            }
            else
            {
                ToastHelper.ShowError(RootGrid, "فشل إنشاء الدفعة. تأكد من البيانات.");
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally
        {
            CommitPreviewBtn.IsEnabled = true;
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    // ══════════════════════════════════════
    // TAB 2: ACTIVE BATCHES
    // ══════════════════════════════════════

    private async Task LoadBatchesListAsync()
    {
        try
        {
            var json = await _api.GetErpBatchesJsonAsync();
            if (json == null) return;
            var batches = JArray.Parse(json);
            var list = batches.Select(b => new BatchComboItem
            {
                batchId = b["batchId"]?.Value<int>() ?? 0,
                display = $"دُفعة {b["batchId"]} — {b["importDate"]?.ToString()?.Substring(0, 10)} ({b["status"]})"
            }).ToList();
            BatchCombo.ItemsSource = list;
            if (list.Count > 0) BatchCombo.SelectedIndex = 0;

            // Load drivers for block assignment
            var driversJson = await _api.GetDriversJsonAsync();
            if (driversJson != null)
            {
                var drivers = JArray.Parse(driversJson);
                _allDrivers = drivers.Select(d => new DriverComboItem
                {
                    driverId = d["driverId"]?.Value<int>() ?? 0,
                    fullName = d["fullName"]?.ToString() ?? ""
                }).ToList();
                BlockDriverCombo.ItemsSource = _allDrivers;
            }

            // Load routes for auto-complete combo box
            _allRoutes = await _api.GetRoutesAsync();
            BlockRouteArea.ItemsSource = _allRoutes;
        }
        catch { }
    }

    private async void BatchCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (BatchCombo.SelectedValue is not int batchId || batchId <= 0) return;
        _selectedBatchId = batchId;
        await LoadBatchDetailAsync(batchId);
    }

    private async Task LoadBatchDetailAsync(int batchId)
    {
        try
        {
            var json = await _api.GetErpBatchDetailJsonAsync(batchId);
            if (json == null) return;
            var detail = JObject.Parse(json);
            var rows = detail["rows"] as JArray;
            if (rows != null)
            {
                _batchRows = rows.Select(r => new ErpBatchRow
                {
                    rowId = r["rowId"]?.Value<int>() ?? 0,
                    rowIndex = r["rowIndex"]?.Value<int>() ?? 0,
                    merchantName = r["matchedMerchantName"]?.ToString() ?? r["merchantNameRaw"]?.ToString() ?? "",
                    amount = (r["amount"]?.Value<decimal>() ?? 0).ToString("N2"),
                    rawAmount = r["amount"]?.Value<decimal>() ?? 0,
                    isAssigned = r["isAssigned"]?.Value<bool>() ?? false,
                    assignedDisplay = (r["isAssigned"]?.Value<bool>() ?? false) ? "✓" : "",
                    assignedReceipt = r["assignedReceiptNumber"]?.ToString() ?? ""
                }).ToList();
                BatchRowsGrid.ItemsSource = _batchRows;
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, ex.Message);
        }
    }

    private async void RefreshBatches_Click(object sender, RoutedEventArgs e)
    {
        await LoadBatchesListAsync();
    }

    // ══════════════════════════════════════
    // DRIVER BOOKS DISPLAY
    // ══════════════════════════════════════

    private async void BlockDriverCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BlockDriverCombo.SelectedValue is not int driverId || driverId <= 0)
        {
            DriverBooksPanel.Visibility = Visibility.Collapsed;
            return;
        }

        try
        {
            var json = await _api.GetDriverBooksJsonAsync(driverId);
            if (json != null)
            {
                var data = Newtonsoft.Json.Linq.JObject.Parse(json);
                var books = data["books"] as Newtonsoft.Json.Linq.JArray;
                if (books != null && books.Count > 0)
                {
                    var lines = books.Select(b =>
                    {
                        int bookNum = b["bookNumber"]?.Value<int>() ?? 0;
                        int start = b["startReceiptNumber"]?.Value<int>() ?? 0;
                        int end = b["endReceiptNumber"]?.Value<int>() ?? 0;
                        int used = b["usedCount"]?.Value<int>() ?? 0;
                        int remaining = b["remainingCount"]?.Value<int>() ?? 0;
                        return $"دفتر #{bookNum}: إيصالات {start}–{end} (مستخدم: {used}, متبقي: {remaining})";
                    });
                    DriverBooksText.Text = string.Join("\n", lines);
                    DriverBooksPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    DriverBooksText.Text = "لا توجد دفاتر مسجلة لهذا السائق";
                    DriverBooksPanel.Visibility = Visibility.Visible;
                }
            }
        }
        catch { DriverBooksPanel.Visibility = Visibility.Collapsed; }
    }

    // ══════════════════════════════════════
    // PREVIEW BLOCK — Show receipt→merchant mapping before confirming
    // ══════════════════════════════════════

    private async void PreviewBlock_Click(object sender, RoutedEventArgs e)
    {
        var selected = _batchRows.Where(r => r.isSelected && !r.isAssigned).ToList();
        if (selected.Count == 0)
        { ToastHelper.ShowError(RootGrid, "اختر صفوفاً غير مُعيّنة أولاً"); return; }

        if (!int.TryParse(BlockStartReceipt.Text, out int startReceipt) || startReceipt <= 0)
        { ToastHelper.ShowError(RootGrid, "أدخل رقم أول إيصال"); return; }
        if (!int.TryParse(BlockEndReceipt.Text, out int endReceipt) || endReceipt <= 0)
        { ToastHelper.ShowError(RootGrid, "أدخل رقم آخر إيصال"); return; }
        if (endReceipt < startReceipt)
        { ToastHelper.ShowError(RootGrid, "رقم آخر إيصال يجب أن يكون أكبر من أو يساوي رقم أول إيصال"); return; }

        var sortedBySheetOrder = selected.OrderBy(r => r.rowIndex).ToList();
        var rowIds = sortedBySheetOrder.Select(r => r.rowId).ToList();

        PreviewBlockBtn.IsEnabled = false;
        try
        {
            var driverIdForPreview = BlockDriverCombo.SelectedValue as int?;
            var result = await _api.PreviewErpBlockAsync(_selectedBatchId, new
            {
                RowIds = rowIds,
                StartReceiptNumber = startReceipt,
                EndReceiptNumber = endReceipt,
                DriverId = driverIdForPreview
            });

            if (result != null)
            {
                // Build preview message
                var sb = new System.Text.StringBuilder();

                bool hasMismatch = (bool)(result["hasMismatch"] ?? false);
                if (hasMismatch)
                {
                    string mismatchMsg = result["mismatchMessage"]?.ToString() ?? "";
                    sb.AppendLine(mismatchMsg);
                    sb.AppendLine();
                    CountValidationPanel.Visibility = Visibility.Visible;
                    CountValidationText.Text = mismatchMsg;
                }
                else
                {
                    CountValidationPanel.Visibility = Visibility.Collapsed;
                }

                bool spansTwoBooks = (bool)(result["spansTwoBooks"] ?? false);
                if (spansTwoBooks)
                {
                    sb.AppendLine("📖 يمتد عبر دفترين:");
                    var breakdown = result["bookBreakdown"] as Newtonsoft.Json.Linq.JArray;
                    if (breakdown != null)
                    {
                        foreach (var b in breakdown)
                        {
                            sb.AppendLine($"  دفتر #{b["bookNumber"]}: إيصالات {b["firstReceipt"]}→{b["lastReceipt"]} ({b["receiptCount"]} إيصال)");
                        }
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("ترتيب الإيصالات:");
                sb.AppendLine("────────────────────────────");
                var mappings = result["mappings"] as Newtonsoft.Json.Linq.JArray;
                if (mappings != null)
                {
                    foreach (var m in mappings)
                    {
                        sb.AppendLine($"إيصال {m["receiptNumber"]} → {m["merchantName"]} ({m["amount"]:N2} ج)");
                    }
                }

                MessageBox.Show(sb.ToString(), "معاينة التعيين", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { PreviewBlockBtn.IsEnabled = true; }
    }

    // ══════════════════════════════════════
    // BLOCK ASSIGNMENT — First+Last Receipt, Two-Book Detection
    // Now uses explicit SkippedReceiptNumbers instead of dangerous ForceAssign
    // ══════════════════════════════════════

    private async void AssignBlock_Click(object sender, RoutedEventArgs e)
    {
        // Get selected (checked) rows
        var selected = _batchRows.Where(r => r.isSelected && !r.isAssigned).ToList();
        if (selected.Count == 0)
        {
            ToastHelper.ShowError(RootGrid, "اختر صفوفاً غير مُعيّنة أولاً (مربعات الاختيار)");
            return;
        }

        if (BlockDriverCombo.SelectedValue is not int driverId || driverId <= 0)
        { ToastHelper.ShowError(RootGrid, "اختر السائق"); return; }
        if (!int.TryParse(BlockStartReceipt.Text, out int startReceipt) || startReceipt <= 0)
        { ToastHelper.ShowError(RootGrid, "أدخل رقم أول إيصال"); return; }
        if (!int.TryParse(BlockEndReceipt.Text, out int endReceipt) || endReceipt <= 0)
        { ToastHelper.ShowError(RootGrid, "أدخل رقم آخر إيصال"); return; }
        if (endReceipt < startReceipt)
        { ToastHelper.ShowError(RootGrid, "رقم آخر إيصال يجب أن يكون أكبر من أو يساوي رقم أول إيصال"); return; }
        if (BlockSessionDate.SelectedDate == null)
        { ToastHelper.ShowError(RootGrid, "اختر تاريخ الجلسة"); return; }

        var sortedBySheetOrder = selected.OrderBy(r => r.rowIndex).ToList();
        var rowIds = sortedBySheetOrder.Select(r => r.rowId).ToList();

        await ExecuteBlockAssignment(driverId, startReceipt, endReceipt, rowIds, sortedBySheetOrder, new List<int>());
    }

    /// <summary>
    /// Executes the block assignment with optional skipped receipt numbers.
    /// Separated from the click handler so the mismatch dialog can retry with skipped numbers.
    /// </summary>
    private async Task ExecuteBlockAssignment(int driverId, int startReceipt, int endReceipt,
        List<int> rowIds, List<ErpBatchRow> sortedRows, List<int> skippedNumbers)
    {
        var dto = new
        {
            DriverId = driverId,
            SessionDate = BlockSessionDate.SelectedDate!.Value.ToString("yyyy-MM-dd"),
            StartReceiptNumber = startReceipt,
            EndReceiptNumber = endReceipt,
            RouteArea = BlockRouteArea.Text.Trim(),
            RowIds = rowIds,
            SkippedReceiptNumbers = skippedNumbers
        };

        AssignBlockBtn.IsEnabled = false;
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            var result = await _api.AssignErpBlockAsync(_selectedBatchId, dto);
            if (result != null)
            {
                int assigned = (int)(result["receiptsCreated"] ?? sortedRows.Count);
                bool spansTwoBooks = (bool)(result["spansTwoBooks"] ?? false);

                string msg = $"✓ تم تعيين {assigned} صف (إيصالات {startReceipt}–{endReceipt})";
                if (spansTwoBooks)
                {
                    var breakdown = result["bookBreakdown"] as Newtonsoft.Json.Linq.JArray;
                    if (breakdown != null)
                    {
                        msg += "\n📖 دفترين:";
                        foreach (var b in breakdown)
                            msg += $"\n  دفتر #{b["bookNumber"]}: {b["firstReceipt"]}→{b["lastReceipt"]}";
                    }
                }

                if (skippedNumbers.Count > 0)
                    msg += $"\n⚠️ إيصالات مفقودة مسجلة: {string.Join(", ", skippedNumbers)}";

                ToastHelper.ShowSuccess(RootGrid, msg);
                // Clear selection and reload
                foreach (var r in _batchRows) r.isSelected = false;
                BlockStartReceipt.Text = "";
                BlockEndReceipt.Text = "";
                BlockRouteArea.Text = "";
                CountValidationPanel.Visibility = Visibility.Collapsed;
                await LoadBatchDetailAsync(_selectedBatchId);
            }
        }
        catch (Exception ex)
        {
            string errorMsg = ex.Message;

            // Check if it's a count mismatch error — guide user to specify missing receipts
            if (errorMsg.Contains("⚠️"))
            {
                int totalReceiptsInRange = endReceipt - startReceipt + 1;
                int merchantCount = rowIds.Count;
                int expectedMissing = totalReceiptsInRange - merchantCount;

                string guidance = errorMsg + "\n\n";
                if (expectedMissing > 0)
                {
                    guidance += $"لديك {merchantCount} تاجر و {totalReceiptsInRange} إيصال في النطاق.\n";
                    guidance += $"يبدو أن {expectedMissing} إيصال(ات) مفقودة.\n\n";
                    guidance += "أدخل أرقام الإيصالات المفقودة مفصولة بفاصلة (مثال: 25,28):\n";
                    guidance += "أو اضغط إلغاء لتعديل النطاق.";
                }
                else
                {
                    guidance += "تحقق من عدد الصفوف المحددة أو نطاق الإيصالات.";
                    ToastHelper.ShowError(RootGrid, guidance);
                    return;
                }

                // Show input dialog for missing receipt numbers
                string? input = ShowInputDialog(
                    "إيصالات مفقودة — تحديد الأرقام",
                    guidance,
                    $"أدخل أرقام الإيصالات المفقودة ({startReceipt}–{endReceipt}):");

                if (string.IsNullOrWhiteSpace(input)) return; // User cancelled

                // Parse the input
                var parsed = new List<int>();
                var parts = input.Split(new[] { ',', '،', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    if (int.TryParse(part.Trim(), out int num))
                    {
                        if (num >= startReceipt && num <= endReceipt)
                            parsed.Add(num);
                        else
                        {
                            ToastHelper.ShowError(RootGrid, $"الرقم {num} ليس ضمن النطاق {startReceipt}–{endReceipt}");
                            return;
                        }
                    }
                    else
                    {
                        ToastHelper.ShowError(RootGrid, $"'{part.Trim()}' ليس رقماً صحيحاً");
                        return;
                    }
                }

                if (parsed.Count == 0)
                {
                    ToastHelper.ShowError(RootGrid, "لم يتم إدخال أي أرقام إيصالات مفقودة");
                    return;
                }

                // Show confirmation of the mapping before proceeding
                int newExpectedCount = totalReceiptsInRange - parsed.Count;
                string confirmMsg = $"سيتم تعيين {merchantCount} تاجر على {newExpectedCount} إيصال\n";
                confirmMsg += $"الإيصالات المفقودة: {string.Join(", ", parsed.OrderBy(n => n))}\n\n";
                confirmMsg += "هل تريد المتابعة؟";

                var confirm = MessageBox.Show(confirmMsg, "تأكيد التعيين مع إيصالات مفقودة",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                // Retry with the skipped numbers
                await ExecuteBlockAssignment(driverId, startReceipt, endReceipt, rowIds, _batchRows.Where(r => r.isSelected && !r.isAssigned).OrderBy(r => r.rowIndex).ToList(), parsed);
            }
            else
            {
                ToastHelper.ShowError(RootGrid, errorMsg);
            }
        }
        finally
        {
            AssignBlockBtn.IsEnabled = true;
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Simple input dialog for collecting text from the user.
    /// </summary>
    private string? ShowInputDialog(string title, string description, string prompt)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 480,
            Height = 320,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this),
            FlowDirection = FlowDirection.RightToLeft,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 40)),
            Foreground = System.Windows.Media.Brushes.White
        };

        var stack = new StackPanel { Margin = new Thickness(20) };
        stack.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = System.Windows.Media.Brushes.LightGray,
            Margin = new Thickness(0, 0, 0, 12)
        });
        stack.Children.Add(new TextBlock
        {
            Text = prompt,
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 6)
        });

        var inputBox = new TextBox
        {
            Height = 36,
            FontSize = 14,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Thickness(8, 6, 8, 6),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 60)),
            Foreground = System.Windows.Media.Brushes.White,
            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 140)),
            Margin = new Thickness(0, 0, 0, 16)
        };
        stack.Children.Add(inputBox);

        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        var okBtn = new Button { Content = "تأكيد", Width = 100, Height = 34, Margin = new Thickness(0, 0, 8, 0), FontSize = 13 };
        var cancelBtn = new Button { Content = "إلغاء", Width = 100, Height = 34, FontSize = 13 };

        string? result = null;
        okBtn.Click += (_, _) => { result = inputBox.Text; dialog.DialogResult = true; dialog.Close(); };
        cancelBtn.Click += (_, _) => { dialog.DialogResult = false; dialog.Close(); };
        btnPanel.Children.Add(okBtn);
        btnPanel.Children.Add(cancelBtn);
        stack.Children.Add(btnPanel);

        dialog.Content = stack;
        inputBox.Focus();
        dialog.ShowDialog();
        return result;
    }

    // ══════════════════════════════════════
    // UNDO BLOCK ASSIGNMENT — Now shows session details before confirming
    // ══════════════════════════════════════

    private async void UndoBlock_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(UndoSessionIdBox.Text, out int sessionId) || sessionId <= 0)
        { ToastHelper.ShowError(RootGrid, "أدخل رقم الجلسة المراد التراجع عنها"); return; }

        UndoBlockBtn.IsEnabled = false;
        try
        {
            // Fetch session details to show the user what they're about to undo
            var summaryJson = await _api.GetSessionSummaryJsonAsync(sessionId);
            if (string.IsNullOrEmpty(summaryJson))
            {
                ToastHelper.ShowError(RootGrid, $"الجلسة رقم {sessionId} غير موجودة");
                return;
            }

            var summary = JObject.Parse(summaryJson);
            string driverName = summary["driverName"]?.ToString() ?? "غير معروف";
            string sessionDate = summary["sessionDate"]?.ToString()?.Substring(0, 10) ?? "";
            string routeArea = summary["routeArea"]?.ToString() ?? "";
            int firstReceipt = summary["firstReceiptNumber"]?.Value<int>() ?? 0;
            int lastReceipt = summary["lastReceiptNumber"]?.Value<int>() ?? 0;
            int receiptCount = summary["totalReceiptsCount"]?.Value<int>() ?? 0;
            decimal amount = summary["totalAmountCollected"]?.Value<decimal>() ?? 0;
            bool isConfirmed = summary["isConfirmed"]?.Value<bool>() ?? false;

            if (isConfirmed)
            {
                ToastHelper.ShowError(RootGrid, $"⛔ الجلسة رقم {sessionId} مؤكدة (مقفلة).\nيجب فتح القفل أولاً من صفحة الجلسات قبل التراجع.");
                return;
            }

            string confirmMsg = $"هل أنت متأكد من التراجع عن هذه الجلسة؟\n\n"
                + $"🔢 رقم الجلسة: {sessionId}\n"
                + $"👤 السائق: {driverName}\n"
                + $"📅 التاريخ: {sessionDate}\n"
                + $"📍 المنطقة: {routeArea}\n"
                + $"📋 الإيصالات: {firstReceipt}–{lastReceipt} ({receiptCount} إيصال)\n"
                + $"💰 المبلغ: {amount:N2} ج\n\n"
                + "⚠️ سيتم حذف جميع الإيصالات وإعادة الصفوف لحالة غير مُعيّنة.";

            var answer = MessageBox.Show(confirmMsg, "تأكيد التراجع عن الجلسة",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes) return;

            var result = await _api.UndoErpBlockAsync(_selectedBatchId, sessionId);
            if (result != null)
            {
                int deleted = result["receiptsDeleted"]?.Value<int>() ?? 0;
                int unassigned = result["rowsUnassigned"]?.Value<int>() ?? 0;
                ToastHelper.ShowSuccess(RootGrid, $"✓ تم التراجع عن جلسة {driverName}: حُذف {deleted} إيصال، أُعيد {unassigned} صف");
                UndoSessionIdBox.Text = "";
                await LoadBatchDetailAsync(_selectedBatchId);
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { UndoBlockBtn.IsEnabled = true; }
    }

    // ══════════════════════════════════════
    // SINGLE ORPHAN ASSIGNMENT
    // ══════════════════════════════════════

    private async void AssignSingle_Click(object sender, RoutedEventArgs e)
    {
        var selected = _batchRows.Where(r => r.isSelected && !r.isAssigned).ToList();
        if (selected.Count != 1)
        {
            ToastHelper.ShowError(RootGrid, "اختر صفاً واحداً فقط للتعيين الفردي");
            return;
        }

        if (!int.TryParse(SingleSessionIdBox.Text, out int sessionId) || sessionId <= 0)
        { ToastHelper.ShowError(RootGrid, "أدخل رقم الجلسة المستهدفة"); return; }
        if (!int.TryParse(SingleReceiptBox.Text, out int receiptNum) || receiptNum <= 0)
        { ToastHelper.ShowError(RootGrid, "أدخل رقم الإيصال"); return; }

        var dto = new
        {
            RowId = selected[0].rowId,
            TargetSessionId = sessionId,
            ReceiptNumber = receiptNum
        };

        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            var result = await _api.AssignErpSingleAsync(_selectedBatchId, dto);
            if (result != null)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم التعيين الفردي");
                SingleSessionIdBox.Text = ""; SingleReceiptBox.Text = "";
                foreach (var r in _batchRows) r.isSelected = false;
                await LoadBatchDetailAsync(_selectedBatchId);
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

    // ══════════════════════════════════════
    // COMPLETE BATCH
    // ══════════════════════════════════════

    private async void CompleteBatch_Click(object sender, RoutedEventArgs e)
    {
        // Warn about unassigned rows
        int unassigned = _batchRows.Count(r => !r.isAssigned);
        if (unassigned > 0)
        {
            var answer = MessageBox.Show(
                $"يوجد {unassigned} صف غير مُعيّن. هل تريد إتمام الدُفعة على أي حال؟",
                "تأكيد الإتمام", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes) return;
        }

        CompleteBatchBtn.IsEnabled = false;
        try
        {
            var result = await _api.CompleteErpBatchAsync(_selectedBatchId);
            if (result != null)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم إتمام الدُفعة");
                await LoadBatchesListAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { CompleteBatchBtn.IsEnabled = true; }
    }

    // ══════════════════════════════════════
    // AUTO-COMPLETE FILTERING
    // ══════════════════════════════════════

    private void SearchableComboBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Guard against re-entry (swapping ItemsSource triggers TextChanged again)
        if (_isFilteringCombo) return;
        if (sender is not ComboBox cmb) return;
        if (e.OriginalSource is not TextBox tb) return;

        string searchText = tb.Text;
        int caretPos = tb.CaretIndex;

        _isFilteringCombo = true;
        try
        {
            if (cmb == BlockDriverCombo)
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    cmb.ItemsSource = _allDrivers;
                    cmb.IsDropDownOpen = false;
                }
                else
                {
                    var filtered = _allDrivers
                        .Where(d => d.fullName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    cmb.ItemsSource = filtered;
                    cmb.IsDropDownOpen = true;
                }
            }
            else if (cmb == BlockRouteArea)
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    cmb.ItemsSource = _allRoutes;
                    cmb.IsDropDownOpen = false;
                }
                else
                {
                    var filtered = _allRoutes
                        .Where(r => r.RouteName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    cmb.ItemsSource = filtered;
                    cmb.IsDropDownOpen = true;
                }
            }

            // Restore the text the user typed (swapping ItemsSource clears it)
            tb.Text = searchText;
            tb.CaretIndex = caretPos;
        }
        finally
        {
            _isFilteringCombo = false;
        }
    }
}

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
            var result = await _api.CreateErpBatchAsync(_previewData);
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
                BlockDriverCombo.ItemsSource = drivers.Select(d => new DriverComboItem
                {
                    driverId = d["driverId"]?.Value<int>() ?? 0,
                    fullName = d["fullName"]?.ToString() ?? ""
                }).ToList();
            }
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
                    merchantName = r["merchantName"]?.ToString() ?? "",
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
    // BLOCK ASSIGNMENT — Sheet-Order Sort
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
        if (BlockSessionDate.SelectedDate == null)
        { ToastHelper.ShowError(RootGrid, "اختر تاريخ الجلسة"); return; }

        // ╔══════════════════════════════════════════════════╗
        // ║  CRITICAL: Sort by original sheet order (rowIndex) ║
        // ║  NOT by click/selection order.                      ║
        // ║  This is the data-correctness requirement.          ║
        // ╚══════════════════════════════════════════════════╝
        var sortedBySheetOrder = selected.OrderBy(r => r.rowIndex).ToList();
        var rowIds = sortedBySheetOrder.Select(r => r.rowId).ToList();

        var dto = new
        {
            DriverId = driverId,
            SessionDate = BlockSessionDate.SelectedDate!.Value.ToString("yyyy-MM-dd"),
            StartReceiptNumber = startReceipt,
            RouteArea = BlockRouteArea.Text.Trim(),
            RowIds = rowIds
        };

        AssignBlockBtn.IsEnabled = false;
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            var result = await _api.AssignErpBlockAsync(_selectedBatchId, dto);
            if (result != null)
            {
                int assigned = sortedBySheetOrder.Count;
                ToastHelper.ShowSuccess(RootGrid, $"✓ تم تعيين {assigned} صف (إيصالات {startReceipt}–{startReceipt + assigned - 1})");
                // Clear selection and reload
                foreach (var r in _batchRows) r.isSelected = false;
                BlockStartReceipt.Text = "";
                BlockRouteArea.Text = "";
                await LoadBatchDetailAsync(_selectedBatchId);
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally
        {
            AssignBlockBtn.IsEnabled = true;
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
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
    // RECONCILE & COMPLETE
    // ══════════════════════════════════════

    private async void ReconcileBatch_Click(object sender, RoutedEventArgs e)
    {
        if (!decimal.TryParse(LedgerTotalBox.Text, out decimal ledger))
        { ToastHelper.ShowError(RootGrid, "أدخل إجمالي دفتر الأستاذ"); return; }
        if (!decimal.TryParse(ActualCashTotalBox.Text, out decimal actual))
        { ToastHelper.ShowError(RootGrid, "أدخل النقد الفعلي"); return; }

        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            var result = await _api.ReconcileErpBatchAsync(_selectedBatchId, ledger, actual);
            if (result != null)
            {
                string msg = result["message"]?.ToString() ?? "تمت التسوية";
                ToastHelper.ShowSuccess(RootGrid, msg);
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

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
}

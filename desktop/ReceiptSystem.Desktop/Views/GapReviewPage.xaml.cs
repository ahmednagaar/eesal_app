using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class GapReviewPage : UserControl
{
    private readonly ApiClient _api;
    private int _selectedGapId;

    public GapReviewPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        Loaded += async (_, _) => await LoadAllAsync();
    }

    private async Task LoadAllAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        try
        {
            await LoadSummaryAsync();
            await LoadGapsAsync();
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private async Task LoadSummaryAsync()
    {
        try
        {
            var json = await _api.GetGapSummaryJsonAsync();
            if (json == null) return;
            var summary = JObject.Parse(json);
            StatOpen.Text = (summary["open"]?.Value<int>() ?? 0).ToString();
            StatInvestigation.Text = (summary["underInvestigation"]?.Value<int>() ?? 0).ToString();
            StatResolved.Text = (summary["resolved"]?.Value<int>() ?? 0).ToString();
            StatTotal.Text = (summary["total"]?.Value<int>() ?? 0).ToString();
        }
        catch { }
    }

    private async Task LoadGapsAsync()
    {
        try
        {
            string? status = null;
            if (StatusFilter.SelectedItem is ComboBoxItem ci && ci.Tag is string tag && !string.IsNullOrEmpty(tag))
                status = tag;

            var json = await _api.GetGapsJsonAsync(status);
            if (json == null) { GapsGrid.ItemsSource = null; EmptyState.Visibility = Visibility.Visible; return; }

            var gaps = JArray.Parse(json);
            if (gaps.Count == 0) { GapsGrid.ItemsSource = null; EmptyState.Visibility = Visibility.Visible; return; }

            var rows = gaps.Select(g =>
            {
                var st = g["status"]?.ToString() ?? "Open";
                return new
                {
                    gapId = g["gapId"]?.Value<int>() ?? 0,
                    missingNumber = g["missingReceiptNumber"]?.Value<int>() ?? 0,
                    driverName = g["driverName"]?.ToString() ?? "",
                    detectedAt = g["detectedAt"]?.ToString()?.Substring(0, 10) ?? "",
                    statusDisplay = st switch
                    {
                        "Open" => "🔴 مفتوحة",
                        "UnderInvestigation" => "🟡 قيد التحقيق",
                        "Resolved" => "🟢 تم الحل",
                        "Explained" => "🔵 مُوضّحة",
                        _ => st
                    },
                    resolution = g["resolution"]?.ToString() ?? "",
                    resolvedBy = g["resolvedByName"]?.ToString() ?? "",
                    canResolve = st == "Open" || st == "UnderInvestigation" ? Visibility.Visible : Visibility.Collapsed
                };
            }).ToList();

            GapsGrid.ItemsSource = rows;
            EmptyState.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, ErrorMessageHelper.GetArabicMessage(ex));
        }
    }

    private async void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        await LoadGapsAsync();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadAllAsync();
    }

    // ── Resolve ──
    private void ResolveRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int gapId)
        {
            _selectedGapId = gapId;
            ResolveGapInfo.Text = $"الفجوة رقم {gapId}";
            ResolveResolution.Text = "";
            ResolveDialog.Visibility = Visibility.Visible;
            ResolveResolution.Focus();
        }
    }

    private async void ResolveConfirm_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ResolveResolution.Text))
        {
            ToastHelper.ShowError(RootGrid, "يرجى إدخال التوضيح أو السبب");
            return;
        }

        string status = "Resolved";
        if (ResolveStatus.SelectedItem is ComboBoxItem ci && ci.Tag is string tag)
            status = tag;

        var btn = (Button)sender; btn.IsEnabled = false;
        try
        {
            bool ok = await _api.ResolveGapAsync(_selectedGapId, status, ResolveResolution.Text.Trim());
            if (ok)
            {
                ToastHelper.ShowSuccess(RootGrid, "✓ تم حل الفجوة");
                ResolveDialog.Visibility = Visibility.Collapsed;
                await LoadAllAsync();
            }
        }
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, ex.Message); }
        finally { btn.IsEnabled = true; }
    }

    private void ResolveCancel_Click(object sender, RoutedEventArgs e)
    {
        ResolveDialog.Visibility = Visibility.Collapsed;
    }
}

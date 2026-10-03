using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AjalDailyPage : UserControl
{
    private readonly ApiClient _api;

    public AjalDailyPage(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        dpDate.SelectedDate = DateTime.Today;
    }

    private async void DpDate_Changed(object? sender, SelectionChangedEventArgs e) => await LoadDataAsync();
    private async void BtnLoad_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

    private async Task LoadDataAsync()
    {
        if (dpDate.SelectedDate == null) return;
        pnlSessions.Children.Clear();

        try
        {
            var json = await _api.GetAjalSessionsJsonAsync(dpDate.SelectedDate.Value);
            if (json == null) return;

            var sessions = JArray.Parse(json);
            if (sessions.Count == 0)
            {
                pnlSessions.Children.Add(new TextBlock
                {
                    Text = "لا توجد جلسات لهذا التاريخ",
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    FontSize = 16, Margin = new Thickness(16),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                return;
            }

            foreach (var session in sessions)
            {
                string routeName = session["routeName"]?.ToString() ?? "—";
                string driverName = session["driverName"]?.ToString() ?? "بدون سائق";
                int entryCount = (int)(session["entryCount"] ?? 0);
                int reviewedCount = (int)(session["reviewedCount"] ?? 0);
                decimal totalAmount = (decimal)(session["totalAmount"] ?? 0);
                int sessionId = (int)(session["sessionId"] ?? 0);

                // Session card
                var card = new Border
                {
                    Background = (Brush)FindResource("BgCardBrush"),
                    CornerRadius = new CornerRadius(12),
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = new Thickness(20, 16, 20, 16)
                };

                var stack = new StackPanel();

                // Header: Route + Driver
                var header = new Grid();
                header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var title = new TextBlock
                {
                    FontSize = 18, FontWeight = FontWeights.Bold,
                    Foreground = (Brush)FindResource("AccentPrimaryBrush")
                };
                title.Inlines.Add(new System.Windows.Documents.Run($"🛣️ {routeName}"));
                title.Inlines.Add(new System.Windows.Documents.Run($"  —  🚐 {driverName}")
                    { Foreground = (Brush)FindResource("TextSecondaryBrush"), FontSize = 15 });
                Grid.SetColumn(title, 0);
                header.Children.Add(title);

                var stats = new TextBlock
                {
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center
                };
                stats.Inlines.Add(new System.Windows.Documents.Run($"📦 {entryCount} فاتورة")
                    { Foreground = (Brush)FindResource("TextPrimaryBrush") });
                stats.Inlines.Add(new System.Windows.Documents.Run($"  |  ✅ {reviewedCount}/{entryCount}")
                    { Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)) });
                stats.Inlines.Add(new System.Windows.Documents.Run($"  |  💰 {totalAmount:N0}")
                    { Foreground = (Brush)FindResource("TextSecondaryBrush") });
                Grid.SetColumn(stats, 1);
                header.Children.Add(stats);

                stack.Children.Add(header);

                // Load detail button
                var btnDetail = new Button
                {
                    Content = "📋 عرض التفاصيل",
                    Style = (Style)FindResource("PrimaryButton"),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 8, 0, 0),
                    MinWidth = 130,
                    Tag = sessionId
                };
                btnDetail.Click += BtnDetail_Click;
                stack.Children.Add(btnDetail);

                // Detail panel (hidden initially)
                var detailPanel = new StackPanel { Tag = $"detail_{sessionId}", Visibility = Visibility.Collapsed, Margin = new Thickness(0, 8, 0, 0) };
                stack.Children.Add(detailPanel);

                card.Child = stack;
                pnlSessions.Children.Add(card);
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, $"خطأ: {ex.Message}");
        }
    }

    private async void BtnDetail_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        int sessionId = (int)btn.Tag;

        // Find the detail panel
        var parent = btn.Parent as StackPanel;
        var detailPanel = parent?.Children.OfType<StackPanel>()
            .FirstOrDefault(p => p.Tag?.ToString() == $"detail_{sessionId}");
        if (detailPanel == null) return;

        if (detailPanel.Visibility == Visibility.Visible)
        {
            detailPanel.Visibility = Visibility.Collapsed;
            btn.Content = "📋 عرض التفاصيل";
            return;
        }

        try
        {
            var json = await _api.GetAjalSessionDetailJsonAsync(sessionId);
            if (json == null) return;

            var detail = JObject.Parse(json);
            detailPanel.Children.Clear();

            var entries = detail["entries"] as JArray;
            if (entries == null || entries.Count == 0)
            {
                detailPanel.Children.Add(new TextBlock
                {
                    Text = "لا توجد فواتير", FontSize = 14,
                    Foreground = (Brush)FindResource("TextSecondaryBrush")
                });
            }
            else
            {
                foreach (var entry in entries)
                {
                    string invoiceNum = entry["invoiceNumber"]?.ToString() ?? "";
                    string merchantName = entry["merchantName"]?.ToString() ?? "";
                    decimal? amount = entry["amount"]?.Type == JTokenType.Null ? null : (decimal?)entry["amount"];
                    bool isReviewed = (bool)(entry["isReviewed"] ?? false);

                    var row = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)),
                        CornerRadius = new CornerRadius(6),
                        Margin = new Thickness(0, 2, 0, 2),
                        Padding = new Thickness(12, 6, 12, 6)
                    };

                    var rowGrid = new Grid();
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

                    rowGrid.Children.Add(MakeCell(0, invoiceNum, FontWeights.Bold));
                    rowGrid.Children.Add(MakeCell(1, merchantName, FontWeights.Normal));
                    rowGrid.Children.Add(MakeCell(2, amount?.ToString("N0") ?? "—", FontWeights.Normal));
                    rowGrid.Children.Add(MakeCell(3, isReviewed ? "✅" : "⏳", FontWeights.Normal));

                    row.Child = rowGrid;
                    detailPanel.Children.Add(row);
                }
            }

            detailPanel.Visibility = Visibility.Visible;
            btn.Content = "🔽 إخفاء التفاصيل";
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, $"خطأ: {ex.Message}");
        }
    }

    private TextBlock MakeCell(int col, string text, FontWeight weight)
    {
        var tb = new TextBlock
        {
            Text = text, FontSize = 14, FontWeight = weight,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = (Brush)FindResource("TextPrimaryBrush")
        };
        Grid.SetColumn(tb, col);
        return tb;
    }

    private async void BtnExport_Click(object sender, RoutedEventArgs e)
    {
        if (dpDate.SelectedDate == null) return;
        var dlg = new SaveFileDialog { Filter = "Excel|*.xlsx", FileName = $"ajal_daily_{dpDate.SelectedDate:yyyyMMdd}.xlsx" };
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
        catch (Exception ex) { ToastHelper.ShowError(RootGrid, $"خطأ: {ex.Message}"); }
    }
}

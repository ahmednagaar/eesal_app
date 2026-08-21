using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class DriverPerformancePage : UserControl
{
    private readonly ApiClient _api;

    public DriverPerformancePage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        DateFrom.SelectedDate = DateTime.Today.AddDays(-30);
        DateTo.SelectedDate = DateTime.Today;
    }

    private async void LoadPerformance_Click(object sender, RoutedEventArgs e)
    {
        await LoadPerformanceAsync();
    }

    private async Task LoadPerformanceAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        DriverCards.Children.Clear();
        try
        {
            string? from = DateFrom.SelectedDate?.ToString("yyyy-MM-dd");
            string? to = DateTo.SelectedDate?.ToString("yyyy-MM-dd");

            var json = await _api.GetDriverPerformanceJsonAsync(from, to);
            if (json == null) return;

            var drivers = JArray.Parse(json);
            if (drivers.Count == 0)
            {
                DriverCards.Children.Add(new TextBlock
                {
                    Text = "لا توجد بيانات أداء لهذه الفترة",
                    FontSize = 14, Foreground = (Brush)FindResource("TextMutedBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 40, 0, 0)
                });
                return;
            }

            // Sort by gap rate descending — worst (needs attention) first
            var sorted = drivers.OrderByDescending(d => d["gapRate"]?.Value<double>() ?? 0).ToList();

            foreach (var driver in sorted)
            {
                DriverCards.Children.Add(BuildDriverCard(driver));
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

    private Border BuildDriverCard(JToken driver)
    {
        var card = new Border
        {
            Background = (Brush)FindResource("BgCardBrush"),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20, 16, 20, 16),
            Margin = new Thickness(0, 0, 0, 12)
        };

        var mainGrid = new Grid();
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

        // Left: Driver info
        var infoStack = new StackPanel { Margin = new Thickness(0, 0, 16, 0) };

        infoStack.Children.Add(new TextBlock
        {
            Text = driver["driverName"]?.ToString() ?? "",
            FontSize = 17, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 8)
        });

        var totalSessions = driver["totalSessions"]?.Value<int>() ?? 0;
        var totalReceipts = driver["totalReceipts"]?.Value<int>() ?? 0;
        var totalAmount = driver["totalAmount"]?.Value<decimal>() ?? 0;
        var avgPerDay = driver["averagePerDay"]?.Value<decimal>() ?? 0;
        var missingCount = driver["missingReceiptsCount"]?.Value<int>() ?? 0;
        var gapRate = driver["gapRate"]?.Value<double>() ?? 0;

        infoStack.Children.Add(BuildStatLine("الجلسات", totalSessions.ToString()));
        infoStack.Children.Add(BuildStatLine("الإيصالات", totalReceipts.ToString()));
        infoStack.Children.Add(BuildStatLine("الإجمالي", $"{totalAmount:N2} جنيه"));
        infoStack.Children.Add(BuildStatLine("المعدل اليومي", $"{avgPerDay:N2} جنيه"));

        Grid.SetColumn(infoStack, 0);
        mainGrid.Children.Add(infoStack);

        // Right: Performance metrics
        var metricsStack = new StackPanel();

        // Gap Rate bar — the key metric for this screen
        var gapRatePanel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
        gapRatePanel.Children.Add(new TextBlock
        {
            Text = $"معدل الفجوات — {gapRate:F1}%",
            FontSize = 12, Foreground = (Brush)FindResource("TextSecondaryBrush"), Margin = new Thickness(0, 0, 0, 4)
        });

        var barBg = new Border
        {
            Background = (Brush)FindResource("BgInputBrush"),
            CornerRadius = new CornerRadius(4), Height = 12
        };
        // Color thresholds: green <2%, yellow 2-5%, red >5%
        var barColor = gapRate < 2 ? FindResource("AccentSuccessBrush")
            : gapRate <= 5 ? FindResource("AccentWarningBrush")
            : FindResource("AccentDangerBrush");
        var barFill = new Border
        {
            Background = (Brush)barColor,
            CornerRadius = new CornerRadius(4), Height = 12,
            HorizontalAlignment = HorizontalAlignment.Right,
            Width = 0 // set via measure
        };

        var barGrid = new Grid { Height = 12 };
        barGrid.Children.Add(barBg);
        barGrid.Children.Add(barFill);

        // Scale bar to a 10% max for visual clarity (a 5% gap rate fills half the bar)
        barGrid.Loaded += (s, e) =>
        {
            double maxW = barGrid.ActualWidth;
            barFill.Width = maxW * Math.Min(gapRate / 10.0, 1.0);
        };

        gapRatePanel.Children.Add(barGrid);
        metricsStack.Children.Add(gapRatePanel);

        // Missing receipts indicator
        if (missingCount > 0)
        {
            var missingBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x33, 0xEF, 0x44, 0x44)),
                CornerRadius = new CornerRadius(6), Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, 0, 0, 8)
            };
            missingBorder.Child = new TextBlock
            {
                Text = $"⚠️ {missingCount} إيصال مفقود خلال الفترة",
                FontSize = 12, Foreground = (Brush)FindResource("AccentDangerBrush")
            };
            metricsStack.Children.Add(missingBorder);
        }
        else
        {
            metricsStack.Children.Add(new TextBlock
            {
                Text = "✓ لا توجد إيصالات مفقودة",
                FontSize = 12, Foreground = (Brush)FindResource("AccentSuccessBrush"),
                Margin = new Thickness(0, 0, 0, 8)
            });
        }

        // Collection summary
        var summaryBorder = new Border
        {
            Background = (Brush)FindResource("BgInputBrush"),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 8, 12, 8)
        };
        var summaryStack = new StackPanel();
        var daysActive = driver["daysActive"]?.Value<int>() ?? 0;
        var highestDay = driver["highestDayAmount"]?.Value<decimal>() ?? 0;
        summaryStack.Children.Add(BuildStatLine("أيام العمل", daysActive.ToString()));
        summaryStack.Children.Add(BuildStatLine("أعلى يوم", $"{highestDay:N2} جنيه"));
        summaryBorder.Child = summaryStack;
        metricsStack.Children.Add(summaryBorder);

        Grid.SetColumn(metricsStack, 1);
        mainGrid.Children.Add(metricsStack);

        card.Child = mainGrid;
        return card;
    }

    private StackPanel BuildStatLine(string label, string value)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
        panel.Children.Add(new TextBlock
        {
            Text = $"{label}: ",
            FontSize = 12, Foreground = (Brush)FindResource("TextSecondaryBrush")
        });
        panel.Children.Add(new TextBlock
        {
            Text = value,
            FontSize = 13, FontWeight = FontWeights.SemiBold
        });
        return panel;
    }
}

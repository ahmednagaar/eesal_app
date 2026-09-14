using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class DailyReportPage : UserControl
{
    private readonly ApiClient _api;
    private JObject? _reportData;

    public DailyReportPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        ReportDate.SelectedDate = DateTime.Today;
    }

    private async void LoadReport_Click(object sender, RoutedEventArgs e)
    {
        await LoadReportAsync();
    }

    private async Task LoadReportAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        DriverSections.Children.Clear();
        try
        {
            string? date = ReportDate.SelectedDate?.ToString("yyyy-MM-dd");
            var json = await _api.GetDailyReportJsonAsync(date);
            if (json == null) { PrintBtn.IsEnabled = false; return; }

            _reportData = JObject.Parse(json);

            // Summary
            StatDrivers.Text = (_reportData["totalDrivers"]?.Value<int>() ?? 0).ToString();
            StatReceipts.Text = (_reportData["totalReceipts"]?.Value<int>() ?? 0).ToString();
            StatTotal.Text = (_reportData["totalAmount"]?.Value<decimal>() ?? 0).ToString("N2");
            StatMissing.Text = (_reportData["missingReceiptsCount"]?.Value<int>() ?? 0).ToString();

            // Per-driver sections
            var drivers = _reportData["driverReports"] as JArray;
            if (drivers != null)
            {
                foreach (var driver in drivers)
                {
                    var section = BuildDriverSection(driver);
                    DriverSections.Children.Add(section);
                }
            }

            PrintBtn.IsEnabled = true;
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

    private Border BuildDriverSection(JToken driver)
    {
        var border = new Border
        {
            Background = (Brush)FindResource("BgCardBrush"),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(0, 0, 0, 12)
        };

        var stack = new StackPanel();

        // Driver header
        var headerGrid = new Grid();
        var nameText = new TextBlock
        {
            Text = driver["driverName"]?.ToString() ?? "",
            FontSize = 15, FontWeight = FontWeights.Bold
        };
        var infoText = new TextBlock
        {
            Text = $"{driver["routeArea"]} — {driver["receiptCount"]} إيصال — {(driver["totalAmount"]?.Value<decimal>() ?? 0):N2} جنيه",
            FontSize = 12, Foreground = (Brush)FindResource("TextSecondaryBrush"),
            HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center
        };
        headerGrid.Children.Add(nameText);
        headerGrid.Children.Add(infoText);
        stack.Children.Add(headerGrid);

        // Receipts table
        var receipts = driver["receipts"] as JArray;
        if (receipts != null && receipts.Count > 0)
        {
            var grid = new DataGrid
            {
                AutoGenerateColumns = false, IsReadOnly = true, CanUserAddRows = false,
                Background = Brushes.Transparent, BorderThickness = new Thickness(0),
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                FontSize = 12, GridLinesVisibility = DataGridGridLinesVisibility.None,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                Margin = new Thickness(0, 8, 0, 0), MaxHeight = 200
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "رقم", Binding = new System.Windows.Data.Binding("num"), Width = new DataGridLength(80) });
            grid.Columns.Add(new DataGridTextColumn { Header = "التاجر", Binding = new System.Windows.Data.Binding("merchant"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            grid.Columns.Add(new DataGridTextColumn { Header = "المبلغ", Binding = new System.Windows.Data.Binding("amount"), Width = new DataGridLength(100) });

            var rows = receipts.Select(r => new
            {
                num = r["receiptNumber"]?.Value<int>() ?? 0,
                merchant = r["merchantName"]?.ToString() ?? "",
                amount = (r["amount"]?.Value<decimal>() ?? 0).ToString("N2")
            }).ToList();
            grid.ItemsSource = rows;
            stack.Children.Add(grid);
        }

        // Missing receipts
        var missing = driver["missingReceipts"] as JArray;
        if (missing != null && missing.Count > 0)
        {
            var missingBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x33, 0xEF, 0x44, 0x44)),
                CornerRadius = new CornerRadius(6), Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, 8, 0, 0)
            };
            var nums = missing.Select(n => n.Value<int>()).ToList();
            missingBorder.Child = new TextBlock
            {
                Text = $"⚠️ إيصالات مفقودة: {string.Join(", ", nums)}",
                FontSize = 12, Foreground = (Brush)FindResource("AccentDangerBrush"), TextWrapping = TextWrapping.Wrap
            };
            stack.Children.Add(missingBorder);
        }

        border.Child = stack;
        return border;
    }

    // ═══ PRINT ═══
    private void Print_Click(object sender, RoutedEventArgs e)
    {
        if (_reportData == null) return;
        PrintDailyReport(_reportData);
    }

    private void PrintDailyReport(JObject report)
    {
        var doc = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 11,
            FlowDirection = FlowDirection.RightToLeft,
            PagePadding = new Thickness(30),
            ColumnWidth = double.MaxValue
        };

        // Title
        doc.Blocks.Add(new Paragraph(new Run("التقرير اليومي للإيصالات"))
        {
            FontSize = 18, FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 4)
        });

        // Date & summary
        string date = ReportDate.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
        doc.Blocks.Add(new Paragraph(new Run(
            $"التاريخ: {date}    |    السائقون: {report["totalDrivers"]}    |    الإيصالات: {report["totalReceipts"]}    |    الإجمالي: {report["totalAmount"]:N2} جنيه"))
        {
            FontSize = 10, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 16)
        });

        // Per-driver tables
        var drivers = report["drivers"] as JArray;
        if (drivers != null)
        {
            foreach (var driver in drivers)
            {
                // Driver header
                doc.Blocks.Add(new Paragraph(new Run(
                    $"{driver["driverName"]} — {driver["routeArea"]} — {driver["receiptCount"]} إيصال — {(driver["totalAmount"]?.Value<decimal>() ?? 0):N2} جنيه"))
                {
                    FontSize = 12, FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 12, 0, 4),
                    Background = Brushes.LightGray, Padding = new Thickness(6)
                });

                // Receipts table
                var receipts = driver["receipts"] as JArray;
                if (receipts != null && receipts.Count > 0)
                {
                    var table = new Table { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
                    table.Columns.Add(new TableColumn { Width = new GridLength(70) });
                    table.Columns.Add(new TableColumn { Width = new GridLength(250) });
                    table.Columns.Add(new TableColumn { Width = new GridLength(100) });

                    var hGroup = new TableRowGroup();
                    var hRow = new TableRow { Background = Brushes.LightGray };
                    hRow.Cells.Add(PrintCell("رقم الإيصال", true));
                    hRow.Cells.Add(PrintCell("التاجر", true));
                    hRow.Cells.Add(PrintCell("المبلغ", true));
                    hGroup.Rows.Add(hRow);
                    table.RowGroups.Add(hGroup);

                    var bGroup = new TableRowGroup();
                    foreach (var r in receipts)
                    {
                        var row = new TableRow();
                        row.Cells.Add(PrintCell(r["receiptNumber"]?.ToString() ?? ""));
                        row.Cells.Add(PrintCell(r["merchantName"]?.ToString() ?? ""));
                        row.Cells.Add(PrintCell((r["amount"]?.Value<decimal>() ?? 0).ToString("N2")));
                        bGroup.Rows.Add(row);
                    }
                    table.RowGroups.Add(bGroup);
                    doc.Blocks.Add(table);
                }

                // Missing receipts
                var missing = driver["missingReceipts"] as JArray;
                if (missing != null && missing.Count > 0)
                {
                    var nums = missing.Select(n => n.Value<int>()).ToList();
                    doc.Blocks.Add(new Paragraph(new Run($"⚠️ مفقودة: {string.Join(", ", nums)}"))
                    {
                        FontSize = 10, Foreground = Brushes.Red, Margin = new Thickness(0, 2, 0, 8)
                    });
                }
            }
        }

        // Print
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
            paginator.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            printDialog.PrintDocument(paginator, "التقرير اليومي");
        }
    }

    private static TableCell PrintCell(string text, bool isHeader = false)
    {
        var para = new Paragraph(new Run(text))
        {
            FontSize = isHeader ? 10 : 9,
            FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
            TextAlignment = TextAlignment.Center, Margin = new Thickness(3)
        };
        return new TableCell(para)
        {
            BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0.5), Padding = new Thickness(3)
        };
    }
}

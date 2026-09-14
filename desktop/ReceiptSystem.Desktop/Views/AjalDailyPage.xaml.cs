using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Helpers;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Views;

public partial class AjalDailyPage : UserControl
{
    private readonly ApiClient _api;
    private JObject? _dailyData;

    public AjalDailyPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        ReportDate.SelectedDate = DateTime.Today;
        Loaded += async (_, _) => await LoadDataAsync();
    }

    private void ReportDate_Changed(object? sender, SelectionChangedEventArgs e) { }

    private async void Load_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

    private async Task LoadDataAsync()
    {
        if (ReportDate.SelectedDate == null) return;
        LoadingOverlay.Visibility = Visibility.Visible;
        RouteSections.Children.Clear();
        try
        {
            var json = await _api.GetAjalDailyJsonAsync(ReportDate.SelectedDate.Value);
            if (json == null)
            {
                StatTotal.Text = "0"; StatActive.Text = "0"; StatAmount.Text = "0.00"; StatCancelled.Text = "0";
                ExportBtn.IsEnabled = false; PrintBtn.IsEnabled = false;
                return;
            }

            _dailyData = JObject.Parse(json);

            // Summary cards
            int totalInv = _dailyData["totalInvoices"]?.Value<int>() ?? 0;
            int activeInv = _dailyData["activeInvoices"]?.Value<int>() ?? 0;
            int cancelledInv = _dailyData["cancelledInvoices"]?.Value<int>() ?? 0;
            decimal totalAmt = _dailyData["totalAmount"]?.Value<decimal>() ?? 0;

            StatTotal.Text = totalInv.ToString();
            StatActive.Text = activeInv.ToString();
            StatAmount.Text = totalAmt.ToString("N2");
            StatCancelled.Text = cancelledInv.ToString();

            // Route sections
            var routes = _dailyData["routes"] as JArray;
            if (routes != null && routes.Count > 0)
            {
                foreach (var route in routes)
                    RouteSections.Children.Add(BuildRouteSection(route));
            }
            else
            {
                RouteSections.Children.Add(new TextBlock
                {
                    Text = "لا توجد فواتير لهذا اليوم",
                    FontSize = 14, Foreground = (Brush)FindResource("TextMutedBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 40, 0, 0)
                });
            }

            ExportBtn.IsEnabled = totalInv > 0;
            PrintBtn.IsEnabled = totalInv > 0;
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

    private Border BuildRouteSection(JToken route)
    {
        var border = new Border
        {
            Background = (Brush)FindResource("BgCardBrush"),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(0, 0, 0, 12)
        };

        var stack = new StackPanel();

        // Route header
        string routeName = route["routeName"]?.ToString() ?? "غير محدد";
        int invoiceCount = route["invoiceCount"]?.Value<int>() ?? 0;
        decimal routeTotal = route["routeTotal"]?.Value<decimal>() ?? 0;

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var nameText = new TextBlock
        {
            Text = $"🚐 {routeName}",
            FontSize = 15, FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("TextPrimaryBrush")
        };
        Grid.SetColumn(nameText, 0);

        var summaryText = new TextBlock
        {
            Text = $"{invoiceCount} فاتورة — {routeTotal:N2} جنيه",
            FontSize = 12, Foreground = (Brush)FindResource("TextSecondaryBrush"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(summaryText, 1);

        headerGrid.Children.Add(nameText);
        headerGrid.Children.Add(summaryText);
        stack.Children.Add(headerGrid);

        // Invoices DataGrid
        var invoices = route["invoices"] as JArray;
        if (invoices != null && invoices.Count > 0)
        {
            var dg = new DataGrid
            {
                AutoGenerateColumns = false, IsReadOnly = true, CanUserAddRows = false,
                CanUserDeleteRows = false, Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                FontSize = 12, GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
                HorizontalGridLinesBrush = (Brush)FindResource("BorderSubtleBrush"),
                HeadersVisibility = DataGridHeadersVisibility.Column,
                Margin = new Thickness(0, 8, 0, 0)
            };

            dg.Columns.Add(new DataGridTextColumn { Header = "رقم الفاتورة", Binding = new Binding("invoiceNumber"), Width = new DataGridLength(120) });
            dg.Columns.Add(new DataGridTextColumn { Header = "التاجر", Binding = new Binding("merchantName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dg.Columns.Add(new DataGridTextColumn { Header = "المبلغ", Binding = new Binding("amount"), Width = new DataGridLength(100) });
            dg.Columns.Add(new DataGridTextColumn { Header = "الموظف", Binding = new Binding("callCenterEmployeeName"), Width = new DataGridLength(120) });
            dg.Columns.Add(new DataGridTextColumn { Header = "الحالة", Binding = new Binding("invoiceStatus"), Width = new DataGridLength(80) });
            dg.Columns.Add(new DataGridTextColumn { Header = "المصدر", Binding = new Binding("importSource"), Width = new DataGridLength(80) });

            var rows = invoices.Select(inv => new
            {
                ajalInvoiceId = inv["ajalInvoiceId"]?.Value<int>() ?? 0,
                invoiceNumber = inv["invoiceNumber"]?.ToString() ?? "",
                merchantName = inv["merchantName"]?.ToString() ?? "",
                merchantId = inv["merchantId"]?.Value<int>() ?? 0,
                amount = (inv["amount"]?.Value<decimal>() ?? 0).ToString("N2"),
                rawAmount = inv["amount"]?.Value<decimal>() ?? 0,
                callCenterEmployeeName = inv["callCenterEmployeeName"]?.ToString() ?? "—",
                invoiceStatus = inv["invoiceStatus"]?.ToString() == "Cancelled" ? "ملغاة" :
                                inv["invoiceStatus"]?.ToString() == "Modified" ? "معدّلة" : "نشطة",
                rawStatus = inv["invoiceStatus"]?.ToString() ?? "Active",
                importSource = inv["importSource"]?.ToString() == "Excel" ? "إكسل" : "يدوي",
                notes = inv["notes"]?.ToString() ?? ""
            }).ToList();

            dg.ItemsSource = rows;

            // Row styling: cancelled = dim / red
            dg.LoadingRow += (s, e) =>
            {
                dynamic? item = e.Row.Item;
                if (item?.rawStatus == "Cancelled")
                {
                    e.Row.Foreground = (Brush)FindResource("AccentDangerBrush");
                    e.Row.Opacity = 0.6;
                }
            };

            // Context menu
            var contextMenu = new ContextMenu();
            var editItem = new MenuItem { Header = "✏️ تعديل الفاتورة" };
            editItem.Click += (s, e) => EditInvoice_Click(dg);
            var cancelItem = new MenuItem { Header = "❌ إلغاء الفاتورة" };
            cancelItem.Click += (s, e) => CancelInvoice_Click(dg);
            contextMenu.Items.Add(editItem);
            contextMenu.Items.Add(cancelItem);
            dg.ContextMenu = contextMenu;

            stack.Children.Add(dg);
        }

        border.Child = stack;
        return border;
    }

    // ══════════════════════════════════════
    // Edit Invoice
    // ══════════════════════════════════════

    private async void EditInvoice_Click(DataGrid dg)
    {
        if (dg.SelectedItem == null) { ToastHelper.ShowError(RootGrid, "اختر فاتورة أولاً"); return; }
        dynamic row = dg.SelectedItem;
        if (row.rawStatus == "Cancelled") { ToastHelper.ShowError(RootGrid, "لا يمكن تعديل فاتورة ملغاة"); return; }

        int id = row.ajalInvoiceId;
        decimal currentAmount = row.rawAmount;
        string currentEmployee = row.callCenterEmployeeName == "—" ? "" : row.callCenterEmployeeName;

        // Simple edit dialog
        var dlg = new Window
        {
            Title = $"تعديل فاتورة {row.invoiceNumber}",
            Width = 400, Height = 300, WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this), FlowDirection = FlowDirection.RightToLeft,
            Background = (Brush)FindResource("BgDeepBrush"),
            Foreground = (Brush)FindResource("TextPrimaryBrush")
        };

        var sp = new StackPanel { Margin = new Thickness(20) };
        sp.Children.Add(new TextBlock { Text = "المبلغ:", FontSize = 13, Margin = new Thickness(0, 0, 0, 4) });
        var amtBox = new TextBox { Text = currentAmount.ToString("F2"), FontSize = 14, Padding = new Thickness(8, 6, 8, 6) };
        sp.Children.Add(amtBox);

        sp.Children.Add(new TextBlock { Text = "اسم الموظف:", FontSize = 13, Margin = new Thickness(0, 12, 0, 4) });
        var empBox = new TextBox { Text = currentEmployee, FontSize = 14, Padding = new Thickness(8, 6, 8, 6) };
        sp.Children.Add(empBox);

        sp.Children.Add(new TextBlock { Text = "ملاحظة التعديل:", FontSize = 13, Margin = new Thickness(0, 12, 0, 4) });
        var noteBox = new TextBox { FontSize = 14, Padding = new Thickness(8, 6, 8, 6) };
        sp.Children.Add(noteBox);

        var saveBtn = new Button
        {
            Content = "💾 حفظ التعديل", Style = (Style)FindResource("PrimaryButton"),
            Padding = new Thickness(16, 8, 16, 8), FontSize = 14, Margin = new Thickness(0, 16, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        sp.Children.Add(saveBtn);
        dlg.Content = sp;

        saveBtn.Click += async (s, ev) =>
        {
            if (!decimal.TryParse(amtBox.Text, out decimal newAmount))
            {
                MessageBox.Show("المبلغ غير صحيح", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                string? status = newAmount != currentAmount ? "Modified" : null;
                await _api.EditAjalInvoiceAsync(id, newAmount, empBox.Text, status, noteBox.Text, null, null);
                dlg.Close();
                ToastHelper.ShowSuccess(RootGrid, "✓ تم تعديل الفاتورة");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        dlg.ShowDialog();
    }

    // ══════════════════════════════════════
    // Cancel Invoice
    // ══════════════════════════════════════

    private async void CancelInvoice_Click(DataGrid dg)
    {
        if (dg.SelectedItem == null) { ToastHelper.ShowError(RootGrid, "اختر فاتورة أولاً"); return; }
        dynamic row = dg.SelectedItem;
        if (row.rawStatus == "Cancelled") { ToastHelper.ShowError(RootGrid, "الفاتورة ملغاة بالفعل"); return; }

        int id = row.ajalInvoiceId;

        // Ask for reason
        var dlg = new Window
        {
            Title = "إلغاء فاتورة",
            Width = 400, Height = 200, WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this), FlowDirection = FlowDirection.RightToLeft,
            Background = (Brush)FindResource("BgDeepBrush"),
            Foreground = (Brush)FindResource("TextPrimaryBrush")
        };

        var sp = new StackPanel { Margin = new Thickness(20) };
        sp.Children.Add(new TextBlock { Text = $"سبب إلغاء الفاتورة {row.invoiceNumber}:", FontSize = 13, Margin = new Thickness(0, 0, 0, 8) });
        var reasonBox = new TextBox { FontSize = 14, Padding = new Thickness(8, 6, 8, 6) };
        sp.Children.Add(reasonBox);

        var cancelBtn = new Button
        {
            Content = "❌ تأكيد الإلغاء",
            Padding = new Thickness(16, 8, 16, 8), FontSize = 14, Margin = new Thickness(0, 16, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = (Brush)FindResource("AccentDangerBrush"),
            Foreground = Brushes.White
        };
        sp.Children.Add(cancelBtn);
        dlg.Content = sp;

        cancelBtn.Click += async (s, ev) =>
        {
            if (string.IsNullOrWhiteSpace(reasonBox.Text))
            {
                MessageBox.Show("يرجى إدخال سبب الإلغاء", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                await _api.CancelAjalInvoiceAsync(id, reasonBox.Text);
                dlg.Close();
                ToastHelper.ShowSuccess(RootGrid, "✓ تم إلغاء الفاتورة");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        dlg.ShowDialog();
    }

    // ══════════════════════════════════════
    // Export
    // ══════════════════════════════════════

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        if (ReportDate.SelectedDate == null) return;
        try
        {
            var bytes = await _api.ExportAjalDailyAsync(ReportDate.SelectedDate.Value);
            if (bytes == null || bytes.Length == 0)
            {
                ToastHelper.ShowError(RootGrid, "فشل التصدير — لا توجد بيانات");
                return;
            }

            var dlg = new SaveFileDialog
            {
                FileName = $"ajal_daily_{ReportDate.SelectedDate.Value:yyyyMMdd}.xlsx",
                Filter = "Excel Files|*.xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                await File.WriteAllBytesAsync(dlg.FileName, bytes);
                ToastHelper.ShowSuccess(RootGrid, $"✓ تم التصدير: {Path.GetFileName(dlg.FileName)}");
            }
        }
        catch (Exception ex)
        {
            ToastHelper.ShowError(RootGrid, ex.Message);
        }
    }

    // ══════════════════════════════════════
    // Print
    // ══════════════════════════════════════

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        if (_dailyData == null) return;

        var printDlg = new PrintDialog();
        if (printDlg.ShowDialog() != true) return;

        var doc = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 11, PagePadding = new Thickness(40),
            FlowDirection = FlowDirection.RightToLeft
        };

        // Title
        doc.Blocks.Add(new Paragraph(new Run($"سجل الآجل اليومي — {ReportDate.SelectedDate?.ToString("dd/MM/yyyy")}"))
        {
            FontSize = 16, FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 12)
        });

        // Summary
        decimal totalAmt = _dailyData["totalAmount"]?.Value<decimal>() ?? 0;
        int activeInv = _dailyData["activeInvoices"]?.Value<int>() ?? 0;
        doc.Blocks.Add(new Paragraph(new Run($"الإجمالي: {totalAmt:N2} جنيه — فواتير نشطة: {activeInv}"))
        {
            FontSize = 12, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 16)
        });

        // Per-route tables
        var routes = _dailyData["routes"] as JArray;
        if (routes != null)
        {
            foreach (var route in routes)
            {
                doc.Blocks.Add(new Paragraph(new Run($"الخط: {route["routeName"]} — {route["invoiceCount"]} فاتورة — {(route["routeTotal"]?.Value<decimal>() ?? 0):N2} جنيه"))
                {
                    FontSize = 13, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 8, 0, 4)
                });

                var invoices = route["invoices"] as JArray;
                if (invoices != null && invoices.Count > 0)
                {
                    var table = new Table { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(0.5) };
                    table.Columns.Add(new TableColumn { Width = new GridLength(100) });
                    table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
                    table.Columns.Add(new TableColumn { Width = new GridLength(80) });
                    table.Columns.Add(new TableColumn { Width = new GridLength(80) });

                    var headerGroup = new TableRowGroup();
                    var headerRow = new TableRow { Background = Brushes.LightGray };
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("رقم الفاتورة")) { FontWeight = FontWeights.Bold }));
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("التاجر")) { FontWeight = FontWeights.Bold }));
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("المبلغ")) { FontWeight = FontWeights.Bold }));
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("الحالة")) { FontWeight = FontWeights.Bold }));
                    headerGroup.Rows.Add(headerRow);

                    foreach (var inv in invoices)
                    {
                        var row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(inv["invoiceNumber"]?.ToString() ?? ""))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(inv["merchantName"]?.ToString() ?? ""))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run((inv["amount"]?.Value<decimal>() ?? 0).ToString("N2")))));
                        string statusText = inv["invoiceStatus"]?.ToString() == "Cancelled" ? "ملغاة" : "نشطة";
                        row.Cells.Add(new TableCell(new Paragraph(new Run(statusText))));
                        headerGroup.Rows.Add(row);
                    }

                    table.RowGroups.Add(headerGroup);
                    doc.Blocks.Add(table);
                }
            }
        }

        var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
        paginator.PageSize = new Size(printDlg.PrintableAreaWidth, printDlg.PrintableAreaHeight);
        printDlg.PrintDocument(paginator, "سجل الآجل اليومي");
        ToastHelper.ShowSuccess(RootGrid, "✓ تمت الطباعة");
    }
}

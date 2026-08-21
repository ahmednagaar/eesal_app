using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using ReceiptSystem.Desktop.Services;

namespace ReceiptSystem.Desktop.Helpers;

public static class PrintHelper
{
    public static async Task PrintSheetAsync(ApiClient api, int deliveryDayId, string mode)
    {
        dynamic? sheet;
        string title;

        if (mode == "loading")
        {
            sheet = await api.GetLoadingSheetAsync(deliveryDayId);
            title = "قائمة التحميل — للعمال فقط";
        }
        else
        {
            sheet = await api.GetDeliverySheetAsync(deliveryDayId);
            title = "قائمة التسليم — للسائق";
        }

        if (sheet == null)
        {
            MessageBox.Show("لا يوجد بيانات للطباعة. يرجى التأكد من وجود تجار في هذا اليوم.",
                "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Build FlowDocument for printing
        var doc = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12,
            FlowDirection = FlowDirection.RightToLeft,
            PagePadding = new Thickness(40),
            ColumnWidth = double.MaxValue
        };

        // Header
        var header = new Paragraph(new Run(title))
        {
            FontSize = 18, FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        doc.Blocks.Add(header);

        // Meta info
        string routeName = sheet.routeName?.ToString() ?? "";
        string deliveryDate = sheet.deliveryDate?.ToString() ?? "";
        string driver = sheet.assignedDriver?.ToString() ?? "";
        string preparedBy = sheet.preparedBy?.ToString() ?? "";

        var meta = new Paragraph
        {
            FontSize = 11,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16)
        };
        meta.Inlines.Add(new Run($"الخط: {routeName}    |    التاريخ: {deliveryDate}"));
        if (!string.IsNullOrEmpty(driver))
            meta.Inlines.Add(new Run($"    |    السائق: {driver}"));
        doc.Blocks.Add(meta);

        // Table
        var table = new Table { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
        table.Columns.Add(new TableColumn { Width = new GridLength(40) });   // م
        table.Columns.Add(new TableColumn { Width = new GridLength(200) });  // اسم التاجر
        table.Columns.Add(new TableColumn { Width = new GridLength(120) });  // الكمية
        table.Columns.Add(new TableColumn { Width = new GridLength(100) });  // رقم الفاتورة
        table.Columns.Add(new TableColumn { Width = new GridLength(40) });   // ✓

        // Table header
        var headerGroup = new TableRowGroup();
        var headerRow = new TableRow { Background = Brushes.LightGray };
        headerRow.Cells.Add(CreateCell("م", true));
        headerRow.Cells.Add(CreateCell("اسم التاجر", true));
        headerRow.Cells.Add(CreateCell("الكمية / البيان", true));
        headerRow.Cells.Add(CreateCell("رقم الفاتورة", true));
        headerRow.Cells.Add(CreateCell("✓", true));
        headerGroup.Rows.Add(headerRow);
        table.RowGroups.Add(headerGroup);

        // Table body
        var bodyGroup = new TableRowGroup();
        var lines = sheet.lines as JArray;
        if (lines != null)
        {
            foreach (var line in lines)
            {
                var row = new TableRow();
                row.Cells.Add(CreateCell(line["sequenceNumber"]?.ToString() ?? ""));
                row.Cells.Add(CreateCell(line["merchantName"]?.ToString() ?? ""));
                row.Cells.Add(CreateCell(line["quantity"]?.ToString() ?? ""));
                row.Cells.Add(CreateCell(line["invoiceNumber"]?.ToString() ?? ""));
                row.Cells.Add(CreateCell("☐"));
                bodyGroup.Rows.Add(row);
            }
        }
        table.RowGroups.Add(bodyGroup);
        doc.Blocks.Add(table);

        // Footer
        int totalMerchants = sheet.totalMerchants != null ? (int)sheet.totalMerchants : 0;
        string signLabel = mode == "loading" ? "توقيع رئيس العمال" : "توقيع السائق";
        var footer = new Paragraph
        {
            FontSize = 11, Margin = new Thickness(0, 20, 0, 0),
            BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 2, 0, 0),
            Padding = new Thickness(0, 12, 0, 0)
        };
        footer.Inlines.Add(new Run($"الإجمالي: {totalMerchants} تاجر"));
        footer.Inlines.Add(new Run($"          {signLabel}: _______________"));
        doc.Blocks.Add(footer);

        // Show Print Dialog
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
            paginator.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            printDialog.PrintDocument(paginator, title);
        }
    }

    private static TableCell CreateCell(string text, bool isHeader = false)
    {
        var para = new Paragraph(new Run(text))
        {
            FontSize = isHeader ? 11 : 10,
            FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(4)
        };
        return new TableCell(para)
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(0.5),
            Padding = new Thickness(4)
        };
    }
}

using ClosedXML.Excel;

var wb = new XLWorkbook();
var ws = wb.Worksheets.Add("Sheet1");
ws.Cell(1, 1).Value = "اسم التاجر";
ws.Cell(1, 2).Value = "المبلغ";

var merchants = new[] {
    ("Merchant_A", 500m), ("Merchant_B", 350m), ("Merchant_C", 200m),
    ("Merchant_D", 150m), ("Merchant_E", 400m), ("Merchant_F", 300m),
    ("Merchant_G", 250m), ("Merchant_H", 180m), ("Merchant_I", 120m),
    ("Merchant_J", 550m)
};

for (int i = 0; i < merchants.Length; i++)
{
    ws.Cell(i + 2, 1).Value = merchants[i].Item1;
    ws.Cell(i + 2, 2).Value = merchants[i].Item2;
}

var path = Path.Combine("..", "ReceiptSystem.API", "test_erp.xlsx");
wb.SaveAs(path);
Console.WriteLine($"Created {Path.GetFullPath(path)} with {merchants.Length} rows");

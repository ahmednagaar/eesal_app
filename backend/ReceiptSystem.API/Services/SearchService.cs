using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;

namespace ReceiptSystem.API.Services;

public class SearchService
{
    private readonly AppDbContext _db;

    public SearchService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SearchReceiptsResponseDto> SearchReceiptsAsync(SearchReceiptsFilterDto filters)
    {
        var query = _db.Receipts
            .Include(r => r.Merchant)
            .Include(r => r.Driver)
            .Include(r => r.Session)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filters.MerchantName))
            query = query.Where(r => r.Merchant.MerchantName.Contains(filters.MerchantName));

        if (filters.DriverId.HasValue)
            query = query.Where(r => r.DriverId == filters.DriverId.Value);

        if (!string.IsNullOrWhiteSpace(filters.RouteArea))
            query = query.Where(r => r.Session.RouteArea.Contains(filters.RouteArea));

        if (filters.DateFrom.HasValue)
            query = query.Where(r => r.CollectionDate >= filters.DateFrom.Value.Date);

        if (filters.DateTo.HasValue)
            query = query.Where(r => r.CollectionDate <= filters.DateTo.Value.Date);

        if (filters.ReceiptNumber.HasValue)
            query = query.Where(r => r.ReceiptNumber == filters.ReceiptNumber.Value);

        if (filters.AmountMin.HasValue)
            query = query.Where(r => r.Amount >= filters.AmountMin.Value);

        if (filters.AmountMax.HasValue)
            query = query.Where(r => r.Amount <= filters.AmountMax.Value);

        if (!string.IsNullOrWhiteSpace(filters.ImportSource))
            query = query.Where(r => r.ImportSource == filters.ImportSource);

        if (filters.WithoutReceipt.HasValue)
            query = query.Where(r => r.IsWithoutReceipt == filters.WithoutReceipt.Value);

        // Get totals before pagination
        var totalCount = await query.CountAsync();
        var totalAmount = totalCount > 0 ? await query.SumAsync(r => r.Amount) : 0;

        // Paginate
        var results = await query
            .OrderByDescending(r => r.CollectionDate)
            .ThenByDescending(r => r.ReceiptNumber)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(r => new SearchReceiptResultDto
            {
                ReceiptId = r.ReceiptId,
                ReceiptNumber = r.ReceiptNumber,
                MerchantName = r.Merchant.MerchantName,
                DriverName = r.Driver.FullName,
                RouteArea = r.Session.RouteArea,
                CollectionDate = r.CollectionDate,
                Amount = r.Amount,
                ImportSource = r.ImportSource,
                IsWithoutReceipt = r.IsWithoutReceipt,
                SessionId = r.SessionId
            })
            .ToListAsync();

        return new SearchReceiptsResponseDto
        {
            TotalCount = totalCount,
            Page = filters.Page,
            Results = results,
            Totals = new SearchResultTotalsDto
            {
                TotalAmount = totalAmount,
                ReceiptCount = totalCount
            }
        };
    }

    public async Task<byte[]> ExportSearchResultsAsync(SearchReceiptsFilterDto filters)
    {
        // Remove pagination for export
        filters.Page = 1;
        filters.PageSize = 10000;
        var searchResult = await SearchReceiptsAsync(filters);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("نتائج البحث");

        // Headers
        ws.Cell(1, 1).Value = "رقم الإيصال";
        ws.Cell(1, 2).Value = "اسم التاجر";
        ws.Cell(1, 3).Value = "السائق";
        ws.Cell(1, 4).Value = "الخط";
        ws.Cell(1, 5).Value = "التاريخ";
        ws.Cell(1, 6).Value = "المبلغ";
        ws.Cell(1, 7).Value = "المصدر";
        ws.Cell(1, 8).Value = "بدون إيصال";

        // Style header
        var headerRange = ws.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#667eea");
        headerRange.Style.Font.FontColor = XLColor.White;

        // Data rows
        for (int i = 0; i < searchResult.Results.Count; i++)
        {
            var r = searchResult.Results[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = r.ReceiptNumber;
            ws.Cell(row, 2).Value = r.MerchantName;
            ws.Cell(row, 3).Value = r.DriverName;
            ws.Cell(row, 4).Value = r.RouteArea;
            ws.Cell(row, 5).Value = r.CollectionDate.ToString("dd/MM/yyyy");
            ws.Cell(row, 6).Value = (double)r.Amount;
            ws.Cell(row, 7).Value = r.ImportSource == "Excel" ? "Excel" : "يدوي";
            ws.Cell(row, 8).Value = r.IsWithoutReceipt ? "نعم" : "لا";
        }

        // Totals row
        var totalRow = searchResult.Results.Count + 2;
        ws.Cell(totalRow, 5).Value = "الإجمالي";
        ws.Cell(totalRow, 5).Style.Font.Bold = true;
        ws.Cell(totalRow, 6).Value = (double)searchResult.Totals.TotalAmount;
        ws.Cell(totalRow, 6).Style.Font.Bold = true;

        ws.Columns().AdjustToContents();
        ws.RightToLeft = true;

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<MerchantPaymentHistoryDto?> GetMerchantPaymentHistoryAsync(
        int merchantId, DateTime? dateFrom, DateTime? dateTo)
    {
        var merchant = await _db.Merchants.FindAsync(merchantId);
        if (merchant == null) return null;

        var query = _db.Receipts
            .Include(r => r.Driver)
            .Include(r => r.Session)
            .Where(r => r.MerchantId == merchantId);

        if (dateFrom.HasValue)
            query = query.Where(r => r.CollectionDate >= dateFrom.Value.Date);
        if (dateTo.HasValue)
            query = query.Where(r => r.CollectionDate <= dateTo.Value.Date);

        var receipts = await query
            .OrderByDescending(r => r.CollectionDate)
            .ThenByDescending(r => r.ReceiptNumber)
            .Select(r => new MerchantPaymentEntryDto
            {
                Date = r.CollectionDate,
                ReceiptNumber = r.ReceiptNumber,
                Amount = r.Amount,
                DriverName = r.Driver.FullName,
                RouteArea = r.Session.RouteArea,
                SessionId = r.SessionId,
                ImportSource = r.ImportSource
            })
            .ToListAsync();

        return new MerchantPaymentHistoryDto
        {
            MerchantId = merchantId,
            MerchantName = merchant.MerchantName,
            TotalCollected = receipts.Sum(r => r.Amount),
            TotalReceipts = receipts.Count,
            History = receipts
        };
    }
}

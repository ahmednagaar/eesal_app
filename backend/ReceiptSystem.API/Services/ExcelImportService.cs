using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Models;

namespace ReceiptSystem.API.Services;

public class ExcelImportService
{
    private readonly AppDbContext _db;
    private readonly GapDetectionService _gapService;

    // Arabic column name variants
    private static readonly string[] MerchantNameVariants = {
        "اسم التاجر", "التاجر", "اسم العميل", "العميل", "الاسم", "اسم المحل"
    };
    private static readonly string[] AmountVariants = {
        "المبلغ", "المبلغ المحصّل", "المبلغ المحصل", "المبلغ المدفوع",
        "الإجمالي", "الاجمالي", "المبلغ المستلم", "التحصيل", "قيمة التحصيل"
    };
    private static readonly string[] WithoutReceiptVariants = {
        "بدون إيصال", "بدون ايصال", "ايصال", "إيصال"
    };
    private static readonly string[] DesktopUserVariants = {
        "المستخدم", "اليوزر", "الموظف", "المدخل", "User"
    };
    private static readonly string[] TrueValues = {
        "نعم", "yes", "1", "true", "✓", "صح"
    };

    public ExcelImportService(AppDbContext db, GapDetectionService gapService)
    {
        _db = db;
        _gapService = gapService;
    }

    public async Task<ExcelPreviewResponseDto> PreviewExcelAsync(Stream fileStream)
    {
        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var ws = workbook.Worksheets.First();

            // Detect columns from header row
            int? merchantCol = null, amountCol = null, withoutReceiptCol = null, desktopUserCol = null;
            string? merchantColName = null, amountColName = null;

            var lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
            for (int c = 1; c <= lastCol; c++)
            {
                var header = ws.Cell(1, c).GetString().Trim();
                if (string.IsNullOrEmpty(header)) continue;

                if (merchantCol == null && MerchantNameVariants.Any(v => header.Contains(v)))
                { merchantCol = c; merchantColName = header; }
                else if (amountCol == null && AmountVariants.Any(v => header.Contains(v)))
                { amountCol = c; amountColName = header; }
                else if (withoutReceiptCol == null && WithoutReceiptVariants.Any(v => header.Contains(v)))
                { withoutReceiptCol = c; }
                else if (desktopUserCol == null && DesktopUserVariants.Any(v => header.Contains(v, StringComparison.OrdinalIgnoreCase)))
                { desktopUserCol = c; }
            }

            if (merchantCol == null || amountCol == null)
            {
                return new ExcelPreviewResponseDto
                {
                    Success = false,
                    Error = "لم يتم التعرف على أعمدة الملف. يجب أن يحتوي الملف على عمود اسم التاجر وعمود المبلغ"
                };
            }

            // Read rows
            var rows = new List<ExcelPreviewRowDto>();
            var warnings = new List<string>();
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int r = 2; r <= lastRow; r++)
            {
                var name = ws.Cell(r, merchantCol.Value).GetString().Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                decimal amount = 0;
                var amountCell = ws.Cell(r, amountCol.Value);
                if (amountCell.TryGetValue(out double dblAmount))
                    amount = (decimal)dblAmount;
                else
                    decimal.TryParse(amountCell.GetString().Trim().Replace(",", ""), out amount);

                bool isWithoutReceipt = false;
                if (withoutReceiptCol != null)
                {
                    var val = ws.Cell(r, withoutReceiptCol.Value).GetString().Trim().ToLower();
                    isWithoutReceipt = TrueValues.Any(tv => val.Contains(tv.ToLower()));
                }

                string? desktopUser = null;
                if (desktopUserCol != null)
                {
                    desktopUser = ws.Cell(r, desktopUserCol.Value).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(desktopUser)) desktopUser = null;
                }

                // Fuzzy match merchant
                var match = await FindMatchingMerchant(name);
                var rowDto = new ExcelPreviewRowDto
                {
                    RowIndex = rows.Count + 1,
                    MerchantNameRaw = name,
                    Amount = amount,
                    IsWithoutReceipt = isWithoutReceipt,
                    DesktopSystemUser = desktopUser,
                    MatchedMerchantId = match?.MerchantId,
                    MatchedMerchantName = match?.MerchantName,
                    IsNewMerchant = match == null,
                    NewMerchantName = match == null ? name : null
                };
                rows.Add(rowDto);

                if (match == null)
                    warnings.Add($"الصف {rowDto.RowIndex}: التاجر '{name}' غير موجود في النظام — سيتم إنشاؤه تلقائياً");
            }

            if (rows.Count == 0)
            {
                return new ExcelPreviewResponseDto { Success = false, Error = "الملف فارغ أو لا يحتوي على بيانات" };
            }

            return new ExcelPreviewResponseDto
            {
                Success = true,
                MerchantNameColumn = merchantColName,
                AmountColumn = amountColName,
                RowCount = rows.Count,
                TotalAmount = rows.Sum(r => r.Amount),
                WithoutReceiptCount = rows.Count(r => r.IsWithoutReceipt),
                Rows = rows,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            return new ExcelPreviewResponseDto
            {
                Success = false,
                Error = $"خطأ في قراءة الملف: {ex.Message}"
            };
        }
    }

    public async Task<SaveExcelResponseDto> SaveExcelSessionAsync(SaveExcelSessionDto dto, int userId)
    {
        // Validate range matches row count
        int rangeSize = dto.EndReceiptNumber - dto.StartReceiptNumber + 1;
        if (rangeSize != dto.Rows.Count)
        {
            return new SaveExcelResponseDto
            {
                Success = false,
                Error = $"عدد الصفوف في الملف ({dto.Rows.Count}) لا يتطابق مع نطاق الإيصالات ({dto.StartReceiptNumber}-{dto.EndReceiptNumber} = {rangeSize} إيصال)"
            };
        }

        // Check for existing receipt numbers in range
        var rangeNumbers = Enumerable.Range(dto.StartReceiptNumber, rangeSize).ToList();
        var existingNumbers = await _db.Receipts
            .Where(r => rangeNumbers.Contains(r.ReceiptNumber))
            .Select(r => r.ReceiptNumber)
            .ToListAsync();

        if (existingNumbers.Any())
        {
            return new SaveExcelResponseDto
            {
                Success = false,
                Error = $"أرقام الإيصالات التالية مسجلة بالفعل: {string.Join(", ", existingNumbers)}"
            };
        }

        // Create new merchants for unmatched rows
        int newMerchantsCreated = 0;

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var row in dto.Rows.Where(r => r.IsNewMerchant))
            {
                var merchantName = row.NewMerchantName ?? row.MerchantNameRaw;
                var newMerchant = new Merchant
                {
                    MerchantName = merchantName,
                    IsActive = true,
                    Notes = "تم إنشاؤه تلقائياً من ملف Excel",
                    CreatedAt = DateTime.UtcNow
                };
                _db.Merchants.Add(newMerchant);
                await _db.SaveChangesAsync();
                row.MatchedMerchantId = newMerchant.MerchantId;
                newMerchantsCreated++;
            }

            // Create session
            var session = new CollectionSession
            {
                DriverId = dto.DriverId,
                SessionDate = dto.SessionDate.Date,
                RouteArea = dto.RouteArea,
                FirstReceiptNumber = dto.StartReceiptNumber,
                LastReceiptNumber = dto.EndReceiptNumber,
                TotalReceiptsCount = dto.Rows.Count,
                TotalAmountCollected = dto.Rows.Sum(r => r.Amount),
                Notes = dto.Notes,
                EnteredByUserId = userId,
                EnteredAt = DateTime.UtcNow,
                ImportSource = "Excel",
                OriginalFileName = dto.OriginalFileName
            };
            _db.CollectionSessions.Add(session);
            await _db.SaveChangesAsync();

            // Create receipts with sequential numbers
            var sortedRows = dto.Rows.OrderBy(r => r.RowIndex).ToList();
            for (int i = 0; i < sortedRows.Count; i++)
            {
                var row = sortedRows[i];
                _db.Receipts.Add(new Receipt
                {
                    ReceiptNumber = dto.StartReceiptNumber + i,
                    BookId = null, // Excel imports have no physical book
                    SessionId = session.SessionId,
                    DriverId = dto.DriverId,
                    MerchantId = row.MatchedMerchantId!.Value,
                    CollectionDate = dto.SessionDate.Date,
                    Amount = row.Amount,
                    IsPartialPayment = false,
                    Notes = null,
                    EnteredByUserId = userId,
                    EnteredAt = DateTime.UtcNow,
                    ImportSource = "Excel",
                    IsWithoutReceipt = row.IsWithoutReceipt,
                    DesktopSystemUser = row.DesktopSystemUser
                });
            }
            await _db.SaveChangesAsync();

            // Run gap detection (same as manual sessions)
            var missingReceipts = await _gapService.DetectAndSaveGapsAsync(session.SessionId);

            await transaction.CommitAsync();

            return new SaveExcelResponseDto
            {
                Success = true,
                SessionId = session.SessionId,
                ReceiptsCreated = sortedRows.Count,
                NewMerchantsCreated = newMerchantsCreated,
                DetectedGaps = missingReceipts,
                HasGaps = missingReceipts.Any()
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<Merchant?> FindMatchingMerchant(string nameFromExcel)
    {
        var trimmed = nameFromExcel.Trim();

        // Step 1: Exact match
        var exact = await _db.Merchants
            .FirstOrDefaultAsync(m => m.MerchantName == trimmed);
        if (exact != null) return exact;

        // Step 2: Case-insensitive / trimmed match
        var lower = trimmed.ToLower();
        var ciMatch = await _db.Merchants
            .FirstOrDefaultAsync(m => m.MerchantName.ToLower() == lower);
        if (ciMatch != null) return ciMatch;

        // Step 3: Contains match (Excel name inside DB name or vice versa)
        var contains = await _db.Merchants
            .Where(m => m.MerchantName.Contains(trimmed) || trimmed.Contains(m.MerchantName))
            .FirstOrDefaultAsync();
        return contains;
    }
}

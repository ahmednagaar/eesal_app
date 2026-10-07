using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Models;

namespace ReceiptSystem.API.Services;

public class ErpImportService
{
    private readonly AppDbContext _db;
    private readonly GapDetectionService _gapService;

    // ERP column name variants — includes real ERP export names
    private static readonly string[] MerchantNameVariants = {
        "البيان", "اسم التاجر", "التاجر", "اسم العميل", "العميل", "الاسم", "اسم المحل"
    };
    private static readonly string[] AmountVariants = {
        "مدين", "المبلغ", "الإجمالي", "الاجمالي", "المبلغ المسدد", "قيمة السداد",
        "المبلغ المحصّل", "المبلغ المحصل", "المبلغ المدفوع", "المبلغ المستلم", "التحصيل"
    };
    private static readonly string[] TransactionTypeVariants = {
        "العملية", "نوع العملية", "نوع الحركة"
    };
    private static readonly string[] InvoiceNumberVariants = {
        "رقم الفاتورة", "رقم الايصال", "رقم الإيصال"
    };
    private static readonly string[] ErpUserVariants = {
        "المستخدم", "اليوزر", "User"
    };
    private static readonly string[] BranchVariants = {
        "الفرع", "الفرع الرئيسي"
    };
    private static readonly string[] TimeVariants = {
        "الوقت", "وقت الإدخال"
    };
    private static readonly string[] TreasuryVariants = {
        "الخزنة", "الخزينة"
    };
    private static readonly string[] ErpIdVariants = {
        "ID", "المعرف"
    };

    // Transaction types to include (cash collections only)
    private static readonly string[] CashCollectionTypes = {
        "سداد نقدي عميل", "سداد نقدي", "تحصيل نقدي"
    };

    // Transaction types to always skip
    private static readonly string[] SkipTransactionTypes = {
        "رصيد مرحل", "ترحيل", "إجمالي", "عجز"
    };

    public ErpImportService(AppDbContext db, GapDetectionService gapService)
    {
        _db = db;
        _gapService = gapService;
    }

    // ════════════════════════════════════════════
    // Method 1 — Preview (parse Excel, match merchants, no DB writes)
    // ════════════════════════════════════════════
    public async Task<ErpImportPreviewDto> PreviewAsync(Stream fileStream)
    {
        try
        {
            using var workbook = new XLWorkbook(fileStream);
            var ws = workbook.Worksheets.First();

            // Detect all columns from header row
            int? merchantCol = null, amountCol = null, transactionTypeCol = null;
            int? invoiceNumberCol = null, erpUserCol = null, branchCol = null;
            int? timeCol = null, treasuryCol = null, erpIdCol = null;

            var lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
            for (int c = 1; c <= lastCol; c++)
            {
                var header = ws.Cell(1, c).GetString().Trim();
                if (string.IsNullOrEmpty(header)) continue;

                if (merchantCol == null && MerchantNameVariants.Any(v => header.Contains(v)))
                    merchantCol = c;
                else if (amountCol == null && AmountVariants.Any(v => header.Contains(v)))
                    amountCol = c;
                else if (transactionTypeCol == null && TransactionTypeVariants.Any(v => header == v))
                    transactionTypeCol = c;
                else if (invoiceNumberCol == null && InvoiceNumberVariants.Any(v => header.Contains(v)))
                    invoiceNumberCol = c;
                else if (erpUserCol == null && ErpUserVariants.Any(v => header.Contains(v, StringComparison.OrdinalIgnoreCase)))
                    erpUserCol = c;
                else if (branchCol == null && BranchVariants.Any(v => header.Contains(v)))
                    branchCol = c;
                else if (timeCol == null && TimeVariants.Any(v => header == v))
                    timeCol = c;
                else if (treasuryCol == null && TreasuryVariants.Any(v => header.Contains(v)))
                    treasuryCol = c;
                else if (erpIdCol == null && ErpIdVariants.Any(v => header == v))
                    erpIdCol = c;
            }

            if (merchantCol == null || amountCol == null)
            {
                return new ErpImportPreviewDto
                {
                    Success = false,
                    Error = "لم يتم التعرف على أعمدة الملف. يجب أن يحتوي على عمود اسم التاجر/البيان وعمود المبلغ/مدين"
                };
            }

            var rows = new List<ErpImportPreviewRowDto>();
            var warnings = new List<string>();
            int skippedCount = 0;
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int r = 2; r <= lastRow; r++)
            {
                var name = ws.Cell(r, merchantCol.Value).GetString().Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                // Filter by transaction type if column exists
                if (transactionTypeCol != null)
                {
                    var txnType = ws.Cell(r, transactionTypeCol.Value).GetString().Trim();

                    // Skip known non-collection types
                    if (SkipTransactionTypes.Any(s => txnType.Contains(s)))
                    {
                        skippedCount++;
                        continue;
                    }

                    // If we can identify cash collection types, only include those
                    if (!CashCollectionTypes.Any(c => txnType.Contains(c)))
                    {
                        skippedCount++;
                        continue;
                    }
                }

                decimal amount = 0;
                var amountCell = ws.Cell(r, amountCol.Value);
                if (amountCell.TryGetValue(out double dblAmount))
                    amount = (decimal)dblAmount;
                else
                    decimal.TryParse(amountCell.GetString().Trim().Replace(",", ""), out amount);

                // Skip zero or negative-amount rows (refunds/credits)
                if (amount <= 0) { skippedCount++; continue; }

                // Read extra columns
                string? erpInvoiceNumber = invoiceNumberCol != null
                    ? ws.Cell(r, invoiceNumberCol.Value).GetString().Trim() : null;
                string? erpUser = erpUserCol != null
                    ? ws.Cell(r, erpUserCol.Value).GetString().Trim() : null;
                string? branch = branchCol != null
                    ? ws.Cell(r, branchCol.Value).GetString().Trim() : null;
                string? entryTime = timeCol != null
                    ? ws.Cell(r, timeCol.Value).GetString().Trim() : null;
                string? treasury = treasuryCol != null
                    ? ws.Cell(r, treasuryCol.Value).GetString().Trim() : null;
                string? erpId = erpIdCol != null
                    ? ws.Cell(r, erpIdCol.Value).GetString().Trim() : null;

                var match = await FindMatchingMerchant(name);
                var rowDto = new ErpImportPreviewRowDto
                {
                    RowIndex = rows.Count + 1,
                    MerchantNameRaw = name,
                    Amount = amount,
                    MatchedMerchantId = match?.MerchantId,
                    MatchedMerchantName = match?.MerchantName,
                    IsNewMerchant = match == null,
                    ErpInvoiceNumber = erpInvoiceNumber,
                    ErpUser = erpUser,
                    Branch = branch,
                    ErpEntryTime = entryTime,
                    Treasury = treasury,
                    ErpId = erpId
                };
                rows.Add(rowDto);

                if (match == null)
                    warnings.Add($"الصف {rowDto.RowIndex}: التاجر '{name}' غير موجود في النظام — سيتم إنشاؤه تلقائياً");
            }

            if (rows.Count == 0)
                return new ErpImportPreviewDto { Success = false, Error = "الملف فارغ أو لا يحتوي على بيانات تحصيل" };

            if (skippedCount > 0)
                warnings.Insert(0, $"تم تجاهل {skippedCount} صف (ترحيل/عجز/رصيد مرحل/إجمالي)");

            return new ErpImportPreviewDto
            {
                Success = true,
                RowCount = rows.Count,
                TotalAmount = rows.Sum(r => r.Amount),
                Rows = rows,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            return new ErpImportPreviewDto { Success = false, Error = $"خطأ في قراءة الملف: {ex.Message}" };
        }
    }

    // ════════════════════════════════════════════
    // Method 2 — Create Batch (commit preview to staging)
    // ════════════════════════════════════════════
    public async Task<int> CreateBatchAsync(CreateBatchDto dto, int userId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Create new merchants for unmatched rows
            foreach (var row in dto.Rows.Where(r => r.IsNewMerchant))
            {
                var merchantName = row.NewMerchantName ?? row.MerchantNameRaw;
                var existing = await _db.Merchants.FirstOrDefaultAsync(m => m.MerchantName == merchantName);
                if (existing != null)
                {
                    row.MatchedMerchantId = existing.MerchantId;
                    row.IsNewMerchant = false;
                    continue;
                }

                var newMerchant = new Merchant
                {
                    MerchantName = merchantName,
                    IsActive = true,
                    Notes = "تم إنشاؤه تلقائياً من ملف ERP",
                    CreatedAt = DateTime.UtcNow
                };
                _db.Merchants.Add(newMerchant);
                await _db.SaveChangesAsync();
                row.MatchedMerchantId = newMerchant.MerchantId;
            }

            // ── Duplicate Import Detection ──
            // Check if any non-discarded batch already has the same ErpId values
            var incomingErpIds = dto.Rows
                .Where(r => !string.IsNullOrWhiteSpace(r.ErpId))
                .Select(r => r.ErpId!)
                .ToList();

            if (incomingErpIds.Any())
            {
                var duplicateErpIds = await _db.ExcelImportRows
                    .Where(r => r.Batch.Status != "Discarded"
                        && r.ErpId != null
                        && incomingErpIds.Contains(r.ErpId))
                    .Select(r => new { r.ErpId, r.BatchId })
                    .Take(5)
                    .ToListAsync();

                if (duplicateErpIds.Any())
                {
                    var batchIds = duplicateErpIds.Select(d => d.BatchId).Distinct().ToList();
                    throw new InvalidOperationException(
                        $"تم اكتشاف بيانات مكررة! أرقام ERP التالية موجودة بالفعل في الدفعة رقم {string.Join(", ", batchIds)}: " +
                        $"{string.Join(", ", duplicateErpIds.Select(d => d.ErpId))}");
                }
            }

            // Create batch
            var batch = new ExcelImportBatch
            {
                UploadedFileName = dto.FileName,
                UploadedAt = DateTime.UtcNow,
                UploadedByUserId = userId,
                TotalRowsCount = dto.Rows.Count,
                AssignedRowsCount = 0,
                Status = "InProgress",
                LedgerTotalAmount = dto.LedgerTotalAmount,
                Notes = dto.Notes
            };
            _db.ExcelImportBatches.Add(batch);
            await _db.SaveChangesAsync();

            // Create rows
            foreach (var row in dto.Rows)
            {
                _db.ExcelImportRows.Add(new ExcelImportRow
                {
                    BatchId = batch.BatchId,
                    RowIndex = row.RowIndex,
                    MerchantNameRaw = row.MerchantNameRaw,
                    MatchedMerchantId = row.MatchedMerchantId,
                    IsNewMerchant = row.IsNewMerchant,
                    Amount = row.Amount,
                    IsAssigned = false,
                    ErpInvoiceNumber = row.ErpInvoiceNumber,
                    ErpUser = row.ErpUser,
                    Branch = row.Branch,
                    ErpEntryTime = row.ErpEntryTime,
                    Treasury = row.Treasury,
                    ErpId = row.ErpId
                });
            }
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            return batch.BatchId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ════════════════════════════════════════════
    // Method 3 — Assign Block (rows → driver + receipt range → session)
    // Supports: first+last receipt, two-book detection, book linking, count validation,
    // and explicit skipped receipt numbers (replaces dangerous ForceAssign)
    // ════════════════════════════════════════════
    public async Task<AssignBlockResultDto> AssignBlockAsync(AssignBlockDto dto, int userId)
    {
        // Load rows
        var rows = await _db.ExcelImportRows
            .Where(r => dto.RowIds.Contains(r.RowId) && r.BatchId == dto.BatchId)
            .ToListAsync();

        // Sort by sheet order (rowIndex) — this is the data-correctness requirement
        rows = rows.OrderBy(r => r.RowIndex).ToList();

        if (rows.Count != dto.RowIds.Count)
            return new AssignBlockResultDto { Success = false, Error = "بعض الصفوف المحددة غير موجودة في هذه الدفعة" };

        var alreadyAssigned = rows.Where(r => r.IsAssigned).Select(r => r.RowIndex).ToList();
        if (alreadyAssigned.Any())
            return new AssignBlockResultDto { Success = false, Error = $"الصفوف التالية مسجلة بالفعل: {string.Join(", ", alreadyAssigned)}" };

        // ╔══════════════════════════════════════════════════╗
        // ║  Book Ownership Validation                        ║
        // ║  Driver must own the book(s) for the receipt range ║
        // ╚══════════════════════════════════════════════════════╝
        var driverBooks = await _db.ReceiptBooks
            .Where(b => b.AssignedToDriverId == dto.DriverId
                && (b.Status == "Assigned" || b.Status == "InProgress"))
            .ToListAsync();

        if (!driverBooks.Any())
            return new AssignBlockResultDto
            {
                Success = false,
                Error = "لا توجد دفاتر إيصالات مسجلة لهذا السائق. يجب تعيين دفتر للسائق أولاً قبل إدخال الإيصالات."
            };

        // Check that every receipt number in the range falls within a book owned by this driver
        var unownedReceipts = new List<int>();
        for (int receiptNum = dto.StartReceiptNumber; receiptNum <= dto.EndReceiptNumber; receiptNum++)
        {
            bool belongsToDriver = driverBooks.Any(b =>
                receiptNum >= b.StartReceiptNumber && receiptNum <= b.EndReceiptNumber);
            if (!belongsToDriver)
                unownedReceipts.Add(receiptNum);
        }

        if (unownedReceipts.Any())
        {
            int minUnowned = unownedReceipts.Min();
            int maxUnowned = unownedReceipts.Max();
            var unownedBooks = await _db.ReceiptBooks
                .Where(b => b.StartReceiptNumber <= maxUnowned && b.EndReceiptNumber >= minUnowned)
                .ToListAsync();

            string errorDetail;
            if (unownedBooks.Any())
            {
                var bookDescriptions = unownedBooks.Select(b =>
                {
                    string owner = b.AssignedToDriverId.HasValue ? $"مسجل لسائق آخر (ID: {b.AssignedToDriverId})" : "غير مسجل لأي سائق";
                    return $"دفتر #{b.BookNumber} (إيصالات {b.StartReceiptNumber}–{b.EndReceiptNumber}) — {owner}";
                });
                errorDetail = $"الإيصالات {unownedReceipts.First()}–{unownedReceipts.Last()} تنتمي لدفاتر غير مسجلة لهذا السائق:\n{string.Join("\n", bookDescriptions)}";
            }
            else
            {
                errorDetail = $"الإيصالات {unownedReceipts.First()}–{unownedReceipts.Last()} لا تنتمي لأي دفتر مسجل في النظام.";
            }

            return new AssignBlockResultDto
            {
                Success = false,
                Error = $"⛔ لا يمكن تعيين إيصالات من دفتر غير مسجل للسائق المحدد.\n{errorDetail}"
            };
        }

        // ╔══════════════════════════════════════════════════╗
        // ║  Generate receipt numbers (handles two-book)     ║
        // ╚══════════════════════════════════════════════════╝
        var (receiptNumbers, bookBreakdown, spansTwoBooks) = await GenerateReceiptNumbers(
            dto.StartReceiptNumber, dto.EndReceiptNumber, dto.DriverId);

        // ╔══════════════════════════════════════════════════╗
        // ║  Handle skipped/missing receipt numbers           ║
        // ║  Remove skipped positions — preserve merchant     ║
        // ║  order by NOT shifting merchants                  ║
        // ╚══════════════════════════════════════════════════╝
        var skippedSet = new HashSet<int>(dto.SkippedReceiptNumbers ?? new List<int>());

        // Validate skipped numbers are actually within the receipt range
        var invalidSkipped = skippedSet.Where(s => !receiptNumbers.Contains(s)).ToList();
        if (invalidSkipped.Any())
            return new AssignBlockResultDto
            {
                Success = false,
                Error = $"أرقام الإيصالات المفقودة التالية ليست ضمن النطاق المحدد: {string.Join(", ", invalidSkipped)}"
            };

        // The receipt numbers that will actually be assigned (excluding skipped)
        var numbersToAssign = receiptNumbers.Where(n => !skippedSet.Contains(n)).ToList();

        // Count validation — after removing skipped numbers, count must match merchant rows
        int expectedCount = numbersToAssign.Count;
        int actualCount = rows.Count;

        if (expectedCount != actualCount)
        {
            string scenario;
            if (actualCount < expectedCount)
                scenario = $"⚠️ بعد استبعاد الإيصالات المفقودة، مطلوب {expectedCount} صف تاجر ولكن تم تحديد {actualCount} صف فقط.";
            else
                scenario = $"⚠️ بعد استبعاد الإيصالات المفقودة، مطلوب {expectedCount} صف تاجر ولكن تم تحديد {actualCount} صف — يوجد فائض.";

            return new AssignBlockResultDto { Success = false, Error = scenario };
        }

        // Validate receipt numbers don't conflict with existing records
        var existingNumbers = await _db.Receipts
            .Where(r => numbersToAssign.Contains(r.ReceiptNumber))
            .Select(r => r.ReceiptNumber)
            .ToListAsync();

        if (existingNumbers.Any())
            return new AssignBlockResultDto { Success = false, Error = $"أرقام الإيصالات التالية مسجلة بالفعل: {string.Join(", ", existingNumbers)}" };

        // Validate driver exists
        var driver = await _db.Drivers.FindAsync(dto.DriverId);
        if (driver == null)
            return new AssignBlockResultDto { Success = false, Error = "السائق غير موجود" };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Create session — uses the full range including skipped positions
            var session = new CollectionSession
            {
                DriverId = dto.DriverId,
                SessionDate = dto.SessionDate.Date,
                RouteArea = dto.RouteArea ?? string.Empty,
                FirstReceiptNumber = dto.StartReceiptNumber,
                LastReceiptNumber = dto.EndReceiptNumber,
                TotalReceiptsCount = numbersToAssign.Count,
                TotalAmountCollected = rows.Sum(r => r.Amount),
                EnteredByUserId = userId,
                EnteredAt = DateTime.UtcNow,
                ImportSource = "ERP"
            };
            _db.CollectionSessions.Add(session);
            await _db.SaveChangesAsync();

            // Validate all rows have a matched merchant before creating any receipts
            var unmatchedRows = rows.Where(r => r.MatchedMerchantId == null).Select(r => r.RowIndex).ToList();
            if (unmatchedRows.Any())
                return new AssignBlockResultDto { Success = false, Error = $"الصفوف التالية ليس لها تاجر مطابق: {string.Join(", ", unmatchedRows)}. يرجى مراجعة الدُفعة." };

            // Create receipts — merchants map to non-skipped positions in order
            var createdReceipts = new List<Receipt>();
            for (int i = 0; i < numbersToAssign.Count; i++)
            {
                var row = rows[i];
                var receiptNum = numbersToAssign[i];

                // Find which book this receipt belongs to
                var book = bookBreakdown.FirstOrDefault(b =>
                    receiptNum >= b.FirstReceipt && receiptNum <= b.LastReceipt);

                var receipt = new Receipt
                {
                    ReceiptNumber = receiptNum,
                    BookId = book?.BookId,
                    SessionId = session.SessionId,
                    DriverId = dto.DriverId,
                    MerchantId = row.MatchedMerchantId!.Value,
                    CollectionDate = dto.SessionDate.Date,
                    Amount = row.Amount,
                    IsPartialPayment = false,
                    EnteredByUserId = userId,
                    EnteredAt = DateTime.UtcNow,
                    ImportSource = "ERP"
                };
                _db.Receipts.Add(receipt);
                createdReceipts.Add(receipt);
                row.IsAssigned = true;
                row.AssignedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();

            // Now that IDs are generated, link receipts back to rows
            for (int i = 0; i < createdReceipts.Count; i++)
            {
                rows[i].AssignedReceiptId = createdReceipts[i].ReceiptId;
            }

            // Create explicit ReceiptGap records for user-specified skipped receipts
            foreach (var skippedNum in skippedSet.OrderBy(n => n))
            {
                // Find which book the skipped receipt belongs to
                var skippedBook = driverBooks.FirstOrDefault(b =>
                    skippedNum >= b.StartReceiptNumber && skippedNum <= b.EndReceiptNumber);

                _db.ReceiptGaps.Add(new ReceiptGap
                {
                    MissingReceiptNumber = skippedNum,
                    DetectedInSessionId = session.SessionId,
                    DriverId = session.DriverId,
                    BookId = skippedBook?.BookId,
                    Status = "Open",
                    ReasonCategory = "Lost",
                    DetectedAt = DateTime.UtcNow
                });
            }

            // Update batch counter
            var batch = await _db.ExcelImportBatches.FindAsync(dto.BatchId);
            batch!.AssignedRowsCount += numbersToAssign.Count;

            // Auto-update book status based on receipt usage
            foreach (var bb in bookBreakdown)
            {
                var book = await _db.ReceiptBooks.FindAsync(bb.BookId);
                if (book == null) continue;

                int totalInBook = book.EndReceiptNumber - book.StartReceiptNumber + 1;
                var usedCount = await _db.Receipts.CountAsync(r => r.BookId == book.BookId);

                if (book.Status == "Assigned" && usedCount > 0)
                    book.Status = "InProgress";

                if (usedCount >= totalInBook)
                    book.Status = "Completed";
            }
            await _db.SaveChangesAsync();

            // Additional gap detection (between-session gaps)
            var missingReceipts = await _gapService.DetectAndSaveGapsAsync(session.SessionId);

            await transaction.CommitAsync();

            return new AssignBlockResultDto
            {
                Success = true,
                SessionId = session.SessionId,
                ReceiptsCreated = numbersToAssign.Count,
                DetectedGaps = missingReceipts,
                HasGaps = missingReceipts.Any() || skippedSet.Any(),
                BookBreakdown = bookBreakdown,
                SpansTwoBooks = spansTwoBooks
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ════════════════════════════════════════════
    // Method 4 — Assign Single (orphan row → existing session)
    // Validates: book ownership, driver consistency, session lock status
    // ════════════════════════════════════════════
    public async Task<AssignSingleResultDto> AssignSingleAsync(AssignSingleDto dto, int userId)
    {
        var row = await _db.ExcelImportRows.FirstOrDefaultAsync(r => r.RowId == dto.RowId && r.BatchId == dto.BatchId);
        if (row == null)
            return new AssignSingleResultDto { Success = false, Error = "الصف غير موجود في هذه الدفعة" };
        if (row.IsAssigned)
            return new AssignSingleResultDto { Success = false, Error = "هذا الصف مسجل بالفعل" };

        var session = await _db.CollectionSessions.FindAsync(dto.TargetSessionId);
        if (session == null)
            return new AssignSingleResultDto { Success = false, Error = "الجلسة غير موجودة" };

        // ╔══════════════════════════════════════════════════╗
        // ║  Session Lock Check — cannot modify confirmed     ║
        // ╚══════════════════════════════════════════════════╝
        if (session.IsConfirmed)
            return new AssignSingleResultDto { Success = false, Error = "⛔ لا يمكن إضافة إيصال لجلسة مؤكدة (مقفلة). يجب فتح القفل أولاً." };

        var numberExists = await _db.Receipts.AnyAsync(r => r.ReceiptNumber == dto.ReceiptNumber);
        if (numberExists)
            return new AssignSingleResultDto { Success = false, Error = $"رقم الإيصال {dto.ReceiptNumber} مسجل بالفعل في النظام" };

        // ╔══════════════════════════════════════════════════╗
        // ║  Book/Driver Ownership Validation                 ║
        // ║  Receipt must belong to a book owned by the       ║
        // ║  target session's driver                          ║
        // ╚══════════════════════════════════════════════════╝
        var receiptBook = await _db.ReceiptBooks
            .FirstOrDefaultAsync(b => dto.ReceiptNumber >= b.StartReceiptNumber
                && dto.ReceiptNumber <= b.EndReceiptNumber);

        if (receiptBook == null)
            return new AssignSingleResultDto
            {
                Success = false,
                Error = $"⛔ رقم الإيصال {dto.ReceiptNumber} لا ينتمي لأي دفتر مسجل في النظام."
            };

        if (receiptBook.AssignedToDriverId != session.DriverId)
        {
            // Find the driver who owns the book for a helpful error message
            string bookOwner = receiptBook.AssignedToDriverId.HasValue
                ? $"مسجل لسائق آخر (ID: {receiptBook.AssignedToDriverId})"
                : "غير مسجل لأي سائق";

            return new AssignSingleResultDto
            {
                Success = false,
                Error = $"⛔ رقم الإيصال {dto.ReceiptNumber} ينتمي لدفتر #{receiptBook.BookNumber} ({receiptBook.StartReceiptNumber}–{receiptBook.EndReceiptNumber}) — {bookOwner}.\nلا يمكن تعيينه لجلسة سائق مختلف."
            };
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Create receipt under target session with correct BookId
            var receipt = new Receipt
            {
                ReceiptNumber = dto.ReceiptNumber,
                BookId = receiptBook.BookId,
                SessionId = session.SessionId,
                DriverId = session.DriverId,
                MerchantId = row.MatchedMerchantId!.Value,
                CollectionDate = session.SessionDate,
                Amount = row.Amount,
                IsPartialPayment = false,
                EnteredByUserId = userId,
                EnteredAt = DateTime.UtcNow,
                ImportSource = "ERP"
            };
            _db.Receipts.Add(receipt);
            await _db.SaveChangesAsync();

            // Update session aggregates
            session.TotalAmountCollected += row.Amount;
            session.TotalReceiptsCount += 1;
            if (dto.ReceiptNumber < session.FirstReceiptNumber)
                session.FirstReceiptNumber = dto.ReceiptNumber;
            if (dto.ReceiptNumber > session.LastReceiptNumber)
                session.LastReceiptNumber = dto.ReceiptNumber;

            // Mark row assigned
            row.IsAssigned = true;
            row.AssignedReceiptId = receipt.ReceiptId;
            row.AssignedAt = DateTime.UtcNow;

            // Update batch counter
            var batch = await _db.ExcelImportBatches.FindAsync(dto.BatchId);
            batch!.AssignedRowsCount += 1;
            await _db.SaveChangesAsync();

            // Auto-resolve matching gap
            int? resolvedGapId = null;
            string? resolvedGapMessage = null;
            var matchingGap = await _db.ReceiptGaps
                .FirstOrDefaultAsync(g => g.DriverId == session.DriverId
                    && g.MissingReceiptNumber == dto.ReceiptNumber
                    && (g.Status == "Open" || g.Status == "UnderInvestigation"));

            if (matchingGap != null)
            {
                matchingGap.Status = "Resolved";
                matchingGap.Resolution = "تم العثور على الإيصال المفقود وإضافته للنظام";
                matchingGap.ResolvedAt = DateTime.UtcNow;
                matchingGap.ResolvedByUserId = userId;
                resolvedGapId = matchingGap.GapId;
                resolvedGapMessage = $"تم إغلاق الفجوة رقم {dto.ReceiptNumber} تلقائياً";
                await _db.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return new AssignSingleResultDto
            {
                Success = true,
                ReceiptId = receipt.ReceiptId,
                ResolvedGapId = resolvedGapId,
                ResolvedGapMessage = resolvedGapMessage
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ════════════════════════════════════════════
    // Method 4b — Undo Single Receipt (correct one mistaken individual assignment)
    // Removes one receipt from a session without destroying the entire session.
    // ════════════════════════════════════════════
    public async Task<UndoSingleResultDto> UndoSingleAsync(int batchId, int receiptId, int userId)
    {
        var receipt = await _db.Receipts.FindAsync(receiptId);
        if (receipt == null)
            return new UndoSingleResultDto { Success = false, Error = "الإيصال غير موجود" };

        var session = await _db.CollectionSessions.FindAsync(receipt.SessionId);
        if (session == null)
            return new UndoSingleResultDto { Success = false, Error = "الجلسة غير موجودة" };

        if (session.IsConfirmed)
            return new UndoSingleResultDto { Success = false, Error = "⛔ لا يمكن حذف إيصال من جلسة مؤكدة (مقفلة). يجب فتح القفل أولاً." };

        // Find the linked Excel row
        var row = await _db.ExcelImportRows
            .FirstOrDefaultAsync(r => r.AssignedReceiptId == receiptId && r.BatchId == batchId);

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Re-open any gap that was auto-resolved when this receipt was assigned
            int? reopenedGapId = null;
            var relatedGap = await _db.ReceiptGaps
                .FirstOrDefaultAsync(g => g.DriverId == receipt.DriverId
                    && g.MissingReceiptNumber == receipt.ReceiptNumber
                    && g.Status == "Resolved");

            if (relatedGap != null)
            {
                relatedGap.Status = "Open";
                relatedGap.Resolution = null;
                relatedGap.ResolvedAt = null;
                relatedGap.ResolvedByUserId = null;
                reopenedGapId = relatedGap.GapId;
            }

            // Update session aggregates
            session.TotalAmountCollected -= receipt.Amount;
            session.TotalReceiptsCount -= 1;

            // Unlink the Excel row
            int rowId = 0;
            if (row != null)
            {
                row.IsAssigned = false;
                row.AssignedReceiptId = null;
                row.AssignedAt = null;
                rowId = row.RowId;

                // Update batch counter
                var batch = await _db.ExcelImportBatches.FindAsync(batchId);
                if (batch != null)
                    batch.AssignedRowsCount = Math.Max(0, batch.AssignedRowsCount - 1);
            }

            // Delete the receipt
            _db.Receipts.Remove(receipt);

            // Update HasGaps flag — check if session still has any open gaps
            var remainingGaps = await _db.ReceiptGaps
                .AnyAsync(g => g.DetectedInSessionId == session.SessionId && g.Status == "Open");
            session.HasGaps = remainingGaps;

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return new UndoSingleResultDto
            {
                Success = true,
                ReceiptId = receiptId,
                RowId = rowId,
                ReopenedGapId = reopenedGapId
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ════════════════════════════════════════════
    // Method 4c — Get Session Summary (for Undo confirmation UI)
    // Returns meaningful business info so the user knows what they're undoing
    // ════════════════════════════════════════════
    public async Task<SessionSummaryDto?> GetSessionSummaryAsync(int sessionId)
    {
        var session = await _db.CollectionSessions
            .Include(s => s.Driver)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null) return null;

        return new SessionSummaryDto
        {
            SessionId = session.SessionId,
            DriverName = session.Driver.FullName,
            SessionDate = session.SessionDate,
            RouteArea = session.RouteArea,
            FirstReceiptNumber = session.FirstReceiptNumber,
            LastReceiptNumber = session.LastReceiptNumber,
            TotalReceiptsCount = session.TotalReceiptsCount,
            TotalAmountCollected = session.TotalAmountCollected,
            IsConfirmed = session.IsConfirmed,
            HasGaps = session.HasGaps,
            ImportSource = session.ImportSource
        };
    }

    // ════════════════════════════════════════════
    // Method 5 — Add Manual Row (late-discovered receipt)
    // ════════════════════════════════════════════
    public async Task<int> AddManualRowAsync(AddManualRowDto dto, int userId)
    {
        var batch = await _db.ExcelImportBatches.FindAsync(dto.BatchId);
        if (batch == null)
            throw new InvalidOperationException("الدفعة غير موجودة");

        int? merchantId = dto.MerchantId;
        bool isNew = false;

        // Validate provided merchantId exists
        if (merchantId.HasValue)
        {
            var merchant = await _db.Merchants.FindAsync(merchantId.Value);
            if (merchant == null)
                throw new InvalidOperationException($"التاجر رقم {merchantId.Value} غير موجود");
        }
        else if (!string.IsNullOrWhiteSpace(dto.NewMerchantName))
        {
            var existing = await _db.Merchants.FirstOrDefaultAsync(m => m.MerchantName == dto.NewMerchantName);
            if (existing != null)
            {
                merchantId = existing.MerchantId;
            }
            else
            {
                var newMerchant = new Merchant
                {
                    MerchantName = dto.NewMerchantName,
                    IsActive = true,
                    Notes = "تم إنشاؤه يدوياً من دفعة ERP",
                    CreatedAt = DateTime.UtcNow
                };
                _db.Merchants.Add(newMerchant);
                await _db.SaveChangesAsync();
                merchantId = newMerchant.MerchantId;
                isNew = true;
            }
        }

        // Get next row index
        var maxRowIndex = await _db.ExcelImportRows
            .Where(r => r.BatchId == dto.BatchId)
            .MaxAsync(r => (int?)r.RowIndex) ?? 0;

        var row = new ExcelImportRow
        {
            BatchId = dto.BatchId,
            RowIndex = maxRowIndex + 1,
            MerchantNameRaw = dto.NewMerchantName ?? (merchantId.HasValue
                ? (await _db.Merchants.FindAsync(merchantId.Value))?.MerchantName ?? "غير معروف"
                : "غير معروف"),
            MatchedMerchantId = merchantId,
            IsNewMerchant = isNew,
            Amount = dto.Amount,
            IsAssigned = false
        };
        _db.ExcelImportRows.Add(row);
        batch.TotalRowsCount += 1;
        await _db.SaveChangesAsync();

        return row.RowId;
    }

    // ════════════════════════════════════════════
    // Method 6 — Batch Queries & Status Management
    // ════════════════════════════════════════════
    public async Task<List<BatchSummaryDto>> GetBatchesAsync(string? status, DateTime? date)
    {
        var query = _db.ExcelImportBatches
            .Include(b => b.UploadedByUser)
            .Include(b => b.Rows)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(b => b.Status == status);
        if (date.HasValue)
            query = query.Where(b => b.UploadedAt.Date == date.Value.Date);

        return await query
            .OrderByDescending(b => b.UploadedAt)
            .Select(b => new BatchSummaryDto
            {
                BatchId = b.BatchId,
                UploadedFileName = b.UploadedFileName,
                UploadedAt = b.UploadedAt,
                UploadedByName = b.UploadedByUser.FullName,
                TotalRowsCount = b.TotalRowsCount,
                AssignedRowsCount = b.AssignedRowsCount,
                RemainingRowsCount = b.TotalRowsCount - b.AssignedRowsCount,
                Status = b.Status,
                TotalAmount = b.Rows.Sum(r => r.Amount)
            })
            .ToListAsync();
    }

    public async Task<BatchDetailDto?> GetBatchDetailAsync(int batchId)
    {
        var batch = await _db.ExcelImportBatches
            .Include(b => b.UploadedByUser)
            .Include(b => b.Rows).ThenInclude(r => r.MatchedMerchant)
            .Include(b => b.Rows).ThenInclude(r => r.AssignedReceipt)
            .FirstOrDefaultAsync(b => b.BatchId == batchId);

        if (batch == null) return null;

        return new BatchDetailDto
        {
            BatchId = batch.BatchId,
            UploadedFileName = batch.UploadedFileName,
            UploadedAt = batch.UploadedAt,
            UploadedByName = batch.UploadedByUser.FullName,
            TotalRowsCount = batch.TotalRowsCount,
            AssignedRowsCount = batch.AssignedRowsCount,
            RemainingRowsCount = batch.TotalRowsCount - batch.AssignedRowsCount,
            Status = batch.Status,
            TotalAmount = batch.Rows.Sum(r => r.Amount),
            LedgerTotalAmount = batch.LedgerTotalAmount,
            ActualCashTotal = batch.ActualCashTotal,
            Difference = (batch.LedgerTotalAmount.HasValue && batch.ActualCashTotal.HasValue)
                ? batch.ActualCashTotal.Value - batch.LedgerTotalAmount.Value : null,
            Notes = batch.Notes,
            Rows = batch.Rows.OrderBy(r => r.RowIndex).Select(r => new BatchRowDto
            {
                RowId = r.RowId,
                RowIndex = r.RowIndex,
                MerchantNameRaw = r.MerchantNameRaw,
                MatchedMerchantId = r.MatchedMerchantId,
                MatchedMerchantName = r.MatchedMerchant?.MerchantName,
                Amount = r.Amount,
                IsAssigned = r.IsAssigned,
                AssignedReceiptId = r.AssignedReceiptId,
                AssignedReceiptNumber = r.AssignedReceipt?.ReceiptNumber,
                AssignedAt = r.AssignedAt
            }).ToList()
        };
    }

    public async Task<ReconcileBatchResultDto> ReconcileAsync(int batchId, ReconcileBatchDto dto)
    {
        var batch = await _db.ExcelImportBatches.FindAsync(batchId)
            ?? throw new InvalidOperationException("الدفعة غير موجودة");

        batch.LedgerTotalAmount = dto.LedgerTotalAmount;
        batch.ActualCashTotal = dto.ActualCashTotal;
        await _db.SaveChangesAsync();

        var difference = dto.ActualCashTotal - dto.LedgerTotalAmount;
        string status, message;
        if (difference == 0)
        { status = "match"; message = "المبالغ متطابقة تماماً"; }
        else if (difference > 0)
        { status = "surplus"; message = $"فائض {difference:N2} جنيه"; }
        else
        { status = "shortage"; message = $"نقص {Math.Abs(difference):N2} جنيه"; }

        return new ReconcileBatchResultDto
        {
            LedgerTotal = dto.LedgerTotalAmount,
            ActualCash = dto.ActualCashTotal,
            Difference = difference,
            Status = status,
            Message = message
        };
    }

    public async Task<(bool completed, int remainingCount)> CompleteAsync(int batchId)
    {
        var batch = await _db.ExcelImportBatches.FindAsync(batchId)
            ?? throw new InvalidOperationException("الدفعة غير موجودة");

        var remaining = batch.TotalRowsCount - batch.AssignedRowsCount;
        batch.Status = "Completed";
        await _db.SaveChangesAsync();

        return (true, remaining);
    }

    public async Task<string> DiscardAsync(int batchId)
    {
        var batch = await _db.ExcelImportBatches
            .Include(b => b.Rows)
            .FirstOrDefaultAsync(b => b.BatchId == batchId)
            ?? throw new InvalidOperationException("الدفعة غير موجودة");

        if (batch.Status == "Completed")
            throw new InvalidOperationException("لا يمكن إلغاء دفعة مكتملة");

        if (batch.Status == "Discarded")
            throw new InvalidOperationException("الدفعة ملغاة بالفعل");

        var assignedCount = batch.Rows.Count(r => r.IsAssigned);
        if (assignedCount > 0)
            throw new InvalidOperationException($"لا يمكن إلغاء الدفعة — يوجد {assignedCount} صف مسجل. يجب التراجع عن جميع التعيينات أولاً.");

        batch.Status = "Discarded";
        await _db.SaveChangesAsync();

        return $"تم إلغاء الدفعة رقم {batchId} بنجاح";
    }

    // ════════════════════════════════════════════
    // TWO-BOOK DETECTION ALGORITHM
    // Generates correct receipt numbers when a range spans two books
    // Example: first=996, last=1466 → Book A (996-1000) + Book B (1451-1466)
    // ════════════════════════════════════════════
    private async Task<(List<int> receiptNumbers, List<BookBreakdownDto> breakdown, bool spansTwoBooks)> GenerateReceiptNumbers(
        int firstReceipt, int lastReceipt, int driverId)
    {
        // Find books that contain these receipt numbers
        var bookA = await _db.ReceiptBooks
            .FirstOrDefaultAsync(b => b.AssignedToDriverId == driverId
                && firstReceipt >= b.StartReceiptNumber && firstReceipt <= b.EndReceiptNumber);

        var bookB = await _db.ReceiptBooks
            .FirstOrDefaultAsync(b => b.AssignedToDriverId == driverId
                && lastReceipt >= b.StartReceiptNumber && lastReceipt <= b.EndReceiptNumber);

        var numbers = new List<int>();
        var breakdown = new List<BookBreakdownDto>();

        // Case 1: Both receipts in the same book (or no books found — simple sequential)
        if (bookA == null || bookB == null || bookA.BookId == bookB.BookId)
        {
            // Simple sequential range
            for (int n = firstReceipt; n <= lastReceipt; n++)
                numbers.Add(n);

            if (bookA != null)
            {
                breakdown.Add(new BookBreakdownDto
                {
                    BookId = bookA.BookId,
                    BookNumber = bookA.BookNumber,
                    FirstReceipt = firstReceipt,
                    LastReceipt = lastReceipt,
                    ReceiptCount = numbers.Count
                });
            }

            return (numbers, breakdown, false);
        }

        // Case 2: Two different books — the driver finished one and started another
        // Book A: from firstReceipt to end of book A
        int bookALastReceipt = bookA.EndReceiptNumber;
        for (int n = firstReceipt; n <= bookALastReceipt; n++)
            numbers.Add(n);

        breakdown.Add(new BookBreakdownDto
        {
            BookId = bookA.BookId,
            BookNumber = bookA.BookNumber,
            FirstReceipt = firstReceipt,
            LastReceipt = bookALastReceipt,
            ReceiptCount = bookALastReceipt - firstReceipt + 1
        });

        // Book B: from start of book B to lastReceipt
        int bookBFirstReceipt = bookB.StartReceiptNumber;
        for (int n = bookBFirstReceipt; n <= lastReceipt; n++)
            numbers.Add(n);

        breakdown.Add(new BookBreakdownDto
        {
            BookId = bookB.BookId,
            BookNumber = bookB.BookNumber,
            FirstReceipt = bookBFirstReceipt,
            LastReceipt = lastReceipt,
            ReceiptCount = lastReceipt - bookBFirstReceipt + 1
        });

        return (numbers, breakdown, true);
    }

    // ════════════════════════════════════════════
    // PREVIEW BLOCK — Show receipt-to-merchant mapping before confirming
    // ════════════════════════════════════════════
    public async Task<PreviewBlockResultDto> PreviewBlockAsync(PreviewBlockDto dto)
    {
        var rows = await _db.ExcelImportRows
            .Include(r => r.MatchedMerchant)
            .Where(r => dto.RowIds.Contains(r.RowId) && r.BatchId == dto.BatchId)
            .OrderBy(r => r.RowIndex)
            .ToListAsync();

        if (rows.Count != dto.RowIds.Count)
            return new PreviewBlockResultDto { Success = false, Error = "بعض الصفوف المحددة غير موجودة" };

        // Book ownership validation (if DriverId provided)
        int driverIdForPreview = dto.DriverId ?? 0;
        if (driverIdForPreview > 0)
        {
            var driverBooks = await _db.ReceiptBooks
                .Where(b => b.AssignedToDriverId == driverIdForPreview)
                .ToListAsync();

            if (!driverBooks.Any())
                return new PreviewBlockResultDto
                {
                    Success = false,
                    Error = "لا توجد دفاتر إيصالات مسجلة لهذا السائق. يجب تعيين دفتر للسائق أولاً."
                };

            var unownedReceipts = new List<int>();
            for (int rn = dto.StartReceiptNumber; rn <= dto.EndReceiptNumber; rn++)
            {
                if (!driverBooks.Any(b => rn >= b.StartReceiptNumber && rn <= b.EndReceiptNumber))
                    unownedReceipts.Add(rn);
            }

            if (unownedReceipts.Any())
                return new PreviewBlockResultDto
                {
                    Success = false,
                    Error = $"⛔ الإيصالات {unownedReceipts.First()}–{unownedReceipts.Last()} لا تنتمي لدفتر مسجل للسائق المحدد. لا يمكن المتابعة."
                };
        }

        // Generate receipt numbers (use driver ID if available for proper book detection)
        var (receiptNumbers, bookBreakdown, spansTwoBooks) = await GenerateReceiptNumbers(
            dto.StartReceiptNumber, dto.EndReceiptNumber, driverIdForPreview);

        // Try with driver ID from batch rows if possible
        // We'll recalculate with proper driver ID when the actual assign happens

        int expectedCount = receiptNumbers.Count;
        int actualCount = rows.Count;
        bool hasMismatch = expectedCount != actualCount;

        string? mismatchMessage = null;
        if (hasMismatch)
        {
            if (actualCount < expectedCount)
                mismatchMessage = $"⚠️ مطلوب {expectedCount} إيصال ({dto.StartReceiptNumber}→{dto.EndReceiptNumber}) ولكن تم تحديد {actualCount} صف فقط — قد يكون هناك إيصال مفقود";
            else
                mismatchMessage = $"⚠️ مطلوب {expectedCount} إيصال ({dto.StartReceiptNumber}→{dto.EndReceiptNumber}) ولكن تم تحديد {actualCount} صف — قد يكون هناك إيصال زائد";
        }

        // Create mappings (use smaller count)
        int mappingCount = Math.Min(expectedCount, actualCount);
        var mappings = new List<PreviewReceiptMappingDto>();
        for (int i = 0; i < mappingCount; i++)
        {
            var row = rows[i];
            var receiptNum = receiptNumbers[i];
            var book = bookBreakdown.FirstOrDefault(b =>
                receiptNum >= b.FirstReceipt && receiptNum <= b.LastReceipt);

            mappings.Add(new PreviewReceiptMappingDto
            {
                RowId = row.RowId,
                ReceiptNumber = receiptNum,
                MerchantName = row.MatchedMerchant?.MerchantName ?? row.MerchantNameRaw,
                Amount = row.Amount,
                BookId = book?.BookId,
                BookNumber = book?.BookNumber
            });
        }

        return new PreviewBlockResultDto
        {
            Success = true,
            HasMismatch = hasMismatch,
            ExpectedCount = expectedCount,
            ActualRowCount = actualCount,
            MismatchMessage = mismatchMessage,
            SpansTwoBooks = spansTwoBooks,
            BookBreakdown = bookBreakdown,
            Mappings = mappings
        };
    }

    // ════════════════════════════════════════════
    // UNDO BLOCK — Reverse a block assignment (delete session + receipts, unmark rows)
    // ════════════════════════════════════════════
    public async Task<UndoBlockResultDto> UndoBlockAsync(int sessionId, int batchId)
    {
        var session = await _db.CollectionSessions
            .Include(s => s.Receipts)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null)
            return new UndoBlockResultDto { Success = false, Error = "الجلسة غير موجودة" };

        if (session.IsConfirmed)
            return new UndoBlockResultDto { Success = false, Error = "لا يمكن التراجع — الجلسة مؤكدة ومقفلة" };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var receiptIds = session.Receipts.Select(r => r.ReceiptId).ToList();
            int receiptCount = receiptIds.Count;

            // Unmark the ExcelImportRows that were assigned to these receipts
            var importRows = await _db.ExcelImportRows
                .Where(r => r.AssignedReceiptId != null && receiptIds.Contains(r.AssignedReceiptId.Value))
                .ToListAsync();

            foreach (var row in importRows)
            {
                row.IsAssigned = false;
                row.AssignedReceiptId = null;
                row.AssignedAt = null;
            }

            // Delete gaps associated with this session
            var gaps = await _db.ReceiptGaps
                .Where(g => g.DetectedInSessionId == sessionId)
                .ToListAsync();
            _db.ReceiptGaps.RemoveRange(gaps);

            // Delete receipts
            _db.Receipts.RemoveRange(session.Receipts);

            // Delete session
            _db.CollectionSessions.Remove(session);

            // Update batch counter
            var batch = await _db.ExcelImportBatches.FindAsync(batchId);
            if (batch != null)
            {
                batch.AssignedRowsCount = Math.Max(0, batch.AssignedRowsCount - importRows.Count);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new UndoBlockResultDto
            {
                Success = true,
                ReceiptsDeleted = receiptCount,
                RowsUnassigned = importRows.Count
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ════════════════════════════════════════════
    // DRIVER'S CURRENT BOOKS — Show which books a driver has
    // ════════════════════════════════════════════
    public async Task<DriverBooksDto?> GetDriverBooksAsync(int driverId)
    {
        var driver = await _db.Drivers.FindAsync(driverId);
        if (driver == null) return null;

        var books = await _db.ReceiptBooks
            .Where(b => b.AssignedToDriverId == driverId
                && (b.Status == "Assigned" || b.Status == "InProgress"))
            .OrderBy(b => b.StartReceiptNumber)
            .ToListAsync();

        var bookInfos = new List<DriverBookInfoDto>();
        foreach (var book in books)
        {
            var usedCount = await _db.Receipts
                .CountAsync(r => r.BookId == book.BookId);

            int totalReceipts = book.EndReceiptNumber - book.StartReceiptNumber + 1;
            bookInfos.Add(new DriverBookInfoDto
            {
                BookId = book.BookId,
                BookNumber = book.BookNumber,
                StartReceiptNumber = book.StartReceiptNumber,
                EndReceiptNumber = book.EndReceiptNumber,
                UsedCount = usedCount,
                RemainingCount = totalReceipts - usedCount,
                Status = book.Status
            });
        }

        return new DriverBooksDto
        {
            DriverId = driverId,
            DriverName = driver.FullName,
            Books = bookInfos
        };
    }

    // ════════════════════════════════════════════
    // Merchant Matching (same logic as ExcelImportService)
    // ════════════════════════════════════════════
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
        return ciMatch;

        // NOTE: Previously had a "Contains" step here that was removed because
        // it could silently match wrong merchants (e.g. "أحمد" matching
        // "أحمد محمود حسن التاجر"). Unmatched merchants are now auto-created
        // during batch creation, which is safer.
    }
}


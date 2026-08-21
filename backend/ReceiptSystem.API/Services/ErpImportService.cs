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

                // Skip zero-amount rows
                if (amount == 0) { skippedCount++; continue; }

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
    // ════════════════════════════════════════════
    public async Task<AssignBlockResultDto> AssignBlockAsync(AssignBlockDto dto, int userId)
    {
        // Load rows
        var rows = await _db.ExcelImportRows
            .Where(r => dto.RowIds.Contains(r.RowId) && r.BatchId == dto.BatchId)
            .ToListAsync();

        // Sort in-memory to preserve caller's order (determines receipt number sequence)
        rows = rows.OrderBy(r => dto.RowIds.IndexOf(r.RowId)).ToList();

        if (rows.Count != dto.RowIds.Count)
            return new AssignBlockResultDto { Success = false, Error = "بعض الصفوف المحددة غير موجودة في هذه الدفعة" };

        var alreadyAssigned = rows.Where(r => r.IsAssigned).Select(r => r.RowIndex).ToList();
        if (alreadyAssigned.Any())
            return new AssignBlockResultDto { Success = false, Error = $"الصفوف التالية مسجلة بالفعل: {string.Join(", ", alreadyAssigned)}" };

        // Validate receipt numbers don't conflict
        int endReceiptNumber = dto.StartReceiptNumber + rows.Count - 1;
        var rangeNumbers = Enumerable.Range(dto.StartReceiptNumber, rows.Count).ToList();
        var existingNumbers = await _db.Receipts
            .Where(r => rangeNumbers.Contains(r.ReceiptNumber))
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
            // Create session
            var session = new CollectionSession
            {
                DriverId = dto.DriverId,
                SessionDate = dto.SessionDate.Date,
                RouteArea = dto.RouteArea ?? string.Empty,
                FirstReceiptNumber = dto.StartReceiptNumber,
                LastReceiptNumber = endReceiptNumber,
                TotalReceiptsCount = rows.Count,
                TotalAmountCollected = rows.Sum(r => r.Amount),
                EnteredByUserId = userId,
                EnteredAt = DateTime.UtcNow,
                ImportSource = "ERP"
            };
            _db.CollectionSessions.Add(session);
            await _db.SaveChangesAsync();

            // Create receipts in caller's order
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var receipt = new Receipt
                {
                    ReceiptNumber = dto.StartReceiptNumber + i,
                    BookId = null,
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
                await _db.SaveChangesAsync();

                // Mark row as assigned
                row.IsAssigned = true;
                row.AssignedReceiptId = receipt.ReceiptId;
                row.AssignedAt = DateTime.UtcNow;
            }

            // Update batch counter
            var batch = await _db.ExcelImportBatches.FindAsync(dto.BatchId);
            batch!.AssignedRowsCount += rows.Count;
            await _db.SaveChangesAsync();

            // Gap detection — identical to existing Excel import
            var missingReceipts = await _gapService.DetectAndSaveGapsAsync(session.SessionId);

            await transaction.CommitAsync();

            return new AssignBlockResultDto
            {
                Success = true,
                SessionId = session.SessionId,
                ReceiptsCreated = rows.Count,
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

    // ════════════════════════════════════════════
    // Method 4 — Assign Single (orphan row → existing session)
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

        var numberExists = await _db.Receipts.AnyAsync(r => r.ReceiptNumber == dto.ReceiptNumber);
        if (numberExists)
            return new AssignSingleResultDto { Success = false, Error = $"رقم الإيصال {dto.ReceiptNumber} مسجل بالفعل في النظام" };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Create receipt under target session
            var receipt = new Receipt
            {
                ReceiptNumber = dto.ReceiptNumber,
                BookId = null,
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
        if (ciMatch != null) return ciMatch;

        // Step 3: Contains match (Excel name inside DB name or vice versa)
        var contains = await _db.Merchants
            .Where(m => m.MerchantName.Contains(trimmed) || trimmed.Contains(m.MerchantName))
            .FirstOrDefaultAsync();
        return contains;
    }
}

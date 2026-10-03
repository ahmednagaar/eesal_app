using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Models;

namespace ReceiptSystem.API.Services;

public class AjalService
{
    private readonly AppDbContext _db;

    public AjalService(AppDbContext db) { _db = db; }

    // ═══════════════════════════════════════
    // CREATE SESSION WITH ENTRIES
    // ═══════════════════════════════════════
    public async Task<CreateAjalSessionResponseDto> CreateSessionAsync(CreateAjalSessionDto dto, int userId)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate route
        var route = await _db.Routes.FindAsync(dto.RouteId);
        if (route == null)
            return new CreateAjalSessionResponseDto { Success = false, Errors = new() { "الخط غير موجود" } };

        // Validate driver if provided
        if (dto.DriverId.HasValue)
        {
            var driver = await _db.Drivers.FindAsync(dto.DriverId.Value);
            if (driver == null)
                return new CreateAjalSessionResponseDto { Success = false, Errors = new() { "السائق غير موجود" } };
        }

        // Check for duplicate invoice numbers within this batch
        var invoiceNumbers = dto.Entries.Select(e => e.InvoiceNumber).ToList();
        var duplicatesInBatch = invoiceNumbers.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicatesInBatch.Any())
            errors.Add($"أرقام فواتير مكررة في الإدخال: {string.Join(", ", duplicatesInBatch)}");

        // Check for existing invoice numbers in database
        var existingNumbers = await _db.AjalEntries
            .Where(ae => invoiceNumbers.Contains(ae.InvoiceNumber))
            .Select(ae => ae.InvoiceNumber)
            .ToListAsync();
        if (existingNumbers.Any())
            warnings.Add($"أرقام فواتير مسجلة مسبقاً (ستُتجاهل): {string.Join(", ", existingNumbers)}");

        // Validate merchants
        var merchantIds = dto.Entries.Select(e => e.MerchantId).Distinct().ToList();
        var validMerchants = await _db.Merchants.Where(m => merchantIds.Contains(m.MerchantId))
            .Select(m => m.MerchantId).ToListAsync();
        var invalidMerchants = merchantIds.Except(validMerchants).ToList();
        if (invalidMerchants.Any())
            errors.Add($"تجار غير موجودين: {string.Join(", ", invalidMerchants)}");

        if (errors.Any())
            return new CreateAjalSessionResponseDto { Success = false, Errors = errors, Warnings = warnings };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var session = new AjalSession
            {
                SessionDate = dto.SessionDate.Date,
                RouteId = dto.RouteId,
                DriverId = dto.DriverId,
                Notes = dto.Notes,
                EnteredByUserId = userId,
                EnteredAt = DateTime.UtcNow
            };
            _db.AjalSessions.Add(session);
            await _db.SaveChangesAsync();

            int sortOrder = 1;
            int saved = 0;
            foreach (var entry in dto.Entries)
            {
                // Skip duplicates
                if (existingNumbers.Contains(entry.InvoiceNumber)) continue;

                _db.AjalEntries.Add(new AjalEntry
                {
                    SessionId = session.SessionId,
                    InvoiceNumber = entry.InvoiceNumber,
                    MerchantId = entry.MerchantId,
                    Amount = entry.Amount,
                    Notes = entry.Notes,
                    SortOrder = sortOrder++,
                    EnteredByUserId = userId,
                    EnteredAt = DateTime.UtcNow
                });
                saved++;
            }
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CreateAjalSessionResponseDto
            {
                Success = true,
                SessionId = session.SessionId,
                EntriesSaved = saved,
                Warnings = warnings
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ═══════════════════════════════════════
    // GET SESSIONS FOR A DATE
    // ═══════════════════════════════════════
    public async Task<List<AjalSessionSummaryDto>> GetSessionsAsync(DateTime date)
    {
        return await _db.AjalSessions
            .Include(s => s.Route).Include(s => s.Driver).Include(s => s.Entries)
            .Where(s => s.SessionDate == date.Date)
            .OrderBy(s => s.Route.RouteName)
            .Select(s => new AjalSessionSummaryDto
            {
                SessionId = s.SessionId,
                SessionDate = s.SessionDate,
                RouteName = s.Route.RouteName,
                DriverName = s.Driver != null ? s.Driver.FullName : null,
                EntryCount = s.Entries.Count,
                ReviewedCount = s.Entries.Count(e => e.IsReviewed),
                TotalAmount = s.Entries.Where(e => e.Amount.HasValue).Sum(e => e.Amount!.Value)
            }).ToListAsync();
    }

    // ═══════════════════════════════════════
    // GET SESSION DETAIL
    // ═══════════════════════════════════════
    public async Task<AjalSessionDetailDto?> GetSessionDetailAsync(int sessionId)
    {
        var session = await _db.AjalSessions
            .Include(s => s.Route).Include(s => s.Driver).Include(s => s.EnteredByUser)
            .Include(s => s.Entries).ThenInclude(e => e.Merchant)
            .Include(s => s.Entries).ThenInclude(e => e.ReviewedByUser)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null) return null;

        return new AjalSessionDetailDto
        {
            SessionId = session.SessionId,
            SessionDate = session.SessionDate,
            RouteId = session.RouteId,
            RouteName = session.Route.RouteName,
            DriverId = session.DriverId,
            DriverName = session.Driver?.FullName,
            Notes = session.Notes,
            EntryCount = session.Entries.Count,
            ReviewedCount = session.Entries.Count(e => e.IsReviewed),
            TotalAmount = session.Entries.Where(e => e.Amount.HasValue).Sum(e => e.Amount!.Value),
            EnteredByUserName = session.EnteredByUser.FullName,
            EnteredAt = session.EnteredAt,
            Entries = session.Entries.OrderBy(e => e.SortOrder).Select(e => new AjalEntryDto
            {
                EntryId = e.EntryId,
                InvoiceNumber = e.InvoiceNumber,
                MerchantId = e.MerchantId,
                MerchantName = e.Merchant.MerchantName,
                MerchantCity = e.Merchant.City,
                Amount = e.Amount,
                SortOrder = e.SortOrder,
                IsReviewed = e.IsReviewed,
                ReviewedByUserName = e.ReviewedByUser?.FullName,
                ReviewedAt = e.ReviewedAt,
                Notes = e.Notes,
                EnteredAt = e.EnteredAt
            }).ToList()
        };
    }

    // ═══════════════════════════════════════
    // UPDATE SESSION (Driver, Notes)
    // ═══════════════════════════════════════
    public async Task<bool> UpdateSessionAsync(int sessionId, UpdateAjalSessionDto dto)
    {
        var session = await _db.AjalSessions.FindAsync(sessionId);
        if (session == null) return false;

        if (dto.DriverId.HasValue) session.DriverId = dto.DriverId;
        if (dto.Notes != null) session.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════
    // DELETE SESSION
    // ═══════════════════════════════════════
    public async Task<bool> DeleteSessionAsync(int sessionId)
    {
        var session = await _db.AjalSessions.Include(s => s.Entries).FirstOrDefaultAsync(s => s.SessionId == sessionId);
        if (session == null) return false;

        // Don't delete if any entry is reviewed
        if (session.Entries.Any(e => e.IsReviewed))
            return false;

        _db.AjalSessions.Remove(session); // Cascade deletes entries
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════
    // ADD ENTRIES TO EXISTING SESSION
    // ═══════════════════════════════════════
    public async Task<CreateAjalSessionResponseDto> AddEntriesAsync(AddAjalEntriesDto dto, int userId)
    {
        var session = await _db.AjalSessions.Include(s => s.Entries).FirstOrDefaultAsync(s => s.SessionId == dto.SessionId);
        if (session == null)
            return new CreateAjalSessionResponseDto { Success = false, Errors = new() { "الجلسة غير موجودة" } };

        var invoiceNumbers = dto.Entries.Select(e => e.InvoiceNumber).ToList();
        var existing = await _db.AjalEntries.Where(ae => invoiceNumbers.Contains(ae.InvoiceNumber))
            .Select(ae => ae.InvoiceNumber).ToListAsync();

        int maxSort = session.Entries.Any() ? session.Entries.Max(e => e.SortOrder) : 0;
        int saved = 0;

        foreach (var entry in dto.Entries)
        {
            if (existing.Contains(entry.InvoiceNumber)) continue;
            _db.AjalEntries.Add(new AjalEntry
            {
                SessionId = session.SessionId,
                InvoiceNumber = entry.InvoiceNumber,
                MerchantId = entry.MerchantId,
                Amount = entry.Amount,
                Notes = entry.Notes,
                SortOrder = ++maxSort,
                EnteredByUserId = userId,
                EnteredAt = DateTime.UtcNow
            });
            saved++;
        }
        await _db.SaveChangesAsync();

        return new CreateAjalSessionResponseDto
        {
            Success = true,
            SessionId = session.SessionId,
            EntriesSaved = saved,
            Warnings = existing.Any() ? new() { $"تم تجاهل {existing.Count} فاتورة مكررة" } : new()
        };
    }

    // ═══════════════════════════════════════
    // UPDATE ENTRY
    // ═══════════════════════════════════════
    public async Task<bool> UpdateEntryAsync(int entryId, UpdateAjalEntryDto dto)
    {
        var entry = await _db.AjalEntries.FindAsync(entryId);
        if (entry == null) return false;

        if (dto.InvoiceNumber != null) entry.InvoiceNumber = dto.InvoiceNumber;
        if (dto.MerchantId.HasValue) entry.MerchantId = dto.MerchantId.Value;
        if (dto.Amount.HasValue) entry.Amount = dto.Amount;
        if (dto.Notes != null) entry.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════
    // DELETE ENTRY
    // ═══════════════════════════════════════
    public async Task<bool> DeleteEntryAsync(int entryId)
    {
        var entry = await _db.AjalEntries.FindAsync(entryId);
        if (entry == null || entry.IsReviewed) return false;

        _db.AjalEntries.Remove(entry);
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════
    // DAILY VIEW (All entries for a date, sorted ascending by InvoiceNumber)
    // This is the KEY view for مراجعة دفتر الآجل
    // ═══════════════════════════════════════
    public async Task<AjalDailyViewDto> GetDailyViewAsync(DateTime date)
    {
        var entries = await _db.AjalEntries
            .Include(e => e.Session).ThenInclude(s => s.Route)
            .Include(e => e.Session).ThenInclude(s => s.Driver)
            .Include(e => e.Merchant)
            .Include(e => e.ReviewedByUser)
            .Where(e => e.Session.SessionDate == date.Date)
            .OrderBy(e => e.InvoiceNumber)  // ← ASCENDING by invoice number for ERP comparison
            .ToListAsync();

        return new AjalDailyViewDto
        {
            SessionDate = date.Date,
            TotalEntries = entries.Count,
            ReviewedCount = entries.Count(e => e.IsReviewed),
            PendingCount = entries.Count(e => !e.IsReviewed),
            RouteCount = entries.Select(e => e.Session.RouteId).Distinct().Count(),
            Entries = entries.Select(MapToDailyEntry).ToList()
        };
    }

    // ═══════════════════════════════════════
    // REVIEW: Mark single entry as reviewed
    // ═══════════════════════════════════════
    public async Task<bool> ReviewEntryAsync(int entryId, int userId)
    {
        var entry = await _db.AjalEntries.FindAsync(entryId);
        if (entry == null) return false;
        if (entry.IsReviewed) return true; // Already reviewed

        entry.IsReviewed = true;
        entry.ReviewedByUserId = userId;
        entry.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════
    // REVIEW: Batch review
    // ═══════════════════════════════════════
    public async Task<int> ReviewBatchAsync(List<int> entryIds, int userId)
    {
        var entries = await _db.AjalEntries.Where(e => entryIds.Contains(e.EntryId) && !e.IsReviewed).ToListAsync();
        foreach (var entry in entries)
        {
            entry.IsReviewed = true;
            entry.ReviewedByUserId = userId;
            entry.ReviewedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return entries.Count;
    }

    // ═══════════════════════════════════════
    // SEARCH
    // ═══════════════════════════════════════
    public async Task<AjalSearchResponseDto> SearchAsync(AjalSearchFilterDto f)
    {
        var q = _db.AjalEntries
            .Include(e => e.Session).ThenInclude(s => s.Route)
            .Include(e => e.Session).ThenInclude(s => s.Driver)
            .Include(e => e.Merchant)
            .Include(e => e.ReviewedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.InvoiceNumber))
            q = q.Where(e => e.InvoiceNumber.Contains(f.InvoiceNumber));
        if (!string.IsNullOrWhiteSpace(f.MerchantName))
            q = q.Where(e => e.Merchant.MerchantName.Contains(f.MerchantName));
        if (f.RouteId.HasValue)
            q = q.Where(e => e.Session.RouteId == f.RouteId);
        if (f.DriverId.HasValue)
            q = q.Where(e => e.Session.DriverId == f.DriverId);
        if (f.DateFrom.HasValue)
            q = q.Where(e => e.Session.SessionDate >= f.DateFrom.Value.Date);
        if (f.DateTo.HasValue)
            q = q.Where(e => e.Session.SessionDate <= f.DateTo.Value.Date);
        if (f.ReviewStatus == "reviewed")
            q = q.Where(e => e.IsReviewed);
        else if (f.ReviewStatus == "pending")
            q = q.Where(e => !e.IsReviewed);

        var total = await q.CountAsync();
        var results = await q.OrderBy(e => e.InvoiceNumber)
            .Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync();

        return new AjalSearchResponseDto
        {
            TotalCount = total,
            Page = f.Page,
            Results = results.Select(MapToDailyEntry).ToList()
        };
    }

    // ═══════════════════════════════════════
    // DUPLICATE CHECK
    // ═══════════════════════════════════════
    public async Task<DuplicateCheckResultDto> CheckDuplicateAsync(string invoiceNumber)
    {
        var existing = await _db.AjalEntries
            .Include(e => e.Session).ThenInclude(s => s.Route)
            .FirstOrDefaultAsync(e => e.InvoiceNumber == invoiceNumber);

        return new DuplicateCheckResultDto
        {
            InvoiceNumber = invoiceNumber,
            IsDuplicate = existing != null,
            ExistingDate = existing?.Session.SessionDate,
            ExistingRoute = existing?.Session.Route.RouteName
        };
    }

    // ═══════════════════════════════════════
    // SETTINGS (Prefix)
    // ═══════════════════════════════════════
    public async Task<AjalPrefixSettingsDto> GetSettingsAsync()
    {
        var settings = await _db.SystemSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
        var prefix = settings.GetValueOrDefault("InvoicePrefix", "441");
        return new AjalPrefixSettingsDto
        {
            Prefix = prefix,
            WarningThreshold = int.Parse(settings.GetValueOrDefault("InvoicePrefixWarningThreshold", "950")),
            TotalDigits = int.Parse(settings.GetValueOrDefault("InvoiceTotalDigits", "6")),
            NextPrefix = (int.Parse(prefix) + 1).ToString()
        };
    }

    public async Task UpdatePrefixAsync(string newPrefix, int userId)
    {
        var setting = await _db.SystemSettings.FindAsync("InvoicePrefix");
        if (setting != null)
        {
            setting.SettingValue = newPrefix;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedByUserId = userId;
        }
        await _db.SaveChangesAsync();
    }

    // ═══════════════════════════════════════
    // EXPORT DAILY
    // ═══════════════════════════════════════
    public async Task<byte[]> ExportDailyAsync(DateTime date)
    {
        var data = await GetDailyViewAsync(date);
        using var wb = new XLWorkbook();

        // Sheet 1: All invoices sorted ascending (for مراجعة)
        var ws = wb.Worksheets.Add("كل الفواتير");
        ws.RightToLeft = true;
        ws.Cell(1, 1).Value = "رقم الفاتورة"; ws.Cell(1, 2).Value = "التاجر";
        ws.Cell(1, 3).Value = "الخط"; ws.Cell(1, 4).Value = "السائق";
        ws.Cell(1, 5).Value = "المبلغ"; ws.Cell(1, 6).Value = "المراجعة";
        var hdr = ws.Range(1, 1, 1, 6);
        hdr.Style.Font.Bold = true;
        hdr.Style.Fill.BackgroundColor = XLColor.FromHtml("#667eea");
        hdr.Style.Font.FontColor = XLColor.White;

        for (int i = 0; i < data.Entries.Count; i++)
        {
            var e = data.Entries[i]; var r = i + 2;
            ws.Cell(r, 1).Value = e.InvoiceNumber;
            ws.Cell(r, 2).Value = e.MerchantName;
            ws.Cell(r, 3).Value = e.RouteName;
            ws.Cell(r, 4).Value = e.DriverName ?? "—";
            ws.Cell(r, 5).Value = e.Amount.HasValue ? (double)e.Amount.Value : 0;
            ws.Cell(r, 6).Value = e.IsReviewed ? "✅" : "⏳";
        }
        ws.Columns().AdjustToContents();

        // Group by route in separate sheets
        var grouped = data.Entries.GroupBy(e => e.RouteName);
        foreach (var group in grouped)
        {
            var sheetName = group.Key.Length > 31 ? group.Key[..31] : group.Key;
            var rws = wb.Worksheets.Add(sheetName);
            rws.RightToLeft = true;
            rws.Cell(1, 1).Value = "رقم الفاتورة"; rws.Cell(1, 2).Value = "التاجر";
            rws.Cell(1, 3).Value = "المبلغ"; rws.Cell(1, 4).Value = "المراجعة";
            var rhdr = rws.Range(1, 1, 1, 4);
            rhdr.Style.Font.Bold = true;
            rhdr.Style.Fill.BackgroundColor = XLColor.FromHtml("#667eea");
            rhdr.Style.Font.FontColor = XLColor.White;

            var routeEntries = group.OrderBy(e => e.InvoiceNumber).ToList();
            for (int i = 0; i < routeEntries.Count; i++)
            {
                var e = routeEntries[i]; var r = i + 2;
                rws.Cell(r, 1).Value = e.InvoiceNumber;
                rws.Cell(r, 2).Value = e.MerchantName;
                rws.Cell(r, 3).Value = e.Amount.HasValue ? (double)e.Amount.Value : 0;
                rws.Cell(r, 4).Value = e.IsReviewed ? "✅" : "⏳";
            }
            rws.Columns().AdjustToContents();
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ═══════════════════════════════════════
    // EXPORT SEARCH
    // ═══════════════════════════════════════
    public async Task<byte[]> ExportSearchAsync(AjalSearchFilterDto f)
    {
        f.Page = 1; f.PageSize = 10000;
        var res = await SearchAsync(f);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("نتائج البحث"); ws.RightToLeft = true;
        ws.Cell(1, 1).Value = "رقم الفاتورة"; ws.Cell(1, 2).Value = "التاجر"; ws.Cell(1, 3).Value = "الخط";
        ws.Cell(1, 4).Value = "السائق"; ws.Cell(1, 5).Value = "التاريخ"; ws.Cell(1, 6).Value = "المبلغ";
        ws.Cell(1, 7).Value = "المراجعة";
        var hdr = ws.Range(1, 1, 1, 7); hdr.Style.Font.Bold = true;
        hdr.Style.Fill.BackgroundColor = XLColor.FromHtml("#667eea"); hdr.Style.Font.FontColor = XLColor.White;
        for (int i = 0; i < res.Results.Count; i++)
        {
            var e = res.Results[i]; var r = i + 2;
            ws.Cell(r, 1).Value = e.InvoiceNumber; ws.Cell(r, 2).Value = e.MerchantName;
            ws.Cell(r, 3).Value = e.RouteName; ws.Cell(r, 4).Value = e.DriverName ?? "—";
            ws.Cell(r, 5).Value = e.EnteredAt.ToString("dd/MM/yyyy");
            ws.Cell(r, 6).Value = e.Amount.HasValue ? (double)e.Amount.Value : 0;
            ws.Cell(r, 7).Value = e.IsReviewed ? "✅" : "⏳";
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream(); wb.SaveAs(ms); return ms.ToArray();
    }

    // ═══════════════════════════════════════
    // HELPER
    // ═══════════════════════════════════════
    private static AjalDailyEntryDto MapToDailyEntry(AjalEntry e) => new()
    {
        EntryId = e.EntryId,
        SessionId = e.SessionId,
        InvoiceNumber = e.InvoiceNumber,
        MerchantId = e.MerchantId,
        MerchantName = e.Merchant.MerchantName,
        MerchantCity = e.Merchant.City,
        Amount = e.Amount,
        RouteId = e.Session.RouteId,
        RouteName = e.Session.Route.RouteName,
        DriverId = e.Session.DriverId,
        DriverName = e.Session.Driver?.FullName,
        IsReviewed = e.IsReviewed,
        ReviewedByUserName = e.ReviewedByUser?.FullName,
        ReviewedAt = e.ReviewedAt,
        Notes = e.Notes,
        EnteredAt = e.EnteredAt
    };
}

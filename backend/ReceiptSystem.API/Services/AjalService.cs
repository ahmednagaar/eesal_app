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

    // ─── Daily Register ───
    public async Task<AjalDailyResponseDto> GetDailyRegisterAsync(DateTime date)
    {
        var invoices = await _db.AjalInvoices
            .Include(a => a.Merchant).Include(a => a.Route).Include(a => a.EnteredByUser)
            .Where(a => a.SessionDate == date.Date)
            .OrderBy(a => a.InvoiceNumber)
            .ToListAsync();

        var grouped = invoices.GroupBy(a => a.RouteId).OrderBy(g => g.Key ?? int.MaxValue);
        var routes = grouped.Select(g => new AjalRouteGroupDto
        {
            RouteId = g.Key,
            RouteName = g.First().Route?.RouteName ?? "غير محدد",
            InvoiceCount = g.Count(),
            RouteTotal = g.Where(i => i.InvoiceStatus != "Cancelled").Sum(i => i.Amount),
            Invoices = g.Select(MapToDto).ToList()
        }).ToList();

        return new AjalDailyResponseDto
        {
            SessionDate = date.Date,
            TotalInvoices = invoices.Count,
            ActiveInvoices = invoices.Count(i => i.InvoiceStatus != "Cancelled"),
            CancelledInvoices = invoices.Count(i => i.InvoiceStatus == "Cancelled"),
            TotalAmount = invoices.Where(i => i.InvoiceStatus != "Cancelled").Sum(i => i.Amount),
            Routes = routes
        };
    }

    // ─── Create Invoices ───
    public async Task<CreateAjalResponseDto> CreateInvoicesAsync(CreateAjalInvoicesDto dto, int userId)
    {
        var errors = new List<string>();
        var numbers = dto.Invoices.Select(i => i.InvoiceNumber).ToList();
        var existing = await _db.AjalInvoices.Where(a => numbers.Contains(a.InvoiceNumber))
            .Select(a => a.InvoiceNumber).ToListAsync();
        if (existing.Any())
            errors.Add($"أرقام الفواتير التالية مسجلة مسبقاً: {string.Join(", ", existing)}");

        var merchantIds = dto.Invoices.Select(i => i.MerchantId).Distinct().ToList();
        var validMerchants = await _db.Merchants.Where(m => merchantIds.Contains(m.MerchantId))
            .Select(m => m.MerchantId).ToListAsync();
        var invalidMerchants = merchantIds.Except(validMerchants).ToList();
        if (invalidMerchants.Any())
            errors.Add($"التجار غير موجودين: {string.Join(", ", invalidMerchants)}");

        if (errors.Any())
            return new CreateAjalResponseDto { Success = false, Errors = errors };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var row in dto.Invoices)
            {
                _db.AjalInvoices.Add(new AjalInvoice
                {
                    InvoiceNumber = row.InvoiceNumber,
                    MerchantId = row.MerchantId,
                    RouteId = dto.RouteId,
                    CallCenterEmployeeName = row.CallCenterEmployeeName,
                    Amount = row.Amount,
                    SessionDate = dto.SessionDate.Date,
                    Notes = row.Notes,
                    ImportSource = "Manual",
                    EnteredByUserId = userId,
                    EnteredAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new CreateAjalResponseDto { Success = true, Saved = dto.Invoices.Count };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ─── Edit Invoice ───
    public async Task<bool> EditInvoiceAsync(int id, EditAjalInvoiceDto dto, int userId)
    {
        var inv = await _db.AjalInvoices.FindAsync(id);
        if (inv == null) return false;

        if (dto.Amount != inv.Amount && inv.OriginalAmount == null)
            inv.OriginalAmount = inv.Amount;

        inv.Amount = dto.Amount;
        inv.CallCenterEmployeeName = dto.CallCenterEmployeeName;
        inv.RouteId = dto.RouteId;
        inv.Notes = dto.Notes;
        if (!string.IsNullOrWhiteSpace(dto.InvoiceStatus))
        {
            inv.InvoiceStatus = dto.InvoiceStatus;
            if (dto.InvoiceStatus == "Modified" && !string.IsNullOrWhiteSpace(dto.ModificationNote))
                inv.ModificationNote = dto.ModificationNote;
        }
        await _db.SaveChangesAsync();
        return true;
    }

    // ─── Cancel Invoice ───
    public async Task<bool> CancelInvoiceAsync(int id, string reason)
    {
        var inv = await _db.AjalInvoices.FindAsync(id);
        if (inv == null) return false;
        inv.InvoiceStatus = "Cancelled";
        inv.ModificationNote = reason;
        await _db.SaveChangesAsync();
        return true;
    }

    // ─── Merchant History ───
    public async Task<AjalMerchantHistoryDto?> GetMerchantHistoryAsync(int merchantId, DateTime? from, DateTime? to)
    {
        var merchant = await _db.Merchants.FindAsync(merchantId);
        if (merchant == null) return null;

        var query = _db.AjalInvoices.Include(a => a.Route)
            .Where(a => a.MerchantId == merchantId);
        if (from.HasValue) query = query.Where(a => a.SessionDate >= from.Value.Date);
        if (to.HasValue) query = query.Where(a => a.SessionDate <= to.Value.Date);

        var invoices = await query.OrderByDescending(a => a.SessionDate)
            .ThenByDescending(a => a.InvoiceNumber)
            .Select(a => new AjalMerchantHistoryEntryDto
            {
                SessionDate = a.SessionDate, InvoiceNumber = a.InvoiceNumber,
                Amount = a.Amount, RouteName = a.Route != null ? a.Route.RouteName : null,
                CallCenterEmployeeName = a.CallCenterEmployeeName, InvoiceStatus = a.InvoiceStatus
            }).ToListAsync();

        return new AjalMerchantHistoryDto
        {
            MerchantId = merchantId, MerchantName = merchant.MerchantName,
            Phone = merchant.PhoneNumber, City = merchant.City,
            TotalInvoices = invoices.Count,
            ActiveTotal = invoices.Where(i => i.InvoiceStatus != "Cancelled").Sum(i => i.Amount),
            Invoices = invoices
        };
    }

    // ─── Employee Performance ───
    public async Task<AjalEmployeePerformanceResponseDto> GetEmployeePerformanceAsync(DateTime from, DateTime to)
    {
        var invoices = await _db.AjalInvoices
            .Where(a => a.SessionDate >= from.Date && a.SessionDate <= to.Date && a.InvoiceStatus != "Cancelled")
            .ToListAsync();

        var grouped = invoices.Where(a => !string.IsNullOrWhiteSpace(a.CallCenterEmployeeName))
            .GroupBy(a => a.CallCenterEmployeeName!)
            .Select(g => new AjalEmployeeDto
            {
                EmployeeName = g.Key,
                InvoiceCount = g.Count(),
                TotalAmount = g.Sum(i => i.Amount),
                AverageInvoice = g.Count() > 0 ? Math.Round(g.Sum(i => i.Amount) / g.Count(), 2) : 0,
                DailyBreakdown = g.GroupBy(i => i.SessionDate).OrderBy(d => d.Key)
                    .Select(d => new AjalEmployeeDailyDto { Date = d.Key, Count = d.Count(), Amount = d.Sum(i => i.Amount) }).ToList()
            }).OrderByDescending(e => e.TotalAmount).ToList();

        return new AjalEmployeePerformanceResponseDto
        {
            From = from.Date, To = to.Date,
            GrandTotalInvoices = invoices.Count,
            GrandTotalAmount = invoices.Sum(i => i.Amount),
            Employees = grouped
        };
    }

    // ─── Search ───
    public async Task<AjalSearchResponseDto> SearchAsync(AjalSearchFilterDto f)
    {
        var q = _db.AjalInvoices.Include(a => a.Merchant).Include(a => a.Route).Include(a => a.EnteredByUser).AsQueryable();
        if (!string.IsNullOrWhiteSpace(f.MerchantName)) q = q.Where(a => a.Merchant.MerchantName.Contains(f.MerchantName));
        if (!string.IsNullOrWhiteSpace(f.InvoiceNumber)) q = q.Where(a => a.InvoiceNumber.Contains(f.InvoiceNumber));
        if (f.RouteId.HasValue) q = q.Where(a => a.RouteId == f.RouteId);
        if (!string.IsNullOrWhiteSpace(f.EmployeeName)) q = q.Where(a => a.CallCenterEmployeeName != null && a.CallCenterEmployeeName.Contains(f.EmployeeName));
        if (f.DateFrom.HasValue) q = q.Where(a => a.SessionDate >= f.DateFrom.Value.Date);
        if (f.DateTo.HasValue) q = q.Where(a => a.SessionDate <= f.DateTo.Value.Date);
        if (!string.IsNullOrWhiteSpace(f.Status)) q = q.Where(a => a.InvoiceStatus == f.Status);

        var total = await q.CountAsync();
        var totalAmt = total > 0 ? await q.Where(a => a.InvoiceStatus != "Cancelled").SumAsync(a => a.Amount) : 0;
        var results = await q.OrderByDescending(a => a.SessionDate).ThenByDescending(a => a.InvoiceNumber)
            .Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync();

        return new AjalSearchResponseDto { TotalCount = total, TotalAmount = totalAmt, Page = f.Page, Results = results.Select(MapToDto).ToList() };
    }

    // ─── Export Daily ───
    public async Task<byte[]> ExportDailyAsync(DateTime date)
    {
        var data = await GetDailyRegisterAsync(date);
        using var wb = new XLWorkbook();
        foreach (var route in data.Routes)
        {
            var ws = wb.Worksheets.Add(route.RouteName.Length > 31 ? route.RouteName[..31] : route.RouteName);
            ws.RightToLeft = true;
            ws.Cell(1, 1).Value = "رقم الفاتورة"; ws.Cell(1, 2).Value = "التاجر";
            ws.Cell(1, 3).Value = "الموظف"; ws.Cell(1, 4).Value = "المبلغ"; ws.Cell(1, 5).Value = "الحالة";
            var hdr = ws.Range(1, 1, 1, 5); hdr.Style.Font.Bold = true;
            hdr.Style.Fill.BackgroundColor = XLColor.FromHtml("#667eea"); hdr.Style.Font.FontColor = XLColor.White;

            for (int i = 0; i < route.Invoices.Count; i++)
            {
                var inv = route.Invoices[i]; var r = i + 2;
                ws.Cell(r, 1).Value = inv.InvoiceNumber; ws.Cell(r, 2).Value = inv.MerchantName;
                ws.Cell(r, 3).Value = inv.CallCenterEmployeeName ?? "—";
                ws.Cell(r, 4).Value = (double)inv.Amount;
                ws.Cell(r, 5).Value = inv.InvoiceStatus == "Active" ? "نشطة" : inv.InvoiceStatus == "Cancelled" ? "ملغاة" : "معدّلة";
            }
            var tr = route.Invoices.Count + 2;
            ws.Cell(tr, 3).Value = "الإجمالي"; ws.Cell(tr, 3).Style.Font.Bold = true;
            ws.Cell(tr, 4).Value = (double)route.RouteTotal; ws.Cell(tr, 4).Style.Font.Bold = true;
            ws.Columns().AdjustToContents();
        }
        using var ms = new MemoryStream(); wb.SaveAs(ms); return ms.ToArray();
    }

    // ─── Export Search ───
    public async Task<byte[]> ExportSearchAsync(AjalSearchFilterDto f)
    {
        f.Page = 1; f.PageSize = 10000;
        var res = await SearchAsync(f);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("نتائج البحث"); ws.RightToLeft = true;
        ws.Cell(1, 1).Value = "رقم الفاتورة"; ws.Cell(1, 2).Value = "التاجر"; ws.Cell(1, 3).Value = "الخط";
        ws.Cell(1, 4).Value = "الموظف"; ws.Cell(1, 5).Value = "التاريخ"; ws.Cell(1, 6).Value = "المبلغ"; ws.Cell(1, 7).Value = "الحالة";
        var hdr = ws.Range(1, 1, 1, 7); hdr.Style.Font.Bold = true;
        hdr.Style.Fill.BackgroundColor = XLColor.FromHtml("#667eea"); hdr.Style.Font.FontColor = XLColor.White;
        for (int i = 0; i < res.Results.Count; i++)
        {
            var inv = res.Results[i]; var r = i + 2;
            ws.Cell(r, 1).Value = inv.InvoiceNumber; ws.Cell(r, 2).Value = inv.MerchantName;
            ws.Cell(r, 3).Value = inv.MerchantCity ?? "—"; ws.Cell(r, 4).Value = inv.CallCenterEmployeeName ?? "—";
            ws.Cell(r, 5).Value = inv.EnteredAt.ToString("dd/MM/yyyy"); ws.Cell(r, 6).Value = (double)inv.Amount;
            ws.Cell(r, 7).Value = inv.InvoiceStatus == "Active" ? "نشطة" : inv.InvoiceStatus == "Cancelled" ? "ملغاة" : "معدّلة";
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream(); wb.SaveAs(ms); return ms.ToArray();
    }

    // ─── Export Employee Performance ───
    public async Task<byte[]> ExportEmployeesAsync(DateTime from, DateTime to)
    {
        var data = await GetEmployeePerformanceAsync(from, to);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("أداء الموظفين"); ws.RightToLeft = true;
        ws.Cell(1, 1).Value = "الموظف"; ws.Cell(1, 2).Value = "عدد الفواتير";
        ws.Cell(1, 3).Value = "الإجمالي"; ws.Cell(1, 4).Value = "المتوسط";
        var hdr = ws.Range(1, 1, 1, 4); hdr.Style.Font.Bold = true;
        hdr.Style.Fill.BackgroundColor = XLColor.FromHtml("#667eea"); hdr.Style.Font.FontColor = XLColor.White;
        for (int i = 0; i < data.Employees.Count; i++)
        {
            var e = data.Employees[i]; var r = i + 2;
            ws.Cell(r, 1).Value = e.EmployeeName; ws.Cell(r, 2).Value = e.InvoiceCount;
            ws.Cell(r, 3).Value = (double)e.TotalAmount; ws.Cell(r, 4).Value = (double)e.AverageInvoice;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream(); wb.SaveAs(ms); return ms.ToArray();
    }

    // ─── Excel Import Preview ───
    private static readonly string[] InvNumVariants = { "رقم الفاتورة", "الفاتورة", "رقم" };
    private static readonly string[] MerchVariants = { "اسم العميل", "العميل", "اسم التاجر", "التاجر" };
    private static readonly string[] AmtVariants = { "الإجمالي", "المبلغ", "إجمالي الفاتورة", "قيمة الفاتورة", "الاجمالي" };
    private static readonly string[] EmpVariants = { "الموظف", "موظف الكول سنتر" };

    public async Task<AjalExcelPreviewResponseDto> PreviewExcelAsync(Stream stream)
    {
        try
        {
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();
            int? invCol = null, merchCol = null, amtCol = null, empCol = null;
            var lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 0;
            for (int c = 1; c <= lastCol; c++)
            {
                var h = ws.Cell(1, c).GetString().Trim();
                if (string.IsNullOrEmpty(h)) continue;
                if (invCol == null && InvNumVariants.Any(v => h.Contains(v))) invCol = c;
                else if (merchCol == null && MerchVariants.Any(v => h.Contains(v))) merchCol = c;
                else if (amtCol == null && AmtVariants.Any(v => h.Contains(v))) amtCol = c;
                else if (empCol == null && EmpVariants.Any(v => h.Contains(v, StringComparison.OrdinalIgnoreCase))) empCol = c;
            }
            if (invCol == null || merchCol == null || amtCol == null)
                return new AjalExcelPreviewResponseDto { Success = false, Error = "لم يتم التعرف على أعمدة الملف. يجب أن يحتوي على: رقم الفاتورة، اسم التاجر، المبلغ" };

            var rows = new List<AjalExcelPreviewRowDto>(); var warnings = new List<string>();
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int r = 2; r <= lastRow; r++)
            {
                var invNum = ws.Cell(r, invCol.Value).GetString().Trim();
                if (string.IsNullOrWhiteSpace(invNum)) continue;
                var name = ws.Cell(r, merchCol.Value).GetString().Trim();
                decimal amount = 0;
                var amtCell = ws.Cell(r, amtCol.Value);
                if (amtCell.TryGetValue(out double d)) amount = (decimal)d;
                else decimal.TryParse(amtCell.GetString().Trim().Replace(",", ""), out amount);

                string? emp = empCol.HasValue ? ws.Cell(r, empCol.Value).GetString().Trim() : null;
                if (string.IsNullOrWhiteSpace(emp)) emp = null;

                var match = await FindMerchant(name);
                var isDup = await _db.AjalInvoices.AnyAsync(a => a.InvoiceNumber == invNum);
                var row = new AjalExcelPreviewRowDto
                {
                    RowIndex = rows.Count + 1, InvoiceNumber = invNum, MerchantNameRaw = name,
                    Amount = amount, CallCenterEmployeeName = emp,
                    MatchedMerchantId = match?.MerchantId, MatchedMerchantName = match?.MerchantName,
                    IsNewMerchant = match == null, IsDuplicate = isDup
                };
                rows.Add(row);
                if (match == null) warnings.Add($"الصف {row.RowIndex}: التاجر '{name}' غير موجود — سيُنشأ تلقائياً");
                if (isDup) warnings.Add($"الصف {row.RowIndex}: الفاتورة {invNum} مسجلة مسبقاً — ستُتجاهل");
            }
            if (rows.Count == 0) return new AjalExcelPreviewResponseDto { Success = false, Error = "الملف فارغ" };

            return new AjalExcelPreviewResponseDto
            {
                Success = true, RowCount = rows.Count, TotalAmount = rows.Where(r => !r.IsDuplicate).Sum(r => r.Amount),
                DuplicateCount = rows.Count(r => r.IsDuplicate), Rows = rows, Warnings = warnings
            };
        }
        catch (Exception ex) { return new AjalExcelPreviewResponseDto { Success = false, Error = $"خطأ في قراءة الملف: {ex.Message}" }; }
    }

    // ─── Excel Import Save ───
    public async Task<SaveAjalExcelResponseDto> SaveExcelAsync(SaveAjalExcelDto dto, int userId)
    {
        int saved = 0, skipped = 0, newMerchants = 0;

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var row in dto.Rows)
            {
                if (await _db.AjalInvoices.AnyAsync(a => a.InvoiceNumber == row.InvoiceNumber)) { skipped++; continue; }
                int merchantId;
                if (row.IsNewMerchant)
                {
                    var m = new Merchant { MerchantName = row.NewMerchantName ?? row.InvoiceNumber, IsActive = true, Notes = "تم إنشاؤه من ملف Excel — دفتر الآجل", CreatedAt = DateTime.UtcNow };
                    _db.Merchants.Add(m); await _db.SaveChangesAsync(); merchantId = m.MerchantId; newMerchants++;
                }
                else { merchantId = row.MerchantId!.Value; }

                _db.AjalInvoices.Add(new AjalInvoice
                {
                    InvoiceNumber = row.InvoiceNumber, MerchantId = merchantId, RouteId = dto.RouteId,
                    CallCenterEmployeeName = row.CallCenterEmployeeName, Amount = row.Amount,
                    SessionDate = dto.SessionDate.Date, ImportSource = "Excel", EnteredByUserId = userId, EnteredAt = DateTime.UtcNow
                });
                saved++;
            }
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return new SaveAjalExcelResponseDto { Success = true, Saved = saved, Skipped = skipped, NewMerchantsCreated = newMerchants };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // ─── Settings ───
    public async Task<AjalPrefixSettingsDto> GetSettingsAsync()
    {
        var settings = await _db.SystemSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
        var prefix = settings.GetValueOrDefault("InvoicePrefix", "441");
        return new AjalPrefixSettingsDto
        {
            Prefix = prefix, WarningThreshold = int.Parse(settings.GetValueOrDefault("InvoicePrefixWarningThreshold", "950")),
            TotalDigits = int.Parse(settings.GetValueOrDefault("InvoiceTotalDigits", "6")),
            NextPrefix = (int.Parse(prefix) + 1).ToString()
        };
    }

    public async Task UpdatePrefixAsync(string newPrefix, int userId)
    {
        var setting = await _db.SystemSettings.FindAsync("InvoicePrefix");
        if (setting != null) { setting.SettingValue = newPrefix; setting.UpdatedAt = DateTime.UtcNow; setting.UpdatedByUserId = userId; }
        await _db.SaveChangesAsync();
    }

    // ─── Dashboard ───
    public async Task<AjalDashboardSummaryDto> GetTodaySummaryAsync()
    {
        var today = DateTime.Today;
        var invoices = await _db.AjalInvoices.Where(a => a.SessionDate == today).ToListAsync();
        return new AjalDashboardSummaryDto
        {
            TodaySessionDate = today, InvoiceCount = invoices.Count(i => i.InvoiceStatus != "Cancelled"),
            TotalAmount = invoices.Where(i => i.InvoiceStatus != "Cancelled").Sum(i => i.Amount),
            RouteCount = invoices.Select(i => i.RouteId).Distinct().Count(),
            CancelledCount = invoices.Count(i => i.InvoiceStatus == "Cancelled")
        };
    }

    // ─── Unique Employee Names (for autocomplete) ───
    public async Task<List<string>> GetEmployeeNamesAsync()
    {
        return await _db.AjalInvoices.Where(a => a.CallCenterEmployeeName != null)
            .Select(a => a.CallCenterEmployeeName!).Distinct().OrderBy(n => n).ToListAsync();
    }

    // ─── Helpers ───
    private AjalInvoiceDto MapToDto(AjalInvoice a) => new()
    {
        AjalInvoiceId = a.AjalInvoiceId, InvoiceNumber = a.InvoiceNumber,
        MerchantId = a.MerchantId, MerchantName = a.Merchant.MerchantName,
        MerchantPhone = a.Merchant.PhoneNumber, MerchantCity = a.Merchant.City,
        Amount = a.Amount, OriginalAmount = a.OriginalAmount,
        CallCenterEmployeeName = a.CallCenterEmployeeName, InvoiceStatus = a.InvoiceStatus,
        ModificationNote = a.ModificationNote, Notes = a.Notes, ImportSource = a.ImportSource,
        EnteredByUserName = a.EnteredByUser.FullName, EnteredAt = a.EnteredAt
    };

    private async Task<Merchant?> FindMerchant(string name)
    {
        var t = name.Trim();
        var exact = await _db.Merchants.FirstOrDefaultAsync(m => m.MerchantName == t);
        if (exact != null) return exact;
        var ci = await _db.Merchants.FirstOrDefaultAsync(m => m.MerchantName.ToLower() == t.ToLower());
        if (ci != null) return ci;
        return await _db.Merchants.FirstOrDefaultAsync(m => m.MerchantName.Contains(t) || t.Contains(m.MerchantName));
    }
}

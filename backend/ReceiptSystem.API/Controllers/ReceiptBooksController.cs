using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Models;
using ReceiptSystem.API.Services;
using System.Security.Claims;

namespace ReceiptSystem.API.Controllers;

[ApiController]
[Route("api/books")]
[Authorize(Roles = "Admin,Treasury")]
public class ReceiptBooksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public ReceiptBooksController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");



    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var books = await _db.ReceiptBooks
            .Where(b => b.Status == "Available")
            .OrderBy(b => b.BookNumber)
            .Select(b => new BookDto
            {
                BookId = b.BookId,
                BookNumber = b.BookNumber,
                StartReceiptNumber = b.StartReceiptNumber,
                EndReceiptNumber = b.EndReceiptNumber,
                Status = b.Status
            })
            .ToListAsync();
        return Ok(books);
    }




    [HttpPut("{id}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignBookDto dto)
    {
        var book = await _db.ReceiptBooks.Include(b => b.Series).FirstOrDefaultAsync(b => b.BookId == id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        if (book.Status != "Available" && book.Status != "Returned")
            return BadRequest(new { message = $"لا يمكن تسليم دفتر بحالة '{book.Status}' — يجب أن يكون متاح أو مُرجع" });

        var driver = await _db.Drivers.FindAsync(dto.DriverId);
        if (driver == null) return BadRequest(new { message = "السائق غير موجود" });

        book.AssignedToDriverId = dto.DriverId;
        book.AssignedDate = dto.AssignedDate ?? DateTime.Today;
        book.AssignedByUserId = UserId;
        book.Status = "Assigned";
        book.IsVerified = false;
        book.VerifiedAt = null;
        book.VerifiedByUserId = null;
        book.ReturnedDate = null;
        book.ReturnedToUserId = null;

        // Record movement
        _db.BookMovements.Add(new BookMovement
        {
            BookId = id,
            ActionType = "Delivered",
            ToDriverId = dto.DriverId,
            PerformedByUserId = UserId,
            PerformedAt = DateTime.UtcNow,
            Notes = $"تسليم دفتر {book.Series?.SeriesCode}-{book.BookNumber} للسائق {driver.FullName}"
        });

        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تسليم دفتر", "ReceiptBook", id, newValues: dto);
        return Ok(new { message = "تم تسليم الدفتر للسائق بنجاح" });
    }

    /// <summary>
    /// Bulk assign multiple books to a single driver
    /// </summary>
    [HttpPost("assign-batch")]
    public async Task<IActionResult> AssignBatch([FromBody] AssignBatchDto dto)
    {
        if (dto.BookIds == null || dto.BookIds.Length == 0)
            return BadRequest(new { message = "اختر دفتر واحد على الأقل" });

        var books = await _db.ReceiptBooks
            .Where(b => dto.BookIds.Contains(b.BookId))
            .ToListAsync();

        var unavailable = books.Where(b => b.Status != "Available").ToList();
        if (unavailable.Count > 0)
            return BadRequest(new { message = $"يوجد {unavailable.Count} دفتر غير متاح للتسليم" });

        var driver = await _db.Drivers.FindAsync(dto.DriverId);
        if (driver == null) return BadRequest(new { message = "السائق غير موجود" });

        var assignDate = dto.AssignedDate ?? DateTime.Today;
        foreach (var book in books)
        {
            book.AssignedToDriverId = dto.DriverId;
            book.AssignedDate = assignDate;
            book.AssignedByUserId = UserId;
            book.Status = "Assigned";

            _db.BookMovements.Add(new BookMovement
            {
                BookId = book.BookId,
                ActionType = "Delivered",
                ToDriverId = dto.DriverId,
                PerformedByUserId = UserId,
                PerformedAt = DateTime.UtcNow,
                Notes = $"تسليم جماعي — دفتر #{book.BookNumber} للسائق {driver.FullName}"
            });
        }
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تسليم دفاتر (جماعي)", "ReceiptBook", 0,
            newValues: new { dto.BookIds, dto.DriverId, Count = books.Count });

        return Ok(new { message = $"تم تسليم {books.Count} دفتر للسائق بنجاح", assignedCount = books.Count });
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EditBookDto dto)
    {
        var book = await _db.ReceiptBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });
        if (book.Status == "Assigned")
            return BadRequest(new { message = "لا يمكن تعديل دفتر تم تسليمه بالفعل" });

        var old = new { book.Notes, book.BookNumber };
        if (dto.BookNumber.HasValue) book.BookNumber = dto.BookNumber.Value;
        if (dto.Notes != null) book.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تعديل دفتر", "ReceiptBook", id, oldValues: old, newValues: dto);

        return Ok(new { message = "تم تعديل الدفتر بنجاح" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var book = await _db.ReceiptBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        book.Status = "Deactivated";
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تعطيل دفتر", "ReceiptBook", id);

        return Ok(new { message = "تم تعطيل الدفتر بنجاح" });
    }

    // ════════════════════════════════════════════
    // BOOK DETAIL — receipt-level drill-down
    // ════════════════════════════════════════════

    [HttpGet("{id}/detail")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var book = await _db.ReceiptBooks
            .Include(b => b.Series)
            .Include(b => b.AssignedToDriver)
            .Include(b => b.Receipts).ThenInclude(r => r.Merchant)
            .Include(b => b.Gaps)
            .Include(b => b.Movements).ThenInclude(m => m.FromDriver)
            .Include(b => b.Movements).ThenInclude(m => m.ToDriver)
            .Include(b => b.Movements).ThenInclude(m => m.PerformedByUser)
            .FirstOrDefaultAsync(b => b.BookId == id);

        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        int totalReceipts = book.EndReceiptNumber - book.StartReceiptNumber + 1;
        int usedCount = book.Receipts.Count;
        string displayName = book.Series != null
            ? $"{book.Series.SeriesCode}-{book.BookNumber}"
            : $"#{book.BookNumber}";

        var detail = new BookDetailDto
        {
            BookId = book.BookId,
            BookNumber = book.BookNumber,
            DisplayName = displayName,
            StartReceiptNumber = book.StartReceiptNumber,
            EndReceiptNumber = book.EndReceiptNumber,
            TotalReceipts = totalReceipts,
            Status = book.Status,
            DriverName = book.AssignedToDriver?.FullName,
            DriverId = book.AssignedToDriverId,
            AssignedDate = book.AssignedDate,
            ReturnedDate = book.ReturnedDate,
            IsVerified = book.IsVerified,
            UsedReceipts = usedCount,
            RemainingReceipts = totalReceipts - usedCount,
            UsagePercent = totalReceipts > 0 ? Math.Round((double)usedCount / totalReceipts * 100, 1) : 0,
            MissingReceipts = book.Gaps.Count(g => g.Status == "Open"),
            Gaps = book.Gaps.OrderBy(g => g.MissingReceiptNumber).Select(g => new BookGapDto
            {
                GapId = g.GapId,
                MissingReceiptNumber = g.MissingReceiptNumber,
                Status = g.Status,
                ReasonCategory = g.ReasonCategory,
                Resolution = g.Resolution,
                DetectedAt = g.DetectedAt
            }).ToList(),
            Receipts = book.Receipts.OrderBy(r => r.ReceiptNumber).Select(r => new BookReceiptDto
            {
                ReceiptId = r.ReceiptId,
                ReceiptNumber = r.ReceiptNumber,
                MerchantName = r.Merchant?.MerchantName ?? "—",
                Amount = r.Amount,
                CollectionDate = r.CollectionDate,
                SessionId = r.SessionId
            }).ToList(),
            Movements = book.Movements.OrderByDescending(m => m.PerformedAt).Select(m => new BookMovementDto
            {
                MovementId = m.MovementId,
                ActionType = m.ActionType,
                ActionDisplay = m.ActionType switch
                {
                    "Delivered" => "📋 تسليم",
                    "Returned" => "↩️ إرجاع",
                    "Transferred" => "🔄 نقل",
                    "Verified" => "✓ تحقق",
                    "Closed" => "🔒 إغلاق",
                    _ => m.ActionType
                },
                FromDriverName = m.FromDriver?.FullName,
                ToDriverName = m.ToDriver?.FullName,
                PerformedByName = m.PerformedByUser?.FullName ?? "—",
                PerformedAt = m.PerformedAt,
                Notes = m.Notes
            }).ToList()
        };

        return Ok(detail);
    }

    // ════════════════════════════════════════════
    // TRANSFER — Move book from one driver to another
    // ════════════════════════════════════════════

    [HttpPut("{id}/transfer")]
    public async Task<IActionResult> TransferBook(int id, [FromBody] TransferBookDto dto)
    {
        var book = await _db.ReceiptBooks
            .Include(b => b.AssignedToDriver)
            .Include(b => b.Series)
            .FirstOrDefaultAsync(b => b.BookId == id);

        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        if (book.Status != "Assigned" && book.Status != "InProgress")
            return BadRequest(new { message = "لا يمكن نقل دفتر ليس مع سائق حالياً" });

        if (book.AssignedToDriverId == dto.ToDriverId)
            return BadRequest(new { message = "الدفتر بالفعل مع هذا السائق" });

        var newDriver = await _db.Drivers.FindAsync(dto.ToDriverId);
        if (newDriver == null) return BadRequest(new { message = "السائق الجديد غير موجود" });

        var oldDriverName = book.AssignedToDriver?.FullName ?? "—";
        int oldDriverId = book.AssignedToDriverId ?? 0;
        string displayName = book.Series != null
            ? $"{book.Series.SeriesCode}-{book.BookNumber}"
            : $"#{book.BookNumber}";

        // Record transfer movement
        _db.BookMovements.Add(new BookMovement
        {
            BookId = id,
            ActionType = "Transferred",
            FromDriverId = oldDriverId > 0 ? oldDriverId : null,
            ToDriverId = dto.ToDriverId,
            PerformedByUserId = UserId,
            PerformedAt = DateTime.UtcNow,
            Notes = dto.Notes ?? $"نقل دفتر {displayName} من {oldDriverName} إلى {newDriver.FullName}"
        });

        // Update book ownership
        book.AssignedToDriverId = dto.ToDriverId;
        book.AssignedDate = DateTime.Today;
        book.AssignedByUserId = UserId;

        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "نقل دفتر", "ReceiptBook", id,
            oldValues: new { FromDriver = oldDriverName },
            newValues: new { ToDriver = newDriver.FullName, dto.Notes });

        return Ok(new { message = $"تم نقل الدفتر {displayName} من {oldDriverName} إلى {newDriver.FullName}" });
    }

    // ════════════════════════════════════════════
    // RETURN RECONCILIATION — Preview before return
    // ════════════════════════════════════════════

    [HttpGet("{id}/return-preview")]
    public async Task<IActionResult> ReturnPreview(int id)
    {
        var book = await _db.ReceiptBooks
            .Include(b => b.Series)
            .Include(b => b.AssignedToDriver)
            .Include(b => b.Receipts)
            .Include(b => b.Gaps)
            .FirstOrDefaultAsync(b => b.BookId == id);

        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        if (book.Status != "Assigned" && book.Status != "InProgress" && book.Status != "Completed")
            return BadRequest(new { message = "هذا الدفتر ليس مع سائق — لا يمكن إرجاعه" });

        int totalReceipts = book.EndReceiptNumber - book.StartReceiptNumber + 1;
        var usedNumbers = book.Receipts.Select(r => r.ReceiptNumber).ToHashSet();
        var allNumbers = Enumerable.Range(book.StartReceiptNumber, totalReceipts).ToList();
        var unusedNumbers = allNumbers.Where(n => !usedNumbers.Contains(n)).ToList();
        var gapNumbers = book.Gaps.Where(g => g.Status == "Open").Select(g => g.MissingReceiptNumber).ToList();

        string displayName = book.Series != null
            ? $"{book.Series.SeriesCode}-{book.BookNumber}"
            : $"#{book.BookNumber}";

        return Ok(new ReturnReconciliationDto
        {
            BookId = book.BookId,
            DisplayName = displayName,
            DriverName = book.AssignedToDriver?.FullName ?? "—",
            TotalReceipts = totalReceipts,
            UsedReceipts = usedNumbers.Count,
            UnusedReceipts = unusedNumbers.Count,
            MissingReceipts = gapNumbers.Count,
            MissingReceiptNumbers = gapNumbers.OrderBy(n => n).ToList(),
            UnusedReceiptNumbers = unusedNumbers.OrderBy(n => n).ToList()
        });
    }

    /// <summary>
    /// Driver returns a finished book — enhanced with reconciliation
    /// </summary>
    [HttpPut("{id}/return")]
    public async Task<IActionResult> ReturnBook(int id, [FromBody] ReturnBookDto dto)
    {
        var book = await _db.ReceiptBooks
            .Include(b => b.AssignedToDriver)
            .Include(b => b.Series)
            .FirstOrDefaultAsync(b => b.BookId == id);

        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        if (book.Status != "Assigned" && book.Status != "InProgress" && book.Status != "Completed")
            return BadRequest(new { message = "هذا الدفتر ليس مع سائق — لا يمكن إرجاعه" });

        string driverName = book.AssignedToDriver?.FullName ?? "—";
        int? fromDriverId = book.AssignedToDriverId;

        book.Status = "Returned";
        book.ReturnedDate = DateTime.UtcNow;
        book.ReturnedToUserId = UserId;
        if (dto.Notes != null) book.Notes = dto.Notes;

        // Record movement
        _db.BookMovements.Add(new BookMovement
        {
            BookId = id,
            ActionType = "Returned",
            FromDriverId = fromDriverId,
            PerformedByUserId = UserId,
            PerformedAt = DateTime.UtcNow,
            Notes = dto.Notes ?? $"إرجاع دفتر من {driverName}"
        });

        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إرجاع دفتر", "ReceiptBook", id,
            newValues: new { ReturnedDate = book.ReturnedDate, dto.Notes, DriverName = driverName });

        return Ok(new { message = $"تم تسجيل إرجاع الدفتر من {driverName}" });
    }

    /// <summary>
    /// Manager verifies a returned book
    /// </summary>
    [HttpPut("{id}/verify")]
    public async Task<IActionResult> VerifyBook(int id)
    {
        var book = await _db.ReceiptBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        if (book.Status != "Returned")
            return BadRequest(new { message = "يجب إرجاع الدفتر أولاً قبل التحقق منه" });

        book.IsVerified = true;
        book.VerifiedAt = DateTime.UtcNow;
        book.VerifiedByUserId = UserId;

        _db.BookMovements.Add(new BookMovement
        {
            BookId = id,
            ActionType = "Verified",
            PerformedByUserId = UserId,
            PerformedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تحقق من دفتر", "ReceiptBook", id);

        return Ok(new { message = "تم التحقق من الدفتر بنجاح ✓" });
    }

    // ════════════════════════════════════════════
    // DRIVER PORTFOLIO — All books for a driver
    // ════════════════════════════════════════════

    [HttpGet("driver/{driverId}/portfolio")]
    public async Task<IActionResult> GetDriverPortfolio(int driverId)
    {
        var driver = await _db.Drivers.FindAsync(driverId);
        if (driver == null) return NotFound(new { message = "السائق غير موجود" });

        var allBooks = await _db.ReceiptBooks
            .Where(b => b.AssignedToDriverId == driverId)
            .Include(b => b.Series)
            .Include(b => b.Receipts)
            .Include(b => b.Gaps)
            .OrderBy(b => b.StartReceiptNumber)
            .ToListAsync();

        DriverPortfolioBookDto MapBook(ReceiptBook b)
        {
            int total = b.EndReceiptNumber - b.StartReceiptNumber + 1;
            int used = b.Receipts.Count;
            int missing = b.Gaps.Count(g => g.Status == "Open");
            string display = b.Series != null ? $"{b.Series.SeriesCode}-{b.BookNumber}" : $"#{b.BookNumber}";

            return new DriverPortfolioBookDto
            {
                BookId = b.BookId,
                BookNumber = b.BookNumber,
                DisplayName = display,
                StartReceiptNumber = b.StartReceiptNumber,
                EndReceiptNumber = b.EndReceiptNumber,
                Status = b.Status,
                UsedReceipts = used,
                RemainingReceipts = total - used,
                MissingReceipts = missing,
                UsagePercent = total > 0 ? Math.Round((double)used / total * 100, 1) : 0,
                AssignedDate = b.AssignedDate,
                ReturnedDate = b.ReturnedDate
            };
        }

        var current = allBooks.Where(b => b.Status == "Assigned" || b.Status == "InProgress").Select(MapBook).ToList();
        var returned = allBooks.Where(b => b.Status == "Returned" || b.Status == "Completed").Select(MapBook).ToList();

        return Ok(new DriverPortfolioDto
        {
            DriverId = driverId,
            DriverName = driver.FullName,
            CurrentBooks = current,
            ReturnedBooks = returned,
            TotalCurrentBooks = current.Count,
            TotalUsedReceipts = current.Sum(b => b.UsedReceipts),
            TotalRemainingReceipts = current.Sum(b => b.RemainingReceipts)
        });
    }

    /// <summary>
    /// Get activity history for a specific book from movement history + audit log
    /// </summary>
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var book = await _db.ReceiptBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        // Get movement history (structured)
        var movementsRaw = await _db.BookMovements
            .Where(m => m.BookId == id)
            .OrderByDescending(m => m.PerformedAt)
            .Include(m => m.FromDriver)
            .Include(m => m.ToDriver)
            .Include(m => m.PerformedByUser)
            .Take(50)
            .ToListAsync();

        var movements = movementsRaw.Select(m => new
            {
                action = m.ActionType switch
                {
                    "Delivered" => "📋 تسليم",
                    "Returned" => "↩️ إرجاع",
                    "Transferred" => "🔄 نقل",
                    "Verified" => "✓ تحقق",
                    "Closed" => "🔒 إغلاق",
                    _ => m.ActionType
                },
                userName = m.PerformedByUser?.FullName ?? "—",
                date = m.PerformedAt,
                details = m.Notes ?? "",
                fromDriver = m.FromDriver?.FullName ?? "",
                toDriver = m.ToDriver?.FullName ?? ""
            }).ToList();

        // Fallback: if no movements, use audit log
        if (movements.Count == 0)
        {
            var logs = await _db.AuditLogs
                .Where(l => l.EntityType == "ReceiptBook" && l.EntityId == id)
                .OrderByDescending(l => l.CreatedAt)
                .Include(l => l.User)
                .Take(50)
                .Select(l => new
                {
                    action = l.Action,
                    userName = l.User.FullName,
                    date = l.CreatedAt,
                    details = l.NewValues ?? "",
                    fromDriver = "",
                    toDriver = ""
                })
                .ToListAsync();

            return Ok(logs);
        }

        return Ok(movements);
    }
}


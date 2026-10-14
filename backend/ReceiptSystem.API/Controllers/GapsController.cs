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
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Treasury")]
public class GapsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public GapsController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        var query = _db.ReceiptGaps
            .Include(g => g.Driver)
            .Include(g => g.Book)
            .Include(g => g.ResolvedByUser)
            .Include(g => g.CancelledByUser)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(g => g.Status == status);

        var gaps = await query
            .OrderByDescending(g => g.DetectedAt)
            .Select(g => new GapDto
            {
                GapId = g.GapId,
                MissingReceiptNumber = g.MissingReceiptNumber,
                DetectedInSessionId = g.DetectedInSessionId,
                DriverId = g.DriverId,
                DriverName = g.Driver.FullName,
                BookId = g.BookId,
                BookNumber = g.Book != null ? g.Book.BookNumber : null,
                ReasonCategory = g.ReasonCategory,
                DetectedAt = g.DetectedAt,
                Status = g.Status,
                Resolution = g.Resolution,
                ResolvedAt = g.ResolvedAt,
                ResolvedByName = g.ResolvedByUser != null ? g.ResolvedByUser.FullName : null,
                CancelledByReason = g.CancelledByReason,
                CancelledByName = g.CancelledByUser != null ? g.CancelledByUser.FullName : null,
                CancelledAt = g.CancelledAt
            })
            .ToListAsync();
        return Ok(gaps);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = new GapSummaryDto
        {
            OpenCount = await _db.ReceiptGaps.CountAsync(g => g.Status == "Open"),
            UnderInvestigationCount = await _db.ReceiptGaps.CountAsync(g => g.Status == "UnderInvestigation"),
            ResolvedCount = await _db.ReceiptGaps.CountAsync(g => g.Status == "Resolved" || g.Status == "Explained"),
            CancelledCount = await _db.ReceiptGaps.CountAsync(g => g.Status == "Cancelled"),
            TotalCount = await _db.ReceiptGaps.CountAsync()
        };
        return Ok(summary);
    }

    [HttpPut("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id, [FromBody] ResolveGapDto dto)
    {
        var gap = await _db.ReceiptGaps.FindAsync(id);
        if (gap == null) return NotFound(new { message = "السجل غير موجود" });

        gap.Status = dto.Status;
        gap.Resolution = dto.Resolution;
        gap.ResolvedAt = DateTime.UtcNow;
        gap.ResolvedByUserId = UserId;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "ResolveGap", "ReceiptGap", id, newValues: dto);

        return Ok(new { message = "تم تحديث حالة الإيصال المفقود بنجاح" });
    }

    /// <summary>
    /// Cancel (لاغي) one or more receipt numbers in a book.
    /// Creates or updates ReceiptGap records with Status="Cancelled".
    /// Prevents cancellation of numbers already used for actual collections.
    /// </summary>
    [HttpPost("cancel")]
    [Authorize(Roles = "Admin,Treasury")]
    public async Task<IActionResult> CancelReceipts([FromBody] CancelReceiptsDto dto)
    {
        // Validate reason category
        var validReasons = new[] { "Cancelled_BookArchive", "Cancelled_Damaged", "Cancelled_Other" };
        if (!validReasons.Contains(dto.ReasonCategory))
            return BadRequest(new { message = "سبب الإلغاء غير صالح" });

        // Load the book
        var book = await _db.ReceiptBooks
            .Include(b => b.AssignedToDriver)
            .FirstOrDefaultAsync(b => b.BookId == dto.BookId);
        if (book == null)
            return NotFound(new { message = "الدفتر غير موجود" });

        // Validate all numbers are within the book range
        var outOfRange = dto.ReceiptNumbers
            .Where(n => n < book.StartReceiptNumber || n > book.EndReceiptNumber)
            .ToList();
        if (outOfRange.Any())
            return BadRequest(new { message = $"الأرقام التالية خارج نطاق الدفتر: {string.Join(", ", outOfRange)}" });

        // Check which numbers are already used for collections (Receipt records exist)
        var usedNumbers = await _db.Receipts
            .Where(r => dto.ReceiptNumbers.Contains(r.ReceiptNumber) && r.BookId == dto.BookId)
            .Select(r => r.ReceiptNumber)
            .ToListAsync();

        if (usedNumbers.Any())
        {
            return BadRequest(new CancelReceiptsResponseDto
            {
                Success = false,
                ConflictNumbers = usedNumbers,
                Error = $"لا يمكن إلغاء إيصالات مستخدمة بالفعل: {string.Join(", ", usedNumbers)}"
            });
        }

        // Check which numbers are already cancelled (idempotent)
        var existingGaps = await _db.ReceiptGaps
            .Where(g => dto.ReceiptNumbers.Contains(g.MissingReceiptNumber) && g.BookId == dto.BookId)
            .ToListAsync();

        int cancelledCount = 0;
        int alreadyCancelledCount = 0;

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var receiptNum in dto.ReceiptNumbers)
            {
                var existingGap = existingGaps.FirstOrDefault(g => g.MissingReceiptNumber == receiptNum);

                if (existingGap != null)
                {
                    if (existingGap.Status == "Cancelled")
                    {
                        alreadyCancelledCount++;
                        continue;
                    }

                    // Update existing gap (e.g., Open -> Cancelled)
                    existingGap.Status = "Cancelled";
                    existingGap.ReasonCategory = dto.ReasonCategory;
                    existingGap.CancelledByReason = dto.Reason;
                    existingGap.CancelledByUserId = UserId;
                    existingGap.CancelledAt = DateTime.UtcNow;
                    cancelledCount++;
                }
                else
                {
                    // Create new gap record as Cancelled
                    _db.ReceiptGaps.Add(new ReceiptGap
                    {
                        MissingReceiptNumber = receiptNum,
                        BookId = dto.BookId,
                        DriverId = book.AssignedToDriverId ?? 0,
                        DetectedInSessionId = null,
                        DetectedAt = DateTime.UtcNow,
                        Status = "Cancelled",
                        ReasonCategory = dto.ReasonCategory,
                        CancelledByReason = dto.Reason,
                        CancelledByUserId = UserId,
                        CancelledAt = DateTime.UtcNow
                    });
                    cancelledCount++;
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            await _audit.LogAsync(UserId, "إلغاء إيصالات (لاغي)", "ReceiptBook", dto.BookId,
                newValues: new { dto.ReceiptNumbers, dto.ReasonCategory, CancelledCount = cancelledCount });

            return Ok(new CancelReceiptsResponseDto
            {
                Success = true,
                CancelledCount = cancelledCount,
                AlreadyCancelledCount = alreadyCancelledCount
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = $"خطأ أثناء إلغاء الإيصالات: {ex.Message}" });
        }
    }
}

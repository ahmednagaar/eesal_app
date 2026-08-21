using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;
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
            .Include(g => g.ResolvedByUser)
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
                DetectedAt = g.DetectedAt,
                Status = g.Status,
                Resolution = g.Resolution,
                ResolvedAt = g.ResolvedAt,
                ResolvedByName = g.ResolvedByUser != null ? g.ResolvedByUser.FullName : null
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
}

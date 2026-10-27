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
[Route("api/camera-audits")]
[Authorize(Roles = "Admin,Camera")]
public class CameraAuditsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public CameraAuditsController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // GET api/camera-audits?driverId=1&status=HasDiscrepancies&from=2026-01-01&to=2026-12-31
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? driverId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var query = _db.LoadingAudits
            .Include(a => a.Driver)
            .Include(a => a.ReviewedByUser)
            .Include(a => a.Discrepancies)
            .AsQueryable();

        if (driverId.HasValue) query = query.Where(a => a.DriverId == driverId.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);
        if (from.HasValue) query = query.Where(a => a.AuditDate >= from.Value);
        if (to.HasValue) query = query.Where(a => a.AuditDate <= to.Value);

        var audits = await query
            .OrderByDescending(a => a.AuditDate).ThenByDescending(a => a.CreatedAt)
            .Select(a => new LoadingAuditListDto
            {
                AuditId = a.AuditId,
                AuditDate = a.AuditDate,
                DriverId = a.DriverId,
                DriverName = a.Driver.FullName,
                TripReference = a.TripReference,
                Status = a.Status,
                ReviewedByUserName = a.ReviewedByUser.FullName,
                ReviewedAt = a.ReviewedAt,
                DiscrepancyCount = a.Discrepancies.Count,
                OpenDiscrepancyCount = a.Discrepancies.Count(d => d.Status == "Open" || d.Status == "UnderInvestigation"),
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
        return Ok(audits);
    }

    // GET api/camera-audits/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var audit = await _db.LoadingAudits
            .Include(a => a.Driver)
            .Include(a => a.ReviewedByUser)
            .Include(a => a.Discrepancies).ThenInclude(d => d.Product).ThenInclude(p => p.Category)
            .Include(a => a.Discrepancies).ThenInclude(d => d.ProductUnit).ThenInclude(pu => pu.Unit)
            .Include(a => a.Discrepancies).ThenInclude(d => d.ResolvedByUser)
            .Include(a => a.Discrepancies).ThenInclude(d => d.CaseNotes)
            .Where(a => a.AuditId == id)
            .FirstOrDefaultAsync();

        if (audit == null) return NotFound(new { message = "المراجعة غير موجودة" });

        return Ok(new LoadingAuditDto
        {
            AuditId = audit.AuditId,
            AuditDate = audit.AuditDate,
            DriverId = audit.DriverId,
            DriverName = audit.Driver.FullName,
            TripReference = audit.TripReference,
            Status = audit.Status,
            ReviewedByUserName = audit.ReviewedByUser.FullName,
            ReviewedAt = audit.ReviewedAt,
            Notes = audit.Notes,
            CreatedAt = audit.CreatedAt,
            Discrepancies = audit.Discrepancies.Select(d => new LoadingDiscrepancyDto
            {
                DiscrepancyId = d.DiscrepancyId,
                ProductId = d.ProductId,
                ProductName = d.Product.ProductName,
                CategoryName = d.Product.Category.CategoryName,
                ProductUnitId = d.ProductUnitId,
                UnitName = d.ProductUnit.Unit.UnitName,
                ExpectedQty = d.ExpectedQty,
                ActualQty = d.ActualQty,
                DifferenceQty = d.DifferenceQty,
                DiscrepancyType = d.DiscrepancyType,
                Status = d.Status,
                Resolution = d.Resolution,
                ResolvedAt = d.ResolvedAt,
                ResolvedByUserName = d.ResolvedByUser?.FullName,
                Notes = d.Notes,
                CreatedAt = d.CreatedAt,
                CaseNoteCount = d.CaseNotes.Count
            }).ToList()
        });
    }

    // POST api/camera-audits — Create audit with optional discrepancies
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLoadingAuditDto dto)
    {
        var driver = await _db.Drivers.FindAsync(dto.DriverId);
        if (driver == null || !driver.IsActive)
            return BadRequest(new { message = "السائق غير موجود أو غير نشط" });

        var audit = new LoadingAudit
        {
            AuditDate = dto.AuditDate,
            DriverId = dto.DriverId,
            TripReference = dto.TripReference,
            Notes = dto.Notes,
            ReviewedByUserId = UserId,
            ReviewedAt = DateTime.UtcNow,
            Status = "Draft"
        };

        if (dto.Discrepancies != null && dto.Discrepancies.Count > 0)
        {
            foreach (var disc in dto.Discrepancies)
            {
                audit.Discrepancies.Add(new LoadingDiscrepancy
                {
                    ProductId = disc.ProductId,
                    ProductUnitId = disc.ProductUnitId,
                    ExpectedQty = disc.ExpectedQty,
                    ActualQty = disc.ActualQty,
                    DifferenceQty = disc.ActualQty - disc.ExpectedQty,
                    DiscrepancyType = disc.DiscrepancyType,
                    Notes = disc.Notes,
                    Status = "Open"
                });
            }
            audit.Status = "HasDiscrepancies";
        }
        else
        {
            audit.Status = "Completed";
        }

        _db.LoadingAudits.Add(audit);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(UserId, "إضافة مراجعة كاميرا", "LoadingAudit", audit.AuditId,
            newValues: new { dto.DriverId, DriverName = driver.FullName, dto.AuditDate, DiscrepancyCount = audit.Discrepancies.Count });

        return CreatedAtAction(nameof(Get), new { id = audit.AuditId },
            new { audit.AuditId, audit.Status, DiscrepancyCount = audit.Discrepancies.Count });
    }

    // POST api/camera-audits/5/discrepancies — Add a discrepancy to an existing audit
    [HttpPost("{auditId}/discrepancies")]
    public async Task<IActionResult> AddDiscrepancy(int auditId, [FromBody] CreateDiscrepancyDto dto)
    {
        var audit = await _db.LoadingAudits.FindAsync(auditId);
        if (audit == null) return NotFound(new { message = "المراجعة غير موجودة" });

        var disc = new LoadingDiscrepancy
        {
            AuditId = auditId,
            ProductId = dto.ProductId,
            ProductUnitId = dto.ProductUnitId,
            ExpectedQty = dto.ExpectedQty,
            ActualQty = dto.ActualQty,
            DifferenceQty = dto.ActualQty - dto.ExpectedQty,
            DiscrepancyType = dto.DiscrepancyType,
            Notes = dto.Notes,
            Status = "Open"
        };

        _db.LoadingDiscrepancies.Add(disc);
        audit.Status = "HasDiscrepancies";
        await _db.SaveChangesAsync();

        await _audit.LogAsync(UserId, "إضافة فرق في المراجعة", "LoadingDiscrepancy", disc.DiscrepancyId,
            newValues: new { dto.ProductId, dto.ExpectedQty, dto.ActualQty, dto.DiscrepancyType });

        return Ok(new { disc.DiscrepancyId, message = "تم إضافة الفرق بنجاح" });
    }

    // PUT api/camera-audits/discrepancies/5/status — Update discrepancy status (case lifecycle)
    [HttpPut("discrepancies/{discrepancyId}/status")]
    public async Task<IActionResult> UpdateDiscrepancyStatus(int discrepancyId, [FromBody] UpdateDiscrepancyStatusDto dto)
    {
        var disc = await _db.LoadingDiscrepancies.FindAsync(discrepancyId);
        if (disc == null) return NotFound(new { message = "الفرق غير موجود" });

        var validStatuses = new[] { "Open", "UnderInvestigation", "Resolved", "Closed" };
        if (!validStatuses.Contains(dto.Status))
            return BadRequest(new { message = "حالة غير صالحة" });

        var oldStatus = disc.Status;
        disc.Status = dto.Status;

        if (dto.Status == "Resolved" || dto.Status == "Closed")
        {
            disc.Resolution = dto.Resolution;
            disc.ResolvedAt = DateTime.UtcNow;
            disc.ResolvedByUserId = UserId;
        }

        // Add a case note for the status change
        if (!string.IsNullOrEmpty(dto.NoteText) || oldStatus != dto.Status)
        {
            _db.CaseNotes.Add(new CaseNote
            {
                DiscrepancyId = discrepancyId,
                NoteText = dto.NoteText ?? $"تغيير الحالة من {oldStatus} إلى {dto.Status}",
                StatusChange = $"{oldStatus}→{dto.Status}",
                CreatedByUserId = UserId
            });
        }

        await _db.SaveChangesAsync();

        // Check if all discrepancies in audit are resolved/closed — auto-complete audit
        var audit = await _db.LoadingAudits
            .Include(a => a.Discrepancies)
            .FirstOrDefaultAsync(a => a.AuditId == disc.AuditId);
        if (audit != null && audit.Discrepancies.All(d => d.Status == "Resolved" || d.Status == "Closed"))
        {
            audit.Status = "Completed";
            await _db.SaveChangesAsync();
        }

        await _audit.LogAsync(UserId, "تحديث حالة فرق", "LoadingDiscrepancy", discrepancyId,
            oldValues: new { Status = oldStatus }, newValues: new { dto.Status, dto.Resolution });

        return Ok(new { message = "تم تحديث الحالة بنجاح" });
    }

    // GET api/camera-audits/discrepancies/5/notes — Get all case notes
    [HttpGet("discrepancies/{discrepancyId}/notes")]
    public async Task<IActionResult> GetCaseNotes(int discrepancyId)
    {
        var notes = await _db.CaseNotes
            .Where(n => n.DiscrepancyId == discrepancyId)
            .Include(n => n.CreatedByUser)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new CaseNoteDto
            {
                NoteId = n.NoteId,
                NoteText = n.NoteText,
                StatusChange = n.StatusChange,
                CreatedByUserName = n.CreatedByUser.FullName,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
        return Ok(notes);
    }

    // POST api/camera-audits/discrepancies/5/notes — Add a case note
    [HttpPost("discrepancies/{discrepancyId}/notes")]
    public async Task<IActionResult> AddCaseNote(int discrepancyId, [FromBody] CreateCaseNoteDto dto)
    {
        var disc = await _db.LoadingDiscrepancies.FindAsync(discrepancyId);
        if (disc == null) return NotFound(new { message = "الفرق غير موجود" });

        var note = new CaseNote
        {
            DiscrepancyId = discrepancyId,
            NoteText = dto.NoteText,
            CreatedByUserId = UserId
        };

        _db.CaseNotes.Add(note);
        await _db.SaveChangesAsync();

        return Ok(new { note.NoteId, message = "تم إضافة الملاحظة بنجاح" });
    }

    // DELETE api/camera-audits/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var audit = await _db.LoadingAudits
            .Include(a => a.Discrepancies).ThenInclude(d => d.CaseNotes)
            .FirstOrDefaultAsync(a => a.AuditId == id);
        if (audit == null) return NotFound(new { message = "المراجعة غير موجودة" });

        // Only delete if draft or no open discrepancies
        if (audit.Discrepancies.Any(d => d.Status == "Open" || d.Status == "UnderInvestigation"))
            return BadRequest(new { message = "لا يمكن حذف مراجعة بها فروقات مفتوحة" });

        foreach (var disc in audit.Discrepancies)
            _db.CaseNotes.RemoveRange(disc.CaseNotes);
        _db.LoadingDiscrepancies.RemoveRange(audit.Discrepancies);
        _db.LoadingAudits.Remove(audit);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(UserId, "حذف مراجعة كاميرا", "LoadingAudit", id);
        return Ok(new { message = "تم حذف المراجعة بنجاح" });
    }
}

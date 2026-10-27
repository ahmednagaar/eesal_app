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
[Route("api/return-reasons")]
[Authorize]
public class ReturnReasonsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    public ReturnReasonsController(AppDbContext db, AuditService audit) { _db = db; _audit = audit; }
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var query = _db.ReturnReasonCategories.Include(r => r.CreatedByUser).AsQueryable();
        if (!includeInactive) query = query.Where(r => r.IsActive);
        var list = await query.OrderBy(r => r.SortOrder).ThenBy(r => r.ReasonName)
            .Select(r => new ReturnReasonCategoryDto
            {
                ReasonId = r.ReasonId, ReasonName = r.ReasonName, ReasonNameEn = r.ReasonNameEn,
                IsActive = r.IsActive, SortOrder = r.SortOrder, IsSystemDefault = r.IsSystemDefault,
                CreatedAt = r.CreatedAt, CreatedByUserName = r.CreatedByUser != null ? r.CreatedByUser.FullName : "النظام"
            }).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReturnReasonCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var reason = new ReturnReasonCategory { ReasonName = dto.ReasonName, ReasonNameEn = dto.ReasonNameEn, SortOrder = dto.SortOrder, CreatedByUserId = UserId };
        _db.ReturnReasonCategories.Add(reason);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إضافة سبب مرتجع", "ReturnReasonCategory", reason.ReasonId, newValues: new { dto.ReasonName });
        return CreatedAtAction(nameof(GetAll), new { reason.ReasonId, reason.ReasonName });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReturnReasonCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var reason = await _db.ReturnReasonCategories.FindAsync(id);
        if (reason == null) return NotFound(new { message = "السبب غير موجود" });
        reason.ReasonName = dto.ReasonName; reason.ReasonNameEn = dto.ReasonNameEn;
        reason.IsActive = dto.IsActive; reason.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم التحديث بنجاح" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var reason = await _db.ReturnReasonCategories.FindAsync(id);
        if (reason == null) return NotFound(new { message = "السبب غير موجود" });
        if (reason.IsSystemDefault) return BadRequest(new { message = "لا يمكن حذف سبب افتراضي" });
        _db.ReturnReasonCategories.Remove(reason);
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم الحذف بنجاح" });
    }
}

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
[Route("api/units-of-measure")]
[Authorize]
public class UnitsOfMeasureController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    public UnitsOfMeasureController(AppDbContext db, AuditService audit) { _db = db; _audit = audit; }
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var query = _db.UnitsOfMeasure.AsQueryable();
        if (!includeInactive) query = query.Where(u => u.IsActive);
        var list = await query.OrderBy(u => u.SortOrder).ThenBy(u => u.UnitName)
            .Select(u => new UnitOfMeasureDto
            {
                UnitId = u.UnitId, UnitName = u.UnitName, UnitAbbreviation = u.UnitAbbreviation,
                IsActive = u.IsActive, SortOrder = u.SortOrder
            }).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUnitOfMeasureDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var unit = new UnitOfMeasure { UnitName = dto.UnitName, UnitAbbreviation = dto.UnitAbbreviation, SortOrder = dto.SortOrder };
        _db.UnitsOfMeasure.Add(unit);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إضافة وحدة قياس", "UnitOfMeasure", unit.UnitId, newValues: new { dto.UnitName });
        return CreatedAtAction(nameof(GetAll), new { unit.UnitId, unit.UnitName });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUnitOfMeasureDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var unit = await _db.UnitsOfMeasure.FindAsync(id);
        if (unit == null) return NotFound(new { message = "الوحدة غير موجودة" });
        unit.UnitName = dto.UnitName; unit.UnitAbbreviation = dto.UnitAbbreviation;
        unit.IsActive = dto.IsActive; unit.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم التحديث بنجاح" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var unit = await _db.UnitsOfMeasure.FindAsync(id);
        if (unit == null) return NotFound(new { message = "الوحدة غير موجودة" });
        bool inUse = await _db.ProductUnits.AnyAsync(pu => pu.UnitId == id);
        if (inUse) return BadRequest(new { message = "لا يمكن حذف — الوحدة مستخدمة في أصناف" });
        _db.UnitsOfMeasure.Remove(unit);
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم الحذف بنجاح" });
    }
}

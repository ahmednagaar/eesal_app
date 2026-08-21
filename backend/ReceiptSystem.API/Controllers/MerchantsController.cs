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
[Authorize]
public class MerchantsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public MerchantsController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Merchants.Where(m => m.IsActive);
        var total = await query.CountAsync();
        var merchants = await query
            .OrderBy(m => m.MerchantName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MerchantDto
            {
                MerchantId = m.MerchantId,
                MerchantName = m.MerchantName,
                City = m.City,
                PhoneNumber = m.PhoneNumber,
                IsActive = m.IsActive,
                Notes = m.Notes
            })
            .ToListAsync();
        return Ok(new { data = merchants, total, page, pageSize });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "")
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(new List<MerchantDto>());
        var merchants = await _db.Merchants
            .Where(m => m.IsActive && m.MerchantName.Contains(q))
            .OrderBy(m => m.MerchantName)
            .Take(10)
            .Select(m => new MerchantDto
            {
                MerchantId = m.MerchantId,
                MerchantName = m.MerchantName,
                City = m.City,
                PhoneNumber = m.PhoneNumber,
                IsActive = m.IsActive
            })
            .ToListAsync();
        return Ok(merchants);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMerchantDto dto)
    {
        var merchant = new Merchant
        {
            MerchantName = dto.MerchantName,
            City = dto.City,
            PhoneNumber = dto.PhoneNumber,
            Notes = dto.Notes
        };
        _db.Merchants.Add(merchant);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إضافة", "Merchant", merchant.MerchantId, newValues: dto);
        return CreatedAtAction(nameof(GetAll), null, new MerchantDto
        {
            MerchantId = merchant.MerchantId,
            MerchantName = merchant.MerchantName,
            City = merchant.City,
            PhoneNumber = merchant.PhoneNumber,
            IsActive = merchant.IsActive,
            Notes = merchant.Notes
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateMerchantDto dto)
    {
        var merchant = await _db.Merchants.FindAsync(id);
        if (merchant == null) return NotFound(new { message = "التاجر غير موجود" });
        var old = new { merchant.MerchantName, merchant.City, merchant.PhoneNumber };
        merchant.MerchantName = dto.MerchantName;
        merchant.City = dto.City;
        merchant.PhoneNumber = dto.PhoneNumber;
        merchant.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تعديل", "Merchant", id, oldValues: old, newValues: dto);
        return Ok(new { message = "تم تحديث بيانات التاجر بنجاح" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Treasury")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var merchant = await _db.Merchants.FindAsync(id);
        if (merchant == null) return NotFound(new { message = "التاجر غير موجود" });

        merchant.IsActive = false;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "DeactivateMerchant", "Merchant", id);

        return Ok(new { message = "تم تعطيل التاجر بنجاح" });
    }
}

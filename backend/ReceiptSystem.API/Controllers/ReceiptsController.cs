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
public class ReceiptsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public ReceiptsController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] int? number = null)
    {
        if (!number.HasValue) return BadRequest(new { message = "رقم الإيصال مطلوب" });

        var receipt = await _db.Receipts
            .Include(r => r.Merchant)
            .Include(r => r.Driver)
            .Where(r => r.ReceiptNumber == number.Value)
            .Select(r => new
            {
                r.ReceiptId,
                r.ReceiptNumber,
                r.Amount,
                MerchantName = r.Merchant.MerchantName,
                DriverName = r.Driver.FullName,
                r.CollectionDate,
                r.SessionId
            })
            .FirstOrDefaultAsync();

        if (receipt == null) return Ok(new { exists = false });
        return Ok(new { exists = true, receipt });
    }

    [HttpGet("check-duplicate/{number}")]
    public async Task<IActionResult> CheckDuplicate(int number)
    {
        var exists = await _db.Receipts.AnyAsync(r => r.ReceiptNumber == number);
        return Ok(new { exists });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateReceiptDto dto)
    {
        var receipt = await _db.Receipts.Include(r => r.Session).FirstOrDefaultAsync(r => r.ReceiptId == id);
        if (receipt == null) return NotFound(new { message = "الإيصال غير موجود" });
        if (receipt.Session.IsConfirmed && !User.IsInRole("Admin"))
            return StatusCode(403, new { message = "الجلسة مقفلة ولا يمكن تعديلها" });
        var old = new { receipt.Amount, receipt.MerchantId, receipt.IsPartialPayment, receipt.Notes };
        receipt.Amount = dto.Amount;
        receipt.MerchantId = dto.MerchantId;
        receipt.IsPartialPayment = dto.IsPartialPayment;
        receipt.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "UpdateReceipt", "Receipt", id, oldValues: old, newValues: dto);

        return Ok(new { message = "تم تحديث الإيصال بنجاح" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var receipt = await _db.Receipts.Include(r => r.Session).FirstOrDefaultAsync(r => r.ReceiptId == id);
        if (receipt == null) return NotFound(new { message = "الإيصال غير موجود" });
        if (receipt.Session.IsConfirmed)
            return StatusCode(403, new { message = "الجلسة مقفلة ولا يمكن تعديلها" });

        _db.Receipts.Remove(receipt);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "DeleteReceipt", "Receipt", id);

        return Ok(new { message = "تم حذف الإيصال بنجاح" });
    }
}

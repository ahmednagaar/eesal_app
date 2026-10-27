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
[Route("api/goods-returns")]
[Authorize(Roles = "Admin,Returns,Camera")]
public class GoodsReturnsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public GoodsReturnsController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // GET api/goods-returns?driverId=1&status=Open&from=2026-01-01&to=2026-12-31
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? driverId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var query = _db.GoodsReturns
            .Include(r => r.Driver)
            .Include(r => r.Reason)
            .Include(r => r.EnteredByUser)
            .Include(r => r.Items)
            .AsQueryable();

        if (driverId.HasValue)
            query = query.Where(r => r.DriverId == driverId.Value);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);
        if (from.HasValue)
            query = query.Where(r => r.ReturnDate >= from.Value);
        if (to.HasValue)
            query = query.Where(r => r.ReturnDate <= to.Value);

        var returns = await query
            .OrderByDescending(r => r.ReturnDate).ThenByDescending(r => r.EnteredAt)
            .Select(r => new GoodsReturnListDto
            {
                ReturnId = r.ReturnId,
                ReturnReceiptNumber = r.ReturnReceiptNumber,
                DriverId = r.DriverId,
                DriverName = r.Driver.FullName,
                ReasonName = r.Reason.ReasonName,
                ReturnDate = r.ReturnDate,
                Status = r.Status,
                CustomerName = r.CustomerName,
                TotalAmount = r.TotalAmount,
                ItemCount = r.Items.Count,
                EnteredAt = r.EnteredAt,
                EnteredByUserName = r.EnteredByUser.FullName
            })
            .ToListAsync();
        return Ok(returns);
    }

    // GET api/goods-returns/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var ret = await _db.GoodsReturns
            .Include(r => r.Driver)
            .Include(r => r.Reason)
            .Include(r => r.EnteredByUser)
            .Include(r => r.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Category)
            .Include(r => r.Items).ThenInclude(i => i.ProductUnit).ThenInclude(pu => pu.Unit)
            .Where(r => r.ReturnId == id)
            .FirstOrDefaultAsync();

        if (ret == null) return NotFound(new { message = "المرتجع غير موجود" });

        return Ok(new GoodsReturnDto
        {
            ReturnId = ret.ReturnId,
            ReturnReceiptNumber = ret.ReturnReceiptNumber,
            BookId = ret.BookId,
            DriverId = ret.DriverId,
            DriverName = ret.Driver.FullName,
            ReasonId = ret.ReasonId,
            ReasonName = ret.Reason.ReasonName,
            ReturnDate = ret.ReturnDate,
            Status = ret.Status,
            CustomerName = ret.CustomerName,
            Notes = ret.Notes,
            TotalAmount = ret.TotalAmount,
            EnteredAt = ret.EnteredAt,
            EnteredByUserName = ret.EnteredByUser.FullName,
            Items = ret.Items.Select(i => new GoodsReturnItemDto
            {
                ItemId = i.ItemId,
                ProductId = i.ProductId,
                ProductName = i.Product.ProductName,
                CategoryName = i.Product.Category.CategoryName,
                ProductUnitId = i.ProductUnitId,
                UnitName = i.ProductUnit.Unit.UnitName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                Notes = i.Notes
            }).ToList()
        });
    }

    // POST api/goods-returns — Create a return with items
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoodsReturnDto dto)
    {
        // Validate driver
        var driver = await _db.Drivers.FindAsync(dto.DriverId);
        if (driver == null || !driver.IsActive)
            return BadRequest(new { message = "السائق غير موجود أو غير نشط" });

        // Validate reason
        var reason = await _db.ReturnReasonCategories.FindAsync(dto.ReasonId);
        if (reason == null || !reason.IsActive)
            return BadRequest(new { message = "سبب المرتجع غير موجود أو غير نشط" });

        // Validate book (optional)
        if (dto.BookId.HasValue)
        {
            var book = await _db.ReturnReceiptBooks.FindAsync(dto.BookId.Value);
            if (book == null) return BadRequest(new { message = "دفتر المرتجعات غير موجود" });
        }

        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest(new { message = "يجب إضافة صنف واحد على الأقل" });

        // Calculate total
        decimal totalAmount = dto.Items.Sum(i => i.TotalPrice);

        var goodsReturn = new GoodsReturn
        {
            ReturnReceiptNumber = dto.ReturnReceiptNumber,
            BookId = dto.BookId,
            DriverId = dto.DriverId,
            ReasonId = dto.ReasonId,
            ReturnDate = dto.ReturnDate,
            CustomerName = dto.CustomerName,
            Notes = dto.Notes,
            TotalAmount = totalAmount,
            EnteredByUserId = UserId
        };

        foreach (var itemDto in dto.Items)
        {
            goodsReturn.Items.Add(new GoodsReturnItem
            {
                ProductId = itemDto.ProductId,
                ProductUnitId = itemDto.ProductUnitId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                TotalPrice = itemDto.TotalPrice,
                Notes = itemDto.Notes
            });
        }

        _db.GoodsReturns.Add(goodsReturn);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(UserId, "إضافة مرتجع", "GoodsReturn", goodsReturn.ReturnId,
            newValues: new { dto.ReturnReceiptNumber, dto.DriverId, DriverName = driver.FullName, dto.ReasonId, ItemCount = dto.Items.Count, totalAmount });

        return CreatedAtAction(nameof(Get), new { id = goodsReturn.ReturnId },
            new { goodsReturn.ReturnId, goodsReturn.ReturnReceiptNumber, goodsReturn.TotalAmount });
    }

    // PUT api/goods-returns/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGoodsReturnDto dto)
    {
        var ret = await _db.GoodsReturns.FindAsync(id);
        if (ret == null) return NotFound(new { message = "المرتجع غير موجود" });

        var old = new { ret.ReasonId, ret.CustomerName, ret.Notes, ret.Status };
        ret.ReasonId = dto.ReasonId;
        ret.CustomerName = dto.CustomerName;
        ret.Notes = dto.Notes;
        ret.Status = dto.Status;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تعديل مرتجع", "GoodsReturn", id, oldValues: old, newValues: dto);

        return Ok(new { message = "تم تحديث المرتجع بنجاح" });
    }

    // DELETE api/goods-returns/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ret = await _db.GoodsReturns.Include(r => r.Items).FirstOrDefaultAsync(r => r.ReturnId == id);
        if (ret == null) return NotFound(new { message = "المرتجع غير موجود" });

        if (ret.Status != "Open")
            return BadRequest(new { message = "لا يمكن حذف مرتجع ليس في حالة مفتوح" });

        _db.GoodsReturnItems.RemoveRange(ret.Items);
        _db.GoodsReturns.Remove(ret);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "حذف مرتجع", "GoodsReturn", id);

        return Ok(new { message = "تم حذف المرتجع بنجاح" });
    }
}

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
[Route("api/product-categories")]
[Authorize]
public class ProductCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    public ProductCategoriesController(AppDbContext db, AuditService audit) { _db = db; _audit = audit; }
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var query = _db.ProductCategories.Include(c => c.CreatedByUser).AsQueryable();
        if (!includeInactive) query = query.Where(c => c.IsActive);
        var list = await query.OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName)
            .Select(c => new ProductCategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder,
                CreatedAt = c.CreatedAt,
                CreatedByUserName = c.CreatedByUser.FullName,
                ProductCount = c.Products.Count
            }).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var cat = new ProductCategory { CategoryName = dto.CategoryName, SortOrder = dto.SortOrder, CreatedByUserId = UserId };
        _db.ProductCategories.Add(cat);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إضافة نوع صنف", "ProductCategory", cat.CategoryId, newValues: new { dto.CategoryName });
        return CreatedAtAction(nameof(GetAll), new { cat.CategoryId, cat.CategoryName });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCategoryDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var cat = await _db.ProductCategories.FindAsync(id);
        if (cat == null) return NotFound(new { message = "نوع الصنف غير موجود" });
        var old = new { cat.CategoryName, cat.IsActive };
        cat.CategoryName = dto.CategoryName;
        cat.IsActive = dto.IsActive;
        cat.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تعديل نوع صنف", "ProductCategory", id, oldValues: old, newValues: new { dto.CategoryName, dto.IsActive });
        return Ok(new { message = "تم التحديث بنجاح" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await _db.ProductCategories.Include(c => c.Products).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (cat == null) return NotFound(new { message = "نوع الصنف غير موجود" });
        if (cat.Products.Count > 0) return BadRequest(new { message = $"لا يمكن حذف — يوجد {cat.Products.Count} صنف مرتبط" });
        _db.ProductCategories.Remove(cat);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "حذف نوع صنف", "ProductCategory", id);
        return Ok(new { message = "تم الحذف بنجاح" });
    }
}

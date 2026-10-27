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
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    public ProductsController(AppDbContext db, AuditService audit) { _db = db; _audit = audit; }
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? categoryId = null, [FromQuery] bool includeInactive = false)
    {
        var query = _db.Products.Include(p => p.Category).Include(p => p.ProductUnits).ThenInclude(pu => pu.Unit).AsQueryable();
        if (!includeInactive) query = query.Where(p => p.IsActive);
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);

        var list = await query.OrderBy(p => p.SortOrder).ThenBy(p => p.ProductName)
            .Select(p => new ProductListDto
            {
                ProductId = p.ProductId,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.CategoryName,
                ProductName = p.ProductName,
                ProductCode = p.ProductCode,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                DefaultUnitName = p.ProductUnits.Where(u => u.IsDefault).Select(u => u.Unit.UnitName).FirstOrDefault(),
                UnitCount = p.ProductUnits.Count
            }).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var p = await _db.Products
            .Include(x => x.Category).Include(x => x.CreatedByUser)
            .Include(x => x.ProductUnits).ThenInclude(pu => pu.Unit)
            .FirstOrDefaultAsync(x => x.ProductId == id);
        if (p == null) return NotFound(new { message = "الصنف غير موجود" });

        return Ok(new ProductDto
        {
            ProductId = p.ProductId, CategoryId = p.CategoryId, CategoryName = p.Category.CategoryName,
            ProductName = p.ProductName, ProductCode = p.ProductCode, IsActive = p.IsActive,
            SortOrder = p.SortOrder, CreatedAt = p.CreatedAt, CreatedByUserName = p.CreatedByUser.FullName,
            Units = p.ProductUnits.Select(pu => new ProductUnitDto
            {
                ProductUnitId = pu.ProductUnitId, UnitId = pu.UnitId, UnitName = pu.Unit.UnitName,
                UnitAbbreviation = pu.Unit.UnitAbbreviation, IsDefault = pu.IsDefault,
                ConversionToBaseQty = pu.ConversionToBaseQty, BaseUnitId = pu.BaseUnitId, Notes = pu.Notes
            }).ToList()
        });
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Ok(new List<ProductSearchDto>());
        var results = await _db.Products.Include(p => p.Category).Include(p => p.ProductUnits).ThenInclude(pu => pu.Unit)
            .Where(p => p.IsActive && (p.ProductName.Contains(q) || (p.ProductCode != null && p.ProductCode.Contains(q))))
            .Take(20)
            .Select(p => new ProductSearchDto
            {
                ProductId = p.ProductId, ProductName = p.ProductName, CategoryName = p.Category.CategoryName,
                ProductCode = p.ProductCode,
                Units = p.ProductUnits.Select(pu => new ProductUnitDto
                {
                    ProductUnitId = pu.ProductUnitId, UnitId = pu.UnitId, UnitName = pu.Unit.UnitName,
                    UnitAbbreviation = pu.Unit.UnitAbbreviation, IsDefault = pu.IsDefault
                }).ToList()
            }).ToListAsync();
        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var product = new Product { CategoryId = dto.CategoryId, ProductName = dto.ProductName, ProductCode = dto.ProductCode, SortOrder = dto.SortOrder, CreatedByUserId = UserId };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إضافة صنف", "Product", product.ProductId, newValues: new { dto.ProductName, dto.ProductCode });
        return CreatedAtAction(nameof(Get), new { id = product.ProductId }, new { product.ProductId, product.ProductName });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound(new { message = "الصنف غير موجود" });
        var old = new { product.ProductName, product.IsActive };
        product.CategoryId = dto.CategoryId; product.ProductName = dto.ProductName;
        product.ProductCode = dto.ProductCode; product.IsActive = dto.IsActive; product.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تعديل صنف", "Product", id, oldValues: old, newValues: new { dto.ProductName, dto.IsActive });
        return Ok(new { message = "تم التحديث بنجاح" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound(new { message = "الصنف غير موجود" });
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "حذف صنف", "Product", id);
        return Ok(new { message = "تم الحذف بنجاح" });
    }

    // ═══ Product Units ═══
    [HttpPost("{productId}/units")]
    public async Task<IActionResult> AddUnit(int productId, [FromBody] CreateProductUnitDto dto)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null) return NotFound(new { message = "الصنف غير موجود" });
        var pu = new ProductUnit { ProductId = productId, UnitId = dto.UnitId, IsDefault = dto.IsDefault, ConversionToBaseQty = dto.ConversionToBaseQty, BaseUnitId = dto.BaseUnitId, Notes = dto.Notes };
        if (dto.IsDefault) await _db.ProductUnits.Where(u => u.ProductId == productId).ForEachAsync(u => u.IsDefault = false);
        _db.ProductUnits.Add(pu);
        await _db.SaveChangesAsync();
        return Ok(new { pu.ProductUnitId, message = "تم إضافة الوحدة" });
    }

    [HttpPut("{productId}/units/{productUnitId}")]
    public async Task<IActionResult> UpdateUnit(int productId, int productUnitId, [FromBody] UpdateProductUnitDto dto)
    {
        var pu = await _db.ProductUnits.FirstOrDefaultAsync(u => u.ProductUnitId == productUnitId && u.ProductId == productId);
        if (pu == null) return NotFound(new { message = "الوحدة غير موجودة" });
        if (dto.IsDefault) await _db.ProductUnits.Where(u => u.ProductId == productId && u.ProductUnitId != productUnitId).ForEachAsync(u => u.IsDefault = false);
        pu.IsDefault = dto.IsDefault; pu.ConversionToBaseQty = dto.ConversionToBaseQty; pu.BaseUnitId = dto.BaseUnitId; pu.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم تحديث الوحدة" });
    }

    [HttpDelete("{productId}/units/{productUnitId}")]
    public async Task<IActionResult> RemoveUnit(int productId, int productUnitId)
    {
        var pu = await _db.ProductUnits.FirstOrDefaultAsync(u => u.ProductUnitId == productUnitId && u.ProductId == productId);
        if (pu == null) return NotFound(new { message = "الوحدة غير موجودة" });
        _db.ProductUnits.Remove(pu);
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم حذف الوحدة" });
    }
}

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
[Route("api/no-receipt-collections")]
[Authorize(Roles = "Admin,Treasury")]
public class NoReceiptCollectionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public NoReceiptCollectionsController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    /// <summary>
    /// List all no-receipt collections, optionally filtered by batch or driver.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? batchId = null,
        [FromQuery] int? driverId = null)
    {
        var query = _db.NoReceiptCollections
            .Include(c => c.Driver)
            .Include(c => c.Merchant)
            .Include(c => c.EnteredByUser)
            .AsQueryable();

        if (batchId.HasValue)
            query = query.Where(c => c.BatchId == batchId.Value);
        if (driverId.HasValue)
            query = query.Where(c => c.DriverId == driverId.Value);

        var items = await query
            .OrderByDescending(c => c.EnteredAt)
            .Select(c => new NoReceiptCollectionDto
            {
                Id = c.Id,
                SessionId = c.SessionId,
                BatchId = c.BatchId,
                DriverId = c.DriverId,
                DriverName = c.Driver != null ? c.Driver.FullName : null,
                MerchantId = c.MerchantId,
                MerchantName = c.Merchant != null ? c.Merchant.MerchantName : null,
                MerchantNameRaw = c.MerchantNameRaw,
                Amount = c.Amount,
                CollectionDate = c.CollectionDate,
                Reason = c.Reason,
                Notes = c.Notes,
                EnteredByName = c.EnteredByUser.FullName,
                EnteredAt = c.EnteredAt
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Create one or more no-receipt collection records.
    /// Used from the ERP Import page when rows have no physical receipt.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoReceiptCollectionDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest(new { message = "يجب تحديد تحصيل واحد على الأقل" });

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var records = new List<NoReceiptCollection>();

            foreach (var item in dto.Items)
            {
                if (item.Amount <= 0)
                    return BadRequest(new { message = $"المبلغ يجب أن يكون أكبر من صفر" });

                var record = new NoReceiptCollection
                {
                    BatchId = dto.BatchId,
                    ImportRowId = item.ImportRowId,
                    DriverId = dto.DriverId,
                    MerchantId = item.MerchantId,
                    MerchantNameRaw = item.MerchantNameRaw,
                    Amount = item.Amount,
                    CollectionDate = item.CollectionDate ?? DateTime.Today,
                    Reason = dto.Reason,
                    EnteredByUserId = UserId,
                    EnteredAt = DateTime.UtcNow
                };
                _db.NoReceiptCollections.Add(record);
                records.Add(record);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            await _audit.LogAsync(UserId, "تسجيل تحصيل بدون إيصال", "NoReceiptCollection", 0,
                newValues: new { Count = records.Count, TotalAmount = records.Sum(r => r.Amount), dto.BatchId });

            return Ok(new CreateNoReceiptCollectionResponseDto
            {
                Success = true,
                CreatedCount = records.Count,
                TotalAmount = records.Sum(r => r.Amount)
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = $"خطأ أثناء تسجيل التحصيل: {ex.Message}" });
        }
    }

    /// <summary>
    /// Summary stats for a specific batch — how many no-receipt collections and their total.
    /// </summary>
    [HttpGet("batch/{batchId}/summary")]
    public async Task<IActionResult> GetBatchSummary(int batchId)
    {
        var collections = await _db.NoReceiptCollections
            .Where(c => c.BatchId == batchId)
            .ToListAsync();

        return Ok(new
        {
            count = collections.Count,
            totalAmount = collections.Sum(c => c.Amount)
        });
    }
}

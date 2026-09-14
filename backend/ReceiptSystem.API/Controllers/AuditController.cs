using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;

namespace ReceiptSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Paginated audit log viewer with filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AuditLogFilterDto filters)
    {
        var query = _db.AuditLogs.Include(l => l.User).AsQueryable();

        if (filters.UserId.HasValue)
            query = query.Where(l => l.UserId == filters.UserId.Value);
        if (!string.IsNullOrWhiteSpace(filters.EntityType))
            query = query.Where(l => l.EntityType == filters.EntityType);
        if (!string.IsNullOrWhiteSpace(filters.Action))
            query = query.Where(l => l.Action.Contains(filters.Action));
        if (filters.DateFrom.HasValue)
            query = query.Where(l => l.CreatedAt >= filters.DateFrom.Value.Date);
        if (filters.DateTo.HasValue)
            query = query.Where(l => l.CreatedAt <= filters.DateTo.Value.Date.AddDays(1));

        var total = await query.CountAsync();

        var results = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(l => new AuditLogDto
            {
                AuditLogId = l.LogId,
                UserId = l.UserId,
                UserName = l.User.FullName,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                OldValues = l.OldValues,
                NewValues = l.NewValues,
                IpAddress = l.IpAddress,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(new AuditLogResponseDto
        {
            TotalCount = total,
            Page = filters.Page,
            Results = results
        });
    }

    /// <summary>
    /// Get audit history for a specific entity
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetByEntity(string entityType, int entityId)
    {
        var logs = await _db.AuditLogs
            .Include(l => l.User)
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(100)
            .Select(l => new AuditLogDto
            {
                AuditLogId = l.LogId,
                UserId = l.UserId,
                UserName = l.User.FullName,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                OldValues = l.OldValues,
                NewValues = l.NewValues,
                IpAddress = l.IpAddress,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(logs);
    }
}

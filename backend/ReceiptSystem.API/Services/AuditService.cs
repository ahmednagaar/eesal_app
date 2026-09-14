using ReceiptSystem.API.Data;
using ReceiptSystem.API.Models;
using System.Text.Json;

namespace ReceiptSystem.API.Services;

public class AuditService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(int userId, string action, string entityType, int entityId,
        object? oldValues = null, object? newValues = null, string? ipAddress = null)
    {
        // Capture IP from HttpContext if not provided
        if (string.IsNullOrEmpty(ipAddress))
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?? context.Connection.RemoteIpAddress?.ToString();
            }
        }

        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Overload for passing a plain description string (used by AjalController).
    /// Wraps the string in a JSON object for consistency in the audit log.
    /// </summary>
    public async Task LogAsync(int userId, string action, string entityType, int entityId, string description)
    {
        await LogAsync(userId, action, entityType, entityId,
            newValues: new { Description = description });
    }
}

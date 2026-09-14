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
public class SessionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SessionService _sessionService;
    private readonly AuditService _audit;

    public SessionsController(AppDbContext db, SessionService sessionService, AuditService audit)
    {
        _db = db;
        _sessionService = sessionService;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? date = null, [FromQuery] int? driverId = null)
    {
        var query = _db.CollectionSessions
            .Include(s => s.Driver)
            .Include(s => s.EnteredByUser)
            .AsQueryable();

        if (date.HasValue) query = query.Where(s => s.SessionDate == date.Value.Date);
        if (driverId.HasValue) query = query.Where(s => s.DriverId == driverId.Value);

        var total = await query.CountAsync();
        var sessions = await query
            .OrderByDescending(s => s.SessionDate)
            .ThenByDescending(s => s.EnteredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SessionDto
            {
                SessionId = s.SessionId,
                DriverId = s.DriverId,
                DriverName = s.Driver.FullName,
                SessionDate = s.SessionDate,
                RouteArea = s.RouteArea,
                FirstReceiptNumber = s.FirstReceiptNumber,
                LastReceiptNumber = s.LastReceiptNumber,
                TotalReceiptsCount = s.TotalReceiptsCount,
                TotalAmountCollected = s.TotalAmountCollected,
                HasGaps = s.HasGaps,
                Notes = s.Notes,
                EnteredByName = s.EnteredByUser.FullName,
                EnteredAt = s.EnteredAt
            })
            .ToListAsync();

        return Ok(new { data = sessions, total, page, pageSize });
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var sessions = await _sessionService.GetTodaySessionsAsync();
        return Ok(sessions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var session = await _sessionService.GetSessionWithReceiptsAsync(id);
        if (session == null) return NotFound(new { message = "الجلسة غير موجودة" });
        return Ok(session);
    }

    [HttpGet("driver/{driverId}/last")]
    public async Task<IActionResult> GetLastForDriver(int driverId)
    {
        var session = await _sessionService.GetLastSessionForDriverAsync(driverId);
        if (session == null) return Ok(new { message = "لا توجد جلسات سابقة لهذا السائق" });
        return Ok(session);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSessionDto dto)
    {
        try
        {
            var (session, missingReceipts) = await _sessionService.CreateSessionAsync(dto, UserId);
            await _audit.LogAsync(UserId, "CreateSession", "CollectionSession", session.SessionId, newValues: dto);
            return Ok(new
            {
                session,
                missingReceipts,
                hasGaps = missingReceipts.Any(),
                message = missingReceipts.Any()
                    ? $"تحذير: تم اكتشاف {missingReceipts.Count} إيصال مفقود!"
                    : "تم حفظ الجلسة بنجاح"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var session = await _db.CollectionSessions.FindAsync(id);
        if (session == null) return NotFound(new { message = "الجلسة غير موجودة" });
        if (session.IsConfirmed) return BadRequest(new { message = "الجلسة مؤكدة بالفعل" });

        session.IsConfirmed = true;
        session.ConfirmedAt = DateTime.UtcNow;
        session.ConfirmedByUserId = UserId;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "ConfirmSession", "CollectionSession", id);

        return Ok(new { message = "تم تأكيد وقفل الجلسة بنجاح" });
    }

    [HttpPut("{id}/unlock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Unlock(int id, [FromBody] UnlockSessionDto dto)
    {
        var session = await _db.CollectionSessions.FindAsync(id);
        if (session == null) return NotFound(new { message = "الجلسة غير موجودة" });
        if (!session.IsConfirmed) return BadRequest(new { message = "الجلسة غير مقفلة" });

        session.IsConfirmed = false;
        session.ConfirmedAt = null;
        session.ConfirmedByUserId = null;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "UnlockSession", "CollectionSession", id, newValues: new { dto.Reason });

        return Ok(new { message = "تم فتح قفل الجلسة بنجاح" });
    }

    /// <summary>
    /// Add a receipt to an existing (unconfirmed) session.
    /// Useful when cash reconciliation reveals a missing entry.
    /// </summary>
    [HttpPost("{id}/receipts")]
    public async Task<IActionResult> AddReceipt(int id, [FromBody] CreateReceiptDto dto)
    {
        try
        {
            var (receipt, gaps) = await _sessionService.AddReceiptToSessionAsync(id, dto, UserId);
            await _audit.LogAsync(UserId, "AddReceiptToSession", "Receipt", receipt.ReceiptId,
                newValues: new { SessionId = id, dto.ReceiptNumber, dto.Amount });
            return Ok(new
            {
                receipt,
                gaps,
                hasGaps = gaps.Any(),
                message = "تم إضافة الإيصال للجلسة بنجاح"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/reconcile")]
    public async Task<IActionResult> Reconcile(int id, [FromBody] ReconcileSessionDto dto)
    {
        var session = await _db.CollectionSessions.FindAsync(id);
        if (session == null) return NotFound(new { message = "الجلسة غير موجودة" });

        var difference = dto.ActualCashCounted - session.TotalAmountCollected;
        session.ActualCashCounted = dto.ActualCashCounted;
        session.CashDifference = difference;
        session.CashReconciled = true;
        session.ReconciledAt = DateTime.UtcNow;
        session.ReconciledByUserId = UserId;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "ReconcileSession", "CollectionSession", id, newValues: new { dto.ActualCashCounted, difference });

        string status, message;
        if (difference == 0)
        {
            status = "match";
            message = "المبالغ متطابقة تماماً";
        }
        else if (difference > 0)
        {
            status = "surplus";
            message = $"فائض {difference:N2} جنيه";
        }
        else
        {
            status = "shortage";
            message = $"نقص {Math.Abs(difference):N2} جنيه — يجب مراجعة السائق";
        }

        return Ok(new ReconcileResponseDto { Status = status, Difference = difference, Message = message });
    }

    [HttpGet("missing-today")]
    public async Task<IActionResult> GetMissingToday()
    {
        if (DateTime.Now.Hour < 15)
            return Ok(new List<MissingDriverDto>());

        var today = DateTime.Today;
        var driversWithSessions = await _db.CollectionSessions
            .Where(s => s.SessionDate == today)
            .Select(s => s.DriverId)
            .Distinct()
            .ToListAsync();

        var missingDrivers = await _db.Drivers
            .Where(d => d.IsActive && !driversWithSessions.Contains(d.DriverId))
            .Select(d => new MissingDriverDto
            {
                DriverId = d.DriverId,
                DriverName = d.FullName,
                PhoneNumber = d.PhoneNumber,
                LastSessionDate = _db.CollectionSessions
                    .Where(s => s.DriverId == d.DriverId)
                    .OrderByDescending(s => s.SessionDate)
                    .Select(s => (DateTime?)s.SessionDate)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(missingDrivers);
    }
}

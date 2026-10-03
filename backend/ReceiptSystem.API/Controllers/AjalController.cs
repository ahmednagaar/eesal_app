using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Services;
using System.Security.Claims;

namespace ReceiptSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AjalController : ControllerBase
{
    private readonly AjalService _ajal;
    private readonly AuditService _audit;
    public AjalController(AjalService ajal, AuditService audit) { _ajal = ajal; _audit = audit; }
    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── Create Session with Entries ───
    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateAjalSessionDto dto)
    {
        var res = await _ajal.CreateSessionAsync(dto, UserId);
        if (res.Success)
            await _audit.LogAsync(UserId, "CreateAjalSession", "AjalSession", res.SessionId,
                $"تسجيل جلسة آجل — {res.EntriesSaved} فاتورة");
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // ─── Get Sessions for a Date ───
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions([FromQuery] DateTime date)
        => Ok(await _ajal.GetSessionsAsync(date));

    // ─── Get Session Detail ───
    [HttpGet("sessions/{id}")]
    public async Task<IActionResult> GetSession(int id)
    {
        var res = await _ajal.GetSessionDetailAsync(id);
        return res != null ? Ok(res) : NotFound(new { message = "الجلسة غير موجودة" });
    }

    // ─── Update Session (Driver, Notes) ───
    [HttpPut("sessions/{id}")]
    public async Task<IActionResult> UpdateSession(int id, [FromBody] UpdateAjalSessionDto dto)
    {
        var ok = await _ajal.UpdateSessionAsync(id, dto);
        if (ok) await _audit.LogAsync(UserId, "UpdateAjalSession", "AjalSession", id, "تعديل جلسة آجل");
        return ok ? Ok(new { success = true }) : NotFound(new { message = "الجلسة غير موجودة" });
    }

    // ─── Delete Session ───
    [HttpDelete("sessions/{id}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var ok = await _ajal.DeleteSessionAsync(id);
        if (ok) await _audit.LogAsync(UserId, "DeleteAjalSession", "AjalSession", id, "حذف جلسة آجل");
        return ok ? Ok(new { success = true }) : BadRequest(new { message = "لا يمكن حذف الجلسة — قد تحتوي على فواتير مُراجَعة" });
    }

    // ─── Add Entries to Existing Session ───
    [HttpPost("entries")]
    public async Task<IActionResult> AddEntries([FromBody] AddAjalEntriesDto dto)
    {
        var res = await _ajal.AddEntriesAsync(dto, UserId);
        if (res.Success)
            await _audit.LogAsync(UserId, "AddAjalEntries", "AjalEntry", res.SessionId,
                $"إضافة {res.EntriesSaved} فاتورة");
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // ─── Update Entry ───
    [HttpPut("entries/{id}")]
    public async Task<IActionResult> UpdateEntry(int id, [FromBody] UpdateAjalEntryDto dto)
    {
        var ok = await _ajal.UpdateEntryAsync(id, dto);
        if (ok) await _audit.LogAsync(UserId, "UpdateAjalEntry", "AjalEntry", id, "تعديل فاتورة آجل");
        return ok ? Ok(new { success = true }) : NotFound(new { message = "الفاتورة غير موجودة" });
    }

    // ─── Delete Entry ───
    [HttpDelete("entries/{id}")]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        var ok = await _ajal.DeleteEntryAsync(id);
        if (ok) await _audit.LogAsync(UserId, "DeleteAjalEntry", "AjalEntry", id, "حذف فاتورة آجل");
        return ok ? Ok(new { success = true }) : BadRequest(new { message = "لا يمكن حذف فاتورة مُراجَعة" });
    }

    // ─── Daily View (ALL entries for a date, sorted ascending by InvoiceNumber) ───
    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily([FromQuery] DateTime date)
        => Ok(await _ajal.GetDailyViewAsync(date));

    // ─── Review: Single Entry ───
    [HttpPut("entries/{id}/review")]
    public async Task<IActionResult> ReviewEntry(int id)
    {
        var ok = await _ajal.ReviewEntryAsync(id, UserId);
        return ok ? Ok(new { success = true }) : NotFound(new { message = "الفاتورة غير موجودة" });
    }

    // ─── Review: Batch ───
    [HttpPut("entries/review-batch")]
    public async Task<IActionResult> ReviewBatch([FromBody] ReviewBatchDto dto)
    {
        int count = await _ajal.ReviewBatchAsync(dto.EntryIds, UserId);
        await _audit.LogAsync(UserId, "ReviewAjalBatch", "AjalEntry", 0, $"مراجعة {count} فاتورة");
        return Ok(new { success = true, reviewed = count });
    }

    // ─── Search ───
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] AjalSearchFilterDto filters)
        => Ok(await _ajal.SearchAsync(filters));

    [HttpGet("search/export")]
    public async Task<IActionResult> ExportSearch([FromQuery] AjalSearchFilterDto filters)
    {
        var bytes = await _ajal.ExportSearchAsync(filters);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ajal_search_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // ─── Daily Export ───
    [HttpGet("daily/export")]
    public async Task<IActionResult> ExportDaily([FromQuery] DateTime date)
    {
        var bytes = await _ajal.ExportDailyAsync(date);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ajal_daily_{date:yyyyMMdd}.xlsx");
    }

    // ─── Duplicate Check ───
    [HttpGet("check-duplicate")]
    public async Task<IActionResult> CheckDuplicate([FromQuery] string invoiceNumber)
        => Ok(await _ajal.CheckDuplicateAsync(invoiceNumber));

    // ─── Settings ───
    [HttpGet("settings/invoice-prefix")]
    public async Task<IActionResult> GetSettings() => Ok(await _ajal.GetSettingsAsync());

    [HttpPut("settings/invoice-prefix")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePrefix([FromBody] UpdatePrefixDto dto)
    {
        await _ajal.UpdatePrefixAsync(dto.NewPrefix, UserId);
        await _audit.LogAsync(UserId, "UpdateInvoicePrefix", "SystemSetting", 0,
            $"تغيير البادئة إلى: {dto.NewPrefix}");
        return Ok(new { success = true, prefix = dto.NewPrefix });
    }
}

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

    // ─── Daily Register ───
    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily([FromQuery] DateTime date) => Ok(await _ajal.GetDailyRegisterAsync(date));

    // ─── Create Invoices ───
    [HttpPost("invoices")]
    public async Task<IActionResult> Create([FromBody] CreateAjalInvoicesDto dto)
    {
        var res = await _ajal.CreateInvoicesAsync(dto, UserId);
        if (res.Success)
            await _audit.LogAsync(UserId, "AddAjalInvoices", "AjalInvoice", 0, $"تم إدخال {res.Saved} فاتورة ليوم {dto.SessionDate:dd/MM/yyyy}");
        return res.Success ? Ok(res) : BadRequest(res);
    }

    // ─── Edit Invoice ───
    [HttpPut("invoices/{id}")]
    public async Task<IActionResult> Edit(int id, [FromBody] EditAjalInvoiceDto dto)
    {
        var ok = await _ajal.EditInvoiceAsync(id, dto, UserId);
        if (ok) await _audit.LogAsync(UserId, "EditAjalInvoice", "AjalInvoice", id, $"تعديل فاتورة — المبلغ: {dto.Amount}");
        return ok ? Ok(new { success = true }) : NotFound(new { message = "الفاتورة غير موجودة" });
    }

    // ─── Cancel Invoice ───
    [HttpPut("invoices/{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelAjalInvoiceDto dto)
    {
        var ok = await _ajal.CancelInvoiceAsync(id, dto.Reason);
        if (ok) await _audit.LogAsync(UserId, "CancelAjalInvoice", "AjalInvoice", id, $"إلغاء فاتورة — السبب: {dto.Reason}");
        return ok ? Ok(new { success = true }) : NotFound(new { message = "الفاتورة غير موجودة" });
    }

    // ─── Merchant History ───
    [HttpGet("merchants/{merchantId}/history")]
    public async Task<IActionResult> MerchantHistory(int merchantId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var res = await _ajal.GetMerchantHistoryAsync(merchantId, from, to);
        return res != null ? Ok(res) : NotFound(new { message = "التاجر غير موجود" });
    }

    // ─── Employee Performance ───
    [HttpGet("employees/performance")]
    public async Task<IActionResult> EmployeePerformance([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _ajal.GetEmployeePerformanceAsync(from, to));

    // ─── Search ───
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] AjalSearchFilterDto filters)
        => Ok(await _ajal.SearchAsync(filters));

    [HttpGet("search/export")]
    public async Task<IActionResult> ExportSearch([FromQuery] AjalSearchFilterDto filters)
    {
        var bytes = await _ajal.ExportSearchAsync(filters);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ajal_search_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // ─── Daily Export ───
    [HttpGet("daily/export")]
    public async Task<IActionResult> ExportDaily([FromQuery] DateTime date)
    {
        var bytes = await _ajal.ExportDailyAsync(date);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ajal_daily_{date:yyyyMMdd}.xlsx");
    }

    // ─── Employee Export ───
    [HttpGet("employees/export")]
    public async Task<IActionResult> ExportEmployees([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var bytes = await _ajal.ExportEmployeesAsync(from, to);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ajal_employees_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
    }

    // ─── Excel Import ───
    [HttpPost("excel/preview")]
    public async Task<IActionResult> ExcelPreview(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { error = "لم يتم رفع أي ملف" });
        if (!file.FileName.EndsWith(".xlsx")) return BadRequest(new { error = "صيغة الملف غير صحيحة — يجب .xlsx" });
        using var stream = file.OpenReadStream();
        return Ok(await _ajal.PreviewExcelAsync(stream));
    }

    [HttpPost("excel/save")]
    public async Task<IActionResult> ExcelSave([FromBody] SaveAjalExcelDto dto)
    {
        var res = await _ajal.SaveExcelAsync(dto, UserId);
        if (res.Success)
            await _audit.LogAsync(UserId, "AjalExcelImport", "AjalInvoice", 0, $"استيراد Excel: {res.Saved} فاتورة، {res.Skipped} مكررة، {res.NewMerchantsCreated} تاجر جديد");
        return Ok(res);
    }

    // ─── Settings ───
    [HttpGet("settings/invoice-prefix")]
    public async Task<IActionResult> GetSettings() => Ok(await _ajal.GetSettingsAsync());

    [HttpPut("settings/invoice-prefix")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePrefix([FromBody] UpdatePrefixDto dto)
    {
        await _ajal.UpdatePrefixAsync(dto.NewPrefix, UserId);
        await _audit.LogAsync(UserId, "UpdateInvoicePrefix", "SystemSetting", 0, $"تغيير البادئة إلى: {dto.NewPrefix}");
        return Ok(new { success = true, prefix = dto.NewPrefix });
    }

    // ─── Dashboard ───
    [HttpGet("dashboard/today-summary")]
    public async Task<IActionResult> TodaySummary() => Ok(await _ajal.GetTodaySummaryAsync());

    // ─── Employee Names (autocomplete) ───
    [HttpGet("employee-names")]
    public async Task<IActionResult> EmployeeNames() => Ok(await _ajal.GetEmployeeNamesAsync());
}

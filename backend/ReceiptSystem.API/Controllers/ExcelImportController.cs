using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Services;
using System.Security.Claims;

namespace ReceiptSystem.API.Controllers;

[ApiController]
[Route("api/sessions/excel")]
[Authorize(Roles = "Admin,Treasury")]
public class ExcelImportController : ControllerBase
{
    private readonly ExcelImportService _excelService;
    private readonly AuditService _auditService;

    public ExcelImportController(ExcelImportService excelService, AuditService auditService)
    {
        _excelService = excelService;
        _auditService = auditService;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    /// <summary>
    /// Upload and parse Excel file — preview only, does NOT save
    /// </summary>
    [HttpPost("preview")]
    public async Task<IActionResult> Preview(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ExcelPreviewResponseDto { Success = false, Error = "لم يتم رفع أي ملف" });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new ExcelPreviewResponseDto { Success = false, Error = "حجم الملف كبير جداً — الحد الأقصى 5 ميجابايت" });

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx")
            return BadRequest(new ExcelPreviewResponseDto { Success = false, Error = "صيغة الملف غير صحيحة — يجب أن يكون ملف Excel (.xlsx)" });

        using var stream = file.OpenReadStream();
        var result = await _excelService.PreviewExcelAsync(stream);
        return Ok(result);
    }

    /// <summary>
    /// Save Excel session — creates session, receipts, runs gap detection
    /// </summary>
    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody] SaveExcelSessionDto dto)
    {
        var result = await _excelService.SaveExcelSessionAsync(dto, UserId);

        if (result.Success)
        {
            await _auditService.LogAsync(UserId, "ExcelImport",
                "CollectionSession", result.SessionId,
                $"تم استيراد جلسة من Excel: {dto.Rows.Count} إيصال، نطاق {dto.StartReceiptNumber}-{dto.EndReceiptNumber}");
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }
}

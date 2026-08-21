using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Services;
using System.Security.Claims;

namespace ReceiptSystem.API.Controllers;

[ApiController]
[Route("api/erp-import")]
[Authorize(Roles = "Admin,Treasury")]
public class ErpImportController : ControllerBase
{
    private readonly ErpImportService _erpService;
    private readonly AuditService _audit;

    public ErpImportController(ErpImportService erpService, AuditService audit)
    {
        _erpService = erpService;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    /// <summary>
    /// Upload and parse 2-column ERP Excel — preview only, does NOT save
    /// </summary>
    [HttpPost("preview")]
    public async Task<IActionResult> Preview(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ErpImportPreviewDto { Success = false, Error = "لم يتم رفع أي ملف" });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new ErpImportPreviewDto { Success = false, Error = "حجم الملف كبير جداً — الحد الأقصى 5 ميجابايت" });

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx")
            return BadRequest(new ErpImportPreviewDto { Success = false, Error = "صيغة الملف غير صحيحة — يجب أن يكون ملف Excel (.xlsx)" });

        using var stream = file.OpenReadStream();
        var result = await _erpService.PreviewAsync(stream);
        return Ok(result);
    }

    /// <summary>
    /// Commit preview to staging — creates batch + rows + any new merchants
    /// </summary>
    [HttpPost("batches")]
    public async Task<IActionResult> CreateBatch([FromBody] CreateBatchDto dto)
    {
        try
        {
            var batchId = await _erpService.CreateBatchAsync(dto, UserId);
            await _audit.LogAsync(UserId, "CreateErpBatch", "ExcelImportBatch", batchId,
                newValues: new { dto.FileName, dto.Rows.Count, dto.LedgerTotalAmount });
            return Ok(new { batchId, message = "تم إنشاء الدفعة بنجاح", rowsCount = dto.Rows.Count });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// List batches with optional filtering
    /// </summary>
    [HttpGet("batches")]
    public async Task<IActionResult> GetBatches([FromQuery] string? status, [FromQuery] DateTime? date)
    {
        var batches = await _erpService.GetBatchesAsync(status, date);
        return Ok(batches);
    }

    /// <summary>
    /// Get full batch detail with all rows and assignment status
    /// </summary>
    [HttpGet("batches/{id}")]
    public async Task<IActionResult> GetBatchDetail(int id)
    {
        var detail = await _erpService.GetBatchDetailAsync(id);
        if (detail == null) return NotFound(new { message = "الدفعة غير موجودة" });
        return Ok(detail);
    }

    /// <summary>
    /// Assign a block of rows to a driver with sequential receipt numbers
    /// </summary>
    [HttpPost("batches/{id}/assign-block")]
    public async Task<IActionResult> AssignBlock(int id, [FromBody] AssignBlockDto dto)
    {
        dto.BatchId = id;
        var result = await _erpService.AssignBlockAsync(dto, UserId);

        if (!result.Success)
            return BadRequest(result);

        await _audit.LogAsync(UserId, "AssignErpBlock", "CollectionSession", result.SessionId,
            newValues: new { dto.DriverId, dto.StartReceiptNumber, dto.RowIds.Count, result.ReceiptsCreated });

        return Ok(result);
    }

    /// <summary>
    /// Assign a single orphan row to an existing session
    /// </summary>
    [HttpPost("batches/{id}/assign-single")]
    public async Task<IActionResult> AssignSingle(int id, [FromBody] AssignSingleDto dto)
    {
        dto.BatchId = id;
        var result = await _erpService.AssignSingleAsync(dto, UserId);

        if (!result.Success)
            return BadRequest(result);

        await _audit.LogAsync(UserId, "AssignErpSingle", "Receipt", result.ReceiptId,
            newValues: new { dto.TargetSessionId, dto.ReceiptNumber, result.ResolvedGapId });

        return Ok(result);
    }

    /// <summary>
    /// Add a manually-discovered row to an existing batch
    /// </summary>
    [HttpPost("batches/{id}/add-manual-row")]
    public async Task<IActionResult> AddManualRow(int id, [FromBody] AddManualRowDto dto)
    {
        dto.BatchId = id;
        try
        {
            var rowId = await _erpService.AddManualRowAsync(dto, UserId);
            await _audit.LogAsync(UserId, "AddErpManualRow", "ExcelImportRow", rowId,
                newValues: new { dto.MerchantId, dto.NewMerchantName, dto.Amount });
            return Ok(new { rowId, message = "تم إضافة الصف بنجاح" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Set ledger vs actual cash amounts for reconciliation
    /// </summary>
    [HttpPut("batches/{id}/reconcile")]
    public async Task<IActionResult> Reconcile(int id, [FromBody] ReconcileBatchDto dto)
    {
        try
        {
            var result = await _erpService.ReconcileAsync(id, dto);
            await _audit.LogAsync(UserId, "ReconcileErpBatch", "ExcelImportBatch", id,
                newValues: new { dto.LedgerTotalAmount, dto.ActualCashTotal });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mark batch as completed
    /// </summary>
    [HttpPut("batches/{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            var (completed, remainingCount) = await _erpService.CompleteAsync(id);
            await _audit.LogAsync(UserId, "CompleteErpBatch", "ExcelImportBatch", id);

            if (remainingCount > 0)
                return Ok(new { message = $"تم إغلاق الدفعة — تنبيه: يوجد {remainingCount} صف غير مسجل", remainingCount, warning = true });

            return Ok(new { message = "تم إغلاق الدفعة بنجاح — جميع الصفوف مسجلة", remainingCount = 0, warning = false });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Services;
using System.Security.Claims;

namespace ReceiptSystem.API.Controllers;

[ApiController]
[Route("api/delivery-days")]
[Authorize]
public class DeliveryDaysController : ControllerBase
{
    private readonly DeliveryDayService _deliveryDayService;
    private readonly AuditService _audit;

    public DeliveryDaysController(DeliveryDayService deliveryDayService, AuditService audit)
    {
        _deliveryDayService = deliveryDayService;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    /// <summary>
    /// GET /api/delivery-days?date=2024-10-15&routeId=5
    /// Returns list of delivery days matching filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDeliveryDays([FromQuery] DateTime? date, [FromQuery] int? routeId)
    {
        var days = await _deliveryDayService.GetDeliveryDaysAsync(date, routeId);
        return Ok(days);
    }

    /// <summary>
    /// GET /api/delivery-days/{id}
    /// Returns delivery day with all its invoices
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeliveryDay(int id)
    {
        var result = await _deliveryDayService.GetDeliveryDayWithInvoicesAsync(id);
        if (result == null)
            return NotFound(new { message = "يوم التسليم غير موجود" });
        
        return Ok(result);
    }

    /// <summary>
    /// POST /api/delivery-days
    /// Creates a new delivery day or returns existing one
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDeliveryDay([FromBody] CreateDeliveryDayDto dto)
    {
        try
        {
            var result = await _deliveryDayService.CreateDeliveryDayAsync(dto, UserId);
            await _audit.LogAsync(UserId, "إنشاء يوم توصيل", "DeliveryDay", 0, newValues: dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/delivery-days/{dayId}/invoices
    /// Adds a merchant to the delivery day checklist
    /// </summary>
    [HttpPost("{dayId}/invoices")]
    public async Task<IActionResult> AddDayInvoice(int dayId, [FromBody] AddDayInvoiceDto dto)
    {
        try
        {
            var result = await _deliveryDayService.AddDayInvoiceAsync(
                dayId, 
                dto.RouteMerchantId, 
                dto.MerchantId, 
                dto.InvoiceNumber, 
                dto.Quantity, 
                dto.Amount, 
                dto.Notes, 
                UserId
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/delivery-days/{dayId}/invoices/{invoiceId}
    /// Updates invoice details
    /// </summary>
    [HttpPut("{dayId}/invoices/{invoiceId}")]
    public async Task<IActionResult> UpdateDayInvoice(int dayId, int invoiceId, [FromBody] UpdateDayInvoiceDto dto)
    {
        var success = await _deliveryDayService.UpdateDayInvoiceAsync(
            dayId, 
            invoiceId, 
            dto.InvoiceNumber, 
            dto.Quantity, 
            dto.Amount, 
            dto.Notes
        );

        if (!success)
            return NotFound(new { message = "الفاتورة غير موجودة" });

        return Ok(new { message = "تم تحديث الفاتورة بنجاح" });
    }

    /// <summary>
    /// DELETE /api/delivery-days/{dayId}/invoices/{invoiceId}
    /// Removes a merchant from the delivery day
    /// </summary>
    [HttpDelete("{dayId}/invoices/{invoiceId}")]
    public async Task<IActionResult> RemoveDayInvoice(int dayId, int invoiceId)
    {
        var success = await _deliveryDayService.RemoveDayInvoiceAsync(dayId, invoiceId);
        if (!success)
            return NotFound(new { message = "الفاتورة غير موجودة" });

        return Ok(new { message = "تم حذف الفاتورة بنجاح" });
    }

    /// <summary>
    /// PUT /api/delivery-days/{dayId}/reorder
    /// Reorders invoices in the delivery sequence
    /// </summary>
    [HttpPut("{dayId}/reorder")]
    public async Task<IActionResult> ReorderInvoices(int dayId, [FromBody] List<ReorderInvoiceDto> reorders)
    {
        var reorderList = reorders.Select(r => (r.DayInvoiceId, r.NewSequenceNumber)).ToList();
        var success = await _deliveryDayService.ReorderDayInvoicesAsync(dayId, reorderList);
        
        if (!success)
            return BadRequest(new { message = "فشل إعادة الترتيب" });

        return Ok(new { message = "تم إعادة ترتيب الفواتير بنجاح" });
    }

    /// <summary>
    /// PUT /api/delivery-days/{dayId}/confirm
    /// Confirms the delivery day (locks it)
    /// </summary>
    [HttpPut("{dayId}/confirm")]
    public async Task<IActionResult> ConfirmDeliveryDay(int dayId)
    {
        var success = await _deliveryDayService.ConfirmDeliveryDayAsync(dayId);
        if (!success)
            return NotFound(new { message = "يوم التسليم غير موجود" });

        await _audit.LogAsync(UserId, "تأكيد يوم توصيل", "DeliveryDay", dayId);
        return Ok(new { message = "تم تأكيد يوم التسليم بنجاح" });
    }

    /// <summary>
    /// PUT /api/delivery-days/{dayId}/mark-printed
    /// Marks the delivery day as printed
    /// </summary>
    [HttpPut("{dayId}/mark-printed")]
    public async Task<IActionResult> MarkPrinted(int dayId)
    {
        var success = await _deliveryDayService.MarkDeliveryDayPrintedAsync(dayId);
        if (!success)
            return NotFound(new { message = "يوم التسليم غير موجود" });

        return Ok(new { message = "تم تسجيل طباعة يوم التسليم" });
    }

    /// <summary>
    /// PUT /api/delivery-days/{dayId}/unlock
    /// Unlocks a confirmed delivery day (Admin only)
    /// </summary>
    [HttpPut("{dayId}/unlock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnlockDeliveryDay(int dayId, [FromBody] UnlockDeliveryDayDto dto)
    {
        var result = await _deliveryDayService.UnlockDeliveryDayAsync(dayId, dto.Reason);
        if (result == null)
            return NotFound(new { message = "يوم التسليم غير موجود" });

        await _audit.LogAsync(UserId, "فتح قفل يوم توصيل", "DeliveryDay", dayId, newValues: dto);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/delivery-days/{dayId}/loading-sheet
    /// Returns loading sheet data for printing
    /// </summary>
    [HttpGet("{dayId}/loading-sheet")]
    public async Task<IActionResult> GetLoadingSheet(int dayId)
    {
        var result = await _deliveryDayService.GetLoadingSheetAsync(dayId);
        if (result == null)
            return NotFound(new { message = "يوم التسليم غير موجود" });

        return Ok(result);
    }

    /// <summary>
    /// GET /api/delivery-days/{dayId}/delivery-sheet
    /// Returns delivery sheet data for printing
    /// </summary>
    [HttpGet("{dayId}/delivery-sheet")]
    public async Task<IActionResult> GetDeliverySheet(int dayId)
    {
        var result = await _deliveryDayService.GetDeliverySheetAsync(dayId);
        if (result == null)
            return NotFound(new { message = "يوم التسليم غير موجود" });

        return Ok(result);
    }
}

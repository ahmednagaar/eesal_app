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
[Route("api/delivery-days")]
[Authorize]
public class DeliveryDaysController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly DeliveryDayService _deliveryService;
    private readonly AuditService _audit;

    public DeliveryDaysController(AppDbContext db, DeliveryDayService deliveryService, AuditService audit)
    {
        _db = db;
        _deliveryService = deliveryService;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    private string UserName => User.FindFirst(ClaimTypes.GivenName)?.Value ?? "مستخدم";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? date = null, [FromQuery] int? routeId = null)
    {
        var days = await _deliveryService.GetDeliveryDaysAsync(date, routeId);
        return Ok(days);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeliveryDayDto dto)
    {
        // Check for duplicate route+date
        var exists = await _db.DeliveryDays.AnyAsync(dd => dd.RouteId == dto.RouteId && dd.DeliveryDate == dto.DeliveryDate.Date);
        if (exists) return BadRequest(new { message = "يوجد بالفعل يوم تسليم لهذا الخط في نفس التاريخ" });

        var day = new DeliveryDay
        {
            RouteId = dto.RouteId,
            DeliveryDate = dto.DeliveryDate.Date,
            AssignedDriver = dto.AssignedDriver,
            Notes = dto.Notes,
            CreatedByUserId = UserId
        };
        _db.DeliveryDays.Add(day);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إنشاء يوم تسليم", "DeliveryDay", day.DeliveryDayId, newValues: dto);

        var result = await _deliveryService.GetDeliveryDayAsync(day.DeliveryDayId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var day = await _deliveryService.GetDeliveryDayAsync(id);
        if (day == null) return NotFound(new { message = "يوم التسليم غير موجود" });

        var invoices = await _deliveryService.GetDayInvoicesAsync(id);
        return Ok(new { day, invoices });
    }

    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        try
        {
            await _deliveryService.ConfirmDayAsync(id);
            await _audit.LogAsync(UserId, "تأكيد يوم تسليم", "DeliveryDay", id);
            return Ok(new { message = "تم تأكيد يوم التسليم بنجاح" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Day Invoices ──

    [HttpPost("{id}/invoices")]
    public async Task<IActionResult> AddInvoice(int id, [FromBody] CreateDayInvoiceDto dto)
    {
        var day = await _db.DeliveryDays.FindAsync(id);
        if (day == null) return NotFound(new { message = "يوم التسليم غير موجود" });
        if (day.Status == "Confirmed") return BadRequest(new { message = "لا يمكن التعديل على يوم مؤكد" });

        var invoice = new DayInvoice
        {
            DeliveryDayId = id,
            RouteMerchantId = dto.RouteMerchantId,
            MerchantId = dto.MerchantId,
            InvoiceNumber = dto.InvoiceNumber,
            Quantity = dto.Quantity,
            Amount = dto.Amount,
            Notes = dto.Notes,
            EnteredByUserId = UserId
        };
        _db.DayInvoices.Add(invoice);
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم إضافة الفاتورة بنجاح", dayInvoiceId = invoice.DayInvoiceId });
    }

    [HttpPut("{id}/invoices/{invoiceId}")]
    public async Task<IActionResult> UpdateInvoice(int id, int invoiceId, [FromBody] UpdateDayInvoiceDto dto)
    {
        var invoice = await _db.DayInvoices.FindAsync(invoiceId);
        if (invoice == null || invoice.DeliveryDayId != id)
            return NotFound(new { message = "الفاتورة غير موجودة" });

        var day = await _db.DeliveryDays.FindAsync(id);
        if (day?.Status == "Confirmed") return BadRequest(new { message = "لا يمكن التعديل على يوم مؤكد" });

        invoice.InvoiceNumber = dto.InvoiceNumber;
        invoice.Quantity = dto.Quantity;
        invoice.Amount = dto.Amount;
        invoice.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم تحديث الفاتورة بنجاح" });
    }

    [HttpDelete("{id}/invoices/{invoiceId}")]
    public async Task<IActionResult> RemoveInvoice(int id, int invoiceId)
    {
        var invoice = await _db.DayInvoices.FindAsync(invoiceId);
        if (invoice == null || invoice.DeliveryDayId != id)
            return NotFound(new { message = "الفاتورة غير موجودة" });

        var day = await _db.DeliveryDays.FindAsync(id);
        if (day?.Status == "Confirmed") return BadRequest(new { message = "لا يمكن التعديل على يوم مؤكد" });

        _db.DayInvoices.Remove(invoice);
        await _db.SaveChangesAsync();
        return Ok(new { message = "تم حذف الفاتورة بنجاح" });
    }

    [HttpPut("{id}/reorder")]
    public async Task<IActionResult> Reorder(int id, [FromBody] List<ReorderInvoiceDto> reorders)
    {
        try
        {
            await _deliveryService.ApplyDailyReorderAsync(id, reorders);
            return Ok(new { message = "تم تحديث ترتيب اليوم بنجاح" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Print Sheets ──

    [HttpGet("{id}/loading-sheet")]
    public async Task<IActionResult> GetLoadingSheet(int id)
    {
        try
        {
            var sheet = await _deliveryService.GetLoadingSheetAsync(id, UserName);
            return Ok(sheet);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/delivery-sheet")]
    public async Task<IActionResult> GetDeliverySheet(int id)
    {
        try
        {
            var sheet = await _deliveryService.GetDeliverySheetAsync(id, UserName);
            return Ok(sheet);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

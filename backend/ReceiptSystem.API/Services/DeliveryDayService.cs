using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Models;

namespace ReceiptSystem.API.Services;

public class DeliveryDayService
{
    private readonly AppDbContext _db;

    public DeliveryDayService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<DeliveryDayDto>> GetDeliveryDaysAsync(DateTime? date = null, int? routeId = null)
    {
        var query = _db.DeliveryDays
            .Include(dd => dd.Route)
            .Include(dd => dd.DayInvoices)
            .AsQueryable();

        if (date.HasValue) query = query.Where(dd => dd.DeliveryDate == date.Value.Date);
        if (routeId.HasValue) query = query.Where(dd => dd.RouteId == routeId.Value);

        return await query
            .OrderByDescending(dd => dd.DeliveryDate)
            .ThenBy(dd => dd.Route.RouteName)
            .Select(dd => new DeliveryDayDto
            {
                DeliveryDayId = dd.DeliveryDayId,
                RouteId = dd.RouteId,
                RouteName = dd.Route.RouteName,
                DeliveryDate = dd.DeliveryDate,
                AssignedDriver = dd.AssignedDriver,
                Status = dd.Status,
                Notes = dd.Notes,
                CreatedAt = dd.CreatedAt,
                InvoiceCount = dd.DayInvoices.Count,
                TotalAmount = dd.DayInvoices.Sum(i => i.Amount ?? 0)
            })
            .ToListAsync();
    }

    public async Task<DeliveryDayDto?> GetDeliveryDayAsync(int id)
    {
        return await _db.DeliveryDays
            .Include(dd => dd.Route)
            .Include(dd => dd.DayInvoices)
            .Where(dd => dd.DeliveryDayId == id)
            .Select(dd => new DeliveryDayDto
            {
                DeliveryDayId = dd.DeliveryDayId,
                RouteId = dd.RouteId,
                RouteName = dd.Route.RouteName,
                DeliveryDate = dd.DeliveryDate,
                AssignedDriver = dd.AssignedDriver,
                Status = dd.Status,
                Notes = dd.Notes,
                CreatedAt = dd.CreatedAt,
                InvoiceCount = dd.DayInvoices.Count,
                TotalAmount = dd.DayInvoices.Sum(i => i.Amount ?? 0)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<DayInvoiceDto>> GetDayInvoicesAsync(int deliveryDayId)
    {
        return await _db.DayInvoices
            .Include(di => di.Merchant)
            .Include(di => di.RouteMerchant)
            .Where(di => di.DeliveryDayId == deliveryDayId)
            .OrderBy(di => di.ManualPositionOverride ?? di.RouteMerchant.PositionOrder)
            .Select(di => new DayInvoiceDto
            {
                DayInvoiceId = di.DayInvoiceId,
                DeliveryDayId = di.DeliveryDayId,
                RouteMerchantId = di.RouteMerchantId,
                MerchantId = di.MerchantId,
                MerchantName = di.Merchant.MerchantName,
                City = di.Merchant.City,
                PositionOrder = di.RouteMerchant.PositionOrder,
                InvoiceNumber = di.InvoiceNumber,
                Quantity = di.Quantity,
                Amount = di.Amount,
                Notes = di.Notes,
                ManualPositionOverride = di.ManualPositionOverride
            })
            .ToListAsync();
    }

    public async Task<SheetDto> GetLoadingSheetAsync(int deliveryDayId, string preparedBy)
    {
        var day = await _db.DeliveryDays
            .Include(dd => dd.Route)
            .FirstOrDefaultAsync(dd => dd.DeliveryDayId == deliveryDayId)
            ?? throw new InvalidOperationException("يوم التسليم غير موجود");

        var invoices = await GetDayInvoicesAsync(deliveryDayId);

        // REVERSE order for loading sheet
        invoices.Reverse();

        return new SheetDto
        {
            RouteName = day.Route.RouteName,
            DeliveryDate = day.DeliveryDate,
            AssignedDriver = day.AssignedDriver,
            PreparedBy = preparedBy,
            TotalMerchants = invoices.Count,
            TotalAmount = invoices.Sum(i => i.Amount ?? 0),
            Lines = invoices.Select((inv, idx) => new SheetLineDto
            {
                SequenceNumber = idx + 1,
                MerchantName = inv.MerchantName,
                Quantity = inv.Quantity,
                InvoiceNumber = inv.InvoiceNumber,
                Amount = inv.Amount,
                Notes = inv.Notes
            }).ToList()
        };
    }

    public async Task<SheetDto> GetDeliverySheetAsync(int deliveryDayId, string preparedBy)
    {
        var day = await _db.DeliveryDays
            .Include(dd => dd.Route)
            .FirstOrDefaultAsync(dd => dd.DeliveryDayId == deliveryDayId)
            ?? throw new InvalidOperationException("يوم التسليم غير موجود");

        var invoices = await GetDayInvoicesAsync(deliveryDayId);

        // NORMAL order for delivery sheet
        return new SheetDto
        {
            RouteName = day.Route.RouteName,
            DeliveryDate = day.DeliveryDate,
            AssignedDriver = day.AssignedDriver,
            PreparedBy = preparedBy,
            TotalMerchants = invoices.Count,
            TotalAmount = invoices.Sum(i => i.Amount ?? 0),
            Lines = invoices.Select((inv, idx) => new SheetLineDto
            {
                SequenceNumber = idx + 1,
                MerchantName = inv.MerchantName,
                Quantity = inv.Quantity,
                InvoiceNumber = inv.InvoiceNumber,
                Amount = inv.Amount,
                Notes = inv.Notes
            }).ToList()
        };
    }

    public async Task ApplyDailyReorderAsync(int deliveryDayId, List<ReorderInvoiceDto> reorders)
    {
        var day = await _db.DeliveryDays.FindAsync(deliveryDayId)
            ?? throw new InvalidOperationException("يوم التسليم غير موجود");

        if (day.Status == "Confirmed")
            throw new InvalidOperationException("لا يمكن تعديل يوم مؤكد");

        foreach (var r in reorders)
        {
            var invoice = await _db.DayInvoices.FindAsync(r.InvoiceId);
            if (invoice != null && invoice.DeliveryDayId == deliveryDayId)
            {
                invoice.ManualPositionOverride = r.NewPosition;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task ConfirmDayAsync(int deliveryDayId)
    {
        var day = await _db.DeliveryDays.FindAsync(deliveryDayId)
            ?? throw new InvalidOperationException("يوم التسليم غير موجود");

        day.Status = "Confirmed";
        day.ConfirmedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Reopen a confirmed delivery day (Admin override).
    /// </summary>
    public async Task UnlockDayAsync(int deliveryDayId)
    {
        var day = await _db.DeliveryDays.FindAsync(deliveryDayId)
            ?? throw new InvalidOperationException("يوم التسليم غير موجود");

        if (day.Status != "Confirmed")
            throw new InvalidOperationException("يوم التسليم غير مؤكد — لا حاجة لفتح القفل");

        day.Status = "Draft";
        day.ConfirmedAt = null;
        await _db.SaveChangesAsync();
    }

    public async Task MarkPrintedAsync(int deliveryDayId)
    {
        var day = await _db.DeliveryDays.FindAsync(deliveryDayId)
            ?? throw new InvalidOperationException("يوم التسليم غير موجود");

        day.PrintedAt = DateTime.UtcNow;
        if (day.Status == "Draft") day.Status = "Printed";
        await _db.SaveChangesAsync();
    }
}

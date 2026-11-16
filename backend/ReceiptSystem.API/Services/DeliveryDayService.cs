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
            .Include(d => d.Route)
            .AsQueryable();

        if (date.HasValue)
            query = query.Where(d => d.DeliveryDate.Date == date.Value.Date);
        
        if (routeId.HasValue)
            query = query.Where(d => d.RouteId == routeId.Value);

        return await query
            .OrderByDescending(d => d.DeliveryDate)
            .ThenBy(d => d.Route.RouteName)
            .Select(d => new DeliveryDayDto
            {
                DeliveryDayId = d.DeliveryDayId,
                RouteId = d.RouteId,
                RouteName = d.Route.RouteName,
                DeliveryDate = d.DeliveryDate,
                AssignedDriver = d.AssignedDriver,
                Status = d.Status,
                Notes = d.Notes,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<object?> GetDeliveryDayWithInvoicesAsync(int deliveryDayId)
    {
        var day = await _db.DeliveryDays
            .Include(d => d.Route)
            .Include(d => d.DayInvoices)
                .ThenInclude(di => di.Merchant)
            .Include(d => d.DayInvoices)
                .ThenInclude(di => di.RouteMerchant)
            .FirstOrDefaultAsync(d => d.DeliveryDayId == deliveryDayId);

        if (day == null)
            return null;

        var invoices = day.DayInvoices
            .OrderBy(di => di.ManualPositionOverride ?? di.RouteMerchant.PositionOrder)
            .Select(di => new DayInvoiceDto
            {
                DayInvoiceId = di.DayInvoiceId,
                DeliveryDayId = di.DeliveryDayId,
                RouteMerchantId = di.RouteMerchantId,
                MerchantId = di.MerchantId,
                MerchantName = di.Merchant.MerchantName,
                InvoiceNumber = di.InvoiceNumber,
                Quantity = di.Quantity,
                Amount = di.Amount,
                Notes = di.Notes,
                SequenceNumber = di.ManualPositionOverride ?? di.RouteMerchant.PositionOrder
            })
            .ToList();

        return new
        {
            day = new DeliveryDayDto
            {
                DeliveryDayId = day.DeliveryDayId,
                RouteId = day.RouteId,
                RouteName = day.Route.RouteName,
                DeliveryDate = day.DeliveryDate,
                AssignedDriver = day.AssignedDriver,
                Status = day.Status,
                Notes = day.Notes,
                CreatedAt = day.CreatedAt
            },
            invoices
        };
    }

    public async Task<object> CreateDeliveryDayAsync(CreateDeliveryDayDto dto, int userId)
    {
        // Check if delivery day already exists for this route and date
        var existing = await _db.DeliveryDays
            .FirstOrDefaultAsync(d => d.RouteId == dto.RouteId && d.DeliveryDate.Date == dto.DeliveryDate.Date);

        if (existing != null)
        {
            // Return existing day with its invoices
            return await GetDeliveryDayWithInvoicesAsync(existing.DeliveryDayId) 
                ?? throw new InvalidOperationException("فشل في تحميل يوم التسليم الموجود");
        }

        // Create new delivery day
        var deliveryDay = new DeliveryDay
        {
            RouteId = dto.RouteId,
            DeliveryDate = dto.DeliveryDate.Date,
            AssignedDriver = dto.AssignedDriver,
            Status = "Draft",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.DeliveryDays.Add(deliveryDay);
        await _db.SaveChangesAsync();

        return new
        {
            day = new DeliveryDayDto
            {
                DeliveryDayId = deliveryDay.DeliveryDayId,
                RouteId = deliveryDay.RouteId,
                RouteName = "",
                DeliveryDate = deliveryDay.DeliveryDate,
                AssignedDriver = deliveryDay.AssignedDriver,
                Status = deliveryDay.Status,
                Notes = deliveryDay.Notes,
                CreatedAt = deliveryDay.CreatedAt
            },
            invoices = new List<DayInvoiceDto>()
        };
    }

    public async Task<object> AddDayInvoiceAsync(int deliveryDayId, int routeMerchantId, int merchantId, 
        string? invoiceNumber, string? quantity, decimal? amount, string? notes, int userId)
    {
        // Check if this merchant is already in the day
        var existing = await _db.DayInvoices
            .FirstOrDefaultAsync(di => di.DeliveryDayId == deliveryDayId && di.RouteMerchantId == routeMerchantId);

        if (existing != null)
        {
            return new { dayInvoiceId = existing.DayInvoiceId, message = "الفاتورة موجودة بالفعل" };
        }

        var invoice = new DayInvoice
        {
            DeliveryDayId = deliveryDayId,
            RouteMerchantId = routeMerchantId,
            MerchantId = merchantId,
            InvoiceNumber = invoiceNumber,
            Quantity = quantity,
            Amount = amount,
            Notes = notes,
            EnteredByUserId = userId,
            EnteredAt = DateTime.UtcNow
        };

        _db.DayInvoices.Add(invoice);
        await _db.SaveChangesAsync();

        return new { dayInvoiceId = invoice.DayInvoiceId, message = "تم إضافة الفاتورة بنجاح" };
    }

    public async Task<bool> UpdateDayInvoiceAsync(int deliveryDayId, int invoiceId, 
        string? invoiceNumber, string? quantity, decimal? amount, string? notes)
    {
        var invoice = await _db.DayInvoices
            .FirstOrDefaultAsync(di => di.DayInvoiceId == invoiceId && di.DeliveryDayId == deliveryDayId);

        if (invoice == null)
            return false;

        invoice.InvoiceNumber = invoiceNumber;
        invoice.Quantity = quantity;
        invoice.Amount = amount;
        invoice.Notes = notes;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveDayInvoiceAsync(int deliveryDayId, int invoiceId)
    {
        var invoice = await _db.DayInvoices
            .FirstOrDefaultAsync(di => di.DayInvoiceId == invoiceId && di.DeliveryDayId == deliveryDayId);

        if (invoice == null)
            return false;

        _db.DayInvoices.Remove(invoice);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderDayInvoicesAsync(int deliveryDayId, List<(int dayInvoiceId, int newSequenceNumber)> reorders)
    {
        foreach (var (dayInvoiceId, newSequenceNumber) in reorders)
        {
            var invoice = await _db.DayInvoices
                .FirstOrDefaultAsync(di => di.DayInvoiceId == dayInvoiceId && di.DeliveryDayId == deliveryDayId);

            if (invoice != null)
            {
                invoice.ManualPositionOverride = newSequenceNumber;
            }
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConfirmDeliveryDayAsync(int deliveryDayId)
    {
        var day = await _db.DeliveryDays.FindAsync(deliveryDayId);
        if (day == null)
            return false;

        day.Status = "Confirmed";
        day.ConfirmedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkDeliveryDayPrintedAsync(int deliveryDayId)
    {
        var day = await _db.DeliveryDays.FindAsync(deliveryDayId);
        if (day == null)
            return false;

        day.PrintedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<object?> UnlockDeliveryDayAsync(int deliveryDayId, string reason)
    {
        var day = await _db.DeliveryDays.FindAsync(deliveryDayId);
        if (day == null)
            return null;

        day.Status = "Draft";
        day.ConfirmedAt = null;
        await _db.SaveChangesAsync();

        return new { message = "تم إلغاء قفل يوم التسليم", reason };
    }

    public async Task<object?> GetLoadingSheetAsync(int deliveryDayId)
    {
        var day = await _db.DeliveryDays
            .Include(d => d.Route)
            .Include(d => d.DayInvoices)
                .ThenInclude(di => di.Merchant)
            .Include(d => d.DayInvoices)
                .ThenInclude(di => di.RouteMerchant)
            .FirstOrDefaultAsync(d => d.DeliveryDayId == deliveryDayId);

        if (day == null)
            return null;

        var items = day.DayInvoices
            .OrderBy(di => di.ManualPositionOverride ?? di.RouteMerchant.PositionOrder)
            .Select(di => new
            {
                sequence = di.ManualPositionOverride ?? di.RouteMerchant.PositionOrder,
                merchantName = di.Merchant.MerchantName,
                quantity = di.Quantity,
                invoiceNumber = di.InvoiceNumber,
                notes = di.Notes
            })
            .ToList();

        return new
        {
            routeName = day.Route.RouteName,
            deliveryDate = day.DeliveryDate,
            assignedDriver = day.AssignedDriver,
            items
        };
    }

    public async Task<object?> GetDeliverySheetAsync(int deliveryDayId)
    {
        return await GetLoadingSheetAsync(deliveryDayId);
    }
}

using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Models;

namespace ReceiptSystem.API.Services;

public class RouteService
{
    private readonly AppDbContext _db;

    public RouteService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RouteDto>> GetAllRoutesAsync()
    {
        return await _db.Routes
            .Where(r => r.IsActive)
            .OrderBy(r => r.RouteName)
            .Select(r => new RouteDto
            {
                RouteId = r.RouteId,
                RouteName = r.RouteName,
                IsActive = r.IsActive,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt,
                MerchantCount = r.RouteMerchants.Count(rm => rm.IsActive)
            })
            .ToListAsync();
    }

    public async Task<List<RouteMerchantDto>> GetRouteMerchantsAsync(int routeId)
    {
        return await _db.RouteMerchants
            .Include(rm => rm.Merchant)
            .Where(rm => rm.RouteId == routeId && rm.IsActive)
            .OrderBy(rm => rm.PositionOrder)
            .Select(rm => new RouteMerchantDto
            {
                RouteMerchantId = rm.RouteMerchantId,
                RouteId = rm.RouteId,
                MerchantId = rm.MerchantId,
                MerchantName = rm.Merchant.MerchantName,
                City = rm.Merchant.City,
                PositionOrder = rm.PositionOrder,
                IsActive = rm.IsActive,
                Notes = rm.Notes
            })
            .ToListAsync();
    }

    public async Task<RouteMerchant> InsertMerchantAtPositionAsync(int routeId, int merchantId, int position, int userId, string? notes = null)
    {
        // Check if merchant already on this route
        var exists = await _db.RouteMerchants.AnyAsync(rm => rm.RouteId == routeId && rm.MerchantId == merchantId && rm.IsActive);
        if (exists)
            throw new InvalidOperationException("التاجر موجود بالفعل في هذا الخط");

        // Shift existing positions up
        var toShift = await _db.RouteMerchants
            .Where(rm => rm.RouteId == routeId && rm.IsActive && rm.PositionOrder >= position)
            .OrderByDescending(rm => rm.PositionOrder)
            .ToListAsync();

        foreach (var rm in toShift)
        {
            rm.PositionOrder += 1;
        }

        // Insert new merchant at the exact position
        var newRm = new RouteMerchant
        {
            RouteId = routeId,
            MerchantId = merchantId,
            PositionOrder = position,
            Notes = notes,
            AddedByUserId = userId
        };

        _db.RouteMerchants.Add(newRm);
        await _db.SaveChangesAsync();
        return newRm;
    }

    public async Task UpdateMerchantPositionAsync(int routeId, int routeMerchantId, int newPosition)
    {
        var rm = await _db.RouteMerchants.FindAsync(routeMerchantId)
            ?? throw new InvalidOperationException("التاجر غير موجود في الخط");

        var oldPosition = rm.PositionOrder;
        if (oldPosition == newPosition) return;

        // Use raw SQL to avoid EF Core circular dependency with unique index on {RouteId, PositionOrder}.
        // Step 1: Move the target item to a sentinel position (-1) to free its slot
        await _db.Database.ExecuteSqlRawAsync(
            "UPDATE RouteMerchants SET PositionOrder = -1 WHERE RouteMerchantId = {0}", routeMerchantId);

        // Step 2: Shift affected items
        if (newPosition > oldPosition)
        {
            // Moving down: shift items between old+1 and new UP by 1
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE RouteMerchants SET PositionOrder = PositionOrder - 1 WHERE RouteId = {0} AND IsActive = 1 AND PositionOrder > {1} AND PositionOrder <= {2} AND RouteMerchantId != {3}",
                routeId, oldPosition, newPosition, routeMerchantId);
        }
        else
        {
            // Moving up: shift items between new and old-1 DOWN by 1
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE RouteMerchants SET PositionOrder = PositionOrder + 1 WHERE RouteId = {0} AND IsActive = 1 AND PositionOrder >= {1} AND PositionOrder < {2} AND RouteMerchantId != {3}",
                routeId, newPosition, oldPosition, routeMerchantId);
        }

        // Step 3: Place the target item at its final position
        await _db.Database.ExecuteSqlRawAsync(
            "UPDATE RouteMerchants SET PositionOrder = {0} WHERE RouteMerchantId = {1}", newPosition, routeMerchantId);
    }

    public async Task RemoveMerchantFromRouteAsync(int routeId, int routeMerchantId)
    {
        var rm = await _db.RouteMerchants.FindAsync(routeMerchantId)
            ?? throw new InvalidOperationException("التاجر غير موجود في الخط");

        var removedPosition = rm.PositionOrder;
        rm.IsActive = false;

        // Shift positions down to fill the gap
        var toShift = await _db.RouteMerchants
            .Where(x => x.RouteId == routeId && x.IsActive && x.PositionOrder > removedPosition)
            .ToListAsync();

        foreach (var x in toShift)
        {
            x.PositionOrder -= 1;
        }

        await _db.SaveChangesAsync();
    }
}

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
[Route("api/[controller]")]
[Authorize]
public class RoutesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly RouteService _routeService;
    private readonly AuditService _audit;

    public RoutesController(AppDbContext db, RouteService routeService, AuditService audit)
    {
        _db = db;
        _routeService = routeService;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var routes = await _routeService.GetAllRoutesAsync();
        return Ok(routes);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRouteDto dto)
    {
        var route = new Models.Route
        {
            RouteName = dto.RouteName,
            Notes = dto.Notes,
            CreatedByUserId = UserId
        };
        _db.Routes.Add(route);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إضافة خط", "Route", route.RouteId, newValues: dto);
        return Ok(new RouteDto
        {
            RouteId = route.RouteId,
            RouteName = route.RouteName,
            IsActive = route.IsActive,
            Notes = route.Notes,
            CreatedAt = route.CreatedAt,
            MerchantCount = 0
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRouteDto dto)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route == null) return NotFound(new { message = "الخط غير موجود" });
        var old = new { route.RouteName, route.Notes };
        route.RouteName = dto.RouteName;
        route.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تعديل خط", "Route", id, oldValues: old, newValues: dto);
        return Ok(new { message = "تم تحديث بيانات الخط بنجاح" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route == null) return NotFound(new { message = "الخط غير موجود" });
        route.IsActive = false;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "حذف خط", "Route", id);
        return Ok(new { message = "تم حذف الخط بنجاح" });
    }

    // ── Route Merchants ──

    [HttpGet("{routeId}/merchants")]
    public async Task<IActionResult> GetMerchants(int routeId)
    {
        var merchants = await _routeService.GetRouteMerchantsAsync(routeId);
        return Ok(merchants);
    }

    [HttpPost("{routeId}/merchants")]
    public async Task<IActionResult> AddMerchant(int routeId, [FromBody] AddMerchantToRouteDto dto)
    {
        try
        {
            var rm = await _routeService.InsertMerchantAtPositionAsync(routeId, dto.MerchantId, dto.Position, UserId, dto.Notes);
            await _audit.LogAsync(UserId, "إضافة تاجر للخط", "RouteMerchant", rm.RouteMerchantId, newValues: dto);
            return Ok(new { message = "تم إضافة التاجر للخط بنجاح", routeMerchantId = rm.RouteMerchantId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{routeId}/merchants/{id}/position")]
    public async Task<IActionResult> UpdatePosition(int routeId, int id, [FromBody] UpdatePositionDto dto)
    {
        try
        {
            await _routeService.UpdateMerchantPositionAsync(routeId, id, dto.NewPosition);
            return Ok(new { message = "تم تحديث ترتيب التاجر بنجاح" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{routeId}/merchants/{id}")]
    public async Task<IActionResult> RemoveMerchant(int routeId, int id)
    {
        try
        {
            await _routeService.RemoveMerchantFromRouteAsync(routeId, id);
            await _audit.LogAsync(UserId, "حذف تاجر من خط", "RouteMerchant", id);
            return Ok(new { message = "تم حذف التاجر من الخط بنجاح" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

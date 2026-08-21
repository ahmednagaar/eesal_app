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
[Authorize(Roles = "Admin,Treasury")]
public class DriversController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public DriversController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var drivers = await _db.Drivers
            .Where(d => d.IsActive)
            .Select(d => new DriverDto
            {
                DriverId = d.DriverId,
                FullName = d.FullName,
                PhoneNumber = d.PhoneNumber,
                IsActive = d.IsActive,
                Notes = d.Notes,
                CreatedAt = d.CreatedAt,
                OpenGapsCount = d.Gaps.Count(g => g.Status == "Open" || g.Status == "UnderInvestigation")
            })
            .ToListAsync();
        return Ok(drivers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var driver = await _db.Drivers
            .Where(d => d.DriverId == id)
            .Select(d => new DriverDto
            {
                DriverId = d.DriverId,
                FullName = d.FullName,
                PhoneNumber = d.PhoneNumber,
                IsActive = d.IsActive,
                Notes = d.Notes,
                CreatedAt = d.CreatedAt,
                OpenGapsCount = d.Gaps.Count(g => g.Status == "Open" || g.Status == "UnderInvestigation")
            })
            .FirstOrDefaultAsync();
        if (driver == null) return NotFound(new { message = "السائق غير موجود" });
        return Ok(driver);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDriverDto dto)
    {
        var driver = new Driver
        {
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Notes = dto.Notes,
            CreatedByUserId = UserId
        };
        _db.Drivers.Add(driver);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إضافة", "Driver", driver.DriverId, newValues: dto);
        return CreatedAtAction(nameof(Get), new { id = driver.DriverId }, new DriverDto
        {
            DriverId = driver.DriverId,
            FullName = driver.FullName,
            PhoneNumber = driver.PhoneNumber,
            IsActive = driver.IsActive,
            Notes = driver.Notes,
            CreatedAt = driver.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateDriverDto dto)
    {
        var driver = await _db.Drivers.FindAsync(id);
        if (driver == null) return NotFound(new { message = "السائق غير موجود" });
        var old = new { driver.FullName, driver.PhoneNumber, driver.Notes };
        driver.FullName = dto.FullName;
        driver.PhoneNumber = dto.PhoneNumber;
        driver.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تعديل", "Driver", id, oldValues: old, newValues: dto);
        return Ok(new { message = "تم تحديث بيانات السائق بنجاح" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var driver = await _db.Drivers.FindAsync(id);
        if (driver == null) return NotFound(new { message = "السائق غير موجود" });
        driver.IsActive = false; // Soft delete
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "حذف", "Driver", id);
        return Ok(new { message = "تم حذف السائق بنجاح" });
    }
}

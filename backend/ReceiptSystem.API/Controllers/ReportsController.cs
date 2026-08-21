using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Services;
using System.Security.Claims;

namespace ReceiptSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Treasury")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ReportService _reportService;

    public ReportsController(AppDbContext db, ReportService reportService)
    {
        _db = db;
        _reportService = reportService;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime? date = null)
    {
        var reportDate = date?.Date ?? DateTime.Today;
        var userName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "مستخدم";
        var report = await _reportService.GetDailyReportAsync(reportDate, userName);
        return Ok(report);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var today = DateTime.Today;
        var todaySessions = await _db.CollectionSessions
            .Include(s => s.Driver)
            .Include(s => s.EnteredByUser)
            .Where(s => s.SessionDate == today)
            .OrderByDescending(s => s.EnteredAt)
            .Select(s => new SessionDto
            {
                SessionId = s.SessionId,
                DriverId = s.DriverId,
                DriverName = s.Driver.FullName,
                SessionDate = s.SessionDate,
                RouteArea = s.RouteArea,
                FirstReceiptNumber = s.FirstReceiptNumber,
                LastReceiptNumber = s.LastReceiptNumber,
                TotalReceiptsCount = s.TotalReceiptsCount,
                TotalAmountCollected = s.TotalAmountCollected,
                HasGaps = s.HasGaps,
                Notes = s.Notes,
                EnteredByName = s.EnteredByUser.FullName,
                EnteredAt = s.EnteredAt,
                ImportSource = s.ImportSource,
                OriginalFileName = s.OriginalFileName
            })
            .ToListAsync();

        var dashboard = new DashboardDto
        {
            TotalAmountToday = todaySessions.Sum(s => s.TotalAmountCollected),
            DriversToday = todaySessions.Select(s => s.DriverId).Distinct().Count(),
            OpenGapsCount = await _db.ReceiptGaps.CountAsync(g => g.Status == "Open" || g.Status == "UnderInvestigation"),
            ReceiptsToday = todaySessions.Sum(s => s.TotalReceiptsCount),
            TodaySessions = todaySessions
        };

        return Ok(dashboard);
    }

    [HttpGet("driver-performance")]
    public async Task<IActionResult> GetDriverPerformance([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _reportService.GetDriverPerformanceAsync(from, to);
        return Ok(result);
    }
}

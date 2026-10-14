using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;

namespace ReceiptSystem.API.Services;

public class ReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DailyReportDto> GetDailyReportAsync(DateTime date, string preparedBy)
    {
        var targetDate = date.Date;

        var sessions = await _db.CollectionSessions
            .Include(s => s.Driver)
            .Include(s => s.Receipts).ThenInclude(r => r.Merchant)
            .Include(s => s.DetectedGaps)
            .Where(s => s.SessionDate == targetDate)
            .ToListAsync();

        var report = new DailyReportDto
        {
            ReportDate = targetDate,
            PreparedBy = preparedBy,
            TotalDrivers = sessions.Select(s => s.DriverId).Distinct().Count(),
            TotalReceipts = sessions.Sum(s => s.TotalReceiptsCount),
            TotalAmount = sessions.Sum(s => s.TotalAmountCollected),
            MissingReceiptsCount = sessions.Sum(s => s.DetectedGaps.Count(g => g.Status == "Open")),
            CancelledReceiptsCount = sessions.Sum(s => s.DetectedGaps.Count(g => g.Status == "Cancelled")),
            DriverReports = sessions.GroupBy(s => s.DriverId).Select(g =>
            {
                var driverSessions = g.ToList();
                var driver = driverSessions.First().Driver;
                var driverGaps = driverSessions.SelectMany(s => s.DetectedGaps).ToList();
                return new DriverDailyReportDto
                {
                    DriverName = driver.FullName,
                    RouteArea = string.Join(" / ", driverSessions.Select(s => s.RouteArea).Distinct()),
                    ReceiptCount = driverSessions.Sum(s => s.TotalReceiptsCount),
                    TotalAmount = driverSessions.Sum(s => s.TotalAmountCollected),
                    Receipts = driverSessions
                        .SelectMany(s => s.Receipts)
                        .OrderBy(r => r.ReceiptNumber)
                        .Select(r => new ReceiptLineDto
                        {
                            ReceiptNumber = r.ReceiptNumber,
                            MerchantName = r.Merchant.MerchantName,
                            Amount = r.Amount
                        }).ToList(),
                    MissingReceipts = driverGaps
                        .Where(g => g.Status == "Open")
                        .Select(g => g.MissingReceiptNumber)
                        .OrderBy(n => n)
                        .ToList(),
                    CancelledReceiptsCount = driverGaps.Count(g => g.Status == "Cancelled"),
                    CancelledReceipts = driverGaps
                        .Where(g => g.Status == "Cancelled")
                        .Select(g => g.MissingReceiptNumber)
                        .OrderBy(n => n)
                        .ToList()
                };
            }).ToList()
        };

        return report;
    }

    public async Task<List<DriverPerformanceDto>> GetDriverPerformanceAsync(DateTime from, DateTime to)
    {
        var drivers = await _db.Drivers.Where(d => d.IsActive).ToListAsync();
        var driverIds = drivers.Select(d => d.DriverId).ToList();

        // Batch load all sessions and gaps in just 2 queries instead of 2*N
        var allSessions = await _db.CollectionSessions
            .Where(s => driverIds.Contains(s.DriverId) && s.SessionDate >= from.Date && s.SessionDate <= to.Date)
            .GroupBy(s => s.DriverId)
            .Select(g => new
            {
                DriverId = g.Key,
                TotalSessions = g.Count(),
                TotalReceipts = g.Sum(s => s.TotalReceiptsCount),
                TotalAmount = g.Sum(s => s.TotalAmountCollected)
            })
            .ToListAsync();

        var allGaps = await _db.ReceiptGaps
            .Where(g => driverIds.Contains(g.DriverId) && g.DetectedAt >= from.Date && g.DetectedAt <= to.Date)
            .GroupBy(g => g.DriverId)
            .Select(g => new
            {
                DriverId = g.Key,
                Total = g.Count(),
                Resolved = g.Count(x => x.Status == "Resolved" || x.Status == "Explained"),
                Open = g.Count(x => x.Status == "Open" || x.Status == "UnderInvestigation")
            })
            .ToListAsync();

        var sessionLookup = allSessions.ToDictionary(s => s.DriverId);
        var gapLookup = allGaps.ToDictionary(g => g.DriverId);

        var results = drivers.Select(driver =>
        {
            sessionLookup.TryGetValue(driver.DriverId, out var sessionData);
            gapLookup.TryGetValue(driver.DriverId, out var gapData);

            var totalReceipts = sessionData?.TotalReceipts ?? 0;
            var totalGaps = gapData?.Total ?? 0;

            return new DriverPerformanceDto
            {
                DriverId = driver.DriverId,
                DriverName = driver.FullName,
                TotalSessions = sessionData?.TotalSessions ?? 0,
                TotalReceipts = totalReceipts,
                TotalAmountCollected = sessionData?.TotalAmount ?? 0,
                TotalGapsDetected = totalGaps,
                TotalGapsResolved = gapData?.Resolved ?? 0,
                TotalGapsOpen = gapData?.Open ?? 0,
                GapRatePercent = totalReceipts > 0 ? Math.Round((decimal)totalGaps / totalReceipts * 100, 2) : 0
            };
        }).ToList();

        return results.OrderByDescending(r => r.TotalGapsOpen).ToList();
    }
}

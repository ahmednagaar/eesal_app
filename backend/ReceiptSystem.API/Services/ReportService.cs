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
            DriverReports = sessions.GroupBy(s => s.DriverId).Select(g =>
            {
                var driverSessions = g.ToList();
                var driver = driverSessions.First().Driver;
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
                    MissingReceipts = driverSessions
                        .SelectMany(s => s.DetectedGaps)
                        .Where(g => g.Status == "Open")
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
        var results = new List<DriverPerformanceDto>();

        foreach (var driver in drivers)
        {
            var sessions = await _db.CollectionSessions
                .Where(s => s.DriverId == driver.DriverId && s.SessionDate >= from.Date && s.SessionDate <= to.Date)
                .ToListAsync();

            var totalReceipts = sessions.Sum(s => s.TotalReceiptsCount);
            var gaps = await _db.ReceiptGaps
                .Where(g => g.DriverId == driver.DriverId && g.DetectedAt >= from.Date && g.DetectedAt <= to.Date)
                .ToListAsync();

            results.Add(new DriverPerformanceDto
            {
                DriverId = driver.DriverId,
                DriverName = driver.FullName,
                TotalSessions = sessions.Count,
                TotalReceipts = totalReceipts,
                TotalAmountCollected = sessions.Sum(s => s.TotalAmountCollected),
                TotalGapsDetected = gaps.Count,
                TotalGapsResolved = gaps.Count(g => g.Status == "Resolved" || g.Status == "Explained"),
                TotalGapsOpen = gaps.Count(g => g.Status == "Open" || g.Status == "UnderInvestigation"),
                GapRatePercent = totalReceipts > 0 ? Math.Round((decimal)gaps.Count / totalReceipts * 100, 2) : 0
            });
        }

        return results.OrderByDescending(r => r.TotalGapsOpen).ToList();
    }
}

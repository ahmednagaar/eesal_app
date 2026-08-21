using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.Models;

namespace ReceiptSystem.API.Services;

public class GapDetectionService
{
    private readonly AppDbContext _db;

    public GapDetectionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<int>> DetectAndSaveGapsAsync(int sessionId)
    {
        var session = await _db.CollectionSessions
            .Include(s => s.Receipts)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId)
            ?? throw new InvalidOperationException("الجلسة غير موجودة");

        var submitted = session.Receipts
            .Select(r => r.ReceiptNumber)
            .OrderBy(n => n)
            .ToList();

        if (!submitted.Any()) return new List<int>();

        var missing = new List<int>();

        // CHECK 1: Gaps WITHIN this session
        for (int i = 1; i < submitted.Count; i++)
        {
            for (int n = submitted[i - 1] + 1; n < submitted[i]; n++)
            {
                missing.Add(n);
            }
        }

        // CHECK 2: Gap BETWEEN last session and this session
        var prevSession = await _db.CollectionSessions
            .Where(s => s.DriverId == session.DriverId
                     && s.SessionId != sessionId
                     && s.SessionDate <= session.SessionDate)
            .OrderByDescending(s => s.SessionDate)
            .ThenByDescending(s => s.LastReceiptNumber)
            .FirstOrDefaultAsync();

        if (prevSession != null)
        {
            for (int n = prevSession.LastReceiptNumber + 1; n < submitted.First(); n++)
            {
                missing.Add(n);
            }
        }

        // Remove any already-tracked gaps
        var existingGapNumbers = await _db.ReceiptGaps
            .Where(g => g.DriverId == session.DriverId)
            .Select(g => g.MissingReceiptNumber)
            .ToListAsync();

        var newMissing = missing.Where(m => !existingGapNumbers.Contains(m)).ToList();

        // Save gaps
        foreach (var num in newMissing)
        {
            _db.ReceiptGaps.Add(new ReceiptGap
            {
                MissingReceiptNumber = num,
                DetectedInSessionId = sessionId,
                DriverId = session.DriverId,
                Status = "Open",
                DetectedAt = DateTime.UtcNow
            });
        }

        if (missing.Any())
        {
            session.HasGaps = true;
        }

        await _db.SaveChangesAsync();

        return missing; // All missing (including previously tracked) for alert
    }
}

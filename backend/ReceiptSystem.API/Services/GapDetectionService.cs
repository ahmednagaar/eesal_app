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

        // ╔══════════════════════════════════════════════════════════╗
        // ║  Load the driver's assigned book ranges so we only      ║
        // ║  flag gaps for receipt numbers that actually belong      ║
        // ║  to books handed to this driver. This prevents false    ║
        // ║  positives when a driver finishes one book and starts   ║
        // ║  another with non-sequential numbers.                   ║
        // ╚══════════════════════════════════════════════════════════╝
        var driverBooks = await _db.ReceiptBooks
            .Where(b => b.AssignedToDriverId == session.DriverId
                     && (b.Status == "Assigned" || b.Status == "InProgress"
                         || b.Status == "Completed" || b.Status == "Returned"))
            .Select(b => new { b.StartReceiptNumber, b.EndReceiptNumber, b.BookId })
            .ToListAsync();

        // Helper: checks whether a receipt number falls inside any of this driver's books
        bool BelongsToDriverBook(int receiptNumber) =>
            driverBooks.Any(b => receiptNumber >= b.StartReceiptNumber
                              && receiptNumber <= b.EndReceiptNumber);

        // Helper: find the BookId for a receipt number
        int? FindBookId(int receiptNumber) =>
            driverBooks.FirstOrDefault(b => receiptNumber >= b.StartReceiptNumber
                                         && receiptNumber <= b.EndReceiptNumber)?.BookId;

        var missing = new List<int>();

        // CHECK 1: Gaps WITHIN this session — only flag if the missing
        // number belongs to a book assigned to the driver (handles the
        // case where a session spans two books with non-sequential numbers)
        for (int i = 1; i < submitted.Count; i++)
        {
            for (int n = submitted[i - 1] + 1; n < submitted[i]; n++)
            {
                if (BelongsToDriverBook(n))
                    missing.Add(n);
            }
        }

        // CHECK 2: Gap BETWEEN last session and this session — only flag
        // numbers that belong to a book assigned to the driver (prevents
        // hundreds of false gaps when switching from Book A to Book B)
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
                if (BelongsToDriverBook(n))
                    missing.Add(n);
            }
        }

        // Remove any already-tracked gaps
        var existingGapNumbers = await _db.ReceiptGaps
            .Where(g => g.DriverId == session.DriverId)
            .Select(g => g.MissingReceiptNumber)
            .ToListAsync();

        // Load cancelled (لاغي) numbers — these should never appear as missing
        var cancelledNumbers = await _db.ReceiptGaps
            .Where(g => g.DriverId == session.DriverId && g.Status == "Cancelled")
            .Select(g => g.MissingReceiptNumber)
            .ToListAsync();
        var cancelledSet = new HashSet<int>(cancelledNumbers);

        var newMissing = missing.Where(m => !existingGapNumbers.Contains(m)).ToList();

        // Cross-session check: don't flag a number as missing if the receipt
        // actually exists in another session for this driver (e.g. entered in
        // the wrong session, or added later to a different one).
        var existingReceiptNumbers = await _db.Receipts
            .Where(r => r.DriverId == session.DriverId && missing.Contains(r.ReceiptNumber))
            .Select(r => r.ReceiptNumber)
            .ToListAsync();

        // Remove receipts that exist elsewhere AND cancelled numbers from the lists
        newMissing = newMissing.Where(m => !existingReceiptNumbers.Contains(m) && !cancelledSet.Contains(m)).ToList();
        var actuallyMissing = missing.Where(m => !existingReceiptNumbers.Contains(m) && !cancelledSet.Contains(m)).ToList();

        // Save gaps — now with BookId for better traceability
        foreach (var num in newMissing)
        {
            _db.ReceiptGaps.Add(new ReceiptGap
            {
                MissingReceiptNumber = num,
                DetectedInSessionId = sessionId,
                DriverId = session.DriverId,
                BookId = FindBookId(num),
                Status = "Open",
                DetectedAt = DateTime.UtcNow
            });
        }

        if (actuallyMissing.Any())
        {
            session.HasGaps = true;
        }
        else
        {
            // No actual gaps — ensure the flag is correctly set to false
            session.HasGaps = false;
        }

        await _db.SaveChangesAsync();

        return actuallyMissing; // Only truly missing numbers (excluding cross-session hits)
    }
}

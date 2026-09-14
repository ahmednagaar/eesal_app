using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Models;

namespace ReceiptSystem.API.Services;

public class SessionService
{
    private readonly AppDbContext _db;
    private readonly GapDetectionService _gapService;

    public SessionService(AppDbContext db, GapDetectionService gapService)
    {
        _db = db;
        _gapService = gapService;
    }

    public async Task<(SessionDto Session, List<int> MissingReceipts)> CreateSessionAsync(CreateSessionDto dto, int userId)
    {
        // Validate no duplicate receipt numbers in request
        var receiptNumbers = dto.Receipts.Select(r => r.ReceiptNumber).ToList();
        if (receiptNumbers.Count != receiptNumbers.Distinct().Count())
            throw new InvalidOperationException("يوجد أرقام إيصالات مكررة في الطلب");

        // Check for existing receipt numbers in DB
        var existingNumbers = await _db.Receipts
            .Where(r => receiptNumbers.Contains(r.ReceiptNumber))
            .Select(r => r.ReceiptNumber)
            .ToListAsync();

        if (existingNumbers.Any())
            throw new InvalidOperationException($"أرقام الإيصالات التالية مسجلة بالفعل: {string.Join(", ", existingNumbers)}");

        var sorted = receiptNumbers.OrderBy(n => n).ToList();

        CollectionSession session;
        List<int> missingReceipts;

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            session = new CollectionSession
            {
                DriverId = dto.DriverId,
                SessionDate = dto.SessionDate.Date,
                RouteArea = dto.RouteArea,
                FirstReceiptNumber = sorted.First(),
                LastReceiptNumber = sorted.Last(),
                TotalReceiptsCount = dto.Receipts.Count,
                TotalAmountCollected = dto.Receipts.Sum(r => r.Amount),
                Notes = dto.Notes,
                EnteredByUserId = userId,
                EnteredAt = DateTime.UtcNow
            };

            _db.CollectionSessions.Add(session);
            await _db.SaveChangesAsync();

            // Add receipts
            foreach (var r in dto.Receipts)
            {
                _db.Receipts.Add(new Receipt
                {
                    ReceiptNumber = r.ReceiptNumber,
                    BookId = r.BookId,
                    SessionId = session.SessionId,
                    DriverId = dto.DriverId,
                    MerchantId = r.MerchantId,
                    CollectionDate = dto.SessionDate.Date,
                    Amount = r.Amount,
                    IsPartialPayment = r.IsPartialPayment,
                    Notes = r.Notes,
                    EnteredByUserId = userId,
                    EnteredAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();

            // Run gap detection
            missingReceipts = await _gapService.DetectAndSaveGapsAsync(session.SessionId);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        var sessionDto = await GetSessionDtoAsync(session.SessionId);
        return (sessionDto!, missingReceipts);
    }

    public async Task<SessionDto?> GetSessionDtoAsync(int sessionId)
    {
        return await _db.CollectionSessions
            .Include(s => s.Driver)
            .Include(s => s.EnteredByUser)
            .Where(s => s.SessionId == sessionId)
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
            .FirstOrDefaultAsync();
    }

    public async Task<List<SessionDto>> GetTodaySessionsAsync()
    {
        var today = DateTime.Today;
        return await _db.CollectionSessions
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
    }

    public async Task<SessionWithReceiptsDto?> GetSessionWithReceiptsAsync(int sessionId)
    {
        var session = await _db.CollectionSessions
            .Include(s => s.Driver)
            .Include(s => s.EnteredByUser)
            .Include(s => s.Receipts).ThenInclude(r => r.Merchant)
            .Include(s => s.DetectedGaps).ThenInclude(g => g.Driver)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null) return null;

        return new SessionWithReceiptsDto
        {
            SessionId = session.SessionId,
            DriverId = session.DriverId,
            DriverName = session.Driver.FullName,
            SessionDate = session.SessionDate,
            RouteArea = session.RouteArea,
            FirstReceiptNumber = session.FirstReceiptNumber,
            LastReceiptNumber = session.LastReceiptNumber,
            TotalReceiptsCount = session.TotalReceiptsCount,
            TotalAmountCollected = session.TotalAmountCollected,
            HasGaps = session.HasGaps,
            Notes = session.Notes,
            EnteredByName = session.EnteredByUser.FullName,
            EnteredAt = session.EnteredAt,
            Receipts = session.Receipts.OrderBy(r => r.ReceiptNumber).Select(r => new ReceiptDto
            {
                ReceiptId = r.ReceiptId,
                ReceiptNumber = r.ReceiptNumber,
                BookId = r.BookId,
                MerchantId = r.MerchantId,
                MerchantName = r.Merchant.MerchantName,
                CollectionDate = r.CollectionDate,
                Amount = r.Amount,
                IsPartialPayment = r.IsPartialPayment,
                Notes = r.Notes
            }).ToList(),
            Gaps = session.DetectedGaps.Select(g => new GapDto
            {
                GapId = g.GapId,
                MissingReceiptNumber = g.MissingReceiptNumber,
                DetectedInSessionId = g.DetectedInSessionId,
                DriverId = g.DriverId,
                DriverName = session.Driver.FullName,
                DetectedAt = g.DetectedAt,
                Status = g.Status,
                Resolution = g.Resolution,
                ResolvedAt = g.ResolvedAt
            }).ToList()
        };
    }

    public async Task<SessionDto?> GetLastSessionForDriverAsync(int driverId)
    {
        return await _db.CollectionSessions
            .Include(s => s.Driver)
            .Include(s => s.EnteredByUser)
            .Where(s => s.DriverId == driverId)
            .OrderByDescending(s => s.SessionDate)
            .ThenByDescending(s => s.LastReceiptNumber)
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
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Recalculate session aggregate fields from its receipts.
    /// Call after any receipt is added, edited, or deleted.
    /// </summary>
    public async Task RecalculateSessionTotalsAsync(int sessionId)
    {
        var session = await _db.CollectionSessions
            .Include(s => s.Receipts)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session == null) return;

        var receipts = session.Receipts.OrderBy(r => r.ReceiptNumber).ToList();

        if (receipts.Count == 0)
        {
            session.TotalReceiptsCount = 0;
            session.TotalAmountCollected = 0;
            session.FirstReceiptNumber = 0;
            session.LastReceiptNumber = 0;
        }
        else
        {
            session.TotalReceiptsCount = receipts.Count;
            session.TotalAmountCollected = receipts.Sum(r => r.Amount);
            session.FirstReceiptNumber = receipts.First().ReceiptNumber;
            session.LastReceiptNumber = receipts.Last().ReceiptNumber;
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Add a receipt to an existing (unconfirmed) session, then re-run gap detection.
    /// </summary>
    public async Task<(ReceiptDto Receipt, List<int> UpdatedGaps)> AddReceiptToSessionAsync(
        int sessionId, CreateReceiptDto dto, int userId)
    {
        var session = await _db.CollectionSessions.FindAsync(sessionId)
            ?? throw new InvalidOperationException("الجلسة غير موجودة");

        if (session.IsConfirmed)
            throw new InvalidOperationException("الجلسة مقفلة — لا يمكن إضافة إيصالات");

        // Check duplicate receipt number
        var exists = await _db.Receipts.AnyAsync(r => r.ReceiptNumber == dto.ReceiptNumber);
        if (exists)
            throw new InvalidOperationException($"رقم الإيصال {dto.ReceiptNumber} مسجل بالفعل");

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var receipt = new Receipt
            {
                ReceiptNumber = dto.ReceiptNumber,
                BookId = dto.BookId,
                SessionId = sessionId,
                DriverId = session.DriverId,
                MerchantId = dto.MerchantId,
                CollectionDate = session.SessionDate,
                Amount = dto.Amount,
                IsPartialPayment = dto.IsPartialPayment,
                Notes = dto.Notes,
                EnteredByUserId = userId,
                EnteredAt = DateTime.UtcNow
            };

            _db.Receipts.Add(receipt);
            await _db.SaveChangesAsync();

            // Recalculate session totals
            await RecalculateSessionTotalsAsync(sessionId);

            // Re-run gap detection — may resolve existing gaps
            var gaps = await _gapService.DetectAndSaveGapsAsync(sessionId);

            // Check if this receipt resolves an existing gap
            var resolvedGap = await _db.ReceiptGaps
                .FirstOrDefaultAsync(g => g.DriverId == session.DriverId
                    && g.MissingReceiptNumber == dto.ReceiptNumber
                    && (g.Status == "Open" || g.Status == "UnderInvestigation"));

            if (resolvedGap != null)
            {
                resolvedGap.Status = "Resolved";
                resolvedGap.Resolution = "تم إضافة الإيصال المفقود إلى الجلسة";
                resolvedGap.ResolvedAt = DateTime.UtcNow;
                resolvedGap.ResolvedByUserId = userId;
                await _db.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            var merchant = await _db.Merchants.FindAsync(dto.MerchantId);
            var receiptDto = new ReceiptDto
            {
                ReceiptId = receipt.ReceiptId,
                ReceiptNumber = receipt.ReceiptNumber,
                BookId = receipt.BookId,
                MerchantId = receipt.MerchantId,
                MerchantName = merchant?.MerchantName ?? "",
                CollectionDate = receipt.CollectionDate,
                Amount = receipt.Amount,
                IsPartialPayment = receipt.IsPartialPayment,
                Notes = receipt.Notes
            };

            return (receiptDto, gaps);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

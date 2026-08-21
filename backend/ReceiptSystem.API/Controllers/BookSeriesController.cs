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
[Route("api/book-series")]
[Authorize(Roles = "Admin,Treasury")]
public class BookSeriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public BookSeriesController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    /// <summary>
    /// Get all series with book counts
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var series = await _db.BookSeries
            .Include(s => s.Books)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new BookSeriesDto
            {
                SeriesId = s.SeriesId,
                SeriesCode = s.SeriesCode,
                TotalBooks = s.TotalBooks,
                ReceiptsPerBook = s.ReceiptsPerBook,
                StartReceiptNumber = s.StartReceiptNumber,
                EndReceiptNumber = s.EndReceiptNumber,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                Notes = s.Notes,
                AvailableBooks = s.Books.Count(b => b.Status == "Available"),
                AssignedBooks = s.Books.Count(b => b.Status == "Assigned" || b.Status == "InProgress"),
                CompletedBooks = s.Books.Count(b => b.Status == "Completed" || b.Status == "Returned")
            })
            .ToListAsync();

        return Ok(series);
    }

    /// <summary>
    /// Create a new series and auto-generate all books
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookSeriesDto dto)
    {
        if (await _db.BookSeries.AnyAsync(s => s.SeriesCode == dto.SeriesCode))
            return BadRequest(new { message = $"الدورة '{dto.SeriesCode}' موجودة بالفعل" });

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            int endReceiptNumber = dto.StartReceiptNumber + (dto.TotalBooks * dto.ReceiptsPerBook) - 1;

            var series = new BookSeries
            {
                SeriesCode = dto.SeriesCode,
                TotalBooks = dto.TotalBooks,
                ReceiptsPerBook = dto.ReceiptsPerBook,
                StartReceiptNumber = dto.StartReceiptNumber,
                EndReceiptNumber = endReceiptNumber,
                Status = "Active",
                CreatedByUserId = UserId,
                Notes = dto.Notes
            };
            _db.BookSeries.Add(series);
            await _db.SaveChangesAsync();

            // Auto-generate all books
            for (int i = 0; i < dto.TotalBooks; i++)
            {
                int bookNumber = i + 1;
                int start = dto.StartReceiptNumber + (i * dto.ReceiptsPerBook);
                int end = start + dto.ReceiptsPerBook - 1;

                _db.ReceiptBooks.Add(new ReceiptBook
                {
                    SeriesId = series.SeriesId,
                    BookNumber = bookNumber,
                    StartReceiptNumber = start,
                    EndReceiptNumber = end,
                    Status = "Available"
                });
            }
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            await _audit.LogAsync(UserId, "إنشاء دورة دفاتر", "BookSeries", series.SeriesId,
                newValues: new { dto.SeriesCode, dto.TotalBooks, dto.ReceiptsPerBook, endReceiptNumber });

            return Ok(new
            {
                seriesId = series.SeriesId,
                message = $"تم إنشاء الدورة '{dto.SeriesCode}' بنجاح — {dto.TotalBooks} دفتر ({dto.StartReceiptNumber} إلى {endReceiptNumber})",
                booksCreated = dto.TotalBooks
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Get series detail with all books and their status
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var series = await _db.BookSeries
            .Include(s => s.Books)
                .ThenInclude(b => b.AssignedToDriver)
            .Include(s => s.Books)
                .ThenInclude(b => b.Receipts)
            .FirstOrDefaultAsync(s => s.SeriesId == id);

        if (series == null) return NotFound(new { message = "الدورة غير موجودة" });

        var books = series.Books.OrderBy(b => b.BookNumber).Select(b => new BookDto
        {
            BookId = b.BookId,
            SeriesId = b.SeriesId,
            SeriesCode = series.SeriesCode,
            BookNumber = b.BookNumber,
            DisplayName = $"{series.SeriesCode}-{b.BookNumber}",
            StartReceiptNumber = b.StartReceiptNumber,
            EndReceiptNumber = b.EndReceiptNumber,
            AssignedToDriverId = b.AssignedToDriverId,
            DriverName = b.AssignedToDriver?.FullName,
            AssignedDate = b.AssignedDate,
            Status = b.Status,
            Notes = b.Notes,
            UsedReceipts = b.Receipts.Count,
            RemainingReceipts = (b.EndReceiptNumber - b.StartReceiptNumber + 1) - b.Receipts.Count,
            ReturnedDate = b.ReturnedDate,
            IsVerified = b.IsVerified,
            VerifiedAt = b.VerifiedAt
        }).ToList();

        return Ok(new
        {
            series = new BookSeriesDto
            {
                SeriesId = series.SeriesId,
                SeriesCode = series.SeriesCode,
                TotalBooks = series.TotalBooks,
                ReceiptsPerBook = series.ReceiptsPerBook,
                StartReceiptNumber = series.StartReceiptNumber,
                EndReceiptNumber = series.EndReceiptNumber,
                Status = series.Status,
                CreatedAt = series.CreatedAt,
                Notes = series.Notes,
                AvailableBooks = series.Books.Count(b => b.Status == "Available"),
                AssignedBooks = series.Books.Count(b => b.Status == "Assigned" || b.Status == "InProgress"),
                CompletedBooks = series.Books.Count(b => b.Status == "Completed" || b.Status == "Returned")
            },
            books
        });
    }

    /// <summary>
    /// Mark series as completed when all books are returned
    /// </summary>
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var series = await _db.BookSeries.Include(s => s.Books).FirstOrDefaultAsync(s => s.SeriesId == id);
        if (series == null) return NotFound(new { message = "الدورة غير موجودة" });

        var unreturned = series.Books.Count(b => b.Status != "Returned" && b.Status != "Completed" && b.Status != "Deactivated");
        if (unreturned > 0)
            return BadRequest(new { message = $"لا يمكن إغلاق الدورة — يوجد {unreturned} دفتر لم يتم إرجاعه بعد" });

        series.Status = "Completed";
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إغلاق دورة دفاتر", "BookSeries", id);

        return Ok(new { message = "تم إغلاق الدورة بنجاح" });
    }

    /// <summary>
    /// Dashboard alerts: low stock, overdue books, series running out
    /// </summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts()
    {
        var activeSeries = await _db.BookSeries
            .Where(s => s.Status == "Active")
            .Include(s => s.Books)
                .ThenInclude(b => b.Receipts)
            .Include(s => s.Books)
                .ThenInclude(b => b.AssignedToDriver)
            .ToListAsync();

        var alerts = new List<object>();

        foreach (var series in activeSeries)
        {
            // Alert: Series running out of available books
            var available = series.Books.Count(b => b.Status == "Available");
            if (available <= 5)
            {
                alerts.Add(new
                {
                    type = "series_low",
                    severity = available == 0 ? "critical" : "warning",
                    message = available == 0
                        ? $"⛔ الدورة {series.SeriesCode}: لا يوجد دفاتر متاحة!"
                        : $"⚠️ الدورة {series.SeriesCode}: باقي {available} دفاتر متاحة فقط",
                    seriesId = series.SeriesId,
                    seriesCode = series.SeriesCode,
                    availableBooks = available
                });
            }

            foreach (var book in series.Books)
            {
                // Alert: Book running low on receipts
                int totalReceipts = book.EndReceiptNumber - book.StartReceiptNumber + 1;
                int used = book.Receipts.Count;
                int remaining = totalReceipts - used;

                if ((book.Status == "Assigned" || book.Status == "InProgress") && remaining <= 10 && remaining > 0)
                {
                    alerts.Add(new
                    {
                        type = "book_low",
                        severity = remaining <= 3 ? "critical" : "warning",
                        message = $"🟡 دفتر {series.SeriesCode}-{book.BookNumber}: باقي {remaining} إيصال فقط (سائق: {book.AssignedToDriver?.FullName})",
                        bookId = book.BookId,
                        bookName = $"{series.SeriesCode}-{book.BookNumber}",
                        driverName = book.AssignedToDriver?.FullName,
                        remaining
                    });
                }

                // Alert: Book finished (all receipts used)
                if ((book.Status == "Assigned" || book.Status == "InProgress") && remaining == 0)
                {
                    alerts.Add(new
                    {
                        type = "book_finished",
                        severity = "critical",
                        message = $"🔴 دفتر {series.SeriesCode}-{book.BookNumber}: خلص! يجب إرجاعه (سائق: {book.AssignedToDriver?.FullName})",
                        bookId = book.BookId,
                        bookName = $"{series.SeriesCode}-{book.BookNumber}",
                        driverName = book.AssignedToDriver?.FullName
                    });
                }

                // Alert: Book overdue (assigned > 7 days ago, not returned)
                if (book.AssignedDate != null && book.Status == "Assigned" &&
                    (DateTime.Today - book.AssignedDate.Value).TotalDays > 7)
                {
                    int days = (int)(DateTime.Today - book.AssignedDate.Value).TotalDays;
                    alerts.Add(new
                    {
                        type = "book_overdue",
                        severity = days > 14 ? "critical" : "warning",
                        message = $"⚠️ دفتر {series.SeriesCode}-{book.BookNumber}: مع السائق {book.AssignedToDriver?.FullName} من {days} يوم!",
                        bookId = book.BookId,
                        bookName = $"{series.SeriesCode}-{book.BookNumber}",
                        driverName = book.AssignedToDriver?.FullName,
                        daysSinceAssigned = days
                    });
                }
            }
        }

        return Ok(new { alertCount = alerts.Count, alerts });
    }
}

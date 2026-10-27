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
[Route("api/return-book-series")]
[Authorize(Roles = "Admin,Returns")]
public class ReturnBookSeriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public ReturnBookSeriesController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // GET api/return-book-series
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var series = await _db.ReturnBookSeries
            .Include(s => s.CreatedByUser)
            .Include(s => s.Books)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new ReturnBookSeriesDto
            {
                SeriesId = s.SeriesId,
                SeriesCode = s.SeriesCode,
                TotalBooks = s.TotalBooks,
                ReceiptsPerBook = s.ReceiptsPerBook,
                StartReceiptNumber = s.StartReceiptNumber,
                EndReceiptNumber = s.EndReceiptNumber,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                CreatedByUserName = s.CreatedByUser.FullName,
                Notes = s.Notes,
                BookCount = s.Books.Count,
                AssignedBookCount = s.Books.Count(b => b.Status == "Assigned" || b.Status == "InProgress")
            })
            .ToListAsync();
        return Ok(series);
    }

    // GET api/return-book-series/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var series = await _db.ReturnBookSeries
            .Include(s => s.CreatedByUser)
            .Include(s => s.Books)
            .Where(s => s.SeriesId == id)
            .Select(s => new ReturnBookSeriesDto
            {
                SeriesId = s.SeriesId,
                SeriesCode = s.SeriesCode,
                TotalBooks = s.TotalBooks,
                ReceiptsPerBook = s.ReceiptsPerBook,
                StartReceiptNumber = s.StartReceiptNumber,
                EndReceiptNumber = s.EndReceiptNumber,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                CreatedByUserName = s.CreatedByUser.FullName,
                Notes = s.Notes,
                BookCount = s.Books.Count,
                AssignedBookCount = s.Books.Count(b => b.Status == "Assigned" || b.Status == "InProgress")
            })
            .FirstOrDefaultAsync();
        if (series == null) return NotFound(new { message = "السلسلة غير موجودة" });
        return Ok(series);
    }

    // POST api/return-book-series — Creates series + all its books
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateReturnBookSeriesDto dto)
    {
        var exists = await _db.ReturnBookSeries.AnyAsync(s => s.SeriesCode == dto.SeriesCode);
        if (exists) return BadRequest(new { message = "كود السلسلة موجود بالفعل" });

        int endNumber = dto.StartReceiptNumber + (dto.TotalBooks * dto.ReceiptsPerBook) - 1;

        var series = new ReturnBookSeries
        {
            SeriesCode = dto.SeriesCode,
            TotalBooks = dto.TotalBooks,
            ReceiptsPerBook = dto.ReceiptsPerBook,
            StartReceiptNumber = dto.StartReceiptNumber,
            EndReceiptNumber = endNumber,
            Notes = dto.Notes,
            CreatedByUserId = UserId
        };
        _db.ReturnBookSeries.Add(series);
        await _db.SaveChangesAsync();

        // Generate all books for this series
        var books = new List<ReturnReceiptBook>();
        for (int i = 0; i < dto.TotalBooks; i++)
        {
            int bookStart = dto.StartReceiptNumber + (i * dto.ReceiptsPerBook);
            int bookEnd = bookStart + dto.ReceiptsPerBook - 1;
            books.Add(new ReturnReceiptBook
            {
                SeriesId = series.SeriesId,
                BookNumber = i + 1,
                StartReceiptNumber = bookStart,
                EndReceiptNumber = bookEnd
            });
        }
        _db.ReturnReceiptBooks.AddRange(books);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(UserId, "إضافة سلسلة مرتجعات", "ReturnBookSeries", series.SeriesId,
            newValues: new { dto.SeriesCode, dto.TotalBooks, dto.ReceiptsPerBook, BooksGenerated = books.Count });

        return CreatedAtAction(nameof(Get), new { id = series.SeriesId }, new { series.SeriesId, series.SeriesCode, BooksGenerated = books.Count });
    }

    // ══════════════════════════════════════
    // Return Receipt Books
    // ══════════════════════════════════════

    // GET api/return-book-series/5/books
    [HttpGet("{seriesId}/books")]
    public async Task<IActionResult> GetBooks(int seriesId, [FromQuery] string? status = null)
    {
        var query = _db.ReturnReceiptBooks
            .Where(b => b.SeriesId == seriesId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(b => b.Status == status);

        var books = await query
            .Include(b => b.AssignedToDriver)
            .Include(b => b.VerifiedByUser)
            .OrderBy(b => b.BookNumber)
            .Select(b => new ReturnReceiptBookDto
            {
                BookId = b.BookId,
                SeriesId = b.SeriesId,
                SeriesCode = b.Series != null ? b.Series.SeriesCode : null,
                BookNumber = b.BookNumber,
                StartReceiptNumber = b.StartReceiptNumber,
                EndReceiptNumber = b.EndReceiptNumber,
                AssignedToDriverId = b.AssignedToDriverId,
                AssignedToDriverName = b.AssignedToDriver != null ? b.AssignedToDriver.FullName : null,
                AssignedDate = b.AssignedDate,
                Status = b.Status,
                Notes = b.Notes,
                CreatedAt = b.CreatedAt,
                IsVerified = b.IsVerified,
                VerifiedAt = b.VerifiedAt,
                VerifiedByUserName = b.VerifiedByUser != null ? b.VerifiedByUser.FullName : null,
                ReturnsCount = b.GoodsReturns.Count
            })
            .ToListAsync();
        return Ok(books);
    }

    // POST api/return-book-series/books/5/assign
    [HttpPost("books/{bookId}/assign")]
    public async Task<IActionResult> AssignBook(int bookId, [FromBody] AssignReturnBookDto dto)
    {
        var book = await _db.ReturnReceiptBooks.FindAsync(bookId);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });
        if (book.Status != "Available" && book.Status != "Returned")
            return BadRequest(new { message = $"لا يمكن تسليم دفتر في حالة {book.Status}" });

        var driver = await _db.Drivers.FindAsync(dto.DriverId);
        if (driver == null || !driver.IsActive)
            return BadRequest(new { message = "السائق غير موجود أو غير نشط" });

        book.AssignedToDriverId = dto.DriverId;
        book.AssignedDate = DateTime.UtcNow;
        book.AssignedByUserId = UserId;
        book.Status = "Assigned";
        book.Notes = dto.Notes;

        // Log movement
        _db.ReturnBookMovements.Add(new ReturnBookMovement
        {
            BookId = bookId,
            ActionType = "Delivered",
            ToDriverId = dto.DriverId,
            PerformedByUserId = UserId,
            Notes = dto.Notes
        });

        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تسليم دفتر مرتجعات", "ReturnReceiptBook", bookId,
            newValues: new { dto.DriverId, DriverName = driver.FullName });

        return Ok(new { message = "تم تسليم الدفتر للسائق بنجاح" });
    }

    // POST api/return-book-series/books/5/return
    [HttpPost("books/{bookId}/return")]
    public async Task<IActionResult> ReturnBook(int bookId, [FromBody] ReturnBookActionDto dto)
    {
        var book = await _db.ReturnReceiptBooks.Include(b => b.AssignedToDriver).FirstOrDefaultAsync(b => b.BookId == bookId);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });
        if (book.Status != "Assigned" && book.Status != "InProgress")
            return BadRequest(new { message = $"لا يمكن استرجاع دفتر في حالة {book.Status}" });

        var previousDriverId = book.AssignedToDriverId;
        book.Status = "Returned";
        book.ReturnedDate = DateTime.UtcNow;
        book.ReturnedToUserId = UserId;

        _db.ReturnBookMovements.Add(new ReturnBookMovement
        {
            BookId = bookId,
            ActionType = "Returned",
            FromDriverId = previousDriverId,
            PerformedByUserId = UserId,
            Notes = dto.Notes
        });

        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "استرجاع دفتر مرتجعات", "ReturnReceiptBook", bookId);

        return Ok(new { message = "تم استرجاع الدفتر بنجاح" });
    }

    // POST api/return-book-series/books/5/verify
    [HttpPost("books/{bookId}/verify")]
    public async Task<IActionResult> VerifyBook(int bookId, [FromBody] ReturnBookActionDto dto)
    {
        var book = await _db.ReturnReceiptBooks.FindAsync(bookId);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        book.IsVerified = true;
        book.VerifiedAt = DateTime.UtcNow;
        book.VerifiedByUserId = UserId;

        _db.ReturnBookMovements.Add(new ReturnBookMovement
        {
            BookId = bookId,
            ActionType = "Verified",
            PerformedByUserId = UserId,
            Notes = dto.Notes
        });

        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "مراجعة دفتر مرتجعات", "ReturnReceiptBook", bookId);

        return Ok(new { message = "تم مراجعة الدفتر بنجاح" });
    }

    // GET api/return-book-series/books/5/movements
    [HttpGet("books/{bookId}/movements")]
    public async Task<IActionResult> GetMovements(int bookId)
    {
        var movements = await _db.ReturnBookMovements
            .Where(m => m.BookId == bookId)
            .Include(m => m.FromDriver)
            .Include(m => m.ToDriver)
            .Include(m => m.PerformedByUser)
            .OrderByDescending(m => m.PerformedAt)
            .Select(m => new
            {
                m.MovementId,
                m.ActionType,
                FromDriverName = m.FromDriver != null ? m.FromDriver.FullName : null,
                ToDriverName = m.ToDriver != null ? m.ToDriver.FullName : null,
                PerformedByUserName = m.PerformedByUser.FullName,
                m.PerformedAt,
                m.Notes
            })
            .ToListAsync();
        return Ok(movements);
    }
}

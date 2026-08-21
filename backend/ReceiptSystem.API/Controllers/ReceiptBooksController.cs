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
[Route("api/books")]
[Authorize(Roles = "Admin,Treasury")]
public class ReceiptBooksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public ReceiptBooksController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var books = await _db.ReceiptBooks
            .Include(b => b.AssignedToDriver)
            .OrderBy(b => b.BookNumber)
            .Select(b => new BookDto
            {
                BookId = b.BookId,
                BookNumber = b.BookNumber,
                StartReceiptNumber = b.StartReceiptNumber,
                EndReceiptNumber = b.EndReceiptNumber,
                AssignedToDriverId = b.AssignedToDriverId,
                DriverName = b.AssignedToDriver != null ? b.AssignedToDriver.FullName : null,
                AssignedDate = b.AssignedDate,
                Status = b.Status,
                Notes = b.Notes
            })
            .ToListAsync();
        return Ok(books);
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var books = await _db.ReceiptBooks
            .Where(b => b.Status == "Available")
            .OrderBy(b => b.BookNumber)
            .Select(b => new BookDto
            {
                BookId = b.BookId,
                BookNumber = b.BookNumber,
                StartReceiptNumber = b.StartReceiptNumber,
                EndReceiptNumber = b.EndReceiptNumber,
                Status = b.Status
            })
            .ToListAsync();
        return Ok(books);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
    {
        if (dto.StartReceiptNumber >= dto.EndReceiptNumber)
            return BadRequest(new { message = "رقم البداية يجب أن يكون أقل من رقم النهاية" });

        if (await _db.ReceiptBooks.AnyAsync(b => b.BookNumber == dto.BookNumber))
            return BadRequest(new { message = "رقم الدفتر موجود بالفعل" });

        var book = new ReceiptBook
        {
            BookNumber = dto.BookNumber,
            StartReceiptNumber = dto.StartReceiptNumber,
            EndReceiptNumber = dto.EndReceiptNumber,
            Notes = dto.Notes,
            Status = "Available"
        };
        _db.ReceiptBooks.Add(book);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إضافة", "ReceiptBook", book.BookId, newValues: dto);
        return Ok(new BookDto
        {
            BookId = book.BookId,
            BookNumber = book.BookNumber,
            StartReceiptNumber = book.StartReceiptNumber,
            EndReceiptNumber = book.EndReceiptNumber,
            Status = book.Status,
            Notes = book.Notes
        });
    }

    [HttpPut("{id}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignBookDto dto)
    {
        var book = await _db.ReceiptBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });
        book.AssignedToDriverId = dto.DriverId;
        book.AssignedDate = dto.AssignedDate ?? DateTime.Today;
        book.AssignedByUserId = UserId;
        book.Status = "Assigned";
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تسليم دفتر", "ReceiptBook", id, newValues: dto);
        return Ok(new { message = "تم تسليم الدفتر للسائق بنجاح" });
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var books = await _db.ReceiptBooks
            .Include(b => b.AssignedToDriver)
            .Include(b => b.Receipts)
            .Where(b => b.Status != "Completed")
            .ToListAsync();

        var lowStock = books
            .Select(b => new LowStockBookDto
            {
                BookId = b.BookId,
                BookNumber = b.BookNumber,
                DriverName = b.AssignedToDriver?.FullName,
                Remaining = (b.EndReceiptNumber - b.StartReceiptNumber + 1) - b.Receipts.Count,
                EndReceiptNumber = b.EndReceiptNumber
            })
            .Where(b => b.Remaining <= 10)
            .OrderBy(b => b.Remaining)
            .ToList();

        return Ok(lowStock);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EditBookDto dto)
    {
        var book = await _db.ReceiptBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });
        if (book.Status == "Assigned")
            return BadRequest(new { message = "لا يمكن تعديل دفتر تم تسليمه بالفعل" });

        var old = new { book.Notes, book.BookNumber };
        if (dto.BookNumber.HasValue) book.BookNumber = dto.BookNumber.Value;
        if (dto.Notes != null) book.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تعديل دفتر", "ReceiptBook", id, oldValues: old, newValues: dto);

        return Ok(new { message = "تم تعديل الدفتر بنجاح" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var book = await _db.ReceiptBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        book.Status = "Deactivated";
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تعطيل دفتر", "ReceiptBook", id);

        return Ok(new { message = "تم تعطيل الدفتر بنجاح" });
    }

    /// <summary>
    /// Driver returns a finished book
    /// </summary>
    [HttpPut("{id}/return")]
    public async Task<IActionResult> ReturnBook(int id, [FromBody] ReturnBookDto dto)
    {
        var book = await _db.ReceiptBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        if (book.Status != "Assigned" && book.Status != "InProgress" && book.Status != "Completed")
            return BadRequest(new { message = "هذا الدفتر ليس مع سائق — لا يمكن إرجاعه" });

        book.Status = "Returned";
        book.ReturnedDate = DateTime.UtcNow;
        book.ReturnedToUserId = UserId;
        if (dto.Notes != null) book.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "إرجاع دفتر", "ReceiptBook", id,
            newValues: new { ReturnedDate = book.ReturnedDate, dto.Notes });

        return Ok(new { message = "تم تسجيل إرجاع الدفتر بنجاح" });
    }

    /// <summary>
    /// Manager verifies a returned book
    /// </summary>
    [HttpPut("{id}/verify")]
    public async Task<IActionResult> VerifyBook(int id)
    {
        var book = await _db.ReceiptBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        if (book.Status != "Returned")
            return BadRequest(new { message = "يجب إرجاع الدفتر أولاً قبل التحقق منه" });

        book.IsVerified = true;
        book.VerifiedAt = DateTime.UtcNow;
        book.VerifiedByUserId = UserId;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تحقق من دفتر", "ReceiptBook", id);

        return Ok(new { message = "تم التحقق من الدفتر بنجاح ✓" });
    }
}

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



    [HttpPut("{id}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignBookDto dto)
    {
        var book = await _db.ReceiptBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        if (book.Status != "Available" && book.Status != "Returned")
            return BadRequest(new { message = $"لا يمكن تسليم دفتر بحالة '{book.Status}' — يجب أن يكون متاح أو مُرجع" });

        book.AssignedToDriverId = dto.DriverId;
        book.AssignedDate = dto.AssignedDate ?? DateTime.Today;
        book.AssignedByUserId = UserId;
        book.Status = "Assigned";
        book.IsVerified = false;
        book.VerifiedAt = null;
        book.VerifiedByUserId = null;
        book.ReturnedDate = null;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تسليم دفتر", "ReceiptBook", id, newValues: dto);
        return Ok(new { message = "تم تسليم الدفتر للسائق بنجاح" });
    }

    /// <summary>
    /// Bulk assign multiple books to a single driver
    /// </summary>
    [HttpPost("assign-batch")]
    public async Task<IActionResult> AssignBatch([FromBody] AssignBatchDto dto)
    {
        if (dto.BookIds == null || dto.BookIds.Length == 0)
            return BadRequest(new { message = "اختر دفتر واحد على الأقل" });

        var books = await _db.ReceiptBooks
            .Where(b => dto.BookIds.Contains(b.BookId))
            .ToListAsync();

        var unavailable = books.Where(b => b.Status != "Available").ToList();
        if (unavailable.Count > 0)
            return BadRequest(new { message = $"يوجد {unavailable.Count} دفتر غير متاح للتسليم" });

        var assignDate = dto.AssignedDate ?? DateTime.Today;
        foreach (var book in books)
        {
            book.AssignedToDriverId = dto.DriverId;
            book.AssignedDate = assignDate;
            book.AssignedByUserId = UserId;
            book.Status = "Assigned";
        }
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "تسليم دفاتر (جماعي)", "ReceiptBook", 0,
            newValues: new { dto.BookIds, dto.DriverId, Count = books.Count });

        return Ok(new { message = $"تم تسليم {books.Count} دفتر للسائق بنجاح", assignedCount = books.Count });
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

    /// <summary>
    /// Get activity history for a specific book from audit log
    /// </summary>
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var book = await _db.ReceiptBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "الدفتر غير موجود" });

        var logs = await _db.AuditLogs
            .Where(l => l.EntityType == "ReceiptBook" && l.EntityId == id)
            .OrderByDescending(l => l.CreatedAt)
            .Include(l => l.User)
            .Take(50)
            .Select(l => new
            {
                action = l.Action,
                userName = l.User.FullName,
                date = l.CreatedAt,
                details = l.NewValues
            })
            .ToListAsync();

        return Ok(logs);
    }
}

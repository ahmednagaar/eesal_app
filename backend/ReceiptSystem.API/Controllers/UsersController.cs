using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReceiptSystem.API.Data;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Services;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ReceiptSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public UsersController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    private static readonly string[] ValidRoles = { "Admin", "Treasury", "CallCenter" };

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .OrderBy(u => u.Username)
            .Select(u => new UserListDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!ValidRoles.Contains(dto.Role))
            return BadRequest(new { message = $"الدور غير صالح. الأدوار المتاحة: {string.Join(", ", ValidRoles)}" });

        if (!ValidatePassword(dto.Password, out var passwordError))
            return BadRequest(new { message = passwordError });

        var exists = await _db.Users.AnyAsync(u => u.Username == dto.Username);
        if (exists)
            return BadRequest(new { message = "اسم المستخدم موجود بالفعل" });

        var user = new Models.User
        {
            Username = dto.Username,
            FullName = dto.FullName,
            Role = dto.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "CreateUser", "User", user.UserId, newValues: new { dto.Username, dto.FullName, dto.Role });

        return Ok(new { message = "تم إنشاء المستخدم بنجاح", userId = user.UserId });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "المستخدم غير موجود" });

        if (!ValidRoles.Contains(dto.Role))
            return BadRequest(new { message = $"الدور غير صالح. الأدوار المتاحة: {string.Join(", ", ValidRoles)}" });

        var old = new { user.FullName, user.Role, user.IsActive };
        user.FullName = dto.FullName;
        user.Role = dto.Role;
        user.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "UpdateUser", "User", id, oldValues: old, newValues: dto);

        return Ok(new { message = "تم تحديث المستخدم بنجاح" });
    }

    [HttpPut("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "المستخدم غير موجود" });

        if (!ValidatePassword(dto.NewPassword, out var passwordError))
            return BadRequest(new { message = passwordError });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "ResetUserPassword", "User", id);

        return Ok(new { message = "تم إعادة تعيين كلمة المرور بنجاح" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        if (id == UserId)
            return BadRequest(new { message = "لا يمكنك حذف حسابك الخاص" });

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "المستخدم غير موجود" });

        user.IsActive = false;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "DeactivateUser", "User", id);

        return Ok(new { message = "تم تعطيل المستخدم بنجاح" });
    }

    private static bool ValidatePassword(string password, out string error)
    {
        error = string.Empty;
        if (password.Length < 8)
        {
            error = "كلمة المرور يجب أن تكون 8 أحرف على الأقل";
            return false;
        }
        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            error = "كلمة المرور يجب أن تحتوي على رقم واحد على الأقل";
            return false;
        }
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            error = "كلمة المرور يجب أن تحتوي على حرف كبير واحد على الأقل";
            return false;
        }
        return true;
    }
}

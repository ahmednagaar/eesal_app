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
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public AuthController(AuthService authService, AppDbContext db, AuditService audit)
    {
        _authService = authService;
        _db = db;
        _audit = audit;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result == null)
            return Unauthorized(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة" });
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var info = await _authService.GetUserInfoAsync(UserId);
        if (info == null) return NotFound();
        return Ok(info);
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            return BadRequest(new { message = "كلمة المرور الجديدة وتأكيدها غير متطابقتين" });

        if (!ValidatePassword(dto.NewPassword, out var passwordError))
            return BadRequest(new { message = passwordError });

        var user = await _db.Users.FindAsync(UserId);
        if (user == null) return NotFound(new { message = "المستخدم غير موجود" });

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "كلمة المرور الحالية غير صحيحة" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(UserId, "ChangePassword", "User", UserId);

        return Ok(new { message = "تم تغيير كلمة المرور بنجاح" });
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> Refresh()
    {
        var user = await _db.Users.FindAsync(UserId);
        if (user == null || !user.IsActive) return Unauthorized(new { message = "المستخدم غير موجود أو معطّل" });

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var token = _authService.GenerateTokenForUser(user);

        return Ok(new { token, expiresAt = DateTime.UtcNow.AddHours(8) });
    }

    private static bool ValidatePassword(string password, out string error)
    {
        error = string.Empty;
        if (password.Length < 8)
        { error = "كلمة المرور يجب أن تكون 8 أحرف على الأقل"; return false; }
        if (!Regex.IsMatch(password, @"[0-9]"))
        { error = "كلمة المرور يجب أن تحتوي على رقم واحد على الأقل"; return false; }
        if (!Regex.IsMatch(password, @"[A-Z]"))
        { error = "كلمة المرور يجب أن تحتوي على حرف كبير واحد على الأقل"; return false; }
        return true;
    }
}

using System.ComponentModel.DataAnnotations;

namespace ReceiptSystem.API.DTOs;

// ── Auth ──
public class LoginDto
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    public string Username { get; set; } = string.Empty;
    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    public string Password { get; set; } = string.Empty;
}

public class TokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class UserInfoDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

// ── Drivers ──
public class DriverDto
{
    public int DriverId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OpenGapsCount { get; set; }
}

public class CreateDriverDto
{
    [Required(ErrorMessage = "اسم السائق مطلوب")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Notes { get; set; }
}

// ── Merchants ──
public class MerchantDto
{
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class CreateMerchantDto
{
    [Required(ErrorMessage = "اسم التاجر مطلوب")]
    [MaxLength(200)]
    public string MerchantName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? City { get; set; }
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    [MaxLength(500)]
    public string? Notes { get; set; }
}

// ── Book Series ──
public class BookSeriesDto
{
    public int SeriesId { get; set; }
    public string SeriesCode { get; set; } = string.Empty;
    public int TotalBooks { get; set; }
    public int ReceiptsPerBook { get; set; }
    public int StartReceiptNumber { get; set; }
    public int EndReceiptNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }

    // Computed
    public int AvailableBooks { get; set; }
    public int AssignedBooks { get; set; }
    public int CompletedBooks { get; set; }
}

public class CreateBookSeriesDto
{
    [Required(ErrorMessage = "رمز الدورة مطلوب")]
    [MaxLength(10)]
    public string SeriesCode { get; set; } = string.Empty;
    public int TotalBooks { get; set; } = 500;
    public int ReceiptsPerBook { get; set; } = 50;
    public int StartReceiptNumber { get; set; } = 1;
    public string? Notes { get; set; }
}

// ── Receipt Books ──
public class BookDto
{
    public int BookId { get; set; }
    public int? SeriesId { get; set; }
    public string? SeriesCode { get; set; }
    public int BookNumber { get; set; }
    public string DisplayName { get; set; } = string.Empty; // e.g. "A-15"
    public int StartReceiptNumber { get; set; }
    public int EndReceiptNumber { get; set; }
    public int? AssignedToDriverId { get; set; }
    public string? DriverName { get; set; }
    public DateTime? AssignedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Lifecycle tracking
    public int UsedReceipts { get; set; }
    public int RemainingReceipts { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public class CreateBookDto
{
    [Required] public int BookNumber { get; set; }
    [Required] public int StartReceiptNumber { get; set; }
    [Required] public int EndReceiptNumber { get; set; }
    public int? SeriesId { get; set; }
    public string? Notes { get; set; }
}

public class EditBookDto
{
    public string? Notes { get; set; }
    public int? BookNumber { get; set; }
}

public class AssignBookDto
{
    [Required] public int DriverId { get; set; }
    public DateTime? AssignedDate { get; set; }
}

public class ReturnBookDto
{
    public string? Notes { get; set; }
}

// ── User Management ──
public class UserListDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class CreateUserDto
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    [Required(ErrorMessage = "الدور مطلوب")]
    public string Role { get; set; } = string.Empty;
    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [MinLength(8, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل")]
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    public string FullName { get; set; } = string.Empty;
    [Required(ErrorMessage = "الدور مطلوب")]
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class ResetPasswordDto
{
    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [MinLength(8, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل")]
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [MinLength(8, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل")]
    public string NewPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

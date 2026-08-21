namespace ReceiptSystem.API.Models;

public class SystemSetting
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedByUserId { get; set; }

    // Navigation
    public User? UpdatedByUser { get; set; }
}

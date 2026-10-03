namespace ReceiptSystem.API.Models;

public class Route
{
    public int RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }

    // Navigation
    public User CreatedByUser { get; set; } = null!;
    public ICollection<RouteMerchant> RouteMerchants { get; set; } = new List<RouteMerchant>();
    public ICollection<AjalSession> AjalSessions { get; set; } = new List<AjalSession>();
}

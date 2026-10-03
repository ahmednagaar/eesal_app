namespace ReceiptSystem.API.Models;

public class AjalSession
{
    public int SessionId { get; set; }
    public DateTime SessionDate { get; set; }
    public int RouteId { get; set; }
    public int? DriverId { get; set; }
    public string? Notes { get; set; }
    public int EnteredByUserId { get; set; }
    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Route Route { get; set; } = null!;
    public Driver? Driver { get; set; }
    public User EnteredByUser { get; set; } = null!;
    public ICollection<AjalEntry> Entries { get; set; } = new List<AjalEntry>();
}

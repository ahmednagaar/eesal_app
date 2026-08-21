namespace ReceiptSystem.API.Models;

public class DeliveryDay
{
    public int DeliveryDayId { get; set; }
    public int RouteId { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string? AssignedDriver { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? PrintedAt { get; set; }

    // Navigation
    public Route Route { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<DayInvoice> DayInvoices { get; set; } = new List<DayInvoice>();
}

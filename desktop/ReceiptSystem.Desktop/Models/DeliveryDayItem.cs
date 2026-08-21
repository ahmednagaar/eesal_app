using Newtonsoft.Json;

namespace ReceiptSystem.Desktop.Models;

public class DeliveryDayItem
{
    [JsonProperty("deliveryDayId")]
    public int DeliveryDayId { get; set; }

    [JsonProperty("routeId")]
    public int RouteId { get; set; }

    [JsonProperty("routeName")]
    public string RouteName { get; set; } = "";

    [JsonProperty("deliveryDate")]
    public DateTime DeliveryDate { get; set; }

    [JsonProperty("assignedDriver")]
    public string? AssignedDriver { get; set; }

    [JsonProperty("totalMerchants")]
    public int TotalMerchants { get; set; }

    [JsonProperty("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = "";
}

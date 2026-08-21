using Newtonsoft.Json;

namespace ReceiptSystem.Desktop.Models;

public class Route
{
    [JsonProperty("routeId")]
    public int RouteId { get; set; }

    [JsonProperty("routeName")]
    public string RouteName { get; set; } = "";

    [JsonProperty("isActive")]
    public bool IsActive { get; set; }

    [JsonProperty("notes")]
    public string? Notes { get; set; }

    [JsonProperty("merchantCount")]
    public int MerchantCount { get; set; }

    // Display helper for ComboBox
    public string DisplayText => $"{RouteName} ({MerchantCount} تاجر)";
}

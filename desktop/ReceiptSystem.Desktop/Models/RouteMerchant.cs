using Newtonsoft.Json;

namespace ReceiptSystem.Desktop.Models;

public class RouteMerchant
{
    [JsonProperty("routeMerchantId")]
    public int RouteMerchantId { get; set; }

    [JsonProperty("merchantId")]
    public int MerchantId { get; set; }

    [JsonProperty("merchantName")]
    public string MerchantName { get; set; } = "";

    [JsonProperty("city")]
    public string? City { get; set; }

    [JsonProperty("positionOrder")]
    public int PositionOrder { get; set; }

    [JsonProperty("notes")]
    public string? Notes { get; set; }
}

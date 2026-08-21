using Newtonsoft.Json;

namespace ReceiptSystem.Desktop.Models;

public class MerchantSearchResult
{
    [JsonProperty("merchantId")]
    public int MerchantId { get; set; }

    [JsonProperty("merchantName")]
    public string MerchantName { get; set; } = "";

    [JsonProperty("city")]
    public string? City { get; set; }

    [JsonProperty("phoneNumber")]
    public string? PhoneNumber { get; set; }
}

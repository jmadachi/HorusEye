namespace HorusEye.Api.Providers;

public class RfidEvent
{
    public string ProviderName { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string EPC { get; set; } = string.Empty;
    public int Antenna { get; set; }
    public int RSSI { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> RawData { get; set; } = new();
}

public interface IRfidProvider
{
    string ProviderName { get; }
    Task<RfidEvent> ParseEventAsync(HttpRequest request);
    bool ValidatePayload(string payload);
}

using System.Text.Json;

namespace HorusEye.Api.Providers;

public class NfcTesterProvider : IRfidProvider
{
    public string ProviderName => "NfcTester";

    public async Task<RfidEvent> ParseEventAsync(HttpRequest request)
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        return new RfidEvent
        {
            ProviderName = ProviderName,
            DeviceId = root.TryGetProperty("reader_id", out var rid) ? rid.GetString() : null,
            EPC = root.TryGetProperty("epc", out var epc) ? epc.GetString() ?? string.Empty : string.Empty,
            Antenna = 0,
            RSSI = 0,
            Timestamp = root.TryGetProperty("timestamp", out var ts)
                ? DateTime.Parse(ts.GetString() ?? DateTime.UtcNow.ToString("o"))
                : DateTime.UtcNow,
            RawData = root.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.ToString())
        };
    }

    public bool ValidatePayload(string payload)
    {
        try
        {
            var json = JsonDocument.Parse(payload);
            return json.RootElement.TryGetProperty("epc", out _);
        }
        catch
        {
            return false;
        }
    }
}

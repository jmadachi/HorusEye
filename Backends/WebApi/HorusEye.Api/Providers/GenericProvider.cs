using System.Text.Json;
using HorusEye.Core.Entities;
using HorusEye.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Providers;

public class GenericProvider : IRfidProvider
{
    private readonly HorusEyeDbContext _context;
    private readonly Guid _fabricanteId;

    public GenericProvider(HorusEyeDbContext context, Guid fabricanteId)
    {
        _context = context;
        _fabricanteId = fabricanteId;
    }

    public string ProviderName => "Generic";

    public async Task<RfidEvent> ParseEventAsync(HttpRequest request)
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        var campos = await _context.CamposPayloadFabricante
            .Where(c => c.FabricanteDispositivoId == _fabricanteId)
            .OrderBy(c => c.OrdenExtraccion)
            .ToListAsync();

        var rawData = root.EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.ToString());

        return new RfidEvent
        {
            ProviderName = ProviderName,
            DeviceId = GetMappedValue(root, campos, "DeviceId"),
            EPC = GetMappedValue(root, campos, "EPC") ?? string.Empty,
            Antenna = int.TryParse(GetMappedValue(root, campos, "Antenna"), out var ant) ? ant : 0,
            RSSI = int.TryParse(GetMappedValue(root, campos, "RSSI"), out var rssi) ? rssi : 0,
            Timestamp = DateTime.TryParse(GetMappedValue(root, campos, "Timestamp"), out var ts) ? ts : DateTime.UtcNow,
            RawData = rawData
        };
    }

    public bool ValidatePayload(string payload)
    {
        try
        {
            JsonDocument.Parse(payload);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string? GetMappedValue(JsonElement root, List<CampoPayloadFabricante> campos, string nombreInterno)
    {
        var campo = campos.FirstOrDefault(c => c.NombreCampoInterno == nombreInterno);
        if (campo == null) return null;

        if (root.TryGetProperty(campo.NombreCampoExterno, out var value))
            return value.ToString();

        return campo.ValorDefecto;
    }
}

namespace HorusEye.Core.Entities;

public class FabricanteDispositivo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? UrlDocumentacion { get; set; }
    public string? EndpointEjemplo { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<CampoPayloadFabricante> CamposPayload { get; set; } = new List<CampoPayloadFabricante>();
    public ICollection<DispositivoRfid> DispositivosRfid { get; set; } = new List<DispositivoRfid>();
}

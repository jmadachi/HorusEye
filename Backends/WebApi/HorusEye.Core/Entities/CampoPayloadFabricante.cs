namespace HorusEye.Core.Entities;

public class CampoPayloadFabricante
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FabricanteDispositivoId { get; set; }
    public string NombreCampoExterno { get; set; } = string.Empty;
    public string NombreCampoInterno { get; set; } = string.Empty;
    public string TipoDato { get; set; } = "string";
    public bool Requerido { get; set; }
    public string? ValorDefecto { get; set; }
    public int OrdenExtraccion { get; set; }

    public FabricanteDispositivo? FabricanteDispositivo { get; set; }
}

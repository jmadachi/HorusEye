namespace HorusEye.Core.Entities;

public class TagDanioHistorial
{
    public long Id { get; set; }
    public string TagId { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTimeOffset FechaReporte { get; set; } = DateTimeOffset.UtcNow;
    public string? ReportadoPor { get; set; }

    public Tag Tag { get; set; } = null!;
}

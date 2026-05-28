using HorusEye.Core.Enums;

namespace HorusEye.Core.Entities;

public class Tag
{
    public string Id { get; set; } = string.Empty;
    public EstadoTag Estado { get; set; } = EstadoTag.REGISTRADO;
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset FechaActualizacion { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<TagDanioHistorial> DaniosHistorial { get; set; } = new List<TagDanioHistorial>();
    public Activo? Activo { get; set; }
}

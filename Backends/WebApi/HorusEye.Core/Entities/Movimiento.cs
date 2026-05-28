using HorusEye.Core.Enums;

namespace HorusEye.Core.Entities;

public class Movimiento
{
    public long Id { get; set; }
    public Guid ActivoId { get; set; }
    public string? PuntoLecturaId { get; set; }
    public TipoMovimiento TipoMovimiento { get; set; }
    public bool Autorizado { get; set; }
    public bool AlarmaActivada { get; set; }
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;

    public Activo Activo { get; set; } = null!;
}

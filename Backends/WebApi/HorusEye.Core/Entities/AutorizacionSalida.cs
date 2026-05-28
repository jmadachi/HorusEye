namespace HorusEye.Core.Entities;

public class AutorizacionSalida
{
    public long Id { get; set; }
    public Guid ActivoId { get; set; }
    public string AutorizadoPor { get; set; } = string.Empty;
    public DateTimeOffset FechaAutorizacion { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FechaVencimiento { get; set; }
    public bool Activa { get; set; } = true;

    public Activo Activo { get; set; } = null!;
}

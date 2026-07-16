using HorusEye.Core.Enums;

namespace HorusEye.Core.Entities;

public class Activo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Placa { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string? TenedorResponsable { get; set; }
    public EstadoUbicacion EstadoUbicacion { get; set; } = EstadoUbicacion.DENTRO_INSTALACIONES;
    public string? TagId { get; set; }
    public Guid? ClienteId { get; set; }
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset FechaActualizacion { get; set; } = DateTimeOffset.UtcNow;

    public Tag? Tag { get; set; }
    public Cliente? Cliente { get; set; }
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    public ICollection<AutorizacionSalida> AutorizacionesSalida { get; set; } = new List<AutorizacionSalida>();
}

using HorusEye.Core.Enums;

namespace HorusEye.Core.Entities;

public class DispositivoRfid
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string? DireccionIP { get; set; }
    public int? Puerto { get; set; }
    public string? Ubicacion { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? ProveedorId { get; set; }
    public Guid? NodoUbicacionId { get; set; }
    public TipoDispositivo TipoDispositivo { get; set; } = TipoDispositivo.FIXED;
    public string? EndpointAPI { get; set; }
    public string? MetodoHTTP { get; set; } = "POST";
    public Guid? FabricanteDispositivoId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;

    public Cliente? Cliente { get; set; }
    public Proveedor? Proveedor { get; set; }
    public NodoUbicacion? NodoUbicacion { get; set; }
    public FabricanteDispositivo? FabricanteDispositivo { get; set; }
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}

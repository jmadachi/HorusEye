namespace HorusEye.Core.Entities;

public class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string RUC { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public Guid? ProveedorId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;

    public Proveedor? Proveedor { get; set; }
    public ICollection<DispositivoRfid> Dispositivos { get; set; } = new List<DispositivoRfid>();
    public ICollection<NodoUbicacion> NodosUbicacion { get; set; } = new List<NodoUbicacion>();
}

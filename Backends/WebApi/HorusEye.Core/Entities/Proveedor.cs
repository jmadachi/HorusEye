namespace HorusEye.Core.Entities;

public class Proveedor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string RUC { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    public ICollection<DispositivoRfid> Dispositivos { get; set; } = new List<DispositivoRfid>();
}

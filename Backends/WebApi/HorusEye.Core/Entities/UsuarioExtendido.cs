namespace HorusEye.Core.Entities;

public class UsuarioExtendido
{
    public string Id { get; set; } = string.Empty;
    public Guid? ProveedorId { get; set; }
    public Guid? ClienteId { get; set; }
    public string? CreadoPorUserId { get; set; }
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;

    public Proveedor? Proveedor { get; set; }
    public Cliente? Cliente { get; set; }
}

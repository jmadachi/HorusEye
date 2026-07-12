namespace HorusEye.Core.Entities;

public class NodoUbicacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string TipoNodo { get; set; } = string.Empty;
    public Guid? PadreId { get; set; }
    public Guid ClienteId { get; set; }
    public int Nivel { get; set; }
    public string? Metadata { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;

    public NodoUbicacion? Padre { get; set; }
    public Cliente? Cliente { get; set; }
    public ICollection<NodoUbicacion> Hijos { get; set; } = new List<NodoUbicacion>();
    public ICollection<DispositivoRfid> Dispositivos { get; set; } = new List<DispositivoRfid>();
}

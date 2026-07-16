namespace HorusEye.Core.Entities;

public class Evento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public Guid NodoUbicacionId { get; set; }
    public Guid ClienteId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTimeOffset FechaRegistro { get; set; } = DateTimeOffset.UtcNow;

    public NodoUbicacion? NodoUbicacion { get; set; }
    public Cliente? Cliente { get; set; }
    public ICollection<EventoLector> Lectores { get; set; } = new List<EventoLector>();
}

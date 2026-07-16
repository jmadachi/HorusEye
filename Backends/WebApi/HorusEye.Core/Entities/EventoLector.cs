namespace HorusEye.Core.Entities;

public class EventoLector
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventoId { get; set; }
    public Guid DispositivoRfidId { get; set; }
    public int Orden { get; set; }

    public Evento? Evento { get; set; }
    public DispositivoRfid? DispositivoRfid { get; set; }
}

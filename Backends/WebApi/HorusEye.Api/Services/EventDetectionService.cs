using HorusEye.Core.Entities;
using HorusEye.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Services;

public class LecturaReciente
{
    public string TagId { get; set; } = string.Empty;
    public Guid DispositivoRfidId { get; set; }
    public DateTime Timestamp { get; set; }
}

public class EventDetectionService
{
    private readonly HorusEyeDbContext _context;
    private readonly TimeSpan _ventanaTiempo = TimeSpan.FromSeconds(30);
    private readonly int _maxLecturasPorTag = 10;

    // In-memory buffer: key = tagId, value = lecturas recientes ordenadas por tiempo
    private readonly Dictionary<string, List<LecturaReciente>> _buffer = new();
    private readonly object _lock = new();

    public EventDetectionService(HorusEyeDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Registra una lectura y determina si coincide con algun evento definido.
    /// Retorna el nombre del evento detectado, o null si no coincide.
    /// </summary>
    public async Task<string?> RegistrarLectura(string tagId, Guid dispositivoRfidId)
    {
        var lectura = new LecturaReciente
        {
            TagId = tagId,
            DispositivoRfidId = dispositivoRfidId,
            Timestamp = DateTime.UtcNow
        };

        List<LecturaReciente> lecturas;
        lock (_lock)
        {
            if (!_buffer.ContainsKey(tagId))
                _buffer[tagId] = new List<LecturaReciente>();

            lecturas = _buffer[tagId];
            lecturas.Add(lectura);

            // Limpiar lecturas fuera de la ventana de tiempo
            lecturas.RemoveAll(l => l.Timestamp < DateTime.UtcNow - _ventanaTiempo);

            // Limitar cantidad de lecturas por tag
            if (lecturas.Count > _maxLecturasPorTag)
                lecturas.RemoveRange(0, lecturas.Count - _maxLecturasPorTag);
        }

        // Buscar eventos definidos para la ubicacion del dispositivo
        var dispositivo = await _context.DispositivosRfid
            .FirstOrDefaultAsync(d => d.Id == dispositivoRfidId);

        if (dispositivo?.NodoUbicacionId == null)
            return null;

        var eventos = await _context.Eventos
            .Include(e => e.Lectores)
            .Where(e => e.NodoUbicacionId == dispositivo.NodoUbicacionId && e.Activo)
            .ToListAsync();

        // Para cada evento, verificar si la secuencia de lecturas coincide
        foreach (var evento in eventos)
        {
            if (CoincideSecuencia(lecturas, evento.Lectores.OrderBy(l => l.Orden).ToList()))
            {
                // Limpiar buffer del tag despues de detectar evento
                lock (_lock)
                {
                    if (_buffer.ContainsKey(tagId))
                        _buffer[tagId].Clear();
                }
                return evento.Nombre;
            }
        }

        return null;
    }

    private bool CoincideSecuencia(List<LecturaReciente> lecturas, List<EventoLector> secuenciaEvento)
    {
        if (secuenciaEvento.Count == 0 || lecturas.Count < secuenciaEvento.Count)
            return false;

        // Obtener las ultimas N lecturas (N = longitud de la secuencia del evento)
        var ultimasLecturas = lecturas
            .TakeLast(secuenciaEvento.Count)
            .ToList();

        // Comparar secuencia: el orden de los dispositivos debe coincidir
        for (int i = 0; i < secuenciaEvento.Count; i++)
        {
            if (ultimasLecturas[i].DispositivoRfidId != secuenciaEvento[i].DispositivoRfidId)
                return false;
        }

        return true;
    }

    /// Limpia el buffer periodicamente (llamar desde un Timer o similar)
    public void LimpiarBuffer()
    {
        lock (_lock)
        {
            var cutoff = DateTime.UtcNow - _ventanaTiempo;
            foreach (var tagId in _buffer.Keys.ToList())
            {
                _buffer[tagId].RemoveAll(l => l.Timestamp < cutoff);
                if (_buffer[tagId].Count == 0)
                    _buffer.Remove(tagId);
            }
        }
    }
}

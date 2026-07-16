using System.ComponentModel.DataAnnotations;

namespace HorusEye.Api.DTOs;

public class EventoRfidRequest
{
    [Required]
    public string TagId { get; set; } = string.Empty;
    public string? PuntoLecturaId { get; set; }
    [RegularExpression("^(INGRESO|SALIDA)$",
        ErrorMessage = "TipoMovimiento debe ser INGRESO o SALIDA")]
    public string? TipoMovimiento { get; set; }
}

public class EventoRfidResponse
{
    public string TagId { get; set; } = string.Empty;
    public string? ActivoPlaca { get; set; }
    public string? ActivoNombre { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public bool Autorizado { get; set; }
    public bool ActivarAlarmaSonora { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ActivoRequest
{
    [Required]
    public string Placa { get; set; } = string.Empty;
    [Required]
    public string Nombre { get; set; } = string.Empty;
    [Required]
    [RegularExpression("^(Computadores|Monitores|Perifericos|Impresoras|Sillas|Redes|Telefonia|Tablets|Audio|Accesorios|Otros)$",
        ErrorMessage = "Categoria inválida. Valores permitidos: Computadores, Monitores, Perifericos, Impresoras, Sillas, Redes, Telefonia, Tablets, Audio, Accesorios, Otros")]
    public string Categoria { get; set; } = string.Empty;
    public string? TenedorResponsable { get; set; }
    public string? TagId { get; set; }
}

public class ActivoResponse
{
    public Guid Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string? TenedorResponsable { get; set; }
    public string EstadoUbicacion { get; set; } = string.Empty;
    public string? TagId { get; set; }
    public DateTimeOffset FechaRegistro { get; set; }
}

public class MovimientoResponse
{
    public long Id { get; set; }
    public Guid ActivoId { get; set; }
    public string? ActivoPlaca { get; set; }
    public string? ActivoNombre { get; set; }
    public string? PuntoLecturaId { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public bool Autorizado { get; set; }
    public bool AlarmaActivada { get; set; }
    public DateTimeOffset FechaRegistro { get; set; }
}

public class ReporteRequest
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string TipoReporte { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace HorusEye.Api.DTOs;

public class CreateAutorizacionRequest
{
    [Required]
    public Guid ActivoId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string AutorizadoPor { get; set; } = string.Empty;

    public DateTimeOffset? FechaVencimiento { get; set; }
}

public class AutorizacionResponse
{
    public long Id { get; set; }
    public Guid ActivoId { get; set; }
    public string ActivoPlaca { get; set; } = string.Empty;
    public string ActivoNombre { get; set; } = string.Empty;
    public string AutorizadoPor { get; set; } = string.Empty;
    public DateTimeOffset FechaAutorizacion { get; set; }
    public DateTimeOffset? FechaVencimiento { get; set; }
    public bool Activa { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HorusEye.Api.DTOs;

public class CreateTagRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string TagId { get; set; } = string.Empty;
}

public class UpdateTagEstadoRequest
{
    [Required]
    [RegularExpression("^(REGISTRADO|ASIGNADO|DISPONIBLE|DAÑADO)$",
        ErrorMessage = "Estado debe ser: REGISTRADO, ASIGNADO, DISPONIBLE o DAÑADO")]
    public string Estado { get; set; } = string.Empty;
}

public class ReportarDanioRequest
{
    [Required]
    [StringLength(600, MinimumLength = 1)]
    public string Descripcion { get; set; } = string.Empty;
}

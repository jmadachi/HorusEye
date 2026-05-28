using HorusEye.Api.DTOs;
using HorusEye.Api.Models;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly HorusEyeDbContext _context;

    public ReportesController(HorusEyeDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> GetReporte([FromBody] ReporteRequest request)
    {
        var fechaFin = request.FechaFin.Date.AddDays(1);

        object? datos = request.TipoReporte switch
        {
            "Movimientos" => await GetMovimientosReport(request.FechaInicio, fechaFin),
            "Tags Registrados" => await GetTagsReport(request.FechaInicio, fechaFin),
            "Activos Dentro" => await GetActivosReport(ubicacion: "DENTRO_INSTALACIONES"),
            "Activos Fuera" => await GetActivosReport(ubicacion: "FUERA_INSTALACIONES"),
            _ => null
        };

        if (datos == null)
            return BadRequest(ApiResponse<object>.Fail(
                $"Tipo de reporte inválido: {request.TipoReporte}"));

        return Ok(ApiResponse<object>.Ok(new
        {
            TipoReporte = request.TipoReporte,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            Datos = datos
        }));
    }

    private async Task<object> GetMovimientosReport(DateTime inicio, DateTime fin)
    {
        return await _context.Movimientos
            .Include(m => m.Activo)
            .Where(m => m.FechaRegistro >= inicio && m.FechaRegistro < fin)
            .OrderByDescending(m => m.FechaRegistro)
            .Select(m => new MovimientoResponse
            {
                Id = m.Id,
                ActivoId = m.ActivoId,
                ActivoPlaca = m.Activo.Placa,
                ActivoNombre = m.Activo.Nombre,
                PuntoLecturaId = m.PuntoLecturaId,
                TipoMovimiento = m.TipoMovimiento.ToString(),
                Autorizado = m.Autorizado,
                AlarmaActivada = m.AlarmaActivada,
                FechaRegistro = m.FechaRegistro
            })
            .ToListAsync();
    }

    private async Task<object> GetTagsReport(DateTime inicio, DateTime fin)
    {
        return await _context.Tags
            .Where(t => t.FechaRegistro >= inicio && t.FechaRegistro < fin)
            .OrderByDescending(t => t.FechaRegistro)
            .ToListAsync();
    }

    private async Task<object> GetActivosReport(string ubicacion)
    {
        return await _context.Activos
            .Where(a => a.EstadoUbicacion.ToString() == ubicacion)
            .OrderBy(a => a.Nombre)
            .Select(a => new ActivoResponse
            {
                Id = a.Id,
                Placa = a.Placa,
                Nombre = a.Nombre,
                Categoria = a.Categoria,
                TenedorResponsable = a.TenedorResponsable,
                EstadoUbicacion = a.EstadoUbicacion.ToString(),
                TagId = a.TagId,
                FechaRegistro = a.FechaRegistro
            })
            .ToListAsync();
    }
}

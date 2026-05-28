using HorusEye.Api.Models;
using HorusEye.Core.Enums;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly HorusEyeDbContext _context;

    public DashboardController(HorusEyeDbContext context)
    {
        _context = context;
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<ApiResponse<object>>> GetKPIs()
    {
        var totalActivos = await _context.Activos.CountAsync();
        var activosDentro = await _context.Activos
            .CountAsync(a => a.EstadoUbicacion == EstadoUbicacion.DENTRO_INSTALACIONES);
        var activosFuera = await _context.Activos
            .CountAsync(a => a.EstadoUbicacion == EstadoUbicacion.FUERA_INSTALACIONES);
        var tagsRegistrados = await _context.Tags.CountAsync();
        var tagsAsignados = await _context.Tags
            .CountAsync(t => t.Estado == EstadoTag.ASIGNADO);
        var tagsDisponibles = await _context.Tags
            .CountAsync(t => t.Estado == EstadoTag.DISPONIBLE);

        var hoy = DateTimeOffset.UtcNow.Date;
        var manana = hoy.AddDays(1);

        var ingresosHoy = await _context.Movimientos
            .CountAsync(m => m.TipoMovimiento == TipoMovimiento.INGRESO
                && m.FechaRegistro >= hoy && m.FechaRegistro < manana);
        var salidasHoy = await _context.Movimientos
            .CountAsync(m => m.TipoMovimiento == TipoMovimiento.SALIDA
                && m.FechaRegistro >= hoy && m.FechaRegistro < manana);

        var categorias = await _context.Activos
            .GroupBy(a => a.Categoria)
            .Select(g => new { Categoria = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            TotalActivos = totalActivos,
            ActivosDentro = activosDentro,
            ActivosFuera = activosFuera,
            TagsRegistrados = tagsRegistrados,
            TagsAsignados = tagsAsignados,
            TagsDisponibles = tagsDisponibles,
            IngresosHoy = ingresosHoy,
            SalidasHoy = salidasHoy,
            Categorias = categorias,
            Fecha = hoy
        }));
    }

    [HttpGet("tendencias")]
    public async Task<ActionResult<ApiResponse<object>>> GetTendencias()
    {
        var hoy = DateTimeOffset.UtcNow.Date;
        var hace7Dias = hoy.AddDays(-6);

        var movimientos = await _context.Movimientos
            .Where(m => m.FechaRegistro >= hace7Dias && m.FechaRegistro < hoy.AddDays(1))
            .ToListAsync();

        var dias = new List<object>();
        for (var d = hace7Dias; d <= hoy; d = d.AddDays(1))
        {
            var diaFin = d.AddDays(1);
            dias.Add(new
            {
                Fecha = d.ToString("yyyy-MM-dd"),
                Dia = d.ToString("ddd", new System.Globalization.CultureInfo("es-MX")),
                Ingresos = movimientos.Count(m =>
                    m.TipoMovimiento == TipoMovimiento.INGRESO
                    && m.FechaRegistro >= d && m.FechaRegistro < diaFin),
                Salidas = movimientos.Count(m =>
                    m.TipoMovimiento == TipoMovimiento.SALIDA
                    && m.FechaRegistro >= d && m.FechaRegistro < diaFin),
                NoAutorizadas = movimientos.Count(m =>
                    m.TipoMovimiento == TipoMovimiento.SALIDA
                    && !m.Autorizado
                    && m.FechaRegistro >= d && m.FechaRegistro < diaFin)
            });
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            Dias = dias,
            Desde = hace7Dias,
            Hasta = hoy
        }));
    }
}

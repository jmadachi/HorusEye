using HorusEye.Api.DTOs;
using HorusEye.Api.Models;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Controllers;

[ApiController]
[Route("api/movimientos")]
[Authorize]
public class MovimientosController : ControllerBase
{
    private readonly HorusEyeDbContext _context;

    public MovimientosController(HorusEyeDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _context.Movimientos
            .Include(m => m.Activo)
            .OrderByDescending(m => m.FechaRegistro);

        var total = await query.CountAsync();
        var movimientos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return Ok(ApiResponse<object>.Ok(new
        {
            Items = movimientos,
            Total = total,
            Page = page,
            PageSize = pageSize
        }));
    }

    [HttpGet("resumen-del-dia")]
    public async Task<ActionResult<ApiResponse<object>>> GetResumenDelDia()
    {
        var hoy = DateTimeOffset.UtcNow.Date;
        var manana = hoy.AddDays(1);

        var ingresos = await _context.Movimientos
            .CountAsync(m => m.TipoMovimiento == Core.Enums.TipoMovimiento.INGRESO
                && m.FechaRegistro >= hoy
                && m.FechaRegistro < manana);

        var salidas = await _context.Movimientos
            .CountAsync(m => m.TipoMovimiento == Core.Enums.TipoMovimiento.SALIDA
                && m.FechaRegistro >= hoy
                && m.FechaRegistro < manana);

        var salidasNoAutorizadas = await _context.Movimientos
            .CountAsync(m => m.TipoMovimiento == Core.Enums.TipoMovimiento.SALIDA
                && !m.Autorizado
                && m.FechaRegistro >= hoy
                && m.FechaRegistro < manana);

        return Ok(ApiResponse<object>.Ok(new
        {
            IngresosDelDia = ingresos,
            SalidasDelDia = salidas,
            SalidasNoAutorizadas = salidasNoAutorizadas,
            Fecha = hoy
        }));
    }
}

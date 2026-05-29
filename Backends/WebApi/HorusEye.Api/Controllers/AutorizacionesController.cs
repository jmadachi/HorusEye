using HorusEye.Api.DTOs;
using HorusEye.Api.Models;
using HorusEye.Core.Entities;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Controllers;

[ApiController]
[Route("api/autorizaciones")]
[Authorize(Roles = "Usuario de Gestión")]
public class AutorizacionesController : ControllerBase
{
    private readonly HorusEyeDbContext _context;
    private readonly ILogger<AutorizacionesController> _logger;

    public AutorizacionesController(HorusEyeDbContext context, ILogger<AutorizacionesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _context.AutorizacionesSalida
            .Include(a => a.Activo)
            .OrderByDescending(a => a.FechaAutorizacion);

        var total = await query.CountAsync();
        var autorizaciones = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AutorizacionResponse
            {
                Id = a.Id,
                ActivoId = a.ActivoId,
                ActivoPlaca = a.Activo.Placa,
                ActivoNombre = a.Activo.Nombre,
                AutorizadoPor = a.AutorizadoPor,
                FechaAutorizacion = a.FechaAutorizacion,
                FechaVencimiento = a.FechaVencimiento,
                Activa = a.Activa
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            Items = autorizaciones,
            Total = total,
            Page = page,
            PageSize = pageSize
        }));
    }

    [HttpGet("activas")]
    public async Task<ActionResult<ApiResponse<List<AutorizacionResponse>>>> GetActivas()
    {
        var autorizaciones = await _context.AutorizacionesSalida
            .Include(a => a.Activo)
            .Where(a => a.Activa && (a.FechaVencimiento == null || a.FechaVencimiento > DateTimeOffset.UtcNow))
            .OrderByDescending(a => a.FechaAutorizacion)
            .Select(a => new AutorizacionResponse
            {
                Id = a.Id,
                ActivoId = a.ActivoId,
                ActivoPlaca = a.Activo.Placa,
                ActivoNombre = a.Activo.Nombre,
                AutorizadoPor = a.AutorizadoPor,
                FechaAutorizacion = a.FechaAutorizacion,
                FechaVencimiento = a.FechaVencimiento,
                Activa = a.Activa
            })
            .ToListAsync();

        return Ok(ApiResponse<List<AutorizacionResponse>>.Ok(autorizaciones));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AutorizacionResponse>>> Create([FromBody] CreateAutorizacionRequest request)
    {
        var activo = await _context.Activos.FindAsync(request.ActivoId);
        if (activo == null)
            return NotFound(ApiResponse<AutorizacionResponse>.Fail("Activo no encontrado"));

        var autorizacion = new AutorizacionSalida
        {
            ActivoId = request.ActivoId,
            AutorizadoPor = request.AutorizadoPor,
            FechaVencimiento = request.FechaVencimiento
        };

        _context.AutorizacionesSalida.Add(autorizacion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Autorización creada para activo {ActivoId} por {Usuario}",
            request.ActivoId, request.AutorizadoPor);

        var response = new AutorizacionResponse
        {
            Id = autorizacion.Id,
            ActivoId = autorizacion.ActivoId,
            ActivoPlaca = activo.Placa,
            ActivoNombre = activo.Nombre,
            AutorizadoPor = autorizacion.AutorizadoPor,
            FechaAutorizacion = autorizacion.FechaAutorizacion,
            FechaVencimiento = autorizacion.FechaVencimiento,
            Activa = autorizacion.Activa
        };

        return Ok(ApiResponse<AutorizacionResponse>.Ok(response, "Autorización creada exitosamente"));
    }

    [HttpPut("{id}/revocar")]
    public async Task<ActionResult<ApiResponse<object>>> Revocar(long id)
    {
        var autorizacion = await _context.AutorizacionesSalida.FindAsync(id);
        if (autorizacion == null)
            return NotFound(ApiResponse<object>.Fail("Autorización no encontrada"));

        autorizacion.Activa = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Autorización {Id} revocada", id);

        return Ok(ApiResponse<object>.Ok(null!, "Autorización revocada exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(long id)
    {
        var autorizacion = await _context.AutorizacionesSalida.FindAsync(id);
        if (autorizacion == null)
            return NotFound(ApiResponse<object>.Fail("Autorización no encontrada"));

        _context.AutorizacionesSalida.Remove(autorizacion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Autorización {Id} eliminada", id);

        return Ok(ApiResponse<object>.Ok(null!, "Autorización eliminada exitosamente"));
    }
}

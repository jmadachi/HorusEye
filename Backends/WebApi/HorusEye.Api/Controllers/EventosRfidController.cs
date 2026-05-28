using HorusEye.Api.DTOs;
using HorusEye.Api.Hubs;
using HorusEye.Api.Models;
using HorusEye.Core.Entities;
using HorusEye.Core.Enums;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HorusEye.Api.Controllers;

[ApiController]
[Route("api/eventos-rfid")]
public class EventosRfidController : ControllerBase
{
    private readonly HorusEyeDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly IHubContext<MovimientosHub> _hubContext;
    private readonly ILogger<EventosRfidController> _logger;

    public EventosRfidController(
        HorusEyeDbContext context,
        IMemoryCache cache,
        IHubContext<MovimientosHub> hubContext,
        ILogger<EventosRfidController> logger)
    {
        _context = context;
        _cache = cache;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<EventoRfidResponse>>> ProcesarEvento(
        [FromBody] EventoRfidRequest request)
    {
        var cacheKey = $"rfid_debounce_{request.TagId}";

        if (_cache.TryGetValue(cacheKey, out _))
        {
            _logger.LogInformation("De-bounce: TAG {TagId} ignorado (ventana de 5s)", request.TagId);
            return Ok(ApiResponse<EventoRfidResponse>.Ok(
                new EventoRfidResponse
                {
                    TagId = request.TagId,
                    Mensaje = "Lectura ignorada por de-bounce",
                    Timestamp = DateTime.UtcNow
                }, "Lectura duplicada dentro de ventana de de-bounce"));
        }

        _cache.Set(cacheKey, true, TimeSpan.FromSeconds(5));

        var tag = await _context.Tags
            .Include(t => t.Activo)
            .FirstOrDefaultAsync(t => t.Id == request.TagId);

        if (tag == null)
        {
            _logger.LogWarning("TAG {TagId} no registrado en el sistema", request.TagId);
            return NotFound(ApiResponse<EventoRfidResponse>.Fail(
                $"TAG {request.TagId} no registrado"));
        }

        if (tag.Estado == EstadoTag.DAÑADO)
        {
            _logger.LogWarning("TAG {TagId} está dañado, lectura rechazada", request.TagId);
            return BadRequest(ApiResponse<EventoRfidResponse>.Fail(
                $"TAG {request.TagId} está marcado como dañado"));
        }

        if (tag.Activo == null)
        {
            _logger.LogWarning("TAG {TagId} no está asignado a ningún activo", request.TagId);
            return BadRequest(ApiResponse<EventoRfidResponse>.Fail(
                $"TAG {request.TagId} no está asignado a ningún activo"));
        }

        if (!Enum.TryParse<TipoMovimiento>(request.TipoMovimiento, true, out var tipoMovimiento))
        {
            return BadRequest(ApiResponse<EventoRfidResponse>.Fail(
                $"Tipo de movimiento inválido: {request.TipoMovimiento}"));
        }

        var activo = tag.Activo;
        var activarAlarma = false;
        var autorizado = true;

        if (tipoMovimiento == TipoMovimiento.SALIDA)
        {
            var autorizacionVigente = await _context.AutorizacionesSalida
                .AnyAsync(a => a.ActivoId == activo.Id
                    && a.Activa
                    && (a.FechaVencimiento == null || a.FechaVencimiento > DateTimeOffset.UtcNow));

            if (!autorizacionVigente)
            {
                activarAlarma = true;
                autorizado = false;
                _logger.LogWarning("ALARMA: Salida no autorizada detectada para Activo {Placa} (TAG {TagId})",
                    activo.Placa, request.TagId);
            }
        }

        if (tipoMovimiento == TipoMovimiento.INGRESO)
        {
            activo.EstadoUbicacion = EstadoUbicacion.DENTRO_INSTALACIONES;
        }
        else
        {
            activo.EstadoUbicacion = EstadoUbicacion.FUERA_INSTALACIONES;
        }

        activo.FechaActualizacion = DateTimeOffset.UtcNow;

        var movimiento = new Movimiento
        {
            ActivoId = activo.Id,
            PuntoLecturaId = request.PuntoLecturaId,
            TipoMovimiento = tipoMovimiento,
            Autorizado = autorizado,
            AlarmaActivada = activarAlarma,
            FechaRegistro = DateTimeOffset.UtcNow
        };

        _context.Movimientos.Add(movimiento);
        await _context.SaveChangesAsync();

        var response = new EventoRfidResponse
        {
            TagId = request.TagId,
            ActivoPlaca = activo.Placa,
            ActivoNombre = activo.Nombre,
            TipoMovimiento = request.TipoMovimiento,
            Autorizado = autorizado,
            ActivarAlarmaSonora = activarAlarma,
            Mensaje = activarAlarma
                ? "¡ALERTA! Salida no autorizada. Active alarma sonora."
                : "Movimiento registrado exitosamente",
            Timestamp = DateTime.UtcNow
        };

        var movimientoMsg = new MovimientoResponse
        {
            Id = movimiento.Id,
            ActivoId = activo.Id,
            ActivoPlaca = activo.Placa,
            ActivoNombre = activo.Nombre,
            PuntoLecturaId = request.PuntoLecturaId,
            TipoMovimiento = tipoMovimiento.ToString(),
            Autorizado = autorizado,
            AlarmaActivada = activarAlarma,
            FechaRegistro = movimiento.FechaRegistro
        };

        await _hubContext.Clients.Group("Dashboard")
            .SendAsync("NuevoMovimiento", movimientoMsg);

        _logger.LogInformation(
            "RFID Evento: TAG {TagId} | Activo {Placa} | {Tipo} | Autorizado: {Autorizado} | Alarma: {Alarma}",
            request.TagId, activo.Placa, request.TipoMovimiento, autorizado, activarAlarma);

        return Ok(ApiResponse<EventoRfidResponse>.Ok(response,
            response.Mensaje));
    }
}

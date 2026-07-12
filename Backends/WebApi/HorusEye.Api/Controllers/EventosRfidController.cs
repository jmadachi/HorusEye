using HorusEye.Api.DTOs;
using HorusEye.Api.Hubs;
using HorusEye.Api.Models;
using HorusEye.Api.Providers;
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
        return await ProcesarEventoInterno(request.TagId, request.PuntoLecturaId, request.TipoMovimiento, null);
    }

    [HttpPost("{providerNombre}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<EventoRfidResponse>>> ProcesarEventoPorProvider(
        string providerNombre)
    {
        IRfidProvider provider = providerNombre.ToLowerInvariant() switch
        {
            "chainway" => new ChainwayProvider(),
            _ => await GetGenericProvider(providerNombre)
        };

        if (provider == null)
            return BadRequest(ApiResponse<EventoRfidResponse>.Fail(
                $"Provider '{providerNombre}' no configurado"));

        RfidEvent rfidEvent;
        try
        {
            rfidEvent = await provider.ParseEventAsync(HttpContext.Request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al parsear payload del provider {Provider}", providerNombre);
            return BadRequest(ApiResponse<EventoRfidResponse>.Fail(
                $"Error al parsear payload: {ex.Message}"));
        }

        if (!provider.ValidatePayload(System.Text.Json.JsonSerializer.Serialize(rfidEvent.RawData)))
            return BadRequest(ApiResponse<EventoRfidResponse>.Fail("Payload invalido"));

        var dispositivo = await _context.DispositivosRfid
            .FirstOrDefaultAsync(d => d.Nombre == rfidEvent.DeviceId || d.DireccionIP == rfidEvent.DeviceId);

        return await ProcesarEventoInterno(
            rfidEvent.EPC,
            rfidEvent.DeviceId,
            "INGRESO",
            dispositivo?.Id);
    }

    private async Task<IRfidProvider?> GetGenericProvider(string providerNombre)
    {
        var fabricante = await _context.FabricantesDispositivo
            .FirstOrDefaultAsync(f => f.Nombre.ToLower() == providerNombre.ToLower());

        if (fabricante == null) return null;

        return new GenericProvider(_context, fabricante.Id);
    }

    private async Task<ActionResult<ApiResponse<EventoRfidResponse>>> ProcesarEventoInterno(
        string tagId, string? puntoLecturaId, string tipoMovimiento, Guid? dispositivoRfidId)
    {
        var cacheKey = $"rfid_debounce_{tagId}";

        if (_cache.TryGetValue(cacheKey, out _))
        {
            _logger.LogInformation("De-bounce: TAG {TagId} ignorado (ventana de 5s)", tagId);
            return Ok(ApiResponse<EventoRfidResponse>.Ok(
                new EventoRfidResponse
                {
                    TagId = tagId,
                    Mensaje = "Lectura ignorada por de-bounce",
                    Timestamp = DateTime.UtcNow
                }, "Lectura duplicada dentro de ventana de de-bounce"));
        }

        _cache.Set(cacheKey, true, TimeSpan.FromSeconds(5));

        var tag = await _context.Tags
            .Include(t => t.Activo)
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
        {
            _logger.LogWarning("TAG {TagId} no registrado en el sistema", tagId);
            return NotFound(ApiResponse<EventoRfidResponse>.Fail(
                $"TAG {tagId} no registrado"));
        }

        if (tag.Estado == EstadoTag.DAÑADO)
        {
            _logger.LogWarning("TAG {TagId} esta danado, lectura rechazada", tagId);
            return BadRequest(ApiResponse<EventoRfidResponse>.Fail(
                $"TAG {tagId} esta marcado como danado"));
        }

        if (tag.Activo == null)
        {
            _logger.LogWarning("TAG {TagId} no esta asignado a ningun activo", tagId);
            return BadRequest(ApiResponse<EventoRfidResponse>.Fail(
                $"TAG {tagId} no esta asignado a ningun activo"));
        }

        if (!Enum.TryParse<TipoMovimiento>(tipoMovimiento, true, out var tipo))
        {
            return BadRequest(ApiResponse<EventoRfidResponse>.Fail(
                $"Tipo de movimiento invalido: {tipoMovimiento}"));
        }

        var activo = tag.Activo;
        var activarAlarma = false;
        var autorizado = true;

        if (tipo == TipoMovimiento.SALIDA)
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
                    activo.Placa, tagId);
            }
        }

        if (tipo == TipoMovimiento.INGRESO)
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
            PuntoLecturaId = puntoLecturaId,
            TipoMovimiento = tipo,
            Autorizado = autorizado,
            AlarmaActivada = activarAlarma,
            DispositivoRfidId = dispositivoRfidId,
            FechaRegistro = DateTimeOffset.UtcNow
        };

        _context.Movimientos.Add(movimiento);
        await _context.SaveChangesAsync();

        var response = new EventoRfidResponse
        {
            TagId = tagId,
            ActivoPlaca = activo.Placa,
            ActivoNombre = activo.Nombre,
            TipoMovimiento = tipoMovimiento,
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
            PuntoLecturaId = puntoLecturaId,
            TipoMovimiento = tipo.ToString(),
            Autorizado = autorizado,
            AlarmaActivada = activarAlarma,
            FechaRegistro = movimiento.FechaRegistro
        };

        await _hubContext.Clients.Group("Dashboard")
            .SendAsync("NuevoMovimiento", movimientoMsg);

        _logger.LogInformation(
            "RFID Evento: TAG {TagId} | Activo {Placa} | {Tipo} | Autorizado: {Autorizado} | Alarma: {Alarma}",
            tagId, activo.Placa, tipoMovimiento, autorizado, activarAlarma);

        return Ok(ApiResponse<EventoRfidResponse>.Ok(response,
            response.Mensaje));
    }
}

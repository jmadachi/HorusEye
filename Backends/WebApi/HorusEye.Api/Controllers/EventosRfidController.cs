using HorusEye.Api.DTOs;
using HorusEye.Api.Hubs;
using HorusEye.Api.Models;
using HorusEye.Api.Providers;
using HorusEye.Api.Services;
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
    private readonly EventDetectionService _eventDetection;

    public EventosRfidController(
        HorusEyeDbContext context,
        IMemoryCache cache,
        IHubContext<MovimientosHub> hubContext,
        ILogger<EventosRfidController> logger,
        EventDetectionService eventDetection)
    {
        _context = context;
        _cache = cache;
        _hubContext = hubContext;
        _logger = logger;
        _eventDetection = eventDetection;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<EventoRfidResponse>>> ProcesarEvento(
        [FromBody] EventoRfidRequest request)
    {
        return await ProcesarEventoInterno(request.TagId, request.PuntoLecturaId, request.TipoMovimiento ?? "INGRESO", null);
    }

    [HttpPost("{providerNombre}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<EventoRfidResponse>>> ProcesarEventoPorProvider(
        string providerNombre)
    {
        // Validar API Key del dispositivo
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        DispositivoRfid? dispositivo = null;

        if (!string.IsNullOrEmpty(apiKey))
        {
            dispositivo = await _context.DispositivosRfid
                .FirstOrDefaultAsync(d => d.ApiKey == apiKey && d.Activo);

            if (dispositivo == null)
            {
                _logger.LogWarning("API Key invalida o dispositivo inactivo: {ApiKey}", apiKey[..Math.Min(8, apiKey.Length)]);
                return Unauthorized(ApiResponse<EventoRfidResponse>.Fail("API Key invalida o dispositivo inactivo"));
            }
        }

        IRfidProvider provider = providerNombre.ToLowerInvariant() switch
        {
            "chainway" => new ChainwayProvider(),
            "nfc-tester" => new NfcTesterProvider(),
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

        // Buscar dispositivo por API key, luego por IP, luego por nombre
        dispositivo ??= await _context.DispositivosRfid
            .FirstOrDefaultAsync(d =>
                (d.DireccionIP != null && d.DireccionIP == rfidEvent.DeviceId) ||
                d.Nombre == rfidEvent.DeviceId);

        // Determinar tipo de movimiento
        string tipoMovimiento;

        if (rfidEvent.RawData.TryGetValue("tipoMovimiento", out var tipoFromPayload)
            && !string.IsNullOrEmpty(tipoFromPayload))
        {
            // El dispositivo envia el tipo en el payload
            tipoMovimiento = tipoFromPayload.ToUpperInvariant();
        }
        else if (dispositivo?.DireccionPredeterminada == "ENTRADA"
              || dispositivo?.DireccionPredeterminada == "SALIDA")
        {
            // El dispositivo tiene una direccion fija configurada
            tipoMovimiento = dispositivo.DireccionPredeterminada == "ENTRADA" ? "INGRESO" : "SALIDA";
        }
        else if (dispositivo != null)
        {
            // Intentar detectar evento por secuencia de lectores
            var eventoDetectado = await _eventDetection.RegistrarLectura(rfidEvent.EPC, dispositivo.Id);
            tipoMovimiento = eventoDetectado ?? "INGRESO";

            if (eventoDetectado != null)
                _logger.LogInformation("Evento detectado por secuencia: {Evento} para TAG {TagId}", eventoDetectado, rfidEvent.EPC);
        }
        else
        {
            // Default: INGRESO (dispositivo bidireccional o sin configurar)
            tipoMovimiento = "INGRESO";
        }

        return await ProcesarEventoInterno(
            rfidEvent.EPC,
            rfidEvent.DeviceId,
            tipoMovimiento,
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
            "RFID Evento: TAG {TagId} | Activo {Placa} | {Tipo} | Autorizado: {Autorizado} | Alarma: {Alarma} | Dispositivo: {Dispositivo}",
            tagId, activo.Placa, tipoMovimiento, autorizado, activarAlarma, dispositivoRfidId);

        return Ok(ApiResponse<EventoRfidResponse>.Ok(response,
            response.Mensaje));
    }
}

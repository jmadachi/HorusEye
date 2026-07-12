using HorusEye.Api.DTOs;
using HorusEye.Api.Models;
using HorusEye.Core.Entities;
using HorusEye.Core.Enums;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Controllers;

[ApiController]
[Route("api/activos")]
[Authorize]
public class ActivosController : ControllerBase
{
    private readonly HorusEyeDbContext _context;
    private readonly ILogger<ActivosController> _logger;

    public ActivosController(HorusEyeDbContext context, ILogger<ActivosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _context.Activos
            .Include(a => a.Tag)
            .OrderByDescending(a => a.FechaRegistro);

        var total = await query.CountAsync();
        var activos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return Ok(ApiResponse<object>.Ok(new
        {
            Items = activos,
            Total = total,
            Page = page,
            PageSize = pageSize
        }));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ActivoResponse>>> GetById(Guid id)
    {
        var activo = await _context.Activos
            .Include(a => a.Tag)
            .Where(a => a.Id == id)
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
            .FirstOrDefaultAsync();

        if (activo == null)
            return NotFound(ApiResponse<ActivoResponse>.Fail("Activo no encontrado"));

        return Ok(ApiResponse<ActivoResponse>.Ok(activo));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador del Sistema,Asistente del Administrador del Sistema")]
    public async Task<ActionResult<ApiResponse<ActivoResponse>>> Create([FromBody] ActivoRequest request)
    {
        var placaExists = await _context.Activos.AnyAsync(a => a.Placa == request.Placa);
        if (placaExists)
            return BadRequest(ApiResponse<ActivoResponse>.Fail(
                $"Ya existe un activo con la placa '{request.Placa}'"));

        if (!string.IsNullOrEmpty(request.TagId))
        {
            var tag = await _context.Tags.FindAsync(request.TagId);
            if (tag == null)
                return BadRequest(ApiResponse<ActivoResponse>.Fail($"TAG {request.TagId} no encontrado"));

            if (tag.Estado != EstadoTag.DISPONIBLE)
                return BadRequest(ApiResponse<ActivoResponse>.Fail(
                    $"TAG {request.TagId} no está disponible (Estado: {tag.Estado})"));

            var tagEnUso = await _context.Activos.AnyAsync(a => a.TagId == request.TagId);
            if (tagEnUso)
                return BadRequest(ApiResponse<ActivoResponse>.Fail(
                    $"TAG {request.TagId} ya está asignado a otro activo"));

            tag.Estado = EstadoTag.ASIGNADO;
        }

        var activo = new Activo
        {
            Placa = request.Placa,
            Nombre = request.Nombre,
            Categoria = request.Categoria,
            TenedorResponsable = request.TenedorResponsable,
            TagId = request.TagId
        };

        _context.Activos.Add(activo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Activo creado: {Placa} - {Nombre}", activo.Placa, activo.Nombre);

        return CreatedAtAction(nameof(GetById), new { id = activo.Id },
            ApiResponse<ActivoResponse>.Ok(new ActivoResponse
            {
                Id = activo.Id,
                Placa = activo.Placa,
                Nombre = activo.Nombre,
                Categoria = activo.Categoria,
                TenedorResponsable = activo.TenedorResponsable,
                EstadoUbicacion = activo.EstadoUbicacion.ToString(),
                TagId = activo.TagId,
                FechaRegistro = activo.FechaRegistro
            }, "Activo creado exitosamente"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador del Sistema,Asistente del Administrador del Sistema")]
    public async Task<ActionResult<ApiResponse<ActivoResponse>>> Update(Guid id, [FromBody] ActivoRequest request)
    {
        var activo = await _context.Activos
            .Include(a => a.Tag)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activo == null)
            return NotFound(ApiResponse<ActivoResponse>.Fail("Activo no encontrado"));

        var placaDuplicada = await _context.Activos
            .AnyAsync(a => a.Placa == request.Placa && a.Id != id);
        if (placaDuplicada)
            return BadRequest(ApiResponse<ActivoResponse>.Fail(
                $"Ya existe otro activo con la placa '{request.Placa}'"));

        if (activo.TagId != request.TagId)
        {
            if (!string.IsNullOrEmpty(activo.TagId))
            {
                var oldTag = await _context.Tags.FindAsync(activo.TagId);
                if (oldTag != null)
                {
                    oldTag.Estado = EstadoTag.DISPONIBLE;
                }
            }

            if (!string.IsNullOrEmpty(request.TagId))
            {
                var newTag = await _context.Tags.FindAsync(request.TagId);
                if (newTag == null)
                    return BadRequest(ApiResponse<ActivoResponse>.Fail($"TAG {request.TagId} no encontrado"));

                if (newTag.Estado != EstadoTag.DISPONIBLE && newTag.Estado != EstadoTag.ASIGNADO)
                    return BadRequest(ApiResponse<ActivoResponse>.Fail(
                        $"TAG {request.TagId} no está disponible"));

                var tagEnUso = await _context.Activos.AnyAsync(a => a.TagId == request.TagId && a.Id != id);
                if (tagEnUso)
                    return BadRequest(ApiResponse<ActivoResponse>.Fail(
                        $"TAG {request.TagId} ya está asignado a otro activo"));

                newTag.Estado = EstadoTag.ASIGNADO;
            }

            activo.TagId = request.TagId;
        }

        activo.Placa = request.Placa;
        activo.Nombre = request.Nombre;
        activo.Categoria = request.Categoria;
        activo.TenedorResponsable = request.TenedorResponsable;
        activo.FechaActualizacion = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Activo actualizado: {Placa}", activo.Placa);

        return Ok(ApiResponse<ActivoResponse>.Ok(new ActivoResponse
        {
            Id = activo.Id,
            Placa = activo.Placa,
            Nombre = activo.Nombre,
            Categoria = activo.Categoria,
            TenedorResponsable = activo.TenedorResponsable,
            EstadoUbicacion = activo.EstadoUbicacion.ToString(),
            TagId = activo.TagId,
            FechaRegistro = activo.FechaRegistro
        }, "Activo actualizado exitosamente"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador del Sistema,Asistente del Administrador del Sistema")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var activo = await _context.Activos
            .Include(a => a.Tag)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activo == null)
            return NotFound(ApiResponse<object>.Fail("Activo no encontrado"));

        if (activo.Tag != null)
        {
            activo.Tag.Estado = EstadoTag.DISPONIBLE;
        }

        _context.Activos.Remove(activo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Activo eliminado: {Placa}", activo.Placa);

        return Ok(ApiResponse<object>.Ok(null!, "Activo eliminado exitosamente"));
    }
}

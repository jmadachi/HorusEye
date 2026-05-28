using HorusEye.Api.Models;
using HorusEye.Core.Entities;
using HorusEye.Core.Enums;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Controllers;

[ApiController]
[Route("api/tags")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly HorusEyeDbContext _context;
    private readonly ILogger<TagsController> _logger;

    public TagsController(HorusEyeDbContext context, ILogger<TagsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Tag>>>> GetAll()
    {
        var tags = await _context.Tags
            .OrderByDescending(t => t.FechaRegistro)
            .ToListAsync();

        return Ok(ApiResponse<List<Tag>>.Ok(tags));
    }

    [HttpGet("disponibles")]
    public async Task<ActionResult<ApiResponse<List<Tag>>>> GetDisponibles()
    {
        var tags = await _context.Tags
            .Where(t => t.Estado == EstadoTag.DISPONIBLE)
            .OrderBy(t => t.Id)
            .ToListAsync();

        return Ok(ApiResponse<List<Tag>>.Ok(tags));
    }

    [HttpPost]
    [Authorize(Roles = "Usuario de Gestión")]
    public async Task<ActionResult<ApiResponse<Tag>>> Create([FromBody] string tagId)
    {
        var existing = await _context.Tags.FindAsync(tagId);
        if (existing != null)
            return BadRequest(ApiResponse<Tag>.Fail($"TAG {tagId} ya existe"));

        var tag = new Tag
        {
            Id = tagId,
            Estado = EstadoTag.DISPONIBLE
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        _logger.LogInformation("TAG registrado: {TagId}", tagId);

        return CreatedAtAction(null, ApiResponse<Tag>.Ok(tag, "TAG registrado exitosamente"));
    }

    [HttpPut("{tagId}/estado")]
    [Authorize(Roles = "Usuario de Gestión")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateEstado(string tagId, [FromBody] string nuevoEstado)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null)
            return NotFound(ApiResponse<object>.Fail($"TAG {tagId} no encontrado"));

        if (!Enum.TryParse<EstadoTag>(nuevoEstado, true, out var estado))
            return BadRequest(ApiResponse<object>.Fail($"Estado inválido: {nuevoEstado}"));

        tag.Estado = estado;
        tag.FechaActualizacion = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("TAG {TagId} actualizado a estado {Estado}", tagId, nuevoEstado);

        return Ok(ApiResponse<object>.Ok(null!, $"TAG actualizado a {nuevoEstado}"));
    }

    [HttpPost("{tagId}/reportar-danio")]
    [Authorize(Roles = "Usuario de Gestión")]
    public async Task<ActionResult<ApiResponse<object>>> ReportarDanio(
        string tagId, [FromBody] string descripcion)
    {
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null)
            return NotFound(ApiResponse<object>.Fail($"TAG {tagId} no encontrado"));

        tag.Estado = EstadoTag.DAÑADO;
        tag.FechaActualizacion = DateTimeOffset.UtcNow;

        var historial = new TagDanioHistorial
        {
            TagId = tagId,
            Descripcion = descripcion,
            ReportadoPor = User.Identity?.Name
        };

        _context.TagsDaniosHistorial.Add(historial);
        await _context.SaveChangesAsync();

        _logger.LogWarning("TAG {TagId} reportado como DAÑADO: {Descripcion}", tagId, descripcion);

        return Ok(ApiResponse<object>.Ok(null!, "TAG reportado como dañado"));
    }
}

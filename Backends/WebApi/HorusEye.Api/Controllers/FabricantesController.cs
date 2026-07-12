using HorusEye.Api.DTOs;
using HorusEye.Api.Models;
using HorusEye.Api.Services;
using HorusEye.Core.Entities;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Controllers;

[ApiController]
[Route("api/fabricantes")]
[Authorize]
public class FabricantesController : ControllerBase
{
    private readonly HorusEyeDbContext _context;
    private readonly PermisoService _permisoService;
    private readonly ILogger<FabricantesController> _logger;

    public FabricantesController(
        HorusEyeDbContext context,
        PermisoService permisoService,
        ILogger<FabricantesController> logger)
    {
        _context = context;
        _permisoService = permisoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll()
    {
        var items = await _context.FabricantesDispositivo
            .Include(f => f.CamposPayload.OrderBy(c => c.OrdenExtraccion))
            .Where(f => f.Activo)
            .OrderBy(f => f.Nombre)
            .Select(f => new FabricanteResponse
            {
                Id = f.Id,
                Nombre = f.Nombre,
                Descripcion = f.Descripcion,
                UrlDocumentacion = f.UrlDocumentacion,
                EndpointEjemplo = f.EndpointEjemplo,
                Activo = f.Activo,
                FechaRegistro = f.FechaRegistro,
                Campos = f.CamposPayload.Select(c => new CampoPayloadResponse
                {
                    Id = c.Id,
                    NombreCampoExterno = c.NombreCampoExterno,
                    NombreCampoInterno = c.NombreCampoInterno,
                    TipoDato = c.TipoDato,
                    Requerido = c.Requerido,
                    ValorDefecto = c.ValorDefecto,
                    OrdenExtraccion = c.OrdenExtraccion
                }).ToList()
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id)
    {
        var fabricante = await _context.FabricantesDispositivo
            .Include(f => f.CamposPayload.OrderBy(c => c.OrdenExtraccion))
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fabricante == null)
            return NotFound(ApiResponse<object>.Fail("Fabricante no encontrado"));

        return Ok(ApiResponse<object>.Ok(new FabricanteResponse
        {
            Id = fabricante.Id,
            Nombre = fabricante.Nombre,
            Descripcion = fabricante.Descripcion,
            UrlDocumentacion = fabricante.UrlDocumentacion,
            EndpointEjemplo = fabricante.EndpointEjemplo,
            Activo = fabricante.Activo,
            FechaRegistro = fabricante.FechaRegistro,
            Campos = fabricante.CamposPayload.Select(c => new CampoPayloadResponse
            {
                Id = c.Id,
                NombreCampoExterno = c.NombreCampoExterno,
                NombreCampoInterno = c.NombreCampoInterno,
                TipoDato = c.TipoDato,
                Requerido = c.Requerido,
                ValorDefecto = c.ValorDefecto,
                OrdenExtraccion = c.OrdenExtraccion
            }).ToList()
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] FabricanteRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        if (role is not ("Administrador del Sistema" or "Asistente del Administrador del Sistema"))
            return Forbid();

        var fabricante = new FabricanteDispositivo
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            UrlDocumentacion = request.UrlDocumentacion,
            EndpointEjemplo = request.EndpointEjemplo
        };

        _context.FabricantesDispositivo.Add(fabricante);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Fabricante {Nombre} creado", fabricante.Nombre);

        return Ok(ApiResponse<object>.Ok(new { fabricante.Id }, "Fabricante creado exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] FabricanteRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        if (role is not ("Administrador del Sistema" or "Asistente del Administrador del Sistema"))
            return Forbid();

        var fabricante = await _context.FabricantesDispositivo.FindAsync(id);
        if (fabricante == null)
            return NotFound(ApiResponse<object>.Fail("Fabricante no encontrado"));

        fabricante.Nombre = request.Nombre;
        fabricante.Descripcion = request.Descripcion;
        fabricante.UrlDocumentacion = request.UrlDocumentacion;
        fabricante.EndpointEjemplo = request.EndpointEjemplo;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Fabricante actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var role = _permisoService.GetUserRole(User);
        if (role != "Administrador del Sistema")
            return Forbid();

        var fabricante = await _context.FabricantesDispositivo.FindAsync(id);
        if (fabricante == null)
            return NotFound(ApiResponse<object>.Fail("Fabricante no encontrado"));

        _context.FabricantesDispositivo.Remove(fabricante);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Fabricante eliminado exitosamente"));
    }

    [HttpPost("{id}/campos")]
    public async Task<ActionResult<ApiResponse<object>>> AddCampo(Guid id, [FromBody] CampoPayloadRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        if (role is not ("Administrador del Sistema" or "Asistente del Administrador del Sistema"))
            return Forbid();

        var fabricante = await _context.FabricantesDispositivo.FindAsync(id);
        if (fabricante == null)
            return NotFound(ApiResponse<object>.Fail("Fabricante no encontrado"));

        var campo = new CampoPayloadFabricante
        {
            FabricanteDispositivoId = id,
            NombreCampoExterno = request.NombreCampoExterno,
            NombreCampoInterno = request.NombreCampoInterno,
            TipoDato = request.TipoDato,
            Requerido = request.Requerido,
            ValorDefecto = request.ValorDefecto,
            OrdenExtraccion = request.OrdenExtraccion
        };

        _context.CamposPayloadFabricante.Add(campo);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { campo.Id }, "Campo agregado exitosamente"));
    }

    [HttpDelete("campo/{campoId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCampo(Guid campoId)
    {
        var role = _permisoService.GetUserRole(User);
        if (role is not ("Administrador del Sistema" or "Asistente del Administrador del Sistema"))
            return Forbid();

        var campo = await _context.CamposPayloadFabricante.FindAsync(campoId);
        if (campo == null)
            return NotFound(ApiResponse<object>.Fail("Campo no encontrado"));

        _context.CamposPayloadFabricante.Remove(campo);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Campo eliminado exitosamente"));
    }
}

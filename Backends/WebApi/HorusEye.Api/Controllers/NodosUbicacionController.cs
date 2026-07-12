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
[Route("api/ubicaciones")]
[Authorize]
public class NodosUbicacionController : ControllerBase
{
    private readonly HorusEyeDbContext _context;
    private readonly PermisoService _permisoService;
    private readonly ILogger<NodosUbicacionController> _logger;

    public NodosUbicacionController(
        HorusEyeDbContext context,
        PermisoService permisoService,
        ILogger<NodosUbicacionController> logger)
    {
        _context = context;
        _permisoService = permisoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll(
        [FromQuery] Guid? clienteId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        var query = _context.NodosUbicacion.AsQueryable();

        if (clienteId.HasValue)
        {
            query = query.Where(n => n.ClienteId == clienteId);
        }
        else if (_permisoService.IsRolCliente(role))
        {
            var cid = await _permisoService.GetUsuarioClienteIdAsync(userId);
            query = cid != null ? query.Where(n => n.ClienteId == cid) : query.Where(_ => false);
        }
        else if (_permisoService.IsRolProveedor(role))
        {
            var pid = await _permisoService.GetUsuarioProveedorIdAsync(userId);
            if (pid != null)
            {
                var clienteIds = await _context.Clientes
                    .Where(c => c.ProveedorId == pid)
                    .Select(c => c.Id)
                    .ToListAsync();
                query = query.Where(n => clienteIds.Contains(n.ClienteId));
            }
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(n => n.ClienteId).ThenBy(n => n.Nivel).ThenBy(n => n.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NodoUbicacionResponse
            {
                Id = n.Id,
                Nombre = n.Nombre,
                TipoNodo = n.TipoNodo,
                PadreId = n.PadreId,
                ClienteId = n.ClienteId,
                Nivel = n.Nivel,
                Metadata = n.Metadata,
                Activo = n.Activo
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { Items = items, Total = total, Page = page, PageSize = pageSize }));
    }

    [HttpGet("arbol")]
    public async Task<ActionResult<ApiResponse<object>>> GetArbol(
        [FromQuery] Guid clienteId)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        if (!await _permisoService.IsInClienteScopeAsync(userId, role, clienteId))
            return Forbid();

        var nodos = await _context.NodosUbicacion
            .Where(n => n.ClienteId == clienteId && n.Activo)
            .OrderBy(n => n.Nivel).ThenBy(n => n.Nombre)
            .ToListAsync();

        var nodosDict = nodos.ToDictionary(n => n.Id);
        var raices = new List<object>();

        foreach (var nodo in nodos)
        {
            var nodoObj = new
            {
                nodo.Id,
                nodo.Nombre,
                nodo.TipoNodo,
                nodo.PadreId,
                nodo.ClienteId,
                nodo.Nivel,
                nodo.Metadata,
                nodo.Activo,
                Children = nodos.Where(n => n.PadreId == nodo.Id).Select(n => new
                {
                    n.Id,
                    n.Nombre,
                    n.TipoNodo,
                    n.PadreId,
                    n.ClienteId,
                    n.Nivel,
                    n.Metadata,
                    n.Activo
                }).ToList()
            };

            if (nodo.PadreId == null)
                raices.Add(nodoObj);
        }

        return Ok(ApiResponse<object>.Ok(raices));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] NodoUbicacionRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        if (!await _permisoService.IsInClienteScopeAsync(userId, role, request.ClienteId))
            return Forbid();

        int nivel = 0;
        if (request.PadreId.HasValue)
        {
            var padre = await _context.NodosUbicacion.FindAsync(request.PadreId.Value);
            if (padre == null)
                return BadRequest(ApiResponse<object>.Fail("Nodo padre no encontrado"));
            nivel = padre.Nivel + 1;
        }

        var nodo = new NodoUbicacion
        {
            Nombre = request.Nombre,
            TipoNodo = request.TipoNodo,
            PadreId = request.PadreId,
            ClienteId = request.ClienteId,
            Nivel = nivel,
            Metadata = request.Metadata
        };

        _context.NodosUbicacion.Add(nodo);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { nodo.Id }, "Ubicacion creada exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] NodoUbicacionRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        var nodo = await _context.NodosUbicacion.FindAsync(id);
        if (nodo == null)
            return NotFound(ApiResponse<object>.Fail("Ubicacion no encontrada"));

        if (!await _permisoService.IsInClienteScopeAsync(userId, role, nodo.ClienteId))
            return Forbid();

        nodo.Nombre = request.Nombre;
        nodo.TipoNodo = request.TipoNodo;
        nodo.PadreId = request.PadreId;
        nodo.Metadata = request.Metadata;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Ubicacion actualizada exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var role = _permisoService.GetUserRole(User);
        if (role != "Administrador del Sistema")
            return Forbid();

        var nodo = await _context.NodosUbicacion.FindAsync(id);
        if (nodo == null)
            return NotFound(ApiResponse<object>.Fail("Ubicacion no encontrada"));

        _context.NodosUbicacion.Remove(nodo);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Ubicacion eliminada exitosamente"));
    }
}

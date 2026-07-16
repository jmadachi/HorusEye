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
[Route("api/eventos")]
[Authorize]
public class EventosController : ControllerBase
{
    private readonly HorusEyeDbContext _context;
    private readonly PermisoService _permisoService;

    public EventosController(HorusEyeDbContext context, PermisoService permisoService)
    {
        _context = context;
        _permisoService = permisoService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll(
        [FromQuery] Guid? clienteId = null,
        [FromQuery] Guid? nodoUbicacionId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        var query = _context.Eventos
            .Include(e => e.Lectores)
                .ThenInclude(l => l.DispositivoRfid)
            .Include(e => e.NodoUbicacion)
            .AsQueryable();

        if (nodoUbicacionId.HasValue)
            query = query.Where(e => e.NodoUbicacionId == nodoUbicacionId);
        else if (clienteId.HasValue)
            query = query.Where(e => e.ClienteId == clienteId);
        else if (_permisoService.IsRolCliente(role))
        {
            var cid = await _permisoService.GetUsuarioClienteIdAsync(userId);
            query = cid != null ? query.Where(e => e.ClienteId == cid) : query.Where(_ => false);
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
                query = query.Where(e => clienteIds.Contains(e.ClienteId));
            }
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventoResponse
            {
                Id = e.Id,
                Nombre = e.Nombre,
                NodoUbicacionId = e.NodoUbicacionId,
                NodoUbicacionNombre = e.NodoUbicacion != null ? e.NodoUbicacion.Nombre : null,
                ClienteId = e.ClienteId,
                Activo = e.Activo,
                Lectores = e.Lectores
                    .OrderBy(l => l.Orden)
                    .Select(l => new EventoLectorResponse
                    {
                        Id = l.Id,
                        DispositivoRfidId = l.DispositivoRfidId,
                        DispositivoNombre = l.DispositivoRfid != null ? l.DispositivoRfid.Nombre : null,
                        Orden = l.Orden
                    }).ToList()
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { Items = items, Total = total, Page = page, PageSize = pageSize }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<EventoResponse>>> GetById(Guid id)
    {
        var e = await _context.Eventos
            .Include(ev => ev.Lectores)
                .ThenInclude(l => l.DispositivoRfid)
            .Include(ev => ev.NodoUbicacion)
            .FirstOrDefaultAsync(ev => ev.Id == id);

        if (e == null)
            return NotFound(ApiResponse<EventoResponse>.Fail("Evento no encontrado"));

        return Ok(ApiResponse<EventoResponse>.Ok(new EventoResponse
        {
            Id = e.Id,
            Nombre = e.Nombre,
            NodoUbicacionId = e.NodoUbicacionId,
            NodoUbicacionNombre = e.NodoUbicacion?.Nombre,
            ClienteId = e.ClienteId,
            Activo = e.Activo,
            Lectores = e.Lectores
                .OrderBy(l => l.Orden)
                .Select(l => new EventoLectorResponse
                {
                    Id = l.Id,
                    DispositivoRfidId = l.DispositivoRfidId,
                    DispositivoNombre = l.DispositivoRfid?.Nombre,
                    Orden = l.Orden
                }).ToList()
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] EventoRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        if (!await _permisoService.IsInClienteScopeAsync(userId, role, request.ClienteId))
            return Forbid();

        // Validar que no exista un evento con el mismo nombre en la misma ubicacion
        var exists = await _context.Eventos
            .AnyAsync(e => e.NodoUbicacionId == request.NodoUbicacionId && e.Nombre == request.Nombre);
        if (exists)
            return BadRequest(ApiResponse<object>.Fail("Ya existe un evento con ese nombre en esta ubicacion"));

        // Validar que no haya secuencias duplicadas
        if (request.Lectores.Count > 0)
        {
            var lectoresOrdenados = request.Lectores.OrderBy(l => l.Orden).Select(l => l.DispositivoRfidId).ToList();
            var secuencia = string.Join(",", lectoresOrdenados);

            var existingEvents = await _context.Eventos
                .Where(e => e.NodoUbicacionId == request.NodoUbicacionId && e.Id != Guid.Empty)
                .Include(e => e.Lectores)
                .ToListAsync();

            foreach (var existing in existingEvents)
            {
                var existingSeq = string.Join(",", existing.Lectores.OrderBy(l => l.Orden).Select(l => l.DispositivoRfidId));
                if (existingSeq == secuencia)
                    return BadRequest(ApiResponse<object>.Fail("Ya existe un evento con la misma secuencia de lectores en esta ubicacion"));
            }
        }

        var evento = new Evento
        {
            Nombre = request.Nombre,
            NodoUbicacionId = request.NodoUbicacionId,
            ClienteId = request.ClienteId
        };

        _context.Eventos.Add(evento);
        await _context.SaveChangesAsync();

        // Agregar lectores
        foreach (var l in request.Lectores.OrderBy(l => l.Orden))
        {
            _context.EventosLectores.Add(new EventoLector
            {
                EventoId = evento.Id,
                DispositivoRfidId = l.DispositivoRfidId,
                Orden = l.Orden
            });
        }
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { evento.Id }, "Evento creado exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] EventoRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        var evento = await _context.Eventos
            .Include(e => e.Lectores)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evento == null)
            return NotFound(ApiResponse<object>.Fail("Evento no encontrado"));

        if (!await _permisoService.IsInClienteScopeAsync(userId, role, evento.ClienteId))
            return Forbid();

        // Validar nombre duplicado en la ubicacion
        var exists = await _context.Eventos
            .AnyAsync(e => e.NodoUbicacionId == request.NodoUbicacionId && e.Nombre == request.Nombre && e.Id != id);
        if (exists)
            return BadRequest(ApiResponse<object>.Fail("Ya existe un evento con ese nombre en esta ubicacion"));

        evento.Nombre = request.Nombre;
        evento.NodoUbicacionId = request.NodoUbicacionId;

        // Reemplazar lectores
        _context.EventosLectores.RemoveRange(evento.Lectores);
        foreach (var l in request.Lectores.OrderBy(l => l.Orden))
        {
            _context.EventosLectores.Add(new EventoLector
            {
                EventoId = id,
                DispositivoRfidId = l.DispositivoRfidId,
                Orden = l.Orden
            });
        }

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null!, "Evento actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var role = _permisoService.GetUserRole(User);
        if (role != "Administrador del Sistema")
            return Forbid();

        var evento = await _context.Eventos.FindAsync(id);
        if (evento == null)
            return NotFound(ApiResponse<object>.Fail("Evento no encontrado"));

        _context.Eventos.Remove(evento);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Evento eliminado exitosamente"));
    }
}

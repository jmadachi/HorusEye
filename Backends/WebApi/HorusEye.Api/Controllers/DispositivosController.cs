using HorusEye.Api.DTOs;
using HorusEye.Api.Models;
using HorusEye.Api.Services;
using HorusEye.Core.Entities;
using HorusEye.Core.Enums;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Controllers;

[ApiController]
[Route("api/dispositivos")]
[Authorize]
public class DispositivosController : ControllerBase
{
    private readonly HorusEyeDbContext _context;
    private readonly PermisoService _permisoService;
    private readonly ILogger<DispositivosController> _logger;

    public DispositivosController(
        HorusEyeDbContext context,
        PermisoService permisoService,
        ILogger<DispositivosController> logger)
    {
        _context = context;
        _permisoService = permisoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll(
        [FromQuery] Guid? clienteId = null,
        [FromQuery] Guid? proveedorId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        var query = _context.DispositivosRfid
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .AsQueryable();

        if (clienteId.HasValue)
        {
            query = query.Where(d => d.ClienteId == clienteId);
        }
        else if (proveedorId.HasValue)
        {
            query = query.Where(d => d.ProveedorId == proveedorId);
        }
        else if (_permisoService.IsRolCliente(role))
        {
            var cid = await _permisoService.GetUsuarioClienteIdAsync(userId);
            query = cid != null ? query.Where(d => d.ClienteId == cid) : query.Where(_ => false);
        }
        else if (_permisoService.IsRolProveedor(role))
        {
            var pid = await _permisoService.GetUsuarioProveedorIdAsync(userId);
            query = pid != null ? query.Where(d => d.ProveedorId == pid) : query.Where(_ => false);
        }

        query = query.OrderByDescending(d => d.FechaRegistro);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DispositivoResponse
            {
                Id = d.Id,
                Nombre = d.Nombre,
                Fabricante = d.Fabricante,
                Modelo = d.Modelo,
                DireccionIP = d.DireccionIP,
                Puerto = d.Puerto,
                Ubicacion = d.Ubicacion,
                ClienteId = d.ClienteId,
                ClienteNombre = d.Cliente != null ? d.Cliente.Nombre : null,
                ProveedorId = d.ProveedorId,
                ProveedorNombre = d.Proveedor != null ? d.Proveedor.Nombre : null,
                TipoDispositivo = d.TipoDispositivo.ToString(),
                EndpointAPI = d.EndpointAPI,
                MetodoHTTP = d.MetodoHTTP,
                FabricanteDispositivoId = d.FabricanteDispositivoId,
                Activo = d.Activo,
                FechaRegistro = d.FechaRegistro
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { Items = items, Total = total, Page = page, PageSize = pageSize }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id)
    {
        var dispositivo = await _context.DispositivosRfid
            .Include(d => d.Cliente)
            .Include(d => d.Proveedor)
            .Include(d => d.NodoUbicacion)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dispositivo == null)
            return NotFound(ApiResponse<object>.Fail("Dispositivo no encontrado"));

        return Ok(ApiResponse<object>.Ok(new DispositivoResponse
        {
            Id = dispositivo.Id,
            Nombre = dispositivo.Nombre,
            Fabricante = dispositivo.Fabricante,
            Modelo = dispositivo.Modelo,
            DireccionIP = dispositivo.DireccionIP,
            Puerto = dispositivo.Puerto,
            Ubicacion = dispositivo.Ubicacion,
            ClienteId = dispositivo.ClienteId,
            ClienteNombre = dispositivo.Cliente?.Nombre,
            ProveedorId = dispositivo.ProveedorId,
            ProveedorNombre = dispositivo.Proveedor?.Nombre,
            NodoUbicacionId = dispositivo.NodoUbicacionId,
            NodoUbicacionNombre = dispositivo.NodoUbicacion?.Nombre,
            TipoDispositivo = dispositivo.TipoDispositivo.ToString(),
            EndpointAPI = dispositivo.EndpointAPI,
            MetodoHTTP = dispositivo.MetodoHTTP,
            FabricanteDispositivoId = dispositivo.FabricanteDispositivoId,
            Activo = dispositivo.Activo,
            FechaRegistro = dispositivo.FechaRegistro
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] DispositivoRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        if (role is not ("Administrador del Sistema" or "Asistente del Administrador del Sistema"
            or "Administrador del Proveedor" or "Administrador del Cliente"))
            return Forbid();

        if (request.ClienteId.HasValue && !await _permisoService.IsInClienteScopeAsync(userId, role, request.ClienteId.Value))
            return Forbid();

        var dispositivo = new DispositivoRfid
        {
            Nombre = request.Nombre,
            Fabricante = request.Fabricante,
            Modelo = request.Modelo,
            DireccionIP = request.DireccionIP,
            Puerto = request.Puerto,
            Ubicacion = request.Ubicacion,
            ClienteId = request.ClienteId,
            ProveedorId = request.ProveedorId,
            NodoUbicacionId = request.NodoUbicacionId,
            TipoDispositivo = Enum.Parse<TipoDispositivo>(request.TipoDispositivo),
            EndpointAPI = request.EndpointAPI,
            MetodoHTTP = request.MetodoHTTP ?? "POST",
            FabricanteDispositivoId = request.FabricanteDispositivoId
        };

        _context.DispositivosRfid.Add(dispositivo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Dispositivo {Nombre} registrado", dispositivo.Nombre);

        return Ok(ApiResponse<object>.Ok(new { dispositivo.Id }, "Dispositivo registrado exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] DispositivoRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        var dispositivo = await _context.DispositivosRfid.FindAsync(id);
        if (dispositivo == null)
            return NotFound(ApiResponse<object>.Fail("Dispositivo no encontrado"));

        if (dispositivo.ClienteId.HasValue && !await _permisoService.IsInClienteScopeAsync(userId, role, dispositivo.ClienteId.Value))
            return Forbid();

        dispositivo.Nombre = request.Nombre;
        dispositivo.Fabricante = request.Fabricante;
        dispositivo.Modelo = request.Modelo;
        dispositivo.DireccionIP = request.DireccionIP;
        dispositivo.Puerto = request.Puerto;
        dispositivo.Ubicacion = request.Ubicacion;
        dispositivo.ClienteId = request.ClienteId;
        dispositivo.ProveedorId = request.ProveedorId;
        dispositivo.NodoUbicacionId = request.NodoUbicacionId;
        dispositivo.TipoDispositivo = Enum.Parse<TipoDispositivo>(request.TipoDispositivo);
        dispositivo.EndpointAPI = request.EndpointAPI;
        dispositivo.MetodoHTTP = request.MetodoHTTP ?? "POST";
        dispositivo.FabricanteDispositivoId = request.FabricanteDispositivoId;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Dispositivo actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var role = _permisoService.GetUserRole(User);
        if (role != "Administrador del Sistema")
            return Forbid();

        var dispositivo = await _context.DispositivosRfid.FindAsync(id);
        if (dispositivo == null)
            return NotFound(ApiResponse<object>.Fail("Dispositivo no encontrado"));

        _context.DispositivosRfid.Remove(dispositivo);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Dispositivo eliminado exitosamente"));
    }
}

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
[Route("api/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly HorusEyeDbContext _context;
    private readonly PermisoService _permisoService;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(
        HorusEyeDbContext context,
        PermisoService permisoService,
        ILogger<ClientesController> logger)
    {
        _context = context;
        _permisoService = permisoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll(
        [FromQuery] Guid? proveedorId = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        var query = _context.Clientes.AsQueryable();

        if (proveedorId.HasValue)
        {
            query = query.Where(c => c.ProveedorId == proveedorId);
        }
        else if (_permisoService.IsRolProveedor(role))
        {
            var proveedorIds = await _permisoService.GetAccessibleProveedorIdsAsync(userId, role);
            query = query.Where(c => c.ProveedorId != null && proveedorIds.Contains(c.ProveedorId.Value));
        }
        else if (_permisoService.IsRolCliente(role))
        {
            var clienteId = await _permisoService.GetUsuarioClienteIdAsync(userId);
            query = clienteId != null ? query.Where(c => c.Id == clienteId) : query.Where(_ => false);
        }

        query = query.OrderByDescending(c => c.FechaRegistro);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClienteResponse
            {
                Id = c.Id,
                Nombre = c.Nombre,
                RazonSocial = c.RazonSocial,
                RUC = c.RUC,
                Direccion = c.Direccion,
                Telefono = c.Telefono,
                Email = c.Email,
                ProveedorId = c.ProveedorId,
                ProveedorNombre = c.Proveedor != null ? c.Proveedor.Nombre : null,
                Activo = c.Activo,
                FechaRegistro = c.FechaRegistro
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { Items = items, Total = total, Page = page, PageSize = pageSize }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        if (!await _permisoService.IsInClienteScopeAsync(userId, role, id))
            return Forbid();

        var cliente = await _context.Clientes
            .Include(c => c.Proveedor)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
            return NotFound(ApiResponse<object>.Fail("Cliente no encontrado"));

        return Ok(ApiResponse<object>.Ok(new ClienteResponse
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            RazonSocial = cliente.RazonSocial,
            RUC = cliente.RUC,
            Direccion = cliente.Direccion,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            ProveedorId = cliente.ProveedorId,
            ProveedorNombre = cliente.Proveedor?.Nombre,
            Activo = cliente.Activo,
            FechaRegistro = cliente.FechaRegistro
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] ClienteRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        if (role is not ("Administrador del Sistema" or "Asistente del Administrador del Sistema" or "Administrador del Proveedor"))
            return Forbid();

        if (request.ProveedorId.HasValue && !_permisoService.IsRolSistema(role))
        {
            if (!await _permisoService.IsInProveedorScopeAsync(userId, role, request.ProveedorId.Value))
                return Forbid();
        }

        var cliente = new Cliente
        {
            Nombre = request.Nombre,
            RazonSocial = request.RazonSocial,
            RUC = request.RUC,
            Direccion = request.Direccion,
            Telefono = request.Telefono,
            Email = request.Email,
            ProveedorId = request.ProveedorId
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cliente {Nombre} creado", cliente.Nombre);

        return Ok(ApiResponse<object>.Ok(new { cliente.Id }, "Cliente creado exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] ClienteRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        if (!await _permisoService.IsInClienteScopeAsync(userId, role, id))
            return Forbid();

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound(ApiResponse<object>.Fail("Cliente no encontrado"));

        cliente.Nombre = request.Nombre;
        cliente.RazonSocial = request.RazonSocial;
        cliente.RUC = request.RUC;
        cliente.Direccion = request.Direccion;
        cliente.Telefono = request.Telefono;
        cliente.Email = request.Email;
        cliente.ProveedorId = request.ProveedorId;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Cliente actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var role = _permisoService.GetUserRole(User);
        if (role != "Administrador del Sistema")
            return Forbid();

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound(ApiResponse<object>.Fail("Cliente no encontrado"));

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Cliente eliminado exitosamente"));
    }
}

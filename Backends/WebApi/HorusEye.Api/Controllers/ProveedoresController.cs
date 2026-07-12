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
[Route("api/proveedores")]
[Authorize]
public class ProveedoresController : ControllerBase
{
    private readonly HorusEyeDbContext _context;
    private readonly PermisoService _permisoService;
    private readonly ILogger<ProveedoresController> _logger;

    public ProveedoresController(
        HorusEyeDbContext context,
        PermisoService permisoService,
        ILogger<ProveedoresController> logger)
    {
        _context = context;
        _permisoService = permisoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _context.Proveedores.AsQueryable();

        query = query.OrderByDescending(p => p.FechaRegistro);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProveedorResponse
            {
                Id = p.Id,
                Nombre = p.Nombre,
                RazonSocial = p.RazonSocial,
                RUC = p.RUC,
                Direccion = p.Direccion,
                Telefono = p.Telefono,
                Email = p.Email,
                Activo = p.Activo,
                FechaRegistro = p.FechaRegistro
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { Items = items, Total = total, Page = page, PageSize = pageSize }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
            return NotFound(ApiResponse<object>.Fail("Proveedor no encontrado"));

        return Ok(ApiResponse<object>.Ok(new ProveedorResponse
        {
            Id = proveedor.Id,
            Nombre = proveedor.Nombre,
            RazonSocial = proveedor.RazonSocial,
            RUC = proveedor.RUC,
            Direccion = proveedor.Direccion,
            Telefono = proveedor.Telefono,
            Email = proveedor.Email,
            Activo = proveedor.Activo,
            FechaRegistro = proveedor.FechaRegistro
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] ProveedorRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        if (role is not ("Administrador del Sistema" or "Asistente del Administrador del Sistema"))
            return Forbid();

        var proveedor = new Proveedor
        {
            Nombre = request.Nombre,
            RazonSocial = request.RazonSocial,
            RUC = request.RUC,
            Direccion = request.Direccion,
            Telefono = request.Telefono,
            Email = request.Email
        };

        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Proveedor {Nombre} creado", proveedor.Nombre);

        return Ok(ApiResponse<object>.Ok(new { proveedor.Id }, "Proveedor creado exitosamente"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] ProveedorRequest request)
    {
        var role = _permisoService.GetUserRole(User);
        if (role is not ("Administrador del Sistema" or "Asistente del Administrador del Sistema"))
            return Forbid();

        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
            return NotFound(ApiResponse<object>.Fail("Proveedor no encontrado"));

        proveedor.Nombre = request.Nombre;
        proveedor.RazonSocial = request.RazonSocial;
        proveedor.RUC = request.RUC;
        proveedor.Direccion = request.Direccion;
        proveedor.Telefono = request.Telefono;
        proveedor.Email = request.Email;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Proveedor actualizado exitosamente"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var role = _permisoService.GetUserRole(User);
        if (role != "Administrador del Sistema")
            return Forbid();

        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null)
            return NotFound(ApiResponse<object>.Fail("Proveedor no encontrado"));

        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null!, "Proveedor eliminado exitosamente"));
    }
}

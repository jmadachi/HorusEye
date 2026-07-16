using System.Security.Claims;
using HorusEye.Api.Models;
using HorusEye.Core.Entities;
using HorusEye.Core.Enums;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly HorusEyeDbContext _context;

    public DashboardController(HorusEyeDbContext context)
    {
        _context = context;
    }

    private (string role, Guid? proveedorId, Guid? clienteId) GetUserContext()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Asistente del Cliente";
        var proveedorIdStr = User.FindFirstValue("proveedor_id");
        var clienteIdStr = User.FindFirstValue("cliente_id");

        Guid? proveedorId = null;
        Guid? clienteId = null;

        if (!string.IsNullOrEmpty(proveedorIdStr) && Guid.TryParse(proveedorIdStr, out var pid))
            proveedorId = pid;
        if (!string.IsNullOrEmpty(clienteIdStr) && Guid.TryParse(clienteIdStr, out var cid))
            clienteId = cid;

        return (role, proveedorId, clienteId);
    }

    private bool IsRolSistema(string role) =>
        role is "Administrador del Sistema" or "Asistente del Administrador del Sistema" or "Soporte del Sistema";

    private IQueryable<Activo> FilterActivosByScope(IQueryable<Activo> query, string role, Guid? proveedorId, Guid? clienteId)
    {
        if (IsRolSistema(role))
            return query;

        if (role is "Administrador del Cliente" or "Asistente del Cliente" && clienteId.HasValue)
            return query.Where(a => a.ClienteId == clienteId.Value);

        if (role is "Administrador del Proveedor" or "Asistente del Proveedor" && proveedorId.HasValue)
        {
            var clienteIds = _context.Clientes
                .Where(c => c.ProveedorId == proveedorId.Value && c.Activo)
                .Select(c => c.Id);
            return query.Where(a => a.ClienteId.HasValue && clienteIds.Contains(a.ClienteId.Value));
        }

        return query.Where(a => false);
    }

    private IQueryable<Movimiento> FilterMovimientosByScope(IQueryable<Movimiento> query, string role, Guid? proveedorId, Guid? clienteId)
    {
        if (IsRolSistema(role))
            return query;

        if (role is "Administrador del Cliente" or "Asistente del Cliente" && clienteId.HasValue)
            return query.Where(m => m.Activo.ClienteId == clienteId.Value);

        if (role is "Administrador del Proveedor" or "Asistente del Proveedor" && proveedorId.HasValue)
        {
            var clienteIds = _context.Clientes
                .Where(c => c.ProveedorId == proveedorId.Value && c.Activo)
                .Select(c => c.Id);
            return query.Where(m => m.Activo.ClienteId.HasValue && clienteIds.Contains(m.Activo.ClienteId.Value));
        }

        return query.Where(m => false);
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<ApiResponse<object>>> GetKPIs()
    {
        var (role, proveedorId, clienteId) = GetUserContext();

        var activosQuery = FilterActivosByScope(_context.Activos.AsQueryable(), role, proveedorId, clienteId);
        var movimientosQuery = FilterMovimientosByScope(_context.Movimientos
            .Include(m => m.Activo).AsQueryable(), role, proveedorId, clienteId);

        var totalActivos = await activosQuery.CountAsync();
        var activosDentro = await activosQuery
            .CountAsync(a => a.EstadoUbicacion == EstadoUbicacion.DENTRO_INSTALACIONES);
        var activosFuera = await activosQuery
            .CountAsync(a => a.EstadoUbicacion == EstadoUbicacion.FUERA_INSTALACIONES);

        var tagsQuery = _context.Tags.AsQueryable();
        if (!IsRolSistema(role))
        {
            var activoIds = await activosQuery.Select(a => a.Id).ToListAsync();
            tagsQuery = tagsQuery.Where(t => t.Activo != null && activoIds.Contains(t.Activo.Id));
        }

        var tagsRegistrados = await tagsQuery.CountAsync();
        var tagsAsignados = await tagsQuery
            .CountAsync(t => t.Estado == EstadoTag.ASIGNADO);
        var tagsDisponibles = await tagsQuery
            .CountAsync(t => t.Estado == EstadoTag.DISPONIBLE);

        var hoy = DateTimeOffset.UtcNow.Date;
        var manana = hoy.AddDays(1);

        var ingresosHoy = await movimientosQuery
            .CountAsync(m => m.TipoMovimiento == TipoMovimiento.INGRESO
                && m.FechaRegistro >= hoy && m.FechaRegistro < manana);
        var salidasHoy = await movimientosQuery
            .CountAsync(m => m.TipoMovimiento == TipoMovimiento.SALIDA
                && m.FechaRegistro >= hoy && m.FechaRegistro < manana);

        var categorias = await activosQuery
            .GroupBy(a => a.Categoria)
            .Select(g => new { Categoria = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            TotalActivos = totalActivos,
            ActivosDentro = activosDentro,
            ActivosFuera = activosFuera,
            TagsRegistrados = tagsRegistrados,
            TagsAsignados = tagsAsignados,
            TagsDisponibles = tagsDisponibles,
            IngresosHoy = ingresosHoy,
            SalidasHoy = salidasHoy,
            Categorias = categorias,
            Fecha = hoy
        }));
    }

    [HttpGet("tendencias")]
    public async Task<ActionResult<ApiResponse<object>>> GetTendencias()
    {
        var (role, proveedorId, clienteId) = GetUserContext();

        var hoy = DateTimeOffset.UtcNow.Date;
        var hace7Dias = hoy.AddDays(-6);

        var movimientosQuery = FilterMovimientosByScope(_context.Movimientos
            .Include(m => m.Activo).AsQueryable(), role, proveedorId, clienteId);

        var movimientos = await movimientosQuery
            .Where(m => m.FechaRegistro >= hace7Dias && m.FechaRegistro < hoy.AddDays(1))
            .ToListAsync();

        var dias = new List<object>();
        for (var d = hace7Dias; d <= hoy; d = d.AddDays(1))
        {
            var diaFin = d.AddDays(1);
            dias.Add(new
            {
                Fecha = d.ToString("yyyy-MM-dd"),
                Dia = d.ToString("ddd", new System.Globalization.CultureInfo("es-MX")),
                Ingresos = movimientos.Count(m =>
                    m.TipoMovimiento == TipoMovimiento.INGRESO
                    && m.FechaRegistro >= d && m.FechaRegistro < diaFin),
                Salidas = movimientos.Count(m =>
                    m.TipoMovimiento == TipoMovimiento.SALIDA
                    && m.FechaRegistro >= d && m.FechaRegistro < diaFin),
                NoAutorizadas = movimientos.Count(m =>
                    m.TipoMovimiento == TipoMovimiento.SALIDA
                    && !m.Autorizado
                    && m.FechaRegistro >= d && m.FechaRegistro < diaFin)
            });
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            Dias = dias,
            Desde = hace7Dias,
            Hasta = hoy
        }));
    }
}

using System.Security.Claims;
using HorusEye.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Services;

public class PermisoService
{
    private readonly HorusEyeDbContext _context;

    public PermisoService(HorusEyeDbContext context)
    {
        _context = context;
    }

    // Roles que pueden crear cada tipo de rol objetivo
    private static readonly Dictionary<string, string[]> CreacionRoles = new()
    {
        ["Administrador del Sistema"] = [
            "Administrador del Sistema",
            "Asistente del Administrador del Sistema",
            "Soporte del Sistema",
            "Administrador del Proveedor",
            "Asistente del Proveedor",
            "Administrador del Cliente",
            "Asistente del Cliente"
        ],
        ["Asistente del Administrador del Sistema"] = [
            "Administrador del Proveedor",
            "Asistente del Proveedor",
            "Administrador del Cliente",
            "Asistente del Cliente"
        ],
        ["Soporte del Sistema"] = [],
        ["Administrador del Proveedor"] = [
            "Asistente del Proveedor",
            "Administrador del Cliente",
            "Asistente del Cliente"
        ],
        ["Asistente del Proveedor"] = [],
        ["Administrador del Cliente"] = [
            "Asistente del Cliente"
        ],
        ["Asistente del Cliente"] = []
    };

    public bool CanCreateRole(string creatorRole, string targetRole)
    {
        if (!CreacionRoles.ContainsKey(creatorRole))
            return false;

        return CreacionRoles[creatorRole].Contains(targetRole);
    }

    public bool IsRolSistema(string role) =>
        role is "Administrador del Sistema" or "Asistente del Administrador del Sistema" or "Soporte del Sistema";

    public bool IsRolProveedor(string role) =>
        role is "Administrador del Proveedor" or "Asistente del Proveedor";

    public bool IsRolCliente(string role) =>
        role is "Administrador del Cliente" or "Asistente del Cliente";

    public bool NeedsProveedorAssociation(string role) =>
        IsRolProveedor(role) || IsRolCliente(role);

    public bool NeedsClienteAssociation(string role) =>
        IsRolCliente(role);

    public async Task<Guid?> GetUsuarioProveedorIdAsync(string userId)
    {
        var extendido = await _context.UsuariosExtendidos
            .FirstOrDefaultAsync(u => u.Id == userId);
        return extendido?.ProveedorId;
    }

    public async Task<Guid?> GetUsuarioClienteIdAsync(string userId)
    {
        var extendido = await _context.UsuariosExtendidos
            .FirstOrDefaultAsync(u => u.Id == userId);
        return extendido?.ClienteId;
    }

    public async Task<List<Guid>> GetAccessibleClienteIdsAsync(string userId, string userRole)
    {
        if (IsRolSistema(userRole))
        {
            return await _context.Clientes
                .Where(c => c.Activo)
                .Select(c => c.Id)
                .ToListAsync();
        }

        if (IsRolProveedor(userRole))
        {
            var proveedorId = await GetUsuarioProveedorIdAsync(userId);
            if (proveedorId == null) return [];

            return await _context.Clientes
                .Where(c => c.ProveedorId == proveedorId && c.Activo)
                .Select(c => c.Id)
                .ToListAsync();
        }

        if (IsRolCliente(userRole))
        {
            var clienteId = await GetUsuarioClienteIdAsync(userId);
            return clienteId != null ? [clienteId.Value] : [];
        }

        return [];
    }

    public async Task<List<Guid>> GetAccessibleProveedorIdsAsync(string userId, string userRole)
    {
        if (IsRolSistema(userRole))
        {
            return await _context.Proveedores
                .Where(p => p.Activo)
                .Select(p => p.Id)
                .ToListAsync();
        }

        if (IsRolProveedor(userRole))
        {
            var proveedorId = await GetUsuarioProveedorIdAsync(userId);
            return proveedorId != null ? [proveedorId.Value] : [];
        }

        return [];
    }

    public async Task<bool> IsInProveedorScopeAsync(string userId, string userRole, Guid proveedorId)
    {
        if (IsRolSistema(userRole)) return true;
        var accessible = await GetAccessibleProveedorIdsAsync(userId, userRole);
        return accessible.Contains(proveedorId);
    }

    public async Task<bool> IsInClienteScopeAsync(string userId, string userRole, Guid clienteId)
    {
        if (IsRolSistema(userRole)) return true;
        var accessible = await GetAccessibleClienteIdsAsync(userId, userRole);
        return accessible.Contains(clienteId);
    }

    public string GetUserRole(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role) ?? "Asistente del Cliente";
    }

    public string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}

using System.Security.Claims;
using HorusEye.Api.DTOs;
using HorusEye.Api.Models;
using HorusEye.Api.Services;
using HorusEye.Core.Entities;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TokenService _tokenService;
    private readonly PermisoService _permisoService;
    private readonly HorusEyeDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        TokenService tokenService,
        PermisoService permisoService,
        HorusEyeDbContext context,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _permisoService = permisoService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterRequest request)
    {
        var creatorRole = _permisoService.GetUserRole(User);
        var creatorId = _permisoService.GetUserId(User);

        if (!_permisoService.CanCreateRole(creatorRole, request.Role))
            return Forbid();

        if (_permisoService.NeedsProveedorAssociation(request.Role))
        {
            if (request.ProveedorId == null)
                return BadRequest(ApiResponse<object>.Fail("ProveedorId es requerido para este rol"));

            if (!await _permisoService.IsInProveedorScopeAsync(creatorId, creatorRole, request.ProveedorId.Value))
                return Forbid();
        }

        if (_permisoService.NeedsClienteAssociation(request.Role))
        {
            if (request.ClienteId == null)
                return BadRequest(ApiResponse<object>.Fail("ClienteId es requerido para este rol"));

            if (!await _permisoService.IsInClienteScopeAsync(creatorId, creatorRole, request.ClienteId.Value))
                return Forbid();
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest(ApiResponse<object>.Fail("El email ya esta registrado"));

        var user = new IdentityUser
        {
            UserName = request.UserName,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<object>.Fail(
                "Error al crear el usuario",
                result.Errors.Select(e => e.Description).ToArray()));

        if (!await _roleManager.RoleExistsAsync(request.Role))
            return BadRequest(ApiResponse<object>.Fail($"El rol '{request.Role}' no existe"));

        await _userManager.AddToRoleAsync(user, request.Role);

        var usuarioExtendido = new UsuarioExtendido
        {
            Id = user.Id,
            ProveedorId = request.ProveedorId,
            ClienteId = request.ClienteId,
            CreadoPorUserId = creatorId
        };
        _context.UsuariosExtendidos.Add(usuarioExtendido);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {Email} registrado con rol {Role}", request.Email, request.Role);

        return Ok(ApiResponse<object>.Ok(new { user.Id, user.Email, user.UserName, Role = request.Role },
            "Usuario registrado exitosamente"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Intento de login fallido para {Email}", request.Email);
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Credenciales invalidas"));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Asistente del Cliente";

        var (proveedorId, clienteId) = await _tokenService.GetUserAssociationsAsync(user.Id);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, role, proveedorId, clienteId);
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

        _logger.LogInformation("Login exitoso para {Email} con rol {Role}", request.Email, role);

        return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            UserId = user.Id,
            Email = user.Email!,
            Role = role,
            UserName = user.UserName ?? string.Empty,
            ProveedorId = proveedorId,
            ClienteId = clienteId
        }, "Login exitoso"));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var existingRefreshToken = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (existingRefreshToken == null)
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Refresh Token invalido o expirado"));

        var user = await _userManager.FindByIdAsync(existingRefreshToken.UserId);
        if (user == null)
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Usuario no encontrado"));

        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Asistente del Cliente";

        var (proveedorId, clienteId) = await _tokenService.GetUserAssociationsAsync(user.Id);

        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, role, proveedorId, clienteId);
        var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

        return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            UserId = user.Id,
            Email = user.Email!,
            Role = role,
            UserName = user.UserName ?? string.Empty,
            ProveedorId = proveedorId,
            ClienteId = clienteId
        }, "Token renovado exitosamente"));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null)
            return BadRequest(ApiResponse<object>.Fail("Usuario no encontrado"));

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<object>.Fail(
                "Error al cambiar la contrasena",
                result.Errors.Select(e => e.Description).ToArray()));

        _logger.LogInformation("Contrasena cambiada para {Email}", user.Email);

        return Ok(ApiResponse<object>.Ok(null!, "Contrasena cambiada exitosamente"));
    }

    [HttpPost("recover-password")]
    public async Task<ActionResult<ApiResponse<object>>> RecoverPassword([FromBody] RecoverPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Ok(ApiResponse<object>.Ok(null!,
                "Si el email existe, recibiras instrucciones para recuperar tu contrasena"));

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        _logger.LogInformation("Solicitud de recuperacion de contrasena para {Email}", request.Email);

        return Ok(ApiResponse<object>.Ok(new { ResetToken = token, Email = request.Email },
            "Token de recuperacion generado. En produccion, enviar por email."));
    }

    [HttpGet("users")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> GetUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userRole = _permisoService.GetUserRole(User);
        var userId = _permisoService.GetUserId(User);

        var query = _userManager.Users.AsQueryable();

        if (_permisoService.IsRolCliente(userRole))
        {
            var clienteId = await _permisoService.GetUsuarioClienteIdAsync(userId);
            if (clienteId != null)
            {
                var clienteUserIds = await _context.UsuariosExtendidos
                    .Where(u => u.ClienteId == clienteId)
                    .Select(u => u.Id)
                    .ToListAsync();
                query = query.Where(u => clienteUserIds.Contains(u.Id));
            }
        }
        else if (_permisoService.IsRolProveedor(userRole))
        {
            var proveedorId = await _permisoService.GetUsuarioProveedorIdAsync(userId);
            if (proveedorId != null)
            {
                var proveedorUserIds = await _context.UsuariosExtendidos
                    .Where(u => u.ProveedorId == proveedorId || u.ClienteId != null)
                    .Select(u => u.Id)
                    .ToListAsync();
                query = query.Where(u => proveedorUserIds.Contains(u.Id));
            }
        }

        query = query.OrderBy(u => u.UserName);

        var total = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var usersData = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var extendido = await _context.UsuariosExtendidos
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            usersData.Add(new
            {
                user.Id,
                user.Email,
                user.UserName,
                Roles = roles,
                extendido?.ProveedorId,
                extendido?.ClienteId,
                extendido?.CreadoPorUserId
            });
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            Items = usersData,
            Total = total,
            Page = page,
            PageSize = pageSize
        }));
    }

    [HttpPut("users/{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        var creatorRole = _permisoService.GetUserRole(User);
        var creatorId = _permisoService.GetUserId(User);

        if (!_permisoService.CanCreateRole(creatorRole, request.Role))
            return Forbid();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("Usuario no encontrado"));

        user.UserName = request.UserName;
        user.Email = request.Email;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return BadRequest(ApiResponse<object>.Fail(
                "Error al actualizar usuario",
                updateResult.Errors.Select(e => e.Description).ToArray()));

        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault();

        if (currentRole != request.Role)
        {
            if (currentRole != null)
                await _userManager.RemoveFromRoleAsync(user, currentRole);
            await _userManager.AddToRoleAsync(user, request.Role);
        }

        var extendido = await _context.UsuariosExtendidos
            .FirstOrDefaultAsync(u => u.Id == id);

        if (extendido != null)
        {
            extendido.ProveedorId = request.ProveedorId;
            extendido.ClienteId = request.ClienteId;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Usuario {Id} actualizado por {Admin}", id, User.Identity?.Name);

        return Ok(ApiResponse<object>.Ok(new { user.Id, user.Email, user.UserName, Role = request.Role },
            "Usuario actualizado exitosamente"));
    }

    [HttpDelete("users/{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == id)
            return BadRequest(ApiResponse<object>.Fail("No puedes eliminar tu propio usuario"));

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("Usuario no encontrado"));

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<object>.Fail(
                "Error al eliminar usuario",
                result.Errors.Select(e => e.Description).ToArray()));

        var extendido = await _context.UsuariosExtendidos
            .FirstOrDefaultAsync(u => u.Id == id);
        if (extendido != null)
        {
            _context.UsuariosExtendidos.Remove(extendido);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Usuario {Id} eliminado por {Admin}", id, User.Identity?.Name);

        return Ok(ApiResponse<object>.Ok(null!, "Usuario eliminado exitosamente"));
    }

    [HttpPost("users/{id}/reset-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("Usuario no encontrado"));

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<object>.Fail(
                "Error al restablecer contrasena",
                result.Errors.Select(e => e.Description).ToArray()));

        _logger.LogInformation("Contrasena restablecida para usuario {Id} por {Admin}", id, User.Identity?.Name);

        return Ok(ApiResponse<object>.Ok(null!, "Contrasena restablecida exitosamente"));
    }
}

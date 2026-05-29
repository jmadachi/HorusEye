using System.Security.Claims;
using HorusEye.Api.DTOs;
using HorusEye.Api.Models;
using HorusEye.Api.Services;
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
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        TokenService tokenService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    [Authorize(Roles = "Usuario de Gestión")]
    public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest(ApiResponse<object>.Fail("El email ya está registrado"));

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
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Credenciales inválidas"));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Usuario de Consulta";

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, role);
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
            UserName = user.UserName ?? string.Empty
        }, "Login exitoso"));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var existingRefreshToken = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (existingRefreshToken == null)
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Refresh Token inválido o expirado"));

        var user = await _userManager.FindByIdAsync(existingRefreshToken.UserId);
        if (user == null)
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Usuario no encontrado"));

        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Usuario de Consulta";

        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, role);
        var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);

        return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            UserId = user.Id,
            Email = user.Email!,
            Role = role,
            UserName = user.UserName ?? string.Empty
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
                "Error al cambiar la contraseña",
                result.Errors.Select(e => e.Description).ToArray()));

        _logger.LogInformation("Contraseña cambiada para {Email}", user.Email);

        return Ok(ApiResponse<object>.Ok(null!, "Contraseña cambiada exitosamente"));
    }

    [HttpPost("recover-password")]
    public async Task<ActionResult<ApiResponse<object>>> RecoverPassword([FromBody] RecoverPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Ok(ApiResponse<object>.Ok(null!,
                "Si el email existe, recibirás instrucciones para recuperar tu contraseña"));

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        _logger.LogInformation("Solicitud de recuperación de contraseña para {Email}", request.Email);

        return Ok(ApiResponse<object>.Ok(new { ResetToken = token, Email = request.Email },
            "Token de recuperación generado. En producción, enviar por email."));
    }

    [HttpGet("users")]
    [Authorize(Roles = "Usuario de Gestión")]
    public async Task<ActionResult<ApiResponse<object>>> GetUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _userManager.Users.OrderBy(u => u.UserName);

        var total = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var usersData = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            usersData.Add(new
            {
                user.Id,
                user.Email,
                user.UserName,
                Roles = roles
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
    [Authorize(Roles = "Usuario de Gestión")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
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

        var roles = await _userManager.GetRolesAsync(user);
        var currentRole = roles.FirstOrDefault();

        if (currentRole != request.Role)
        {
            if (currentRole != null)
                await _userManager.RemoveFromRoleAsync(user, currentRole);
            await _userManager.AddToRoleAsync(user, request.Role);
        }

        _logger.LogInformation("Usuario {Id} actualizado por {Admin}", id, User.Identity?.Name);

        return Ok(ApiResponse<object>.Ok(new { user.Id, user.Email, user.UserName, Role = request.Role },
            "Usuario actualizado exitosamente"));
    }

    [HttpDelete("users/{id}")]
    [Authorize(Roles = "Usuario de Gestión")]
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

        _logger.LogInformation("Usuario {Id} eliminado por {Admin}", id, User.Identity?.Name);

        return Ok(ApiResponse<object>.Ok(null!, "Usuario eliminado exitosamente"));
    }

    [HttpPost("users/{id}/reset-password")]
    [Authorize(Roles = "Usuario de Gestión")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<object>.Fail("Usuario no encontrado"));

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<object>.Fail(
                "Error al restablecer contraseña",
                result.Errors.Select(e => e.Description).ToArray()));

        _logger.LogInformation("Contraseña restablecida para usuario {Id} por {Admin}", id, User.Identity?.Name);

        return Ok(ApiResponse<object>.Ok(null!, "Contraseña restablecida exitosamente"));
    }
}

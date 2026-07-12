using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HorusEye.Core.Entities;
using HorusEye.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HorusEye.Api.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;
    private readonly HorusEyeDbContext _context;

    public TokenService(IConfiguration configuration, HorusEyeDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public string GenerateAccessToken(string userId, string email, string role,
        Guid? proveedorId = null, Guid? clienteId = null)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (proveedorId.HasValue)
            claims.Add(new Claim("proveedor_id", proveedorId.Value.ToString()));

        if (clienteId.HasValue)
            claims.Add(new Claim("cliente_id", clienteId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(string userId)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateRefreshToken(),
            FechaExpiracion = DateTimeOffset.UtcNow.AddDays(7),
            FechaCreacion = DateTimeOffset.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token
                && !rt.Revocado
                && rt.FechaExpiracion > DateTimeOffset.UtcNow);
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken != null)
        {
            refreshToken.Revocado = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(Guid? proveedorId, Guid? clienteId)> GetUserAssociationsAsync(string userId)
    {
        var extendido = await _context.UsuariosExtendidos
            .FirstOrDefaultAsync(u => u.Id == userId);
        return (extendido?.ProveedorId, extendido?.ClienteId);
    }
}

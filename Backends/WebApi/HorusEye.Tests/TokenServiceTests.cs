using FluentAssertions;
using HorusEye.Api.Services;
using HorusEye.Core.Entities;
using HorusEye.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HorusEye.Tests;

public class TokenServiceTests
{
    private readonly IConfiguration _config;
    private readonly HorusEyeDbContext _context;

    public TokenServiceTests()
    {
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestKeyThatIsAtLeast32CharactersLong!!!!!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })!
            .Build();
        var options = new DbContextOptionsBuilder<HorusEyeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new HorusEyeDbContext(options);
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwt()
    {
        var service = new TokenService(_config, _context);
        var token = service.GenerateAccessToken("user-1", "test@test.com", "Usuario de Gestión");
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64String()
    {
        var service = new TokenService(_config, _context);
        var token = service.GenerateRefreshToken();
        token.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(token).Should().HaveCount(64);
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_PersistsToken()
    {
        var service = new TokenService(_config, _context);
        var result = await service.CreateRefreshTokenAsync("user-1");
        result.Should().NotBeNull();
        result.UserId.Should().Be("user-1");
        result.Revocado.Should().BeFalse();
        result.FechaExpiracion.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
        _context.RefreshTokens.Count().Should().Be(1);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ValidToken_ReturnsToken()
    {
        var service = new TokenService(_config, _context);
        var created = await service.CreateRefreshTokenAsync("user-1");
        var validated = await service.ValidateRefreshTokenAsync(created.Token);
        validated.Should().NotBeNull();
        validated!.Token.Should().Be(created.Token);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_RevokedToken_ReturnsNull()
    {
        var service = new TokenService(_config, _context);
        var created = await service.CreateRefreshTokenAsync("user-1");
        await service.RevokeRefreshTokenAsync(created.Token);
        var validated = await service.ValidateRefreshTokenAsync(created.Token);
        validated.Should().BeNull();
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ExpiredToken_ReturnsNull()
    {
        var expired = new RefreshToken
        {
            UserId = "user-1",
            Token = "expired-token-value",
            FechaExpiracion = DateTimeOffset.UtcNow.AddHours(-1),
            FechaCreacion = DateTimeOffset.UtcNow.AddDays(-8)
        };
        _context.RefreshTokens.Add(expired);
        await _context.SaveChangesAsync();
        var service = new TokenService(_config, _context);
        var validated = await service.ValidateRefreshTokenAsync("expired-token-value");
        validated.Should().BeNull();
    }
}

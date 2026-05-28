namespace HorusEye.Core.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset FechaExpiracion { get; set; }
    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
    public bool Revocado { get; set; }
}

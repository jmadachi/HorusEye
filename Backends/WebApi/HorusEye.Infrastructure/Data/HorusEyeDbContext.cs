using HorusEye.Core.Entities;
using HorusEye.Core.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HorusEye.Infrastructure.Data;

public class HorusEyeDbContext : IdentityDbContext
{
    public HorusEyeDbContext(DbContextOptions<HorusEyeDbContext> options) : base(options) { }

    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TagDanioHistorial> TagsDaniosHistorial => Set<TagDanioHistorial>();
    public DbSet<Activo> Activos => Set<Activo>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<AutorizacionSalida> AutorizacionesSalida => Set<AutorizacionSalida>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(200);
            entity.Property(e => e.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);
            entity.HasMany(e => e.DaniosHistorial)
                .WithOne(e => e.Tag)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TagDanioHistorial>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Descripcion).HasMaxLength(600);
            entity.Property(e => e.ReportadoPor).HasMaxLength(300);
        });

        builder.Entity<Activo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Placa).HasMaxLength(100);
            entity.HasIndex(e => e.Placa).IsUnique();
            entity.Property(e => e.Nombre).HasMaxLength(300);
            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.TenedorResponsable).HasMaxLength(300);
            entity.Property(e => e.EstadoUbicacion)
                .HasConversion<string>()
                .HasMaxLength(30);
            entity.HasOne(e => e.Tag)
                .WithOne(e => e.Activo)
                .HasForeignKey<Activo>(e => e.TagId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasMany(e => e.Movimientos)
                .WithOne(e => e.Activo)
                .HasForeignKey(e => e.ActivoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.AutorizacionesSalida)
                .WithOne(e => e.Activo)
                .HasForeignKey(e => e.ActivoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Movimiento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PuntoLecturaId).HasMaxLength(100);
            entity.Property(e => e.TipoMovimiento)
                .HasConversion<string>()
                .HasMaxLength(10);
            entity.HasIndex(e => e.FechaRegistro);
        });

        builder.Entity<AutorizacionSalida>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.AutorizadoPor).HasMaxLength(300);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(450);
        });
    }
}

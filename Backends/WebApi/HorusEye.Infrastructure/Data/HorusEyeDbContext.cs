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

    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<UsuarioExtendido> UsuariosExtendidos => Set<UsuarioExtendido>();
    public DbSet<DispositivoRfid> DispositivosRfid => Set<DispositivoRfid>();
    public DbSet<NodoUbicacion> NodosUbicacion => Set<NodoUbicacion>();
    public DbSet<FabricanteDispositivo> FabricantesDispositivo => Set<FabricanteDispositivo>();
    public DbSet<CampoPayloadFabricante> CamposPayloadFabricante => Set<CampoPayloadFabricante>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<EventoLector> EventosLectores => Set<EventoLector>();

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
            entity.HasIndex(e => e.ClienteId);
            entity.HasOne(e => e.Tag)
                .WithOne(e => e.Activo)
                .HasForeignKey<Activo>(e => e.TagId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
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
            entity.HasOne(e => e.DispositivoRfid)
                .WithMany(d => d.Movimientos)
                .HasForeignKey(e => e.DispositivoRfidId)
                .OnDelete(DeleteBehavior.SetNull);
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

        // === Nuevas entidades ===

        builder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.RazonSocial).HasMaxLength(300);
            entity.Property(e => e.RUC).HasMaxLength(13);
            entity.HasIndex(e => e.RUC).IsUnique();
            entity.Property(e => e.Direccion).HasMaxLength(300);
            entity.Property(e => e.Telefono).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(200);
        });

        builder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.RazonSocial).HasMaxLength(300);
            entity.Property(e => e.RUC).HasMaxLength(13);
            entity.Property(e => e.Direccion).HasMaxLength(300);
            entity.Property(e => e.Telefono).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.HasOne(e => e.Proveedor)
                .WithMany(p => p.Clientes)
                .HasForeignKey(e => e.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<UsuarioExtendido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(450);
            entity.HasIndex(e => e.ProveedorId);
            entity.HasIndex(e => e.ClienteId);
            entity.HasOne(e => e.Proveedor)
                .WithMany()
                .HasForeignKey(e => e.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<DispositivoRfid>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Fabricante).HasMaxLength(100);
            entity.Property(e => e.Modelo).HasMaxLength(100);
            entity.Property(e => e.DireccionIP).HasMaxLength(45);
            entity.Property(e => e.Ubicacion).HasMaxLength(300);
            entity.Property(e => e.TipoDispositivo)
                .HasConversion<string>()
                .HasMaxLength(20);
            entity.Property(e => e.EndpointAPI).HasMaxLength(500);
            entity.Property(e => e.MetodoHTTP).HasMaxLength(10);
            entity.Property(e => e.DireccionPredeterminada).HasMaxLength(20);
            entity.Property(e => e.ApiKey).HasMaxLength(200);
            entity.HasIndex(e => e.ApiKey);
            entity.HasIndex(e => e.DireccionIP);
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Dispositivos)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Proveedor)
                .WithMany(p => p.Dispositivos)
                .HasForeignKey(e => e.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.NodoUbicacion)
                .WithMany(n => n.Dispositivos)
                .HasForeignKey(e => e.NodoUbicacionId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.FabricanteDispositivo)
                .WithMany(f => f.DispositivosRfid)
                .HasForeignKey(e => e.FabricanteDispositivoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<NodoUbicacion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.TipoNodo).HasMaxLength(100);
            entity.HasIndex(e => new { e.ClienteId, e.Nombre });
            entity.HasOne(e => e.Padre)
                .WithMany(n => n.Hijos)
                .HasForeignKey(e => e.PadreId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.NodosUbicacion)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FabricanteDispositivo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.UrlDocumentacion).HasMaxLength(500);
            entity.Property(e => e.EndpointEjemplo).HasMaxLength(2000);
        });

        builder.Entity<CampoPayloadFabricante>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreCampoExterno).HasMaxLength(100);
            entity.Property(e => e.NombreCampoInterno).HasMaxLength(100);
            entity.Property(e => e.TipoDato).HasMaxLength(50);
            entity.Property(e => e.ValorDefecto).HasMaxLength(500);
            entity.HasOne(e => e.FabricanteDispositivo)
                .WithMany(f => f.CamposPayload)
                .HasForeignKey(e => e.FabricanteDispositivoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.FabricanteDispositivoId, e.NombreCampoExterno }).IsUnique();
        });

        builder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.HasIndex(e => new { e.NodoUbicacionId, e.Nombre }).IsUnique();
            entity.HasOne(e => e.NodoUbicacion)
                .WithMany()
                .HasForeignKey(e => e.NodoUbicacionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EventoLector>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EventoId, e.Orden }).IsUnique();
            entity.HasOne(e => e.Evento)
                .WithMany(ev => ev.Lectores)
                .HasForeignKey(e => e.EventoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.DispositivoRfid)
                .WithMany()
                .HasForeignKey(e => e.DispositivoRfidId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

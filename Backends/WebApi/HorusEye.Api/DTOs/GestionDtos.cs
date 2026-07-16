using System.ComponentModel.DataAnnotations;

namespace HorusEye.Api.DTOs;

// === Proveedor ===

public class ProveedorRequest
{
    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(300)]
    public string RazonSocial { get; set; } = string.Empty;
    [Required, StringLength(13, MinimumLength = 10)]
    public string RUC { get; set; } = string.Empty;
    [StringLength(300)]
    public string? Direccion { get; set; }
    [StringLength(50)]
    public string? Telefono { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
}

public class ProveedorResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string RUC { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }
    public DateTimeOffset FechaRegistro { get; set; }
}

// === Cliente ===

public class ClienteRequest
{
    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(300)]
    public string RazonSocial { get; set; } = string.Empty;
    [Required, StringLength(13, MinimumLength = 10)]
    public string RUC { get; set; } = string.Empty;
    [StringLength(300)]
    public string? Direccion { get; set; }
    [StringLength(50)]
    public string? Telefono { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public Guid? ProveedorId { get; set; }
}

public class ClienteResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string RUC { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public Guid? ProveedorId { get; set; }
    public string? ProveedorNombre { get; set; }
    public bool Activo { get; set; }
    public DateTimeOffset FechaRegistro { get; set; }
}

// === DispositivoRfid ===

public class DispositivoRequest
{
    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string Fabricante { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string Modelo { get; set; } = string.Empty;
    [StringLength(45)]
    public string? DireccionIP { get; set; }
    public int? Puerto { get; set; }
    [StringLength(300)]
    public string? Ubicacion { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? ProveedorId { get; set; }
    public Guid? NodoUbicacionId { get; set; }
    [Required]
    public string TipoDispositivo { get; set; } = "FIXED";
    [StringLength(500)]
    public string? EndpointAPI { get; set; }
    [StringLength(10)]
    public string? MetodoHTTP { get; set; }
    public Guid? FabricanteDispositivoId { get; set; }
    [StringLength(20)]
    public string? DireccionPredeterminada { get; set; }
    [StringLength(200)]
    public string? ApiKey { get; set; }
    public bool RequiereDireccion { get; set; }
}

public class DispositivoResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string? DireccionIP { get; set; }
    public int? Puerto { get; set; }
    public string? Ubicacion { get; set; }
    public Guid? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public Guid? ProveedorId { get; set; }
    public string? ProveedorNombre { get; set; }
    public Guid? NodoUbicacionId { get; set; }
    public string? NodoUbicacionNombre { get; set; }
    public string TipoDispositivo { get; set; } = "FIXED";
    public string? EndpointAPI { get; set; }
    public string? MetodoHTTP { get; set; }
    public Guid? FabricanteDispositivoId { get; set; }
    public string? DireccionPredeterminada { get; set; }
    public string? ApiKey { get; set; }
    public bool RequiereDireccion { get; set; }
    public bool Activo { get; set; }
    public DateTimeOffset FechaRegistro { get; set; }
}

// === NodoUbicacion ===

public class NodoUbicacionRequest
{
    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string TipoNodo { get; set; } = string.Empty;
    public Guid? PadreId { get; set; }
    [Required]
    public Guid ClienteId { get; set; }
    public string? Metadata { get; set; }
}

public class NodoUbicacionResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoNodo { get; set; } = string.Empty;
    public Guid? PadreId { get; set; }
    public Guid ClienteId { get; set; }
    public int Nivel { get; set; }
    public string? Metadata { get; set; }
    public bool Activo { get; set; }
}

// === FabricanteDispositivo ===

public class FabricanteRequest
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Descripcion { get; set; }
    [StringLength(500)]
    public string? UrlDocumentacion { get; set; }
    [StringLength(2000)]
    public string? EndpointEjemplo { get; set; }
}

public class FabricanteResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? UrlDocumentacion { get; set; }
    public string? EndpointEjemplo { get; set; }
    public bool Activo { get; set; }
    public DateTimeOffset FechaRegistro { get; set; }
    public List<CampoPayloadResponse> Campos { get; set; } = new();
}

// === CampoPayloadFabricante ===

public class CampoPayloadRequest
{
    [Required, StringLength(100)]
    public string NombreCampoExterno { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string NombreCampoInterno { get; set; } = string.Empty;
    [Required, StringLength(50)]
    public string TipoDato { get; set; } = "string";
    public bool Requerido { get; set; }
    [StringLength(500)]
    public string? ValorDefecto { get; set; }
    public int OrdenExtraccion { get; set; }
}

public class CampoPayloadResponse
{
    public Guid Id { get; set; }
    public string NombreCampoExterno { get; set; } = string.Empty;
    public string NombreCampoInterno { get; set; } = string.Empty;
    public string TipoDato { get; set; } = "string";
    public bool Requerido { get; set; }
    public string? ValorDefecto { get; set; }
    public int OrdenExtraccion { get; set; }
}

// === Evento ===

public class EventoRequest
{
    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;
    [Required]
    public Guid NodoUbicacionId { get; set; }
    [Required]
    public Guid ClienteId { get; set; }
    public List<EventoLectorRequest> Lectores { get; set; } = new();
}

public class EventoLectorRequest
{
    [Required]
    public Guid DispositivoRfidId { get; set; }
    public int Orden { get; set; }
}

public class EventoResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid NodoUbicacionId { get; set; }
    public string? NodoUbicacionNombre { get; set; }
    public Guid ClienteId { get; set; }
    public bool Activo { get; set; }
    public List<EventoLectorResponse> Lectores { get; set; } = new();
}

public class EventoLectorResponse
{
    public Guid Id { get; set; }
    public Guid DispositivoRfidId { get; set; }
    public string? DispositivoNombre { get; set; }
    public int Orden { get; set; }
}

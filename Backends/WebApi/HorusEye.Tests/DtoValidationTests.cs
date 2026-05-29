using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using HorusEye.Api.DTOs;

namespace HorusEye.Tests;

public class DtoValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }

    [Fact]
    public void CreateAutorizacionRequest_Valid_Passes()
    {
        var dto = new CreateAutorizacionRequest
        {
            ActivoId = Guid.NewGuid(),
            AutorizadoPor = "Juan Pérez"
        };
        var errors = ValidateModel(dto);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void CreateAutorizacionRequest_EmptyAutorizadoPor_Fails()
    {
        var dto = new CreateAutorizacionRequest
        {
            ActivoId = Guid.NewGuid(),
            AutorizadoPor = ""
        };
        var errors = ValidateModel(dto);
        errors.Should().Contain(e => e.MemberNames.Contains(nameof(CreateAutorizacionRequest.AutorizadoPor)));
    }

    [Fact]
    public void ActivoRequest_InvalidCategoria_Fails()
    {
        var dto = new ActivoRequest
        {
            Placa = "TEST-001",
            Nombre = "Test",
            Categoria = "CategoriaInvalida"
        };
        var errors = ValidateModel(dto);
        errors.Should().Contain(e => e.MemberNames.Contains(nameof(ActivoRequest.Categoria)));
    }

    [Fact]
    public void ActivoRequest_ValidCategoria_Passes()
    {
        var dto = new ActivoRequest
        {
            Placa = "TEST-001",
            Nombre = "Test",
            Categoria = "Computadores"
        };
        var errors = ValidateModel(dto);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void EventoRfidRequest_InvalidTipoMovimiento_Fails()
    {
        var dto = new EventoRfidRequest
        {
            TagId = "TAG-001",
            TipoMovimiento = "INVALIDO"
        };
        var errors = ValidateModel(dto);
        errors.Should().Contain(e => e.MemberNames.Contains(nameof(EventoRfidRequest.TipoMovimiento)));
    }

    [Fact]
    public void EventoRfidRequest_ValidTipoMovimiento_Passes()
    {
        var dto = new EventoRfidRequest
        {
            TagId = "TAG-001",
            TipoMovimiento = "INGRESO"
        };
        var errors = ValidateModel(dto);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void UpdateTagEstadoRequest_ValidEstado_Passes()
    {
        var dto = new UpdateTagEstadoRequest { Estado = "DISPONIBLE" };
        var errors = ValidateModel(dto);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void UpdateTagEstadoRequest_InvalidEstado_Fails()
    {
        var dto = new UpdateTagEstadoRequest { Estado = "INEXISTENTE" };
        var errors = ValidateModel(dto);
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void RegisterRequest_InvalidEmail_Fails()
    {
        var dto = new RegisterRequest
        {
            Email = "no-email",
            Password = "Valid1!",
            UserName = "Test",
            Role = "Usuario de Consulta"
        };
        var errors = ValidateModel(dto);
        errors.Should().Contain(e => e.MemberNames.Contains(nameof(RegisterRequest.Email)));
    }

    [Fact]
    public void RegisterRequest_ShortPassword_Fails()
    {
        var dto = new RegisterRequest
        {
            Email = "test@test.com",
            Password = "Ab1",
            UserName = "Test",
            Role = "Usuario de Consulta"
        };
        var errors = ValidateModel(dto);
        errors.Should().Contain(e => e.MemberNames.Contains(nameof(RegisterRequest.Password)));
    }

    [Fact]
    public void ResetPasswordRequest_ShortPassword_Fails()
    {
        var dto = new ResetPasswordRequest { NewPassword = "Ab1" };
        var errors = ValidateModel(dto);
        errors.Should().Contain(e => e.MemberNames.Contains(nameof(ResetPasswordRequest.NewPassword)));
    }

    [Fact]
    public void ResetPasswordRequest_ValidPassword_Passes()
    {
        var dto = new ResetPasswordRequest { NewPassword = "NewPassword123!" };
        var errors = ValidateModel(dto);
        errors.Should().BeEmpty();
    }
}

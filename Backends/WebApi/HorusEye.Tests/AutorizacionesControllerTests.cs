using System.Security.Claims;
using FluentAssertions;
using HorusEye.Api.Controllers;
using HorusEye.Api.DTOs;
using HorusEye.Api.Models;
using HorusEye.Core.Entities;
using HorusEye.Core.Enums;
using HorusEye.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HorusEye.Tests;

public class AutorizacionesControllerTests
{
    private readonly HorusEyeDbContext _context;
    private readonly Mock<ILogger<AutorizacionesController>> _loggerMock;
    private readonly AutorizacionesController _controller;

    public AutorizacionesControllerTests()
    {
        var options = new DbContextOptionsBuilder<HorusEyeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new HorusEyeDbContext(options);
        _loggerMock = new Mock<ILogger<AutorizacionesController>>();
        _controller = new AutorizacionesController(_context, _loggerMock.Object);
        SetupUser();
        SeedData();
    }

    private void SetupUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Role, "Administrador del Sistema")
        }));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    private void SeedData()
    {
        var tag = new Tag
        {
            Id = "TAG-001",
            Estado = EstadoTag.DISPONIBLE,
            FechaRegistro = DateTimeOffset.UtcNow
        };
        _context.Tags.Add(tag);
        _context.Activos.Add(new Activo
        {
            Id = Guid.NewGuid(),
            Placa = "NOTE-001",
            Nombre = "Test Notebook",
            Categoria = "Computadores",
            EstadoUbicacion = EstadoUbicacion.DENTRO_INSTALACIONES,
            TagId = tag.Id,
            FechaRegistro = DateTimeOffset.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAll_ReturnsList()
    {
        var result = await _controller.GetAll();
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var response = ok!.Value as ApiResponse<object>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        var activoId = _context.Activos.First().Id;
        var request = new CreateAutorizacionRequest
        {
            ActivoId = activoId,
            AutorizadoPor = "Test User"
        };
        var result = await _controller.Create(request);
        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        var response = ok!.Value as ApiResponse<AutorizacionResponse>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.AutorizadoPor.Should().Be("Test User");
        response.Data.Activa.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithInvalidActivo_ReturnsNotFound()
    {
        var request = new CreateAutorizacionRequest
        {
            ActivoId = Guid.NewGuid(),
            AutorizadoPor = "Test User"
        };
        var result = await _controller.Create(request);
        var notFound = result.Result as NotFoundObjectResult;
        notFound.Should().NotBeNull();
    }

    [Fact]
    public async Task Revoke_SetsActivaToFalse()
    {
        var activoId = _context.Activos.First().Id;
        var createReq = new CreateAutorizacionRequest
        {
            ActivoId = activoId,
            AutorizadoPor = "Test User"
        };
        var createResult = await _controller.Create(createReq);
        var ok = createResult.Result as OkObjectResult;
        var response = ok!.Value as ApiResponse<AutorizacionResponse>;
        var id = response!.Data.Id;
        var revokeResult = await _controller.Revocar(id);
        var revokedOk = revokeResult.Result as OkObjectResult;
        revokedOk.Should().NotBeNull();
        _context.AutorizacionesSalida.Find(id)!.Activa.Should().BeFalse();
    }

    [Fact]
    public async Task Revoke_WithInvalidId_ReturnsNotFound()
    {
        var result = await _controller.Revocar(999);
        var notFound = result.Result as NotFoundObjectResult;
        notFound.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_RemovesAutorizacion()
    {
        var activoId = _context.Activos.First().Id;
        var createReq = new CreateAutorizacionRequest
        {
            ActivoId = activoId,
            AutorizadoPor = "Test User"
        };
        var createResult = await _controller.Create(createReq);
        var ok = createResult.Result as OkObjectResult;
        var response = ok!.Value as ApiResponse<AutorizacionResponse>;
        var id = response!.Data.Id;
        var deleteResult = await _controller.Delete(id);
        var deletedOk = deleteResult.Result as OkObjectResult;
        deletedOk.Should().NotBeNull();
        _context.AutorizacionesSalida.Find(id).Should().BeNull();
    }

    [Fact]
    public async Task GetActivas_ReturnsOnlyActive()
    {
        var activoId = _context.Activos.First().Id;
        await _controller.Create(new CreateAutorizacionRequest
        {
            ActivoId = activoId,
            AutorizadoPor = "User A"
        });
        var create2 = await _controller.Create(new CreateAutorizacionRequest
        {
            ActivoId = activoId,
            AutorizadoPor = "User B"
        });
        var ok = create2.Result as OkObjectResult;
        var response = ok!.Value as ApiResponse<AutorizacionResponse>;
        await _controller.Revocar(response!.Data.Id);
        var activas = await _controller.GetActivas();
        var activasOk = activas.Result as OkObjectResult;
        var activasResponse = activasOk!.Value as ApiResponse<List<AutorizacionResponse>>;
        activasResponse!.Data.Should().HaveCount(1);
    }
}

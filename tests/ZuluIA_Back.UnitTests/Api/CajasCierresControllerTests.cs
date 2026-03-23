using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cajas.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class CajasCierresControllerTests
{
    [Fact]
    public async Task GetCierreCajaById_CuandoNoExiste_DevuelveNotFoundConError()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cierres = MockDbSetHelper.CreateMockDbSet<CierreCaja>();
        cierres.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<CierreCaja?>((CierreCaja?)null));
        db.CierresCaja.Returns(cierres);
        var controller = CreateController(mediator, db);

        var result = await controller.GetCierreCajaById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Cierre de caja 99 no encontrado");
    }

    [Fact]
    public async Task GetCierreCajaById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cierre = BuildCierreCaja(7, new DateTimeOffset(2026, 3, 20, 8, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 3, 20, 18, 0, 0, TimeSpan.Zero), 5, 10);
        var cierres = MockDbSetHelper.CreateMockDbSet(new[] { cierre });
        cierres.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<CierreCaja?>(cierre));
        db.CierresCaja.Returns(cierres);
        var controller = CreateController(mediator, db);

        var result = await controller.GetCierreCajaById(7, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(cierre);
    }

    [Fact]
    public async Task CreateCierreCaja_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateCierreCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El número de cierre debe ser mayor a cero."));

        var result = await controller.CreateCierreCaja(
            new RegistrarCierreRequest(new DateTimeOffset(2026, 3, 20, 8, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 3, 20, 18, 0, 0, TimeSpan.Zero), 5, 0),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateCierreCaja_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateCierreCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));

        var result = await controller.CreateCierreCaja(
            new RegistrarCierreRequest(new DateTimeOffset(2026, 3, 20, 8, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 3, 20, 18, 0, 0, TimeSpan.Zero), 5, 10),
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetCierreCajaById");
        AssertAnonymousProperty(created.Value!, "Id", 15L);
    }

    [Fact]
    public async Task RegistrarControlTesoreria_CuandoNoExiste_DevuelveNotFoundConError()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<RegistrarControlTesoreriaCierreCajaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Cierre de caja 12 no encontrado."));

        var result = await controller.RegistrarControlTesoreria(12, new RegistrarControlTesoreriaRequest(DateTimeOffset.UtcNow), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Cierre de caja 12 no encontrado");
    }

    [Fact]
    public async Task GetCierreDetalle_CuandoHayLineas_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var detalle = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildCierreDetalle(1, 7, 11, 12.5m),
            BuildCierreDetalle(2, 8, 12, 20m)
        });
        db.CierresCajaDetalle.Returns(detalle);
        var controller = CreateController(mediator, db);

        var result = await controller.GetCierreDetalle(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().ContainSingle();
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "CierreId", 7L);
        AssertAnonymousProperty(items[0], "CajaCuentaBancariaId", 11L);
        AssertAnonymousProperty(items[0], "Diferencia", 12.5m);
    }

    [Fact]
    public async Task AddCierreDetalle_CuandoNoExiste_DevuelveNotFoundConError()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<AddCierreCajaDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Cierre de caja 7 no encontrado."));

        var result = await controller.AddCierreDetalle(7, new AddCierreDetalleRequest(11, 12.5m), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Cierre de caja 7 no encontrado");
    }

    [Fact]
    public async Task AddCierreDetalle_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<AddCierreCajaDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));

        var result = await controller.AddCierreDetalle(7, new AddCierreDetalleRequest(11, 12.5m), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.ActionName.Should().Be(nameof(CajasController.GetCierreDetalle));
    }

    [Fact]
    public async Task RemoveCierreDetalle_CuandoNoExiste_DevuelveNotFoundConError()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<RemoveCierreCajaDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Linea de detalle no encontrada."));

        var result = await controller.RemoveCierreDetalle(7, 1, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Linea de detalle no encontrada");
    }

    [Fact]
    public async Task RemoveCierreDetalle_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<RemoveCierreCajaDetalleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.RemoveCierreDetalle(7, 1, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static CajasController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new CajasController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static CierreCaja BuildCierreCaja(long id, DateTimeOffset apertura, DateTimeOffset cierre, long usuarioId, int nroCierre)
    {
        var entity = CierreCaja.Crear(apertura, cierre, usuarioId, nroCierre);
        typeof(CierreCaja).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static CierreCajaDetalle BuildCierreDetalle(long id, long cierreId, long cajaCuentaBancariaId, decimal diferencia)
    {
        var entity = CierreCajaDetalle.Crear(cierreId, cajaCuentaBancariaId, diferencia);
        typeof(CierreCajaDetalle).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
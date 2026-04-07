using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Compras.Commands;
using ZuluIA_Back.Application.Features.Compras.DTOs;
using ZuluIA_Back.Application.Features.Compras.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class CotizacionesCompraControllerTests
{
    [Fact]
    public async Task GetAll_EnviaQueryCorrectaYDevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<CotizacionCompraListDto>(
        [
            new CotizacionCompraListDto { Id = 7, SucursalId = 10, ProveedorId = 30, Estado = "Pendiente", Total = 150 }
        ],
            1,
            20,
            1);
        mediator.Send(Arg.Any<GetCotizacionesCompraPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = new CotizacionesCompraController(mediator);

        var result = await controller.GetAll(1, 20, 10, 30, "Pendiente", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetCotizacionesCompraPagedQuery>(q =>
                q.Page == 1 &&
                q.PageSize == 20 &&
                q.SucursalId == 10 &&
                q.ProveedorId == 30 &&
                q.Estado == "Pendiente"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetCotizacionCompraDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((CotizacionCompraDto?)null);
        var controller = new CotizacionesCompraController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new CotizacionCompraDto
        {
            Id = 7,
            SucursalId = 10,
            ProveedorId = 30,
            Estado = "Pendiente",
            Total = 150,
            Items = [new CotizacionCompraItemDto { Id = 1, Descripcion = "Item", Cantidad = 2, PrecioUnitario = 75, Total = 150 }]
        };
        mediator.Send(Arg.Any<GetCotizacionCompraDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = new CotizacionesCompraController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(
            Arg.Is<GetCotizacionCompraDetalleQuery>(q => q.Id == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearCotizacionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(18L));
        var controller = new CotizacionesCompraController(mediator);

        var result = await controller.Crear(BuildCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetCotizacionCompraById");
        created.RouteValues!["id"].Should().Be(18L);
        AssertAnonymousProperty(created.Value!, "id", 18L);
    }

    [Fact]
    public async Task Crear_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearCotizacionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La cotizacion es invalida."));
        var controller = new CotizacionesCompraController(mediator);

        var result = await controller.Crear(BuildCommand(), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "La cotizacion es invalida.");
    }

    [Fact]
    public async Task Aceptar_CuandoTieneExito_DevuelveOkYEnviaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AceptarCotizacionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = new CotizacionesCompraController(mediator);

        var result = await controller.Aceptar(18, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<AceptarCotizacionCompraCommand>(c => c.Id == 18),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Aceptar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AceptarCotizacionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede aceptar."));
        var controller = new CotizacionesCompraController(mediator);

        var result = await controller.Aceptar(18, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "No se puede aceptar.");
    }

    [Fact]
    public async Task Rechazar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RechazarCotizacionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = new CotizacionesCompraController(mediator);

        var result = await controller.Rechazar(18, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<RechazarCotizacionCompraCommand>(c => c.Id == 18),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rechazar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RechazarCotizacionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede rechazar."));
        var controller = new CotizacionesCompraController(mediator);

        var result = await controller.Rechazar(18, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "No se puede rechazar.");
    }

    private static CrearCotizacionCompraCommand BuildCommand()
    {
        return new CrearCotizacionCompraCommand(
            10,
            5,
            30,
            new DateOnly(2026, 3, 21),
            new DateOnly(2026, 3, 28),
            "Obs",
            [new CrearCotizacionItemDto(100, "Item", 2, 75)]);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object? expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
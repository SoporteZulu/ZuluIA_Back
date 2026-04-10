using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Compras.Commands;
using ZuluIA_Back.Application.Features.Compras.DTOs;
using ZuluIA_Back.Application.Features.Compras.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class ComprasControllerTests
{
    [Fact]
    public async Task GetCotizaciones_CuandoSeInvoca_DelegaaQueryReal()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<CotizacionCompraListDto>(
            [new CotizacionCompraListDto { Id = 41, Estado = "PENDIENTE" }],
            1,
            1,
            20);
        mediator.Send(Arg.Any<GetCotizacionesCompraPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);

        var result = await controller.GetCotizaciones(2, 30, 10, 20, "PENDIENTE", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetCotizacionesCompraPagedQuery>(x =>
                x.Page == 2 &&
                x.PageSize == 30 &&
                x.SucursalId == 10 &&
                x.ProveedorId == 20 &&
                x.Estado == "PENDIENTE"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCotizacionById_CuandoExiste_DevuelveDetalleReal()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new CotizacionCompraDto { Id = 77, Estado = "ACEPTADA" };
        mediator.Send(Arg.Any<GetCotizacionCompraDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.GetCotizacionById(77, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task CrearCotizacion_CuandoTieneExito_UsaCommandReal()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearCotizacionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(88L));
        var controller = CreateController(mediator);
        var command = new CrearCotizacionCompraCommand(
            1,
            5,
            9,
            new DateOnly(2026, 4, 20),
            new DateOnly(2026, 4, 25),
            "Obs",
            [new CrearCotizacionItemDto(11, "Item", 2, 15)]);

        var result = await controller.CrearCotizacion(command, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetCompraCotizacionById");
        await mediator.Received(1).Send(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AceptarCotizacion_CuandoTieneExito_UsaTransitionReal()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AceptarCotizacionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.AceptarCotizacion(88, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(Arg.Is<AceptarCotizacionCompraCommand>(x => x.Id == 88), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RechazarCotizacion_CuandoTieneExito_UsaTransitionReal()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RechazarCotizacionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.RechazarCotizacion(89, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(Arg.Is<RechazarCotizacionCompraCommand>(x => x.Id == 89), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRequisiciones_CuandoSeInvoca_DelegaaQueryReal()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<RequisicionCompraListDto>(
            [new RequisicionCompraListDto { Id = 51, Estado = "BORRADOR" }],
            1,
            1,
            20);
        mediator.Send(Arg.Any<GetRequisicionesCompraPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);

        var result = await controller.GetRequisiciones(3, 15, 10, 22, "BORRADOR", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetRequisicionesCompraPagedQuery>(x =>
                x.Page == 3 &&
                x.PageSize == 15 &&
                x.SucursalId == 10 &&
                x.SolicitanteId == 22 &&
                x.Estado == "BORRADOR"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRequisicionById_CuandoExiste_DevuelveDetalleReal()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new RequisicionCompraDto { Id = 66, Estado = "ENVIADA" };
        mediator.Send(Arg.Any<GetRequisicionCompraDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.GetRequisicionById(66, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task CrearRequisicion_CuandoTieneExito_UsaCommandReal()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(99L));
        var controller = CreateController(mediator);
        var command = new CrearRequisicionCompraCommand(
            1,
            7,
            new DateOnly(2026, 4, 20),
            "Reposición crítica",
            "Obs",
            [new CrearRequisicionItemDto(11, "Item", 2, "unid", "Detalle")]);

        var result = await controller.CrearRequisicion(command, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetCompraRequisicionById");
        await mediator.Received(1).Send(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnviarRequisicion_CuandoTieneExito_UsaTransitionReal()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<EnviarRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.EnviarRequisicion(99, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(Arg.Is<EnviarRequisicionCompraCommand>(x => x.Id == 99), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AprobarRequisicion_CuandoTieneExito_UsaTransitionReal()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AprobarRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.AprobarRequisicion(100, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(Arg.Is<AprobarRequisicionCompraCommand>(x => x.Id == 100), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RechazarRequisicion_CuandoTieneExito_UsaTransitionReal()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RechazarRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.RechazarRequisicion(101, "Motivo", CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<RechazarRequisicionCompraCommand>(x => x.Id == 101 && x.Motivo == "Motivo"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelarRequisicion_CuandoTieneExito_UsaTransitionReal()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelarRequisicionCompraCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.CancelarRequisicion(102, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(Arg.Is<CancelarRequisicionCompraCommand>(x => x.Id == 102), Arg.Any<CancellationToken>());
    }

    private static ComprasController CreateController(IMediator mediator)
    {
        var controller = new ComprasController(mediator, Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }
}

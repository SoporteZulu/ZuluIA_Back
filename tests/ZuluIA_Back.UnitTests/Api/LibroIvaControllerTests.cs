using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Queries;

namespace ZuluIA_Back.UnitTests.Api;

public class LibroIvaControllerTests
{
    [Fact]
    public async Task GetVentas_CuandoHastaEsMenorQueDesde_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);

        var result = await controller.GetVentas(3, new DateOnly(2026, 3, 21), new DateOnly(2026, 3, 1), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await mediator.DidNotReceiveWithAnyArgs().Send(default(GetLibroIvaQuery)!, default);
    }

    [Fact]
    public async Task GetVentas_CuandoRangoEsValido_DevuelveOkYMandaTipoVentas()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new LibroIvaDto { SucursalId = 3, TipoLibro = "Ventas", CantidadComprobantes = 2, TotalGeneral = 500m };
        mediator.Send(Arg.Any<GetLibroIvaQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);
        var desde = new DateOnly(2026, 3, 1);
        var hasta = new DateOnly(2026, 3, 21);

        var result = await controller.GetVentas(3, desde, hasta, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(
            Arg.Is<GetLibroIvaQuery>(query =>
                query.SucursalId == 3 &&
                query.Desde == desde &&
                query.Hasta == hasta &&
                query.Tipo == TipoLibroIva.Ventas),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCompras_CuandoHastaEsMenorQueDesde_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);

        var result = await controller.GetCompras(4, new DateOnly(2026, 4, 10), new DateOnly(2026, 4, 1), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await mediator.DidNotReceiveWithAnyArgs().Send(default(GetLibroIvaQuery)!, default);
    }

    [Fact]
    public async Task GetCompras_CuandoRangoEsValido_DevuelveOkYMandaTipoCompras()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new LibroIvaDto { SucursalId = 4, TipoLibro = "Compras", CantidadComprobantes = 1, TotalGeneral = 250m };
        mediator.Send(Arg.Any<GetLibroIvaQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);
        var desde = new DateOnly(2026, 4, 1);
        var hasta = new DateOnly(2026, 4, 10);

        var result = await controller.GetCompras(4, desde, hasta, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(
            Arg.Is<GetLibroIvaQuery>(query =>
                query.SucursalId == 4 &&
                query.Desde == desde &&
                query.Hasta == hasta &&
                query.Tipo == TipoLibroIva.Compras),
            Arg.Any<CancellationToken>());
    }

    private static LibroIvaController CreateController(IMediator mediator)
    {
        return new LibroIvaController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }
}
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.NotasPedido.Commands;
using ZuluIA_Back.Application.Features.NotasPedido.DTOs;
using ZuluIA_Back.Application.Features.NotasPedido.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class NotasPedidoControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConPaginado()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<NotaPedidoListDto>(
            [
                new NotaPedidoListDto
                {
                    Id = 1,
                    SucursalId = 3,
                    TerceroId = 10,
                    TerceroRazonSocial = "Cliente Uno",
                    Fecha = new DateOnly(2026, 3, 21),
                    FechaVencimiento = new DateOnly(2026, 3, 25),
                    Total = 150m,
                    Estado = "Pendiente"
                }
            ],
            2,
            15,
            31);
        mediator.Send(Arg.Any<GetNotasPedidoPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);

        var result = await controller.GetAll(2, 15, 3, 10, "Pendiente", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<PagedResult<NotaPedidoListDto>>().Subject;
        payload.Page.Should().Be(2);
        payload.PageSize.Should().Be(15);
        payload.TotalCount.Should().Be(31);
        payload.Items.Should().ContainSingle();

        await mediator.Received(1).Send(
            Arg.Is<GetNotasPedidoPagedQuery>(q =>
                q.Page == 2 &&
                q.PageSize == 15 &&
                q.SucursalId == 3 &&
                q.TerceroId == 10 &&
                q.Estado == "Pendiente"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetNotaPedidoDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((NotaPedidoDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetNotaPedidoDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(new NotaPedidoDto
            {
                Id = 7,
                SucursalId = 3,
                TerceroId = 10,
                TerceroRazonSocial = "Cliente Uno",
                Fecha = new DateOnly(2026, 3, 21),
                FechaVencimiento = new DateOnly(2026, 3, 25),
                Total = 150m,
                Estado = "Pendiente",
                Observacion = "Nota de prueba",
                Items =
                [
                    new NotaPedidoItemDto { Id = 1, ItemId = 50, Cantidad = 2, CantidadPendiente = 2, PrecioUnitario = 75m, Bonificacion = 0m, Subtotal = 150m }
                ]
            });
        var controller = CreateController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<NotaPedidoDto>().Which.Id.Should().Be(7);
    }

    [Fact]
    public async Task GetReimpresion_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetNotaPedidoDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((NotaPedidoDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetReimpresion(88, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        await mediator.Received(1).Send(
            Arg.Is<GetNotaPedidoDetalleQuery>(query => query.Id == 88),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReimpresion_CuandoExiste_DevuelvePayloadDedicado()
    {
        var mediator = Substitute.For<IMediator>();
        var detalle = new NotaPedidoDto
        {
            Id = 11,
            SucursalId = 3,
            TerceroId = 10,
            Fecha = new DateOnly(2026, 3, 21),
            Total = 150m,
            Estado = "Pendiente"
        };
        mediator.Send(Arg.Any<GetNotaPedidoDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(detalle);
        var controller = CreateController(mediator);
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        var result = await controller.GetReimpresion(11, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<NotaPedidoReimpresionResponse>();
        var payload = (NotaPedidoReimpresionResponse)ok.Value!;
        payload.EsReimpresion.Should().BeTrue();
        payload.GeneradoEn.Should().BeOnOrAfter(before);
        payload.Documento.Should().BeSameAs(detalle);
        await mediator.Received(1).Send(
            Arg.Is<GetNotaPedidoDetalleQuery>(query => query.Id == 11),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPendientes_CuandoHayDatos_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetNotasPedidoPendientesQuery>(), Arg.Any<CancellationToken>())
            .Returns([
                new NotaPedidoListDto
                {
                    Id = 3,
                    SucursalId = 5,
                    TerceroId = 10,
                    TerceroRazonSocial = "Cliente Uno",
                    Fecha = new DateOnly(2026, 3, 21),
                    Total = 150m,
                    Estado = "Pendiente"
                }
            ]);
        var controller = CreateController(mediator);

        var result = await controller.GetPendientes(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<NotaPedidoListDto>>().Subject.Should().ContainSingle();
        await mediator.Received(1).Send(
            Arg.Is<GetNotasPedidoPendientesQuery>(q => q.SucursalId == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearNotaPedidoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Debe informar al menos un ítem."));
        var controller = CreateController(mediator);

        var result = await controller.Crear(BuildCrearNotaPedidoCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("al menos un ítem");
    }

    [Fact]
    public async Task Crear_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearNotaPedidoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator);

        var result = await controller.Crear(BuildCrearNotaPedidoCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetNotaPedidoById");
        AssertAnonymousProperty(created.Value!, "id", 21L);
    }

    [Fact]
    public async Task Anular_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularNotaPedidoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("La nota ya está anulada."));
        var controller = CreateController(mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ya está anulada");
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularNotaPedidoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static NotasPedidoController CreateController(IMediator mediator)
    {
        var controller = new NotasPedidoController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static CrearNotaPedidoCommand BuildCrearNotaPedidoCommand()
    {
        return new CrearNotaPedidoCommand(
            3,
            10,
            new DateOnly(2026, 3, 21),
            new DateOnly(2026, 3, 25),
            "Nota de prueba",
            20,
            [new CrearNotaPedidoItemDto(50, 2m, 75m, 0m)]);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
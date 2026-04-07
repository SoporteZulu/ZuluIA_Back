using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Recibos.Commands;
using ZuluIA_Back.Application.Features.Recibos.DTOs;
using ZuluIA_Back.Application.Features.Recibos.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class RecibosControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConPaginado()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<ReciboListDto>(
            [
                new ReciboListDto
                {
                    Id = 1,
                    SucursalId = 3,
                    TerceroId = 10,
                    TerceroRazonSocial = "Cliente Uno",
                    Fecha = new DateOnly(2026, 3, 21),
                    Serie = "A",
                    Numero = 123,
                    Total = 150m,
                    Estado = "EMITIDO"
                }
            ],
            2,
            15,
            31);
        mediator.Send(Arg.Any<GetRecibosPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);

        var result = await controller.GetAll(2, 15, 3, 10, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<PagedResult<ReciboListDto>>().Subject;
        payload.Page.Should().Be(2);
        payload.PageSize.Should().Be(15);
        payload.TotalCount.Should().Be(31);
        payload.Items.Should().ContainSingle();

        await mediator.Received(1).Send(
            Arg.Is<GetRecibosPagedQuery>(q =>
                q.Page == 2 &&
                q.PageSize == 15 &&
                q.SucursalId == 3 &&
                q.TerceroId == 10 &&
                q.Desde == new DateOnly(2026, 3, 1) &&
                q.Hasta == new DateOnly(2026, 3, 31)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetReciboDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((ReciboDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetReciboDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ReciboDto
            {
                Id = 7,
                SucursalId = 3,
                TerceroId = 10,
                TerceroRazonSocial = "Cliente Uno",
                Fecha = new DateOnly(2026, 3, 21),
                Serie = "A",
                Numero = 123,
                Total = 150m,
                Estado = "EMITIDO",
                Observacion = "Recibo de prueba",
                Items =
                [
                    new ReciboItemDto { Id = 1, Descripcion = "Pago contado", Importe = 150m }
                ]
            });
        var controller = CreateController(mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ReciboDto>().Which.Id.Should().Be(7);
    }

    [Fact]
    public async Task Emitir_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<EmitirReciboCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Debe informar al menos un ítem."));
        var controller = CreateController(mediator);

        var result = await controller.Emitir(BuildEmitirReciboCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("al menos un ítem");
    }

    [Fact]
    public async Task Emitir_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<EmitirReciboCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator);

        var result = await controller.Emitir(BuildEmitirReciboCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetReciboById");
        AssertAnonymousProperty(created.Value!, "id", 21L);
    }

    [Fact]
    public async Task Anular_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularReciboCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El recibo ya está anulado."));
        var controller = CreateController(mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ya está anulado");
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularReciboCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static RecibosController CreateController(IMediator mediator)
    {
        var controller = new RecibosController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static EmitirReciboCommand BuildEmitirReciboCommand()
    {
        return new EmitirReciboCommand(
            3,
            10,
            new DateOnly(2026, 3, 21),
            "A",
            "Recibo de prueba",
            40,
            [new EmitirReciboItemDto("Pago contado", 150m)]);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
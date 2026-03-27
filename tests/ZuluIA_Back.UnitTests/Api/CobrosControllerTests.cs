using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Application.Features.Finanzas.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class CobrosControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConPaginado()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<CobroListDto>(
            [
                new CobroListDto
                {
                    Id = 1,
                    TerceroId = 10,
                    TerceroRazonSocial = "Cliente Uno",
                    Fecha = new DateOnly(2026, 3, 21),
                    MonedaSimbolo = "$",
                    Total = 150m,
                    Estado = "Emitido"
                }
            ],
            2,
            15,
            31);
        mediator.Send(Arg.Any<GetCobrosPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);

        var result = await controller.GetAll(2, 15, 3, 7, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<PagedResult<CobroListDto>>().Subject;
        payload.Page.Should().Be(2);
        payload.PageSize.Should().Be(15);
        payload.TotalCount.Should().Be(31);
        payload.Items.Should().ContainSingle();

        await mediator.Received(1).Send(
            Arg.Is<GetCobrosPagedQuery>(q =>
                q.Page == 2 &&
                q.PageSize == 15 &&
                q.SucursalId == 3 &&
                q.TerceroId == 7 &&
                q.Desde == new DateOnly(2026, 3, 1) &&
                q.Hasta == new DateOnly(2026, 3, 31)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetCobroDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((CobroDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetCobroDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(new CobroDto
            {
                Id = 9,
                SucursalId = 3,
                TerceroId = 10,
                TerceroRazonSocial = "Cliente Uno",
                Fecha = new DateOnly(2026, 3, 21),
                MonedaId = 1,
                MonedaSimbolo = "$",
                Cotizacion = 1,
                Total = 150m,
                Estado = "Emitido",
                Medios =
                [
                    new CobroMedioDto
                    {
                        Id = 1,
                        CajaId = 5,
                        CajaDescripcion = "Caja Principal",
                        FormaPagoId = 2,
                        FormaPagoDescripcion = "Efectivo",
                        Importe = 150m,
                        MonedaId = 1,
                        MonedaSimbolo = "$",
                        Cotizacion = 1
                    }
                ]
            });
        var controller = CreateController(mediator);

        var result = await controller.GetById(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<CobroDto>().Which.Id.Should().Be(9);
    }

    [Fact]
    public async Task Registrar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarCobroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Debe informar al menos un medio de cobro."));
        var controller = CreateController(mediator);

        var result = await controller.Registrar(BuildRegistrarCobroCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("al menos un medio de cobro");
    }

    [Fact]
    public async Task Registrar_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarCobroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator);

        var result = await controller.Registrar(BuildRegistrarCobroCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetCobroById");
        AssertAnonymousProperty(created.Value!, "id", 21L);
    }

    [Fact]
    public async Task Anular_CuandoNoExiste_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularCobroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Cobro no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Cobro no encontrado");
    }

    [Fact]
    public async Task Anular_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularCobroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El cobro ya se encuentra anulado."));
        var controller = CreateController(mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ya se encuentra anulado");
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularCobroCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static CobrosController CreateController(IMediator mediator)
    {
        var controller = new CobrosController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static RegistrarCobroCommand BuildRegistrarCobroCommand()
    {
        return new RegistrarCobroCommand(
            3,
            10,
            new DateOnly(2026, 3, 21),
            1,
            1m,
            "Cobro de prueba",
            [new MedioCobroInput(5, 2, null, 150m, 1, 1m)],
            [new ComprobanteAImputarInput(40, 150m)]);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
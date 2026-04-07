using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Caea.Commands;
using ZuluIA_Back.Application.Features.Caea.DTOs;
using ZuluIA_Back.Application.Features.Caea.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class CaeaControllerTests
{
    [Fact]
    public async Task GetAll_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        IReadOnlyList<CaeaListDto> data = [new() { Id = 1, NroCaea = "123", Estado = "Pendiente" }];
        mediator.Send(Arg.Any<GetCaeasPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator);

        var result = await controller.GetAll(2, 15, 7, "Pendiente", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
        await mediator.Received(1).Send(
            Arg.Is<GetCaeasPagedQuery>(query =>
                query.Page == 2 &&
                query.PageSize == 15 &&
                query.PuntoFacturacionId == 7 &&
                query.Estado == "Pendiente"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetCaeaDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((CaeaListDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetById(4, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new CaeaListDto { Id = 4, NroCaea = "0004", Estado = "Pendiente" };
        mediator.Send(Arg.Any<GetCaeaDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.GetById(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Solicitar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SolicitarCaeaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Error de validación."));
        var controller = CreateController(mediator);
        var command = new SolicitarCaeaCommand(5, "123", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 15), "FA", 20);

        var result = await controller.Solicitar(command, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Solicitar_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SolicitarCaeaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator);
        var command = new SolicitarCaeaCommand(5, "123", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 15), "FA", 20);

        var result = await controller.Solicitar(command, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetCaeaById");
        AssertAnonymousProperty(created.Value!, "id", 12L);
    }

    [Fact]
    public async Task SolicitarAfip_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SolicitarCaeaAfipCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("AFIP rechazó la solicitud."));
        var controller = CreateController(mediator);

        var result = await controller.SolicitarAfip(new SolicitarCaeaAfipRequestDto(8, 202603, 1, "FA", 30), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SolicitarAfip_CuandoTieneExito_DevuelveCreatedAtRouteYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<SolicitarCaeaAfipCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(22L));
        var controller = CreateController(mediator);

        var result = await controller.SolicitarAfip(new SolicitarCaeaAfipRequestDto(8, 202603, 2, "FB", 30), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetCaeaById");
        AssertAnonymousProperty(created.Value!, "id", 22L);
        await mediator.Received(1).Send(
            Arg.Is<SolicitarCaeaAfipCommand>(command =>
                command.PuntoFacturacionId == 8 &&
                command.Periodo == 202603 &&
                command.Orden == 2 &&
                command.TipoComprobante == "FB" &&
                command.CantidadAsignada == 30),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Informar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<InformarCaeaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("CAEA no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.Informar(9, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Informar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<InformarCaeaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Informar(9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Anular_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularCaeaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("CAEA no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.Anular(11, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularCaeaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Anular(11, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static CaeaController CreateController(IMediator mediator)
    {
        return new CaeaController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.TasasInteres.Commands;
using ZuluIA_Back.Application.Features.TasasInteres.DTOs;
using ZuluIA_Back.Application.Features.TasasInteres.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class TasasInteresControllerTests
{
    [Fact]
    public async Task GetAll_DevuelveOkYMandaFiltroCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        List<TasaInteresDto> data = [new(1, "General", 5m, new DateOnly(2026, 3, 1), null, true)];
        mediator.Send(Arg.Any<GetTasasInteresQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator);

        var result = await controller.GetAll(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
        await mediator.Received(1).Send(
            Arg.Is<GetTasasInteresQuery>(query => query.SoloActivas == true),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearTasaInteresCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La tasa debe ser mayor a cero."));
        var controller = CreateController(mediator);

        var result = await controller.Crear(new CrearTasaInteresCommand("General", 0m, new DateOnly(2026, 3, 1), null, 2), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Crear_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearTasaInteresCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(7L));
        var controller = CreateController(mediator);

        var result = await controller.Crear(new CrearTasaInteresCommand("General", 5m, new DateOnly(2026, 3, 1), null, 2), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(7L);
    }

    [Fact]
    public async Task Desactivar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DesactivarTasaInteresCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tasa de interés no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.Desactivar(4, 3, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DesactivarTasaInteresCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Desactivar(4, 3, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DesactivarTasaInteresCommand>(command => command.Id == 4 && command.UserId == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivarTasaInteresCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Activar(5, 9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivarTasaInteresCommand>(command => command.Id == 5 && command.UserId == 9),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivarTasaInteresCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tasa de interés no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.Activar(5, 9, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    private static TasasInteresController CreateController(IMediator mediator)
    {
        return new TasasInteresController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }
}
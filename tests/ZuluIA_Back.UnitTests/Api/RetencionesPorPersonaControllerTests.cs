using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.RetencionesPorPersona.Commands;
using ZuluIA_Back.Application.Features.RetencionesPorPersona.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class RetencionesPorPersonaControllerTests
{
    [Fact]
    public async Task Get_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        IReadOnlyList<RetencionPorPersonaDto> data = [new(1, 7, 3, "Ganancias", "General")];
        mediator.Send(Arg.Any<GetRetencionesPorPersonaQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator);

        var result = await controller.Get(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
        await mediator.Received(1).Send(
            Arg.Is<GetRetencionesPorPersonaQuery>(query => query.TerceroId == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Asignar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AsignarRetencionAPersonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe la asignación."));
        var controller = CreateController(mediator);

        var result = await controller.Asignar(7, new AsignarRetencionBodyRequest(3, "General"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Asignar_CuandoTieneExito_DevuelveCreatedConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AsignarRetencionAPersonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator);

        var result = await controller.Asignar(7, new AsignarRetencionBodyRequest(3, "General"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Location.Should().Be("api/terceros/7/retenciones/15");
        AssertAnonymousProperty(created.Value!, "id", 15L);
        await mediator.Received(1).Send(
            Arg.Is<AsignarRetencionAPersonaCommand>(command =>
                command.TerceroId == 7 &&
                command.TipoRetencionId == 3 &&
                command.Descripcion == "General"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Quitar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<QuitarRetencionDePersonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Asignación no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.Quitar(7, 15, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Quitar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<QuitarRetencionDePersonaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Quitar(7, 15, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<QuitarRetencionDePersonaCommand>(command => command.Id == 15),
            Arg.Any<CancellationToken>());
    }

    private static RetencionesPorPersonaController CreateController(IMediator mediator)
    {
        return new RetencionesPorPersonaController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Terceros.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class TercerosUsuarioClienteControllerTests
{
    [Fact]
    public async Task GetUsuarioCliente_CuandoTieneExito_DevuelveOkEInvocaQuery()
    {
        var mediator = Substitute.For<IMediator>();
        var payload = Result.Success<TerceroUsuarioClienteDto?>(new TerceroUsuarioClienteDto { UsuarioId = 7, UserName = "cliente.demo", Activo = true });
        mediator.Send(Arg.Any<GetTerceroUsuarioClienteQuery>(), Arg.Any<CancellationToken>())
            .Returns(payload);
        var controller = new TercerosController(mediator);

        var result = await controller.GetUsuarioCliente(42, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(payload);
        await mediator.Received(1).Send(
            Arg.Is<GetTerceroUsuarioClienteQuery>(x => x.TerceroId == 42),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpsertUsuarioCliente_CuandoIdNoCoincide_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = new TercerosController(mediator);

        var result = await controller.UpsertUsuarioCliente(42, new UpsertTerceroUsuarioClienteCommand(7, "cliente.demo", null, null, null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        await mediator.DidNotReceive().Send(Arg.Any<UpsertTerceroUsuarioClienteCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveUsuarioCliente_CuandoTieneExito_InvocaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RemoveTerceroUsuarioClienteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = new TercerosController(mediator);

        var result = await controller.RemoveUsuarioCliente(15, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        await mediator.Received(1).Send(
            Arg.Is<RemoveTerceroUsuarioClienteCommand>(x => x.TerceroId == 15),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetUsuarioClientePermisos_CuandoTieneExito_InvocaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var response = Result.Success(new TerceroUsuarioClienteDto { UsuarioId = 7, UserName = "cliente.demo", Activo = true });
        mediator.Send(Arg.Any<SetTerceroUsuarioClientePermisosCommand>(), Arg.Any<CancellationToken>())
            .Returns(response);
        var controller = new TercerosController(mediator);
        var command = new SetTerceroUsuarioClientePermisosCommand(21, [new SetTerceroUsuarioClientePermisoItem(4, true)]);

        var result = await controller.SetUsuarioClientePermisos(21, command, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(response);
        await mediator.Received(1).Send(
            Arg.Is<SetTerceroUsuarioClientePermisosCommand>(x => x.TerceroId == 21 && x.Permisos.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetUsuarioClienteParametrosBasicos_CuandoTieneExito_InvocaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var response = Result.Success(new TerceroUsuarioClienteDto { UsuarioId = 7, UserName = "cliente.demo", Activo = true });
        mediator.Send(Arg.Any<SetTerceroUsuarioClienteParametrosBasicosCommand>(), Arg.Any<CancellationToken>())
            .Returns(response);
        var controller = new TercerosController(mediator);
        var command = new SetTerceroUsuarioClienteParametrosBasicosCommand(21, 3, "DEFAULT");

        var result = await controller.SetUsuarioClienteParametrosBasicos(21, command, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(response);
        await mediator.Received(1).Send(
            Arg.Is<SetTerceroUsuarioClienteParametrosBasicosCommand>(x => x.TerceroId == 21 && x.DefaultSucursalId == 3 && x.DefaultLayoutProfile == "DEFAULT"),
            Arg.Any<CancellationToken>());
    }
}

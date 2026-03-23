using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Auditoria.Commands;
using ZuluIA_Back.Application.Features.Auditoria.Queries;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Api;

public class AuditoriaComprobantesControllerTests
{
    [Fact]
    public async Task GetByComprobante_DevuelveOkConResultadoDelMediator()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        var expected = new List<AuditoriaComprobanteDto>
        {
            new(1, 25, 7, nameof(AccionAuditoria.Creado), DateTime.UtcNow, "detalle", "127.0.0.1")
        };

        mediator.Send(Arg.Any<GetAuditoriaComprobanteQuery>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await controller.GetByComprobante(25, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task Registrar_CuandoAccionEsInvalida_DevuelveBadRequestYSinEnviarCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);

        var result = await controller.Registrar(
            new RegistrarAuditoriaComprobanteRequest(25, 7, "accion-rara", "detalle", "127.0.0.1"),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Accion inválida");
        await mediator.DidNotReceive().Send(Arg.Any<RegistrarAuditoriaComprobanteCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Registrar_CuandoAccionEsValida_DevuelveOkYEnviaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<RegistrarAuditoriaComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Unit.Value);

        var result = await controller.Registrar(
            new RegistrarAuditoriaComprobanteRequest(25, 7, nameof(AccionAuditoria.AfipSolicitud), "detalle", "127.0.0.1"),
            CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<RegistrarAuditoriaComprobanteCommand>(x =>
                x.ComprobanteId == 25 &&
                x.UsuarioId == 7 &&
                x.Accion == AccionAuditoria.AfipSolicitud &&
                x.DetalleCambio == "detalle" &&
                x.IpOrigen == "127.0.0.1"),
            Arg.Any<CancellationToken>());
    }

    private static AuditoriaComprobantesController CreateController(IMediator mediator)
    {
        var controller = new AuditoriaComprobantesController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }
}
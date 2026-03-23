using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Comprobantes.Queries;

namespace ZuluIA_Back.UnitTests.Api;

public class ComprobantesControllerReimpresionTests
{
    [Fact]
    public async Task GetReimpresion_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetComprobanteDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((ComprobanteDetalleDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetReimpresion(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        await mediator.Received(1).Send(
            Arg.Is<GetComprobanteDetalleQuery>(query => query.Id == 99),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReimpresion_CuandoExiste_DevuelvePayloadDedicado()
    {
        var mediator = Substitute.For<IMediator>();
        var detalle = new ComprobanteDetalleDto
        {
            Id = 7,
            NumeroFormateado = "0001-00000007",
            TerceroRazonSocial = "Cliente Demo",
            Total = 1250m,
            Estado = "EMITIDO"
        };
        mediator.Send(Arg.Any<GetComprobanteDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(detalle);
        var controller = CreateController(mediator);
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        var result = await controller.GetReimpresion(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<ComprobanteReimpresionResponse>();
        var payload = (ComprobanteReimpresionResponse)ok.Value!;
        payload.EsReimpresion.Should().BeTrue();
        payload.GeneradoEn.Should().BeOnOrAfter(before);
        payload.Documento.Should().BeSameAs(detalle);
        await mediator.Received(1).Send(
            Arg.Is<GetComprobanteDetalleQuery>(query => query.Id == 7),
            Arg.Any<CancellationToken>());
    }

    private static ComprobantesController CreateController(IMediator mediator)
    {
        return new ComprobantesController(mediator, Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }
}
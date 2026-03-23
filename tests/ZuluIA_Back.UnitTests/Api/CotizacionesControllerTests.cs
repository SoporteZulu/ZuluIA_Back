using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Cotizaciones.Commands;
using ZuluIA_Back.Application.Features.Cotizaciones.DTOs;
using ZuluIA_Back.Application.Features.Cotizaciones.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class CotizacionesControllerTests
{
    [Fact]
    public async Task GetVigente_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetCotizacionVigenteQuery>(), Arg.Any<CancellationToken>())
            .Returns((CotizacionMonedaDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetVigente(2, new DateOnly(2026, 3, 21), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetVigente_CuandoNoSeInformaFecha_UsaHoyYDevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new CotizacionMonedaDto
        {
            Id = 7,
            MonedaId = 2,
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Cotizacion = 1250.50m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        mediator.Send(Arg.Any<GetCotizacionVigenteQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.GetVigente(2, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(
            Arg.Is<GetCotizacionVigenteQuery>(query =>
                query.MonedaId == 2 &&
                query.Fecha == DateOnly.FromDateTime(DateTime.Today)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetHistorico_DevuelveOkConListado()
    {
        var mediator = Substitute.For<IMediator>();
        IReadOnlyList<CotizacionMonedaDto> historial =
        [
            new() { Id = 1, MonedaId = 2, Fecha = new DateOnly(2026, 3, 20), Cotizacion = 1200m },
            new() { Id = 2, MonedaId = 2, Fecha = new DateOnly(2026, 3, 21), Cotizacion = 1250m }
        ];
        mediator.Send(Arg.Any<GetHistoricoCotizacionesQuery>(), Arg.Any<CancellationToken>())
            .Returns(historial);
        var controller = CreateController(mediator);

        var result = await controller.GetHistorico(2, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 21), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(historial);
    }

    [Fact]
    public async Task Registrar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarCotizacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La cotización debe ser mayor a cero."));
        var controller = CreateController(mediator);

        var result = await controller.Registrar(new RegistrarCotizacionCommand(2, new DateOnly(2026, 3, 21), 0m), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Registrar_CuandoTieneExito_DevuelveOkConIdYMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarCotizacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator);

        var result = await controller.Registrar(new RegistrarCotizacionCommand(2, new DateOnly(2026, 3, 21), 1250m), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 15L);
        AssertAnonymousProperty(ok.Value!, "mensaje", "Cotización registrada correctamente.");
    }

    [Fact]
    public async Task Importar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ImportarCotizacionesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ImportarCotizacionesResultDto>("Debe informar al menos una cotización."));
        var controller = CreateController(mediator);

        var result = await controller.Importar(new ImportarCotizacionesRequest(2, []), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Importar_CuandoTieneExito_DevuelveResumenYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ImportarCotizacionesResultDto(2, 2, 1, 1);
        mediator.Send(Arg.Any<ImportarCotizacionesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(dto));
        var controller = CreateController(mediator);
        var request = new ImportarCotizacionesRequest(2,
        [
            new ImportarCotizacionItemRequest(new DateOnly(2026, 3, 20), 1200m),
            new ImportarCotizacionItemRequest(new DateOnly(2026, 3, 21), 1250m)
        ]);

        var result = await controller.Importar(request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(
            Arg.Is<ImportarCotizacionesCommand>(command =>
                command.MonedaId == 2 &&
                command.Items.Count == 2 &&
                command.Items[0].Fecha == new DateOnly(2026, 3, 20) &&
                command.Items[0].Cotizacion == 1200m &&
                command.Items[1].Fecha == new DateOnly(2026, 3, 21) &&
                command.Items[1].Cotizacion == 1250m),
            Arg.Any<CancellationToken>());
    }

    private static CotizacionesController CreateController(IMediator mediator)
    {
        return new CotizacionesController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
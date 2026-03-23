using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ImputacionesControllerTests
{
    [Fact]
    public async Task GetByOrigen_DevuelveDtosConNumerosFormateados()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IImputacionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var fecha = new DateOnly(2026, 3, 21);
        var imputaciones = new[]
        {
            BuildImputacion(1, 10, 20, 150m, fecha),
            BuildImputacion(2, 10, 21, 75m, fecha)
        };
        repo.GetByComprobanteOrigenAsync(10, Arg.Any<CancellationToken>()).Returns(imputaciones);
        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildComprobante(10, 1, 100),
            BuildComprobante(20, 2, 200),
            BuildComprobante(21, 3, 300)
        });
        db.Comprobantes.Returns(comprobantes);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetByOrigen(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeAssignableTo<IEnumerable<ImputacionDto>>().Subject.ToList();
        payload.Should().HaveCount(2);
        payload[0].Id.Should().Be(1);
        payload[0].NumeroOrigen.Should().Be("0001-00000100");
        payload[0].NumeroDestino.Should().Be("0002-00000200");
        payload[1].NumeroDestino.Should().Be("0003-00000300");
    }

    [Fact]
    public async Task GetByOrigen_DevuelveDtosConFallbackCuandoFaltaNumeroDestino()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IImputacionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var imputaciones = new[]
        {
            BuildImputacion(1, 10, 20, 150m, new DateOnly(2026, 3, 21))
        };
        repo.GetByComprobanteOrigenAsync(10, Arg.Any<CancellationToken>()).Returns(imputaciones);
        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildComprobante(10, 1, 100)
        });
        db.Comprobantes.Returns(comprobantes);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetByOrigen(10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeAssignableTo<IEnumerable<ImputacionDto>>().Subject.ToList();
        payload.Should().ContainSingle();
        payload[0].NumeroOrigen.Should().Be("0001-00000100");
        payload[0].NumeroDestino.Should().Be("—");
    }

    [Fact]
    public async Task GetByDestino_DevuelveDtosConFallbackCuandoFaltaNumeroOrigen()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IImputacionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var imputaciones = new[]
        {
            BuildImputacion(1, 10, 20, 150m, new DateOnly(2026, 3, 21))
        };
        repo.GetByComprobanteDestinoAsync(20, Arg.Any<CancellationToken>()).Returns(imputaciones);
        var comprobantes = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildComprobante(20, 2, 200)
        });
        db.Comprobantes.Returns(comprobantes);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetByDestino(20, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeAssignableTo<IEnumerable<ImputacionDto>>().Subject.ToList();
        payload.Should().ContainSingle();
        payload[0].NumeroOrigen.Should().Be("—");
        payload[0].NumeroDestino.Should().Be("0002-00000200");
    }

    [Fact]
    public async Task GetTotalImputado_DevuelveOkConTotal()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IImputacionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        repo.GetTotalImputadoAsync(20, Arg.Any<CancellationToken>()).Returns(225.5m);
        var controller = CreateController(mediator, repo, db);

        var result = await controller.GetTotalImputado(20, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "comprobanteId", 20L);
        AssertAnonymousProperty(ok.Value!, "totalImputado", 225.5m);
    }

    [Fact]
    public async Task Imputar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IImputacionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ImputarComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El importe de la imputación debe ser mayor a 0."));
        var controller = CreateController(mediator, repo, db);

        var result = await controller.Imputar(new ImputarComprobanteCommand(10, 20, 0m, new DateOnly(2026, 3, 21)), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Imputar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IImputacionRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var fecha = new DateOnly(2026, 3, 21);
        mediator.Send(Arg.Any<ImputarComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(55L));
        var controller = CreateController(mediator, repo, db);
        var command = new ImputarComprobanteCommand(10, 20, 100m, fecha);

        var result = await controller.Imputar(command, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "imputacionId", 55L);
        ok.Value!.ToString().Should().Contain("Imputación registrada correctamente");
        await mediator.Received(1).Send(
            Arg.Is<ImputarComprobanteCommand>(request =>
                request.ComprobanteOrigenId == 10 &&
                request.ComprobanteDestinoId == 20 &&
                request.Importe == 100m &&
                request.Fecha == fecha),
            Arg.Any<CancellationToken>());
    }

    private static ImputacionesController CreateController(IMediator mediator, IImputacionRepository repo, IApplicationDbContext db)
    {
        return new ImputacionesController(mediator, repo, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static Imputacion BuildImputacion(long id, long origenId, long destinoId, decimal importe, DateOnly fecha)
    {
        var imputacion = Imputacion.Crear(origenId, destinoId, importe, fecha, null);
        typeof(Imputacion).BaseType!
            .GetProperty("Id")!
            .SetValue(imputacion, id);
        return imputacion;
    }

    private static Comprobante BuildComprobante(long id, short prefijo, long numero)
    {
        var comprobante = Comprobante.Crear(1, 1, 1, prefijo, numero, new DateOnly(2026, 3, 21), null, 1, 1, 1m, null, null);
        typeof(Comprobante).BaseType!
            .GetProperty("Id")!
            .SetValue(comprobante, id);
        return comprobante;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
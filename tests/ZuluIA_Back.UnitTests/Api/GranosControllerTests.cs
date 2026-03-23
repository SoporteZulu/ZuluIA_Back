using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Granos.Commands;
using ZuluIA_Back.Application.Features.Granos.DTOs;
using ZuluIA_Back.Application.Features.Granos.Queries;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.UnitTests.Api;

public class GranosControllerTests
{
    [Fact]
    public async Task GetPaged_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<LiquidacionGranosListDto>(
            [new LiquidacionGranosListDto(1, 3, 7, "Soja", new DateOnly(2026, 3, 1), 10m, 200m, 2000m, "Borrador")],
            2,
            15,
            30);
        mediator.Send(Arg.Any<GetLiquidacionesGranoPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);

        var result = await controller.GetPaged(3, 7, "Borrador", 2, 15, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetLiquidacionesGranoPagedQuery>(query =>
                query.SucursalId == 3 &&
                query.TerceroId == 7 &&
                query.Estado == "Borrador" &&
                query.Page == 2 &&
                query.PageSize == 15),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetLiquidacionGranosDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((LiquidacionGranosDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetById(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new LiquidacionGranosDto(5, 3, 7, "Maiz", new DateOnly(2026, 3, 1), 10m, 200m, 50m, 1950m, "Emitida", 15, [new ConceptoDto(1, "Flete", 50m, true)]);
        mediator.Send(Arg.Any<GetLiquidacionGranosDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Crear_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearLiquidacionGranosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Producto requerido."));
        var controller = CreateController(mediator);

        var result = await controller.Crear(new CrearLiquidacionGranosCommand(3, 7, "", new DateOnly(2026, 3, 1), 10m, 200m, 2), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Crear_CuandoTieneExito_DevuelveOkConId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CrearLiquidacionGranosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(11L));
        var controller = CreateController(mediator);

        var result = await controller.Crear(new CrearLiquidacionGranosCommand(3, 7, "Soja", new DateOnly(2026, 3, 1), 10m, 200m, 2), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(11L);
    }

    [Fact]
    public async Task AgregarConcepto_UsaIdDeRutaYCuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AgregarConceptoLiquidacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.AgregarConcepto(12, new AgregarConceptoLiquidacionCommand(1, "Flete", 50m, true, 9), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<AgregarConceptoLiquidacionCommand>(command =>
                command.LiquidacionId == 12 &&
                command.Concepto == "Flete" &&
                command.Importe == 50m &&
                command.EsDeduccion &&
                command.UserId == 9),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AgregarCertificacion_UsaIdDeRutaYCuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AgregarCertificacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Liquidación no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.AgregarCertificacion(13, new AgregarCertificacionCommand(1, "CERT-1", new DateOnly(2026, 3, 2), 100m, 10m, 2m, null, 9), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AgregarCertificacion_UsaIdDeRutaYCuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AgregarCertificacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator);

        var result = await controller.AgregarCertificacion(13, new AgregarCertificacionCommand(1, "CERT-1", new DateOnly(2026, 3, 2), 100m, 10m, 2m, null, 9), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(21L);
        await mediator.Received(1).Send(
            Arg.Is<AgregarCertificacionCommand>(command =>
                command.LiquidacionId == 13 &&
                command.NroCertificado == "CERT-1" &&
                command.PesoNeto == 100m &&
                command.Humedad == 10m &&
                command.Impureza == 2m &&
                command.UserId == 9),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Emitir_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<EmitirLiquidacionGranosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Emitir(14, 5, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Anular_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularLiquidacionGranosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Liquidación no encontrada."));
        var controller = CreateController(mediator);

        var result = await controller.Anular(15, 6, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularLiquidacionGranosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Anular(15, 6, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<AnularLiquidacionGranosCommand>(command =>
                command.Id == 15 &&
                command.UserId == 6),
            Arg.Any<CancellationToken>());
    }

    private static GranosController CreateController(IMediator mediator)
    {
        return new GranosController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }
}
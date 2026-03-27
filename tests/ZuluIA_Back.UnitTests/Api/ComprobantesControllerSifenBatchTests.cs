using System.Text;
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
using ZuluIA_Back.Application.Features.Comprobantes.Queries;
using ZuluIA_Back.Application.Features.ExportacionesFiscales.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Api;

public class ComprobantesControllerSifenBatchTests
{
    [Fact]
    public async Task PreviewReintentarSifenParaguayPendientes_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ReintentoSifenParaguayPreviewDto
        {
            MaxItems = 25,
            SucursalId = 7,
            Encontrados = 2,
            TotalElegibles = 3,
            HayMasResultados = true
        };
        mediator.Send(Arg.Any<PreviewReintentarSifenParaguayPendientesQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);
        var desde = new DateOnly(2026, 1, 1);
        var hasta = new DateOnly(2026, 1, 31);

        var result = await controller.PreviewReintentarSifenParaguayPendientes(25, 7, "Error", "S001", desde, hasta, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(
            Arg.Is<PreviewReintentarSifenParaguayPendientesQuery>(query =>
                query.MaxItems == 25 &&
                query.SucursalId == 7 &&
                query.EstadoSifen == "Error" &&
                query.CodigoRespuesta == "S001" &&
                query.FechaDesde == desde &&
                query.FechaHasta == hasta),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReintentarSifenParaguayPendientes_CuandoTieneExito_DevuelveOkConPayload()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ReintentoSifenParaguayBatchResultDto
        {
            MaxItems = 20,
            SucursalId = 5,
            Encontrados = 4,
            Procesados = 4,
            Exitosos = 3,
            Fallidos = 1
        };
        mediator.Send(Arg.Any<ReintentarSifenParaguayPendientesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(dto));
        var controller = CreateController(mediator);
        var request = new ReintentarSifenParaguayPendientesRequest(20, 5, "Rechazado", "S002", new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28));

        var result = await controller.ReintentarSifenParaguayPendientes(request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(
            Arg.Is<ReintentarSifenParaguayPendientesCommand>(command =>
                command.MaxItems == 20 &&
                command.SucursalId == 5 &&
                command.EstadoSifen == "Rechazado" &&
                command.CodigoRespuesta == "S002" &&
                command.FechaDesde == new DateOnly(2026, 2, 1) &&
                command.FechaHasta == new DateOnly(2026, 2, 28)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSifenPendientes_DevuelveOkYPagedResult()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ComprobanteSifenPendienteDto
        {
            ComprobanteId = 10,
            SucursalId = 3,
            TerceroId = 2,
            Prefijo = 1,
            Numero = 100,
            Fecha = new DateOnly(2026, 3, 10),
            EstadoComprobante = EstadoComprobante.Emitido,
            EstadoSifen = EstadoSifenParaguay.Error,
            PuedeReintentar = true,
            PuedeConciliar = true,
            TieneIdentificadores = true
        };
        var paged = new PagedResult<ComprobanteSifenPendienteDto>([dto], 2, 30, 45);
        mediator.Send(Arg.Any<GetComprobantesSifenPendientesQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator);

        var result = await controller.GetSifenPendientes(2, 30, 3, "Error", "S003", true, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), "fecha_desc", true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetComprobantesSifenPendientesQuery>(query =>
                query.Page == 2 &&
                query.PageSize == 30 &&
                query.SucursalId == 3 &&
                query.EstadoSifen == "Error" &&
                query.CodigoRespuesta == "S003" &&
                query.PuedeReintentar == true &&
                query.SoloConIdentificadores == true &&
                query.FechaDesde == new DateOnly(2026, 3, 1) &&
                query.FechaHasta == new DateOnly(2026, 3, 31) &&
                query.SortBy == "fecha_desc"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExportSifenPendientes_DevuelveCsv()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ExportacionArchivoResultDto("sifen.csv", "h1,h2", 1);
        mediator.Send(Arg.Any<ExportarComprobantesSifenPendientesCsvQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.ExportSifenPendientes(4, "Pendiente", "S004", false, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), "fecha_asc", true, CancellationToken.None);

        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("text/csv");
        file.FileDownloadName.Should().Be("sifen.csv");
        Encoding.Latin1.GetString(file.FileContents).Should().Be("h1,h2");
        await mediator.Received(1).Send(
            Arg.Is<ExportarComprobantesSifenPendientesCsvQuery>(query =>
                query.SucursalId == 4 &&
                query.EstadoSifen == "Pendiente" &&
                query.CodigoRespuesta == "S004" &&
                query.PuedeReintentar == false &&
                query.SoloConIdentificadores == true &&
                query.FechaDesde == new DateOnly(2026, 4, 1) &&
                query.FechaHasta == new DateOnly(2026, 4, 30) &&
                query.SortBy == "fecha_asc"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSifenPendientesResumen_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ComprobanteSifenPendientesResumenDto
        {
            Total = 10,
            Reintentables = 3,
            ConIdentificadores = 8,
            Conciliables = 7,
            SinEstadoSifen = 1
        };
        mediator.Send(Arg.Any<GetComprobantesSifenPendientesResumenQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.GetSifenPendientesResumen(9, "Error", "S005", true, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31), 12, 8, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(
            Arg.Is<GetComprobantesSifenPendientesResumenQuery>(query =>
                query.SucursalId == 9 &&
                query.EstadoSifen == "Error" &&
                query.CodigoRespuesta == "S005" &&
                query.PuedeReintentar == true &&
                query.SoloConIdentificadores == true &&
                query.FechaDesde == new DateOnly(2026, 5, 1) &&
                query.FechaHasta == new DateOnly(2026, 5, 31) &&
                query.TopCodigos == 12 &&
                query.TopMensajes == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PreviewConciliarSifenParaguayPendientes_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ConciliacionSifenParaguayPreviewDto
        {
            MaxItems = 15,
            SucursalId = 4,
            Encontrados = 6,
            TotalElegibles = 9,
            HayMasResultados = false
        };
        mediator.Send(Arg.Any<PreviewConciliarSifenParaguayPendientesQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.PreviewConciliarSifenParaguayPendientes(15, 4, "Pendiente", "S006", false, new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(
            Arg.Is<PreviewConciliarSifenParaguayPendientesQuery>(query =>
                query.MaxItems == 15 &&
                query.SucursalId == 4 &&
                query.EstadoSifen == "Pendiente" &&
                query.CodigoRespuesta == "S006" &&
                query.PuedeReintentar == false &&
                query.FechaDesde == new DateOnly(2026, 6, 1) &&
                query.FechaHasta == new DateOnly(2026, 6, 30)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConciliarSifenParaguayPendientes_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ConciliarSifenParaguayPendientesCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ConciliacionSifenParaguayBatchResultDto>("Sin comprobantes conciliables."));
        var controller = CreateController(mediator);
        var request = new ConciliarSifenParaguayPendientesRequest(18, 8, "Pendiente", "S007", true, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31));

        var result = await controller.ConciliarSifenParaguayPendientes(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Sin comprobantes conciliables");
        await mediator.Received(1).Send(
            Arg.Is<ConciliarSifenParaguayPendientesCommand>(command =>
                command.MaxItems == 18 &&
                command.SucursalId == 8 &&
                command.EstadoSifen == "Pendiente" &&
                command.CodigoRespuesta == "S007" &&
                command.PuedeReintentar == true &&
                command.FechaDesde == new DateOnly(2026, 7, 1) &&
                command.FechaHasta == new DateOnly(2026, 7, 31)),
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
using System.Text;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.ExportacionesFiscales.DTOs;
using ZuluIA_Back.Application.Features.ExportacionesFiscales.Queries;

namespace ZuluIA_Back.UnitTests.Api;

public class ExportacionesFiscalesControllerTests
{
    [Fact]
    public async Task CitiVentas_DevuelveArchivoYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ExportacionArchivoResultDto("ventas.txt", "LINEA1", 1);
        mediator.Send(Arg.Any<ExportarCitiVentasQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.CitiVentas(3, 2026, 3, CancellationToken.None);

        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("text/plain");
        file.FileDownloadName.Should().Be("ventas.txt");
        Encoding.Latin1.GetString(file.FileContents).Should().Be("LINEA1");
        await mediator.Received(1).Send(
            Arg.Is<ExportarCitiVentasQuery>(query => query.SucursalId == 3 && query.Anio == 2026 && query.Mes == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CitiCompras_DevuelveArchivo()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ExportacionArchivoResultDto("compras.txt", "COMPRA", 1);
        mediator.Send(Arg.Any<ExportarCitiComprasQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.CitiCompras(4, 2026, 4, CancellationToken.None);

        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("text/plain");
        file.FileDownloadName.Should().Be("compras.txt");
        Encoding.Latin1.GetString(file.FileContents).Should().Be("COMPRA");
        await mediator.Received(1).Send(
            Arg.Is<ExportarCitiComprasQuery>(query => query.SucursalId == 4 && query.Anio == 2026 && query.Mes == 4),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IibbPercepciones_DevuelveArchivo()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ExportacionArchivoResultDto("iibb.txt", "IIBB", 1);
        mediator.Send(Arg.Any<ExportarIibbPercepcionesQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.IibbPercepciones(5, 2026, 5, CancellationToken.None);

        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("text/plain");
        file.FileDownloadName.Should().Be("iibb.txt");
        Encoding.Latin1.GetString(file.FileContents).Should().Be("IIBB");
        await mediator.Received(1).Send(
            Arg.Is<ExportarIibbPercepcionesQuery>(query => query.SucursalId == 5 && query.Anio == 2026 && query.Mes == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetencionesGanancias_DevuelveArchivo()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ExportacionArchivoResultDto("ganancias.txt", "GANANCIAS", 1);
        mediator.Send(Arg.Any<ExportarRetencionesGananciasQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.RetencionesGanancias(6, 2026, 6, CancellationToken.None);

        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("text/plain");
        file.FileDownloadName.Should().Be("ganancias.txt");
        Encoding.Latin1.GetString(file.FileContents).Should().Be("GANANCIAS");
        await mediator.Received(1).Send(
            Arg.Is<ExportarRetencionesGananciasQuery>(query => query.SucursalId == 6 && query.Anio == 2026 && query.Mes == 6),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetencionesIva_DevuelveArchivoYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ExportacionArchivoResultDto("iva.txt", "IVA", 1);
        mediator.Send(Arg.Any<ExportarRetencionesIvaQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator);

        var result = await controller.RetencionesIva(7, 2026, 7, CancellationToken.None);

        var file = result.Should().BeOfType<FileContentResult>().Subject;
        file.ContentType.Should().Be("text/plain");
        file.FileDownloadName.Should().Be("iva.txt");
        Encoding.Latin1.GetString(file.FileContents).Should().Be("IVA");
        await mediator.Received(1).Send(
            Arg.Is<ExportarRetencionesIvaQuery>(query => query.SucursalId == 7 && query.Anio == 2026 && query.Mes == 7),
            Arg.Any<CancellationToken>());
    }

    private static ExportacionesFiscalesController CreateController(IMediator mediator)
    {
        return new ExportacionesFiscalesController(mediator)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }
}
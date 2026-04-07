using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.UnitTests.Api;

public class PeriodosIvaControllerTests
{
    [Fact]
    public async Task GetBySucursal_DevuelveOkConDtosMapeados()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPeriodoIvaRepository>();
        var periodo = BuildPeriodoIva(1, 10, 3, new DateOnly(2026, 3, 18), cerrado: true, new DateTimeOffset(2026, 3, 21, 0, 0, 0, TimeSpan.Zero));
        repo.GetBySucursalAsync(3, 10, Arg.Any<CancellationToken>())
            .Returns([periodo]);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetBySucursal(3, 10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<IReadOnlyList<PeriodoIvaDto>>().Subject;
        data.Should().HaveCount(1);
        data[0].Id.Should().Be(1);
        data[0].EjercicioId.Should().Be(10);
        data[0].SucursalId.Should().Be(3);
        data[0].Periodo.Should().Be(new DateOnly(2026, 3, 1));
        data[0].Cerrado.Should().BeTrue();
        data[0].PeriodoDescripcion.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetEstado_CuandoPeriodoEstaAbierto_DevuelveMensajeCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPeriodoIvaRepository>();
        var fecha = new DateOnly(2026, 3, 21);
        repo.EstaAbiertoPeriodoAsync(3, fecha, Arg.Any<CancellationToken>())
            .Returns(true);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetEstado(3, fecha, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "sucursalId", 3L);
        AssertAnonymousProperty(ok.Value!, "fecha", fecha);
        AssertAnonymousProperty(ok.Value!, "periodoAbierto", true);
        AssertAnonymousProperty(ok.Value!, "mensaje", "El período está abierto. Se pueden emitir comprobantes.");
    }

    [Fact]
    public async Task GetEstado_CuandoPeriodoEstaCerrado_DevuelveMensajeCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPeriodoIvaRepository>();
        var fecha = new DateOnly(2026, 3, 21);
        repo.EstaAbiertoPeriodoAsync(3, fecha, Arg.Any<CancellationToken>())
            .Returns(false);
        var controller = CreateController(mediator, repo);

        var result = await controller.GetEstado(3, fecha, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "periodoAbierto", false);
        AssertAnonymousProperty(ok.Value!, "mensaje", "El período está cerrado. No se pueden emitir comprobantes.");
    }

    [Fact]
    public async Task Abrir_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPeriodoIvaRepository>();
        mediator.Send(Arg.Any<AbrirPeriodoIvaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El período ya existe."));
        var controller = CreateController(mediator, repo);

        var result = await controller.Abrir(new AbrirPeriodoIvaCommand(10, 3, new DateOnly(2026, 3, 1)), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Abrir_CuandoTieneExito_DevuelveOkConIdYMensaje()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPeriodoIvaRepository>();
        mediator.Send(Arg.Any<AbrirPeriodoIvaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator, repo);

        var result = await controller.Abrir(new AbrirPeriodoIvaCommand(10, 3, new DateOnly(2026, 3, 1)), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "id", 12L);
        AssertAnonymousProperty(ok.Value!, "mensaje", "Período IVA abierto correctamente.");
    }

    [Fact]
    public async Task Cerrar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPeriodoIvaRepository>();
        mediator.Send(Arg.Any<CerrarPeriodoIvaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El período ya está cerrado."));
        var controller = CreateController(mediator, repo);

        var result = await controller.Cerrar(new CerrarPeriodoIvaCommand(3, new DateOnly(2026, 3, 1)), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Cerrar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var repo = Substitute.For<IPeriodoIvaRepository>();
        mediator.Send(Arg.Any<CerrarPeriodoIvaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, repo);

        var result = await controller.Cerrar(new CerrarPeriodoIvaCommand(3, new DateOnly(2026, 3, 1)), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static PeriodosIvaController CreateController(IMediator mediator, IPeriodoIvaRepository repo)
    {
        return new PeriodosIvaController(mediator, repo)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static PeriodoIva BuildPeriodoIva(long id, long ejercicioId, long sucursalId, DateOnly periodo, bool cerrado, DateTimeOffset createdAt)
    {
        var entity = PeriodoIva.Crear(ejercicioId, sucursalId, periodo);
        SetProperty(entity, nameof(PeriodoIva.Id), id);
        SetProperty(entity, nameof(PeriodoIva.CreatedAt), createdAt);
        SetProperty(entity, nameof(PeriodoIva.Cerrado), cerrado);
        return entity;
    }

    private static void SetProperty(object target, string propertyName, object value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
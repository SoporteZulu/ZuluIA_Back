using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class SeguimientoOrdenPagoControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItemsOrdenadosPorFechaDescEIdDesc()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var seguimientos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildSeguimiento(2, 50, 3, new DateOnly(2026, 3, 20), "Aprobado", "Obs 2", 10),
            BuildSeguimiento(3, 50, 3, new DateOnly(2026, 3, 20), "Pagado", "Obs 3", 11),
            BuildSeguimiento(1, 40, 2, new DateOnly(2026, 3, 19), "Pendiente", "Obs 1", 9)
        });
        db.SeguimientosOrdenPago.Returns(seguimientos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(3);
        AssertAnonymousProperty(items[0], "Id", 3L);
        AssertAnonymousProperty(items[1], "Id", 2L);
        AssertAnonymousProperty(items[2], "Id", 1L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var seguimientos = MockDbSetHelper.CreateMockDbSet<SeguimientoOrdenPago>();
        db.SeguimientosOrdenPago.Returns(seguimientos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var seguimientos = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildSeguimiento(5, 50, 3, new DateOnly(2026, 3, 20), "Pagado", "Observacion", 11)
        });
        db.SeguimientosOrdenPago.Returns(seguimientos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "PagoId", 50L);
        AssertAnonymousProperty(ok.Value!, "SucursalId", 3L);
        AssertAnonymousProperty(ok.Value!, "Estado", "PAGADO");
        AssertAnonymousProperty(ok.Value!, "Observacion", "Observacion");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSeguimientoOrdenPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Estado requerido."));
        var controller = CreateController(mediator);

        var result = await controller.Create(new SeguimientoOrdenPagoRequest(50, 3, new DateOnly(2026, 3, 20), "", "Obs", 11), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSeguimientoOrdenPagoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));
        var controller = CreateController(mediator);

        var result = await controller.Create(new SeguimientoOrdenPagoRequest(50, 3, new DateOnly(2026, 3, 20), "Pagado", "Obs", 11), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(SeguimientoOrdenPagoController.GetById));
        AssertAnonymousProperty(created.Value!, "Id", 9L);
    }

    [Fact]
    public async Task UpdateObservacion_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSeguimientoOrdenPagoObservacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<UpdateSeguimientoOrdenPagoObservacionResult>("Seguimiento 8 no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.UpdateObservacion(8, new UpdateSeguimientoObservacionRequest("Nueva obs", 11), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateObservacion_CuandoTieneExito_DevuelveOkConResultado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSeguimientoOrdenPagoObservacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new UpdateSeguimientoOrdenPagoObservacionResult(8, "Nueva obs")));
        var controller = CreateController(mediator);

        var result = await controller.UpdateObservacion(8, new UpdateSeguimientoObservacionRequest("Nueva obs", 11), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
        AssertAnonymousProperty(ok.Value!, "Observacion", "Nueva obs");
    }

    private static SeguimientoOrdenPagoController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new SeguimientoOrdenPagoController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static SeguimientoOrdenPago BuildSeguimiento(
        long id,
        long pagoId,
        long sucursalId,
        DateOnly fecha,
        string estado,
        string? observacion,
        long? usuarioId)
    {
        var entity = SeguimientoOrdenPago.Registrar(pagoId, sucursalId, fecha, estado, observacion, usuarioId);
        typeof(SeguimientoOrdenPago).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class HabilitacionesComprobantesControllerReadAndWorkflowTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosYOrdenDescendente()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var items = MockDbSetHelper.CreateMockDbSet([
            BuildHabilitacion(1, 10, 2, "comprobante", EstadoHabilitacion.Pendiente, createdAt: new DateTimeOffset(2026, 3, 1, 8, 0, 0, TimeSpan.Zero)),
            BuildHabilitacion(2, 10, 2, "COMPROBANTE", EstadoHabilitacion.Pendiente, createdAt: new DateTimeOffset(2026, 3, 2, 8, 0, 0, TimeSpan.Zero)),
            BuildHabilitacion(3, 11, 2, "ORDEN_COMPRA", EstadoHabilitacion.Habilitado, createdAt: new DateTimeOffset(2026, 3, 3, 8, 0, 0, TimeSpan.Zero))
        ]);
        db.HabilitacionesComprobantes.Returns(items);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(2, 10, "Pendiente", " comprobante ", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 2L);
        AssertAnonymousProperty(data[0], "Estado", "Pendiente");
        AssertAnonymousProperty(data[1], "Id", 1L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var items = MockDbSetHelper.CreateMockDbSet(new List<HabilitacionComprobante>());
        db.HabilitacionesComprobantes.Returns(items);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var items = MockDbSetHelper.CreateMockDbSet([
            BuildHabilitacion(5, 14, 3, "COMPROBANTE", EstadoHabilitacion.Habilitado, "bloq", "aprobado", 7)
        ]);
        db.HabilitacionesComprobantes.Returns(items);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "TipoDocumento", "COMPROBANTE");
        AssertAnonymousProperty(ok.Value!, "Estado", "Habilitado");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateHabilitacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El tipo de documento es obligatorio."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CrearHabilitacionComprobanteRequest(10, 2, string.Empty, "bloq"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateHabilitacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CrearHabilitacionComprobanteRequest(10, 2, "COMPROBANTE", "bloq"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(HabilitacionesComprobantesController.GetById));
        AssertAnonymousProperty(created.Value!, "Id", 12L);
        await mediator.Received(1).Send(
            Arg.Is<CreateHabilitacionComprobanteCommand>(command =>
                command.ComprobanteId == 10 &&
                command.SucursalId == 2 &&
                command.TipoDocumento == "COMPROBANTE" &&
                command.MotivoBloqueo == "bloq"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Habilitar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<HabilitarComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Habilitación 9 no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Habilitar(9, new ResolverHabilitacionRequest(7, "ok"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Habilitar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<HabilitarComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Habilitar(9, new ResolverHabilitacionRequest(7, "ok"), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<HabilitarComprobanteCommand>(command =>
                command.Id == 9 &&
                command.ResponsableId == 7 &&
                command.Observacion == "ok"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rechazar_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<RechazarHabilitacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Solo se pueden rechazar solicitudes pendientes."));
        var controller = CreateController(mediator, db);

        var result = await controller.Rechazar(9, new ResolverHabilitacionRequest(7, "obs"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Rechazar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<RechazarHabilitacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Rechazar(9, new ResolverHabilitacionRequest(7, "obs"), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<RechazarHabilitacionComprobanteCommand>(command =>
                command.Id == 9 &&
                command.ResponsableId == 7 &&
                command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    private static HabilitacionesComprobantesController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new HabilitacionesComprobantesController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static HabilitacionComprobante BuildHabilitacion(long id, long comprobanteId, long sucursalId, string tipoDocumento, EstadoHabilitacion estado, string? motivoBloqueo = null, string? observacion = null, long? habilitadoPor = null, DateTimeOffset? createdAt = null)
    {
        var entity = HabilitacionComprobante.Crear(comprobanteId, sucursalId, tipoDocumento, motivoBloqueo, 1);
        entity.GetType().GetProperty(nameof(HabilitacionComprobante.Id))!.SetValue(entity, id);
        if (createdAt.HasValue)
        {
            entity.GetType().GetProperty(nameof(HabilitacionComprobante.CreatedAt))!.SetValue(entity, createdAt.Value);
            entity.GetType().GetProperty(nameof(HabilitacionComprobante.UpdatedAt))!.SetValue(entity, createdAt.Value);
        }

        if (estado == EstadoHabilitacion.Habilitado)
            entity.Habilitar(habilitadoPor ?? 1, observacion, 1);
        else if (estado == EstadoHabilitacion.Rechazado)
            entity.Rechazar(habilitadoPor ?? 1, observacion, 1);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
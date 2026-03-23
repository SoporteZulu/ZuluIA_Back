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

public class AutorizacionesComprobantesControllerReadAndWorkflowTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosYOrdenDescendente()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var items = MockDbSetHelper.CreateMockDbSet([
            BuildAutorizacion(1, 10, 2, "venta", EstadoAutorizacion.Pendiente, createdAt: new DateTimeOffset(2026, 3, 1, 8, 0, 0, TimeSpan.Zero)),
            BuildAutorizacion(2, 10, 2, "VENTA", EstadoAutorizacion.Pendiente, createdAt: new DateTimeOffset(2026, 3, 2, 8, 0, 0, TimeSpan.Zero)),
            BuildAutorizacion(3, 11, 2, "COMPRA", EstadoAutorizacion.Autorizado, createdAt: new DateTimeOffset(2026, 3, 3, 8, 0, 0, TimeSpan.Zero))
        ]);
        db.AutorizacionesComprobantes.Returns(items);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(2, 10, "Pendiente", " venta ", CancellationToken.None);

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
        var items = MockDbSetHelper.CreateMockDbSet(new List<AutorizacionComprobante>());
        db.AutorizacionesComprobantes.Returns(items);
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
            BuildAutorizacion(5, 14, 3, "VENTA", EstadoAutorizacion.Autorizado, "aprobado", 7)
        ]);
        db.AutorizacionesComprobantes.Returns(items);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "TipoOperacion", "VENTA");
        AssertAnonymousProperty(ok.Value!, "Estado", "Autorizado");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateAutorizacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El tipo de operación es obligatorio."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CrearAutorizacionComprobanteRequest(10, 2, string.Empty), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateAutorizacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(12L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new CrearAutorizacionComprobanteRequest(10, 2, "VENTA"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(AutorizacionesComprobantesController.GetById));
        AssertAnonymousProperty(created.Value!, "Id", 12L);
        await mediator.Received(1).Send(
            Arg.Is<CreateAutorizacionComprobanteCommand>(command =>
                command.ComprobanteId == 10 &&
                command.SucursalId == 2 &&
                command.TipoOperacion == "VENTA"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Autorizar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AutorizarComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Autorización 9 no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.Autorizar(9, new ResolverAutorizacionRequest(7, "ok"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Autorizar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AutorizarComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Autorizar(9, new ResolverAutorizacionRequest(7, "ok"), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<AutorizarComprobanteCommand>(command =>
                command.Id == 9 &&
                command.ResponsableId == 7 &&
                command.Motivo == "ok"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rechazar_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<RechazarAutorizacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Solo se pueden rechazar solicitudes pendientes."));
        var controller = CreateController(mediator, db);

        var result = await controller.Rechazar(9, new ResolverAutorizacionRequest(7, "obs"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Rechazar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<RechazarAutorizacionComprobanteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Rechazar(9, new ResolverAutorizacionRequest(7, "obs"), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<RechazarAutorizacionComprobanteCommand>(command =>
                command.Id == 9 &&
                command.ResponsableId == 7 &&
                command.Motivo == "obs"),
            Arg.Any<CancellationToken>());
    }

    private static AutorizacionesComprobantesController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new AutorizacionesComprobantesController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static AutorizacionComprobante BuildAutorizacion(long id, long comprobanteId, long sucursalId, string tipoOperacion, EstadoAutorizacion estado, string? motivo = null, long? autorizadoPor = null, DateTimeOffset? createdAt = null)
    {
        var entity = AutorizacionComprobante.Crear(comprobanteId, sucursalId, tipoOperacion, 1);
        entity.GetType().GetProperty(nameof(AutorizacionComprobante.Id))!.SetValue(entity, id);
        if (createdAt.HasValue)
        {
            entity.GetType().GetProperty(nameof(AutorizacionComprobante.CreatedAt))!.SetValue(entity, createdAt.Value);
            entity.GetType().GetProperty(nameof(AutorizacionComprobante.UpdatedAt))!.SetValue(entity, createdAt.Value);
        }

        if (estado == EstadoAutorizacion.Autorizado)
            entity.Autorizar(autorizadoPor ?? 1, motivo, 1);
        else if (estado == EstadoAutorizacion.Rechazado)
            entity.Rechazar(autorizadoPor ?? 1, motivo, 1);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
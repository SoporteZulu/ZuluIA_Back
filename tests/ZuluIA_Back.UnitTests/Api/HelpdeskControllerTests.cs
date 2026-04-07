using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Documentos.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Documentos;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class HelpdeskControllerTests
{
    [Fact]
    public async Task GetTickets_AplicaFiltrosYExcluyeArchivadosPorDefecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildMesaEntrada(1, 2, new DateOnly(2026, 3, 20), EstadoMesaEntrada.Pendiente, 7, "HD-1", "Ticket 1", "Obs 1"),
            BuildMesaEntrada(2, 2, new DateOnly(2026, 3, 21), EstadoMesaEntrada.Archivado, 7, "HD-2", "Ticket 2", "Obs 2"),
            BuildMesaEntrada(3, 2, new DateOnly(2026, 3, 22), EstadoMesaEntrada.Pendiente, 99, "HD-3", "Ticket 3", null)
        });
        db.MesasEntrada.Returns(mesas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetTickets(2, 7, "pendiente", false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((System.Collections.IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(1);
        AssertAnonymousProperty(items[0], "TicketId", 1L);
        AssertAnonymousProperty(items[0], "Titulo", "Ticket 1");
        AssertAnonymousProperty(items[0], "EstadoFlow", "Pendiente");
        AssertAnonymousProperty(items[0], "AsignadoA", 7L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var mesas = MockDbSetHelper.CreateMockDbSet<MesaEntrada>();
        db.MesasEntrada.Returns(mesas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOkConDetalle()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var mesas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildMesaEntrada(8, 2, new DateOnly(2026, 3, 20), EstadoMesaEntrada.Pendiente, 7, "HD-8", "Ticket detalle", "Obs detalle", new DateOnly(2026, 3, 25))
        });
        db.MesasEntrada.Returns(mesas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "TicketId", 8L);
        AssertAnonymousProperty(ok.Value!, "Titulo", "Ticket detalle");
        AssertAnonymousProperty(ok.Value!, "EstadoFlow", "Pendiente");
        AssertAnonymousProperty(ok.Value!, "AsignadoA", 7L);
    }

    [Fact]
    public async Task CreateTicket_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateHelpdeskTicketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("error create"));

        var result = await controller.CreateTicket(
            new HelpdeskCreateTicketRequest(1, 2, 3, "HD-1", "Ticket", new DateOnly(2026, 3, 20), null, null),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("error create");
    }

    [Fact]
    public async Task CreateTicket_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateHelpdeskTicketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(18L));

        var request = new HelpdeskCreateTicketRequest(1, 2, 3, "HD-1", "Ticket", new DateOnly(2026, 3, 20), null, null);

        var result = await controller.CreateTicket(
            request,
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(HelpdeskController.GetById));
        AssertAnonymousProperty(created.Value!, "ticketId", 18L);
        await mediator.Received(1).Send(
            Arg.Is<CreateHelpdeskTicketCommand>(command =>
                command.SucursalId == request.SucursalId &&
                command.TipoId == request.TipoId &&
                command.TerceroId == request.TerceroId &&
                command.NroDocumento == request.NroDocumento &&
                command.Titulo == request.Titulo),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Asignar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<AssignHelpdeskTicketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<HelpdeskAsignacionResult>("Ticket 10 no encontrado."));

        var result = await controller.Asignar(10, new HelpdeskAsignarRequest(7), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Asignar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<AssignHelpdeskTicketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new HelpdeskAsignacionResult(10, 7)));

        var result = await controller.Asignar(10, new HelpdeskAsignarRequest(7), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "ticketId", 10L);
        AssertAnonymousProperty(ok.Value!, "AsignadoA", 7L);
        await mediator.Received(1).Send(
            Arg.Is<AssignHelpdeskTicketCommand>(command => command.Id == 10 && command.UsuarioId == 7),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CambiarEstado_CuandoEstadoFlowEsInvalido_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ChangeHelpdeskTicketStateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<HelpdeskEstadoResult>("EstadoFlow inválido."));

        var result = await controller.CambiarEstado(10, new HelpdeskEstadoRequest(2, "XXX", null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CambiarEstado_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ChangeHelpdeskTicketStateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<HelpdeskEstadoResult>("Ticket 10 no encontrado."));

        var result = await controller.CambiarEstado(10, new HelpdeskEstadoRequest(2, "EnProceso", null), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CambiarEstado_CuandoTieneExito_DevuelveOkConPayload()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ChangeHelpdeskTicketStateCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new HelpdeskEstadoResult(10, "EnProceso", 2)));

        var result = await controller.CambiarEstado(10, new HelpdeskEstadoRequest(2, "EnProceso", "ok"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "ticketId", 10L);
        AssertAnonymousProperty(ok.Value!, "EstadoFlow", "EnProceso");
        AssertAnonymousProperty(ok.Value!, "EstadoId", 2L);
        await mediator.Received(1).Send(
            Arg.Is<ChangeHelpdeskTicketStateCommand>(command =>
                command.Id == 10 &&
                command.EstadoId == 2 &&
                command.EstadoFlow == "EnProceso" &&
                command.Observacion == "ok"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cerrar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CloseHelpdeskTicketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Ticket 10 no encontrado."));

        var result = await controller.Cerrar(10, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Cerrar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CloseHelpdeskTicketCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Cerrar(10, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<CloseHelpdeskTicketCommand>(command => command.Id == 10),
            Arg.Any<CancellationToken>());
    }

    private static HelpdeskController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new HelpdeskController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static MesaEntrada BuildMesaEntrada(
        long id,
        long sucursalId,
        DateOnly fechaIngreso,
        EstadoMesaEntrada estadoFlow,
        long? asignadoA,
        string nroDocumento,
        string asunto,
        string? observacion,
        DateOnly? fechaVencimiento = null)
    {
        var entity = MesaEntrada.Crear(sucursalId, 2, 3, nroDocumento, asunto, fechaIngreso, fechaVencimiento, observacion, userId: null);
        typeof(MesaEntrada).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (asignadoA.HasValue)
            entity.AsignarResponsable(asignadoA.Value, userId: null);

        if (estadoFlow != EstadoMesaEntrada.Pendiente)
            entity.CambiarEstado(1, estadoFlow, observacion, userId: null);

        if (estadoFlow == EstadoMesaEntrada.Archivado)
            entity.Archivar(userId: null);
        else if (estadoFlow == EstadoMesaEntrada.Anulado)
            entity.Anular(userId: null);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
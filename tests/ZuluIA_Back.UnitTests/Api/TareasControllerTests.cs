using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Proyectos.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Proyectos;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class TareasControllerTests
{
    [Fact]
    public async Task GetEstimadas_AplicaFiltrosYOrdenDescendente()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tareas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTareaEstimada(1, 10, 20, 30, "Tarea 1", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 4m, "obs", true),
            BuildTareaEstimada(3, 10, 20, 30, "Tarea 3", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 6m, null, true),
            BuildTareaEstimada(2, 10, 21, 30, "Tarea 2", new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), 5m, null, true),
            BuildTareaEstimada(4, 10, 20, 31, "Tarea 4", new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30), 7m, null, false)
        });
        db.TareasEstimadas.Returns(tareas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetEstimadas(10, 20, 30, true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 3L);
        AssertAnonymousProperty(data[1], "Id", 1L);
    }

    [Fact]
    public async Task CreateEstimada_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateTareaEstimadaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Las horas estimadas deben ser mayores a 0."));
        var controller = CreateController(mediator, db);

        var result = await controller.CreateEstimada(
            new CreateTareaEstimadaRequest(10, 20, 30, "Tarea", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 0m, "obs"),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateEstimada_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateTareaEstimadaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(11L));
        var controller = CreateController(mediator, db);
        var fechaDesde = new DateOnly(2026, 1, 1);
        var fechaHasta = new DateOnly(2026, 1, 31);

        var result = await controller.CreateEstimada(
            new CreateTareaEstimadaRequest(10, 20, 30, "Tarea", fechaDesde, fechaHasta, 8m, "obs"),
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TareasController.GetEstimadas));
        AssertAnonymousProperty(created.Value!, "Id", 11L);
        await mediator.Received(1).Send(
            Arg.Is<CreateTareaEstimadaCommand>(command =>
                command.ProyectoId == 10 &&
                command.SucursalId == 20 &&
                command.AsignadoA == 30 &&
                command.Descripcion == "Tarea" &&
                command.FechaDesde == fechaDesde &&
                command.FechaHasta == fechaHasta &&
                command.HorasEstimadas == 8m &&
                command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateEstimada_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateTareaEstimadaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tarea estimada 8 no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateEstimada(
            8,
            new UpdateTareaEstimadaRequest(30, "Tarea", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 8m, "obs"),
            CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateEstimada_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateTareaEstimadaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Las horas estimadas deben ser mayores a 0."));
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateEstimada(
            8,
            new UpdateTareaEstimadaRequest(30, "Tarea", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 0m, "obs"),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateEstimada_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateTareaEstimadaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateEstimada(
            8,
            new UpdateTareaEstimadaRequest(30, "Tarea", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 8m, "obs"),
            CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateTareaEstimadaCommand>(command =>
                command.Id == 8 &&
                command.AsignadoA == 30 &&
                command.Descripcion == "Tarea" &&
                command.HorasEstimadas == 8m &&
                command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DesactivarEstimada_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateTareaEstimadaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tarea estimada 8 no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.DesactivarEstimada(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DesactivarEstimada_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeactivateTareaEstimadaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.DesactivarEstimada(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ActivarEstimada_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ActivateTareaEstimadaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.ActivarEstimada(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetReales_AplicaFiltrosYOrdenDescendente()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var tareas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildTareaReal(1, 10, 20, 40, 30, new DateOnly(2026, 1, 1), "Tarea 1", 4m, false, "obs"),
            BuildTareaReal(3, 10, 20, 40, 30, new DateOnly(2026, 3, 1), "Tarea 3", 6m, false, null),
            BuildTareaReal(2, 10, 21, 40, 31, new DateOnly(2026, 2, 1), "Tarea 2", 5m, true, null),
            BuildTareaReal(4, 10, 20, 41, 30, new DateOnly(2026, 4, 1), "Tarea 4", 7m, false, null)
        });
        db.TareasReales.Returns(tareas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetReales(10, 20, 40, false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 3L);
        AssertAnonymousProperty(data[1], "Id", 1L);
    }

    [Fact]
    public async Task CreateReal_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateTareaRealCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Las horas reales deben ser mayores a 0."));
        var controller = CreateController(mediator, db);

        var result = await controller.CreateReal(
            new CreateTareaRealRequest(10, 20, 30, 40, new DateOnly(2026, 1, 1), "Tarea real", 0m, "obs"),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateReal_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateTareaRealCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator, db);
        var fecha = new DateOnly(2026, 1, 1);

        var result = await controller.CreateReal(
            new CreateTareaRealRequest(10, 20, 30, 40, fecha, "Tarea real", 5m, "obs"),
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TareasController.GetReales));
        AssertAnonymousProperty(created.Value!, "Id", 15L);
        await mediator.Received(1).Send(
            Arg.Is<CreateTareaRealCommand>(command =>
                command.ProyectoId == 10 &&
                command.SucursalId == 20 &&
                command.TareaEstimadaId == 30 &&
                command.UsuarioId == 40 &&
                command.Fecha == fecha &&
                command.Descripcion == "Tarea real" &&
                command.HorasReales == 5m &&
                command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateReal_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateTareaRealCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tarea real 8 no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateReal(8, new UpdateTareaRealRequest("Tarea real", 5m, "obs"), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateReal_CuandoFallaPorNegocio_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateTareaRealCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se puede modificar una tarea ya aprobada."));
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateReal(8, new UpdateTareaRealRequest("Tarea real", 5m, "obs"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateReal_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateTareaRealCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.UpdateReal(8, new UpdateTareaRealRequest("Tarea real", 5m, "obs"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateTareaRealCommand>(command =>
                command.Id == 8 &&
                command.Descripcion == "Tarea real" &&
                command.HorasReales == 5m &&
                command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AprobarReal_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ApproveTareaRealCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tarea real 8 no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.AprobarReal(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AprobarReal_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ApproveTareaRealCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.AprobarReal(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DeleteReal_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteTareaRealCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.DeleteReal(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeleteTareaRealCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteReal_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<DeleteTareaRealCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Tarea real 8 no encontrada."));
        var controller = CreateController(mediator, db);

        var result = await controller.DeleteReal(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private static TareasController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new TareasController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static TareaEstimada BuildTareaEstimada(long id, long proyectoId, long sucursalId, long? asignadoA, string descripcion, DateOnly fechaDesde, DateOnly fechaHasta, decimal horasEstimadas, string? observacion, bool activa)
    {
        var entity = TareaEstimada.Crear(proyectoId, sucursalId, asignadoA, descripcion, fechaDesde, fechaHasta, horasEstimadas, observacion, null);
        typeof(TareaEstimada).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (!activa)
            entity.Desactivar(null);

        return entity;
    }

    private static TareaReal BuildTareaReal(long id, long proyectoId, long sucursalId, long usuarioId, long? tareaEstimadaId, DateOnly fecha, string descripcion, decimal horasReales, bool aprobada, string? observacion)
    {
        var entity = TareaReal.Registrar(proyectoId, sucursalId, tareaEstimadaId, usuarioId, fecha, descripcion, horasReales, observacion, null);
        typeof(TareaReal).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (aprobada)
            entity.Aprobar(null);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
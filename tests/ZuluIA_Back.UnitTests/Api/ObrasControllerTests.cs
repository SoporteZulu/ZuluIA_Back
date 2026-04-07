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

public class ObrasControllerTests
{
    [Fact]
    public async Task GetAll_AplicaFiltrosYOrdenDescendente()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var proyectos = MockDbSetHelper.CreateMockDbSet([
            BuildProyecto(1, "OB001", "Obra 1", 10, 20, "activo", false, 12, false),
            BuildProyecto(3, "OB003", "Obra 3", 10, 20, "activo", false, 24, true),
            BuildProyecto(2, "OB002", "Obra 2", 11, 21, "finalizado", false, 6, false)
        ]);
        db.Proyectos.Returns(proyectos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(10, 20, "ACTIVO", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 3L);
        AssertAnonymousProperty(data[0], "Estado", "activo");
        AssertAnonymousProperty(data[1], "Id", 1L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var proyectos = MockDbSetHelper.CreateMockDbSet(new List<Proyecto>());
        db.Proyectos.Returns(proyectos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var proyectos = MockDbSetHelper.CreateMockDbSet([
            BuildProyecto(5, "OB005", "Obra 5", 10, 20, "activo", false, 18, true, "obs")
        ]);
        db.Proyectos.Returns(proyectos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "Codigo", "OB005");
        AssertAnonymousProperty(ok.Value!, "SoloPadre", true);
        AssertAnonymousProperty(ok.Value!, "Observacion", "obs");
    }

    [Fact]
    public async Task Create_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La sucursal es requerida."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(
            new CreateObraRequest("OB001", "Obra 1", 0, 20, new DateOnly(2026, 1, 1), null, 12, false, "obs"),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtActionYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreateProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(15L));
        var controller = CreateController(mediator, db);
        var fechaInicio = new DateOnly(2026, 1, 1);
        var fechaFin = new DateOnly(2026, 12, 31);

        var result = await controller.Create(
            new CreateObraRequest("OB001", "Obra 1", 10, 20, fechaInicio, fechaFin, 12, true, "obs"),
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ObrasController.GetById));
        AssertAnonymousProperty(created.Value!, "id", 15L);
        await mediator.Received(1).Send(
            Arg.Is<CreateProyectoCommand>(command =>
                command.Codigo == "OB001" &&
                command.Descripcion == "Obra 1" &&
                command.SucursalId == 10 &&
                command.TerceroId == 20 &&
                command.FechaInicio == fechaInicio &&
                command.FechaFin == fechaFin &&
                command.TotalCuotas == 12 &&
                command.SoloPadre &&
                command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el proyecto."));
        var controller = CreateController(mediator, db);

        var result = await controller.Update(8, new UpdateObraRequest("Obra", null, null, 10, false, null), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<UpdateProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);
        var fechaInicio = new DateOnly(2026, 2, 1);
        var fechaFin = new DateOnly(2026, 11, 30);

        var result = await controller.Update(8, new UpdateObraRequest("Obra", fechaInicio, fechaFin, 10, true, "obs"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateProyectoCommand>(command =>
                command.Id == 8 &&
                command.Descripcion == "Obra" &&
                command.FechaInicio == fechaInicio &&
                command.FechaFin == fechaFin &&
                command.TotalCuotas == 10 &&
                command.SoloPadre &&
                command.Observacion == "obs"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Finalizar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<FinalizarProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("No se encontró el proyecto."));
        var controller = CreateController(mediator, db);

        var result = await controller.Finalizar(4, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Finalizar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<FinalizarProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Finalizar(4, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<FinalizarProyectoCommand>(command => command.Id == 4),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AnularProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Anular(4, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<AnularProyectoCommand>(command => command.Id == 4),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reactivar_CuandoTieneExito_DevuelveOkYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<ReactivarProyectoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, db);

        var result = await controller.Reactivar(4, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ReactivarProyectoCommand>(command => command.Id == 4),
            Arg.Any<CancellationToken>());
    }

    private static ObrasController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new ObrasController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static Proyecto BuildProyecto(long id, string codigo, string descripcion, long sucursalId, long? terceroId, string estado, bool anulada, int totalCuotas, bool soloPadre, string? observacion = null)
    {
        var entity = Proyecto.Crear(codigo, descripcion, sucursalId, terceroId, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), totalCuotas, soloPadre, observacion);
        entity.GetType().GetProperty(nameof(Proyecto.Id))!.SetValue(entity, id);

        if (estado == "finalizado")
            entity.Finalizar();
        else if (estado == "anulado")
            entity.Anular();

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
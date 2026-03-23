using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Extras.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class MatriculasControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConItemsOrdenadosPorFechaAltaDescEIdDesc()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var matriculas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildMatricula(2, 20, 10, "B-2", "Dos", new DateOnly(2026, 3, 20), null, true),
            BuildMatricula(3, 20, 10, "B-3", "Tres", new DateOnly(2026, 3, 20), null, true),
            BuildMatricula(1, 20, 10, "A-1", "Uno", new DateOnly(2026, 3, 19), null, true)
        });
        db.Matriculas.Returns(matriculas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, null, null, CancellationToken.None);

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
        var matriculas = MockDbSetHelper.CreateMockDbSet<Matricula>();
        db.Matriculas.Returns(matriculas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(999, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var matriculas = MockDbSetHelper.CreateMockDbSet(new[]
        {
            BuildMatricula(5, 20, 10, "A-1", "Uno", new DateOnly(2026, 3, 19), new DateOnly(2026, 12, 31), true)
        });
        db.Matriculas.Returns(matriculas);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 5L);
        AssertAnonymousProperty(ok.Value!, "TerceroId", 20L);
        AssertAnonymousProperty(ok.Value!, "SucursalId", 10L);
        AssertAnonymousProperty(ok.Value!, "NroMatricula", "A-1");
        AssertAnonymousProperty(ok.Value!, "Activa", true);
    }

    [Fact]
    public async Task Create_CuandoFallaPorDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateMatriculaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe una matricula con ese numero."));

        var result = await controller.Create(
            new CrearMatriculaRequest(20, 10, "A-1", "Uno", new DateOnly(2026, 3, 19), null),
            CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoFallaPorValidacion_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateMatriculaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("NroMatricula requerido."));

        var result = await controller.Create(
            new CrearMatriculaRequest(20, 10, "", "Uno", new DateOnly(2026, 3, 19), null),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<CreateMatriculaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(9L));

        var result = await controller.Create(
            new CrearMatriculaRequest(20, 10, "A-1", "Uno", new DateOnly(2026, 3, 19), null),
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetMatriculaById");
        AssertAnonymousProperty(created.Value!, "Id", 9L);
    }

    [Fact]
    public async Task Update_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateMatriculaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Matricula 8 no encontrada."));

        var result = await controller.Update(8, new ActualizarMatriculaRequest("Nueva", null), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_CuandoTieneExito_DevuelveOkConIdYMandaCommandCorrecto()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<UpdateMatriculaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var request = new ActualizarMatriculaRequest("Nueva", null);

        var result = await controller.Update(8, request, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 8L);
        await mediator.Received(1).Send(
            Arg.Is<UpdateMatriculaCommand>(command =>
                command.Id == 8 &&
                command.Descripcion == request.Descripcion &&
                command.FechaVencimiento == request.FechaVencimiento),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desactivar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateMatriculaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Matricula 8 no encontrada."));

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Desactivar_CuandoTieneExito_DevuelveOkYUsaCommand()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<DeactivateMatriculaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Desactivar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<DeactivateMatriculaCommand>(command => command.Id == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = CreateController(mediator);
        mediator.Send(Arg.Any<ActivateMatriculaCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await controller.Activar(8, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static MatriculasController CreateController(IMediator mediator, IApplicationDbContext? db = null)
    {
        var controller = new MatriculasController(mediator, db ?? Substitute.For<IApplicationDbContext>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static Matricula BuildMatricula(long id, long terceroId, long sucursalId, string nro, string? descripcion, DateOnly fechaAlta, DateOnly? fechaVencimiento, bool activa)
    {
        var entity = Matricula.Crear(terceroId, sucursalId, nro, descripcion, fechaAlta, fechaVencimiento, userId: null);
        typeof(Matricula).BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (!activa)
            entity.Desactivar(userId: null);

        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
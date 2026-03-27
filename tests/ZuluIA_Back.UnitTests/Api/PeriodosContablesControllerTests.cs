using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class PeriodosContablesControllerTests
{
    [Fact]
    public async Task GetAll_SinFiltro_DevuelveOrdenadosDescPorPeriodo()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var items = MockDbSetHelper.CreateMockDbSet([
            BuildPeriodo(1, "2026-02", new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), true),
            BuildPeriodo(2, "2026-03", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), false)
        ]);
        db.PeriodosContables.Returns(items);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 2L);
        AssertAnonymousProperty(data[0], "Periodo", "2026-03");
    }

    [Fact]
    public async Task GetAll_ConSoloAbiertos_FiltraCorrectamente()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var items = MockDbSetHelper.CreateMockDbSet([
            BuildPeriodo(1, "2026-02", new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), true),
            BuildPeriodo(2, "2026-03", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), false)
        ]);
        db.PeriodosContables.Returns(items);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(1);
        AssertAnonymousProperty(data[0], "Id", 1L);
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var periodos = MockDbSetHelper.CreateMockDbSet(new List<PeriodoContable>());
        db.PeriodosContables.Returns(periodos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var periodos = MockDbSetHelper.CreateMockDbSet([
            BuildPeriodo(3, "2026-03", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), true)
        ]);
        db.PeriodosContables.Returns(periodos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetById(3, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 3L);
        AssertAnonymousProperty(ok.Value!, "Periodo", "2026-03");
        AssertAnonymousProperty(ok.Value!, "Abierto", true);
    }

    [Fact]
    public async Task Create_CuandoHayDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreatePeriodoContableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un período contable '2026-03'."));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new PeriodoContableRequest("2026-03", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CreatePeriodoContableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(10L));
        var controller = CreateController(mediator, db);

        var result = await controller.Create(new PeriodoContableRequest("2026-03", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetPeriodoContableById");
        AssertAnonymousProperty(created.Value!, "Id", 10L);
    }

    [Fact]
    public async Task Cerrar_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CerrarPeriodoContableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PeriodoContableEstadoResult>("Período contable 4 no encontrado."));
        var controller = CreateController(mediator, db);

        var result = await controller.Cerrar(4, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Cerrar_CuandoTieneExito_DevuelveOkConEstado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<CerrarPeriodoContableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new PeriodoContableEstadoResult(4, "2026-03", false)));
        var controller = CreateController(mediator, db);

        var result = await controller.Cerrar(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 4L);
        AssertAnonymousProperty(ok.Value!, "Periodo", "2026-03");
        AssertAnonymousProperty(ok.Value!, "abierto", false);
    }

    [Fact]
    public async Task Abrir_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AbrirPeriodoContableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PeriodoContableEstadoResult>("El período contable ya está abierto."));
        var controller = CreateController(mediator, db);

        var result = await controller.Abrir(4, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Abrir_CuandoTieneExito_DevuelveOkConEstado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        mediator.Send(Arg.Any<AbrirPeriodoContableCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new PeriodoContableEstadoResult(4, "2026-03", true)));
        var controller = CreateController(mediator, db);

        var result = await controller.Abrir(4, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 4L);
        AssertAnonymousProperty(ok.Value!, "Periodo", "2026-03");
        AssertAnonymousProperty(ok.Value!, "abierto", true);
    }

    [Fact]
    public async Task GetActivo_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var periodos = MockDbSetHelper.CreateMockDbSet([
            BuildPeriodo(1, "2026-02", new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), false)
        ]);
        db.PeriodosContables.Returns(periodos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetActivo(CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetActivo_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var periodos = MockDbSetHelper.CreateMockDbSet([
            BuildPeriodo(1, "2026-02", new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), false),
            BuildPeriodo(2, "2026-03", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), true)
        ]);
        db.PeriodosContables.Returns(periodos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetActivo(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 2L);
        AssertAnonymousProperty(ok.Value!, "Periodo", "2026-03");
    }

    private static PeriodosContablesController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new PeriodosContablesController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static PeriodoContable BuildPeriodo(long id, string periodo, DateOnly fechaInicio, DateOnly fechaFin, bool abierto)
    {
        var entity = PeriodoContable.Crear(periodo, fechaInicio, fechaFin);
        entity.GetType().GetProperty(nameof(PeriodoContable.Id))!.SetValue(entity, id);
        entity.GetType().GetProperty(nameof(PeriodoContable.Abierto))!.SetValue(entity, abierto);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
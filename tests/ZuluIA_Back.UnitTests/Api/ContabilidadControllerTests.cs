using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Application.Features.Contabilidad.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ContabilidadControllerTests
{
    [Fact]
    public async Task GetAsientos_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var db = BuildDb();
        var paged = new PagedResult<AsientoListDto>(
            [new AsientoListDto { Id = 7, EjercicioId = 2026, SucursalId = 3, Numero = 15, Estado = "Confirmado" }],
            2,
            10,
            12);
        mediator.Send(Arg.Any<GetAsientosPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAsientos(2, 10, 2026, 3, EstadoAsiento.Confirmado, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetAsientosPagedQuery>(query =>
                query.Page == 2 &&
                query.PageSize == 10 &&
                query.EjercicioId == 2026 &&
                query.SucursalId == 3 &&
                query.Estado == EstadoAsiento.Confirmado &&
                query.Desde == new DateOnly(2026, 3, 1) &&
                query.Hasta == new DateOnly(2026, 3, 31)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsientoById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = BuildDb();
        mediator.Send(Arg.Any<GetAsientoByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns((AsientoDto?)null);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAsientoById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAsientoById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var db = BuildDb();
        var dto = new AsientoDto
        {
            Id = 7,
            EjercicioId = 2026,
            SucursalId = 3,
            Fecha = new DateOnly(2026, 3, 21),
            Numero = 15,
            Descripcion = "Asiento manual",
            Estado = "CONFIRMADO",
            TotalDebe = 100m,
            TotalHaber = 100m,
            Cuadra = true
        };
        mediator.Send(Arg.Any<GetAsientoByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAsientoById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task CreateAsiento_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        var db = BuildDb();
        mediator.Send(Arg.Any<CreateAsientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El asiento no balancea."));
        var controller = CreateController(mediator, db);
        var command = BuildCreateCommand();

        var result = await controller.CreateAsiento(command, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "El asiento no balancea.");
    }

    [Fact]
    public async Task CreateAsiento_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        var db = BuildDb();
        mediator.Send(Arg.Any<CreateAsientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator, db);
        var command = BuildCreateCommand();

        var result = await controller.CreateAsiento(command, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetAsientoById");
        created.RouteValues!["id"].Should().Be(21L);
        AssertAnonymousProperty(created.Value!, "id", 21L);
        await mediator.Received(1).Send(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMayor_CuandoFaltanParametrosRequeridos_DevuelveBadRequest()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.GetMayor(0, 2026, null, null, 1, 50, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "cuentaId y ejercicioId son requeridos.");
    }

    [Fact]
    public async Task GetMayor_CuandoHayDatos_DevuelvePaginadoOrdenado()
    {
        var asiento1 = BuildAsiento(1, 2026, 3, new DateOnly(2026, 3, 20), 10, "A1");
        var asiento2 = BuildAsiento(2, 2026, 3, new DateOnly(2026, 3, 21), 5, "A2");
        var asiento3 = BuildAsiento(3, 2025, 3, new DateOnly(2026, 3, 22), 30, "A3");
        AsientoLinea[] lineas =
        [
            BuildAsientoLinea(1, 1, 100, 100m, 0m, "Debe", 1, 9),
            BuildAsientoLinea(2, 2, 100, 0m, 50m, "Haber", 1, 9),
            BuildAsientoLinea(3, 2, 200, 20m, 0m, "Otra cuenta", 2, 9),
            BuildAsientoLinea(4, 3, 100, 70m, 0m, "Otro ejercicio", 1, 8)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb([asiento1, asiento2, asiento3], lineas));

        var result = await controller.GetMayor(100, 2026, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 1, 10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "page", 1);
        AssertAnonymousProperty(ok.Value!, "pageSize", 10);
        AssertAnonymousProperty(ok.Value!, "totalCount", 2);
        AssertAnonymousProperty(ok.Value!, "totalPages", 1);
        var items = ((IEnumerable)ok.Value!.GetType().GetProperty("lineas")!.GetValue(ok.Value)!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "AsientoId", 1L);
        AssertAnonymousProperty(items[0], "Numero", 10L);
        AssertAnonymousProperty(items[0], "LineaDescripcion", "Debe");
        AssertAnonymousProperty(items[1], "AsientoId", 2L);
        AssertAnonymousProperty(items[1], "Numero", 5L);
    }

    [Fact]
    public async Task GetMayorPorCentroCosto_CuandoHayDatos_DevuelveFiltradosOrdenados()
    {
        var asiento1 = BuildAsiento(1, 2026, 3, new DateOnly(2026, 3, 19), 8, "A1");
        var asiento2 = BuildAsiento(2, 2026, 3, new DateOnly(2026, 3, 21), 3, "A2");
        AsientoLinea[] lineas =
        [
            BuildAsientoLinea(1, 1, 100, 10m, 0m, "CC 9", 1, 9),
            BuildAsientoLinea(2, 1, 100, 5m, 0m, "CC 8", 2, 8),
            BuildAsientoLinea(3, 2, 100, 15m, 0m, "CC 9 segunda", 1, 9)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb([asiento1, asiento2], lineas));

        var result = await controller.GetMayorPorCentroCosto(9, 100, 2026, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 1L);
        AssertAnonymousProperty(items[0], "Numero", 8L);
        AssertAnonymousProperty(items[1], "Id", 3L);
        AssertAnonymousProperty(items[1], "Numero", 3L);
        AssertAnonymousProperty(items[1], "CentroCostoId", 9L);
    }

    private static ContabilidadController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new ContabilidadController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static IApplicationDbContext BuildDb(IEnumerable<Asiento>? asientos = null, IEnumerable<AsientoLinea>? lineas = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var asientosDbSet = MockDbSetHelper.CreateMockDbSet(asientos);
        var lineasDbSet = MockDbSetHelper.CreateMockDbSet(lineas);
        db.Asientos.Returns(asientosDbSet);
        db.AsientosLineas.Returns(lineasDbSet);
        return db;
    }

    private static CreateAsientoCommand BuildCreateCommand()
    {
        return new CreateAsientoCommand(
            2026,
            3,
            new DateOnly(2026, 3, 21),
            "Asiento manual",
            "comprobantes",
            10,
            [
                new CreateAsientoLineaDto(10, 100m, 0m, "Debe", 1),
                new CreateAsientoLineaDto(20, 0m, 100m, "Haber", 2)
            ]);
    }

    private static Asiento BuildAsiento(long id, long ejercicioId, long sucursalId, DateOnly fecha, long numero, string descripcion)
    {
        var entity = Asiento.Crear(ejercicioId, sucursalId, fecha, numero, descripcion, null, null, 1);
        SetProperty(entity, nameof(Asiento.Id), id);
        return entity;
    }

    private static AsientoLinea BuildAsientoLinea(long id, long asientoId, long cuentaId, decimal debe, decimal haber, string? descripcion, short orden, long? centroCostoId)
    {
        var entity = AsientoLinea.Crear(asientoId, cuentaId, debe, haber, descripcion, orden, centroCostoId);
        SetProperty(entity, nameof(AsientoLinea.Id), id);
        return entity;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
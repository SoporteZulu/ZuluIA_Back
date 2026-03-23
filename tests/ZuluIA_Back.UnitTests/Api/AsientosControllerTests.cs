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

public class AsientosControllerTests
{
    [Fact]
    public async Task GetAll_CuandoHayDatos_DevuelveOkConPaginado()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<AsientoListDto>(
            [
                new AsientoListDto
                {
                    Id = 1,
                    EjercicioId = 2026,
                    SucursalId = 3,
                    Fecha = new DateOnly(2026, 3, 21),
                    Numero = 10,
                    Descripcion = "Asiento manual",
                    Estado = "CONFIRMADO",
                    TotalDebe = 100m,
                    TotalHaber = 100m
                }
            ],
            2,
            15,
            31);
        mediator.Send(Arg.Any<GetAsientosPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator: mediator);

        var result = await controller.GetAll(2026, 2, 15, 3, "confirmado", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<PagedResult<AsientoListDto>>().Subject;
        payload.Page.Should().Be(2);
        payload.PageSize.Should().Be(15);
        payload.TotalCount.Should().Be(31);

        await mediator.Received(1).Send(
            Arg.Is<GetAsientosPagedQuery>(q =>
                q.Page == 2 &&
                q.PageSize == 15 &&
                q.EjercicioId == 2026 &&
                q.SucursalId == 3 &&
                q.Estado == EstadoAsiento.Confirmado &&
                q.Desde == new DateOnly(2026, 3, 1) &&
                q.Hasta == new DateOnly(2026, 3, 31)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAsientoDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns((AsientoDto?)null);
        var controller = CreateController(mediator: mediator);

        var result = await controller.GetById(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetAsientoDetalleQuery>(), Arg.Any<CancellationToken>())
            .Returns(new AsientoDto
            {
                Id = 7,
                EjercicioId = 2026,
                EjercicioDescripcion = "Ejercicio 2026",
                SucursalId = 3,
                Fecha = new DateOnly(2026, 3, 21),
                Numero = 15,
                Descripcion = "Asiento manual",
                Estado = "CONFIRMADO",
                TotalDebe = 100m,
                TotalHaber = 100m,
                Cuadra = true,
                Lineas =
                [
                    new AsientoLineaDto { Id = 1, CuentaId = 10, CuentaCodigo = "1.1.1", CuentaDenominacion = "Caja", Debe = 100m, Orden = 1 },
                    new AsientoLineaDto { Id = 2, CuentaId = 20, CuentaCodigo = "2.1.1", CuentaDenominacion = "Ventas", Haber = 100m, Orden = 2 }
                ]
            });
        var controller = CreateController(mediator: mediator);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<AsientoDto>().Which.Id.Should().Be(7);
    }

    [Fact]
    public async Task GetByOrigen_CuandoFaltaOrigenTabla_DevuelveBadRequest()
    {
        var controller = CreateController();

        var result = await controller.GetByOrigen(" ", 10, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("origenTabla es obligatorio");
    }

    [Fact]
    public async Task GetByOrigen_CuandoHayDatos_DevuelveFiltradosOrdenados()
    {
        var db = BuildDb(
            asientos:
            [
                BuildAsiento(1, 2026, 3, new DateOnly(2026, 3, 20), 11, "A1", EstadoAsiento.Confirmado, "comprobantes", 10),
                BuildAsiento(2, 2026, 3, new DateOnly(2026, 3, 22), 12, "A2", EstadoAsiento.Anulado, "comprobantes", 10),
                BuildAsiento(3, 2026, 3, new DateOnly(2026, 3, 21), 13, "A3", EstadoAsiento.Confirmado, "pagos", 10)
            ]);
        var controller = CreateController(db: db);

        var result = await controller.GetByOrigen("comprobantes", 10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[0], "Estado", "ANULADO");
        AssertAnonymousProperty(items[1], "Id", 1L);
    }

    [Fact]
    public async Task GetLibroDiario_CuandoHayDatos_DevuelveTotalesYLineasEnriquecidas()
    {
        var asiento1 = BuildAsiento(1, 2026, 3, new DateOnly(2026, 3, 20), 10, "A1", EstadoAsiento.Borrador);
        AddLinea(asiento1, BuildAsientoLinea(1, 1, 10, 100m, 0m, "Debe", 1));
        AddLinea(asiento1, BuildAsientoLinea(2, 1, 20, 0m, 100m, "Haber", 2));
        SetProperty(asiento1, nameof(Asiento.Estado), EstadoAsiento.Confirmado);

        var asiento2 = BuildAsiento(2, 2026, 3, new DateOnly(2026, 3, 21), 11, "A2", EstadoAsiento.Borrador);
        AddLinea(asiento2, BuildAsientoLinea(3, 2, 10, 50m, 0m, "Debe", 1));
        AddLinea(asiento2, BuildAsientoLinea(4, 2, 30, 0m, 50m, "Haber", 2));
        SetProperty(asiento2, nameof(Asiento.Estado), EstadoAsiento.Confirmado);

        var db = BuildDb(
            asientos: [asiento1, asiento2, BuildAsiento(3, 2026, 3, new DateOnly(2026, 3, 21), 12, "A3", EstadoAsiento.Borrador)],
            planCuentas:
            [
                BuildPlanCuenta(10, 2026, "1.1.1", "Caja"),
                BuildPlanCuenta(20, 2026, "4.1.1", "Ventas"),
                BuildPlanCuenta(30, 2026, "2.1.1", "Proveedores")
            ]);
        var controller = CreateController(db: db);

        var result = await controller.GetLibroDiario(2026, 3, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "ejercicioId", 2026L);
        AssertAnonymousProperty(ok.Value!, "sucursalId", 3L);
        AssertAnonymousProperty(ok.Value!, "totalAsientos", 2);
        AssertAnonymousProperty(ok.Value!, "totalDebe", 150m);
        AssertAnonymousProperty(ok.Value!, "totalHaber", 150m);

        var asientos = ((IEnumerable)ok.Value!.GetType().GetProperty("asientos")!.GetValue(ok.Value)!).Cast<object>().ToList();
        asientos.Should().HaveCount(2);
        var lineas = ((IEnumerable)asientos[0].GetType().GetProperty("Lineas")!.GetValue(asientos[0])!).Cast<object>().ToList();
        AssertAnonymousProperty(lineas[0], "CuentaCodigo", "1.1.1");
        AssertAnonymousProperty(lineas[0], "CuentaDenominacion", "Caja");
    }

    [Fact]
    public async Task GetBalance_CuandoHayResultado_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetBalanceSumasYSaldosQuery>(), Arg.Any<CancellationToken>())
            .Returns(new BalanceSumasYSaldosDto
            {
                EjercicioId = 2026,
                EjercicioDescripcion = "Ejercicio 2026",
                Desde = new DateOnly(2026, 3, 1),
                Hasta = new DateOnly(2026, 3, 31),
                TotalDebe = 100m,
                TotalHaber = 100m,
                Lineas = [new BalanceLineaDto { CuentaId = 10, CodigoCuenta = "1.1.1", Denominacion = "Caja", SumasDebe = 100m }]
            });
        var controller = CreateController(mediator: mediator);

        var result = await controller.GetBalance(2026, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 3, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<BalanceSumasYSaldosDto>().Which.EjercicioId.Should().Be(2026);
        await mediator.Received(1).Send(
            Arg.Is<GetBalanceSumasYSaldosQuery>(q => q.EjercicioId == 2026 && q.SucursalId == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Registrar_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarAsientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El asiento no balancea."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Registrar(BuildRegistrarAsientoCommand(), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("no balancea");
    }

    [Fact]
    public async Task Registrar_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<RegistrarAsientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(21L));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Registrar(BuildRegistrarAsientoCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetAsientoById");
        AssertAnonymousProperty(created.Value!, "id", 21L);
    }

    [Fact]
    public async Task Anular_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularAsientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El asiento ya está anulado."));
        var controller = CreateController(mediator: mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ya está anulado");
    }

    [Fact]
    public async Task Anular_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AnularAsientoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator: mediator);

        var result = await controller.Anular(7, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public void GetEstados_DevuelveValoresEsperados()
    {
        var controller = CreateController();

        var result = controller.GetEstados();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(4);
        AssertAnonymousProperty(items[0], "valor", "BORRADOR");
        AssertAnonymousProperty(items[0], "descripcion", "Borrador");
        AssertAnonymousProperty(items[3], "valor", "ANULADO");
    }

    private static AsientosController CreateController(IMediator? mediator = null, IApplicationDbContext? db = null)
    {
        var controller = new AsientosController(mediator ?? Substitute.For<IMediator>(), db ?? BuildDb())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static IApplicationDbContext BuildDb(IEnumerable<Asiento>? asientos = null, IEnumerable<PlanCuenta>? planCuentas = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var asientosDbSet = MockDbSetHelper.CreateMockDbSet(asientos);
        var planCuentasDbSet = MockDbSetHelper.CreateMockDbSet(planCuentas);
        db.Asientos.Returns(asientosDbSet);
        db.PlanCuentas.Returns(planCuentasDbSet);
        return db;
    }

    private static Asiento BuildAsiento(long id, long ejercicioId, long sucursalId, DateOnly fecha, long numero, string descripcion, EstadoAsiento estado, string? origenTabla = null, long? origenId = null)
    {
        var entity = Asiento.Crear(ejercicioId, sucursalId, fecha, numero, descripcion, origenTabla, origenId, 1);
        SetProperty(entity, nameof(Asiento.Id), id);
        SetProperty(entity, nameof(Asiento.Estado), estado);
        return entity;
    }

    private static AsientoLinea BuildAsientoLinea(long id, long asientoId, long cuentaId, decimal debe, decimal haber, string descripcion, short orden)
    {
        var entity = AsientoLinea.Crear(asientoId, cuentaId, debe, haber, descripcion, orden);
        SetProperty(entity, nameof(AsientoLinea.Id), id);
        return entity;
    }

    private static PlanCuenta BuildPlanCuenta(long id, long ejercicioId, string codigo, string denominacion)
    {
        var entity = PlanCuenta.Crear(ejercicioId, null, codigo, denominacion, 3, codigo, true, null, null);
        SetProperty(entity, nameof(PlanCuenta.Id), id);
        return entity;
    }

    private static void AddLinea(Asiento asiento, AsientoLinea linea)
    {
        asiento.AgregarLinea(linea);
    }

    private static RegistrarAsientoCommand BuildRegistrarAsientoCommand()
    {
        return new RegistrarAsientoCommand(
            2026,
            3,
            new DateOnly(2026, 3, 21),
            "Asiento manual",
            "comprobantes",
            10,
            [
                new LineaAsientoInput(10, 100m, 0m, "Debe", null),
                new LineaAsientoInput(20, 0m, 100m, "Haber", null)
            ]);
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
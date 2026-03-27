using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Rankings.Queries;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ThorControllerTests
{
    [Fact]
    public async Task Dashboard_DevuelveOkYMandaQueriesCorrectas()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        List<RankingClienteDto> clientes = [new(10, 1500m, 3)];
        List<RankingItemDto> items = [new(20, 2200m, 7)];
        mediator.Send(Arg.Any<GetRankingClientesQuery>(), Arg.Any<CancellationToken>()).Returns(clientes);
        mediator.Send(Arg.Any<GetRankingItemsQuery>(), Arg.Any<CancellationToken>()).Returns(items);
        var controller = CreateController(mediator, db);
        var desde = new DateOnly(2026, 1, 1);
        var hasta = new DateOnly(2026, 1, 31);

        var result = await controller.Dashboard(4, desde, hasta, 8, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Top", 8);
        var periodo = ok.Value!.GetType().GetProperty("Periodo")!.GetValue(ok.Value);
        AssertAnonymousProperty(periodo!, "desde", desde);
        AssertAnonymousProperty(periodo!, "hasta", hasta);
        ok.Value!.GetType().GetProperty("RankingClientes")!.GetValue(ok.Value).Should().BeSameAs(clientes);
        ok.Value!.GetType().GetProperty("RankingItems")!.GetValue(ok.Value).Should().BeSameAs(items);

        await mediator.Received(1).Send(
            Arg.Is<GetRankingClientesQuery>(query =>
                query.SucursalId == 4 &&
                query.Desde == desde &&
                query.Hasta == hasta &&
                query.Top == 8),
            Arg.Any<CancellationToken>());
        await mediator.Received(1).Send(
            Arg.Is<GetRankingItemsQuery>(query =>
                query.SucursalId == 4 &&
                query.Desde == desde &&
                query.Hasta == hasta &&
                query.Top == 8),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCubos_AplicaFiltrosDeUsuarioYAmbiente()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cubos = MockDbSetHelper.CreateMockDbSet([
            BuildCubo(1, "Ventas", 2, false, 11),
            BuildCubo(2, "Compras", 2, true, null),
            BuildCubo(3, "Stock", 1, false, 11),
            BuildCubo(4, "Sueldos", 2, false, 99)
        ]);
        db.Cubos.Returns(cubos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetCubos(11, 2, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "Id", 2L);
        AssertAnonymousProperty(data[0], "Descripcion", "Compras");
        AssertAnonymousProperty(data[1], "Id", 1L);
        AssertAnonymousProperty(data[1], "Descripcion", "Ventas");
    }

    [Fact]
    public async Task GetCubos_SinFiltros_DevuelveTodosOrdenadosPorDescripcion()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cubos = MockDbSetHelper.CreateMockDbSet([
            BuildCubo(1, "Ventas", 2, false, 11),
            BuildCubo(2, "Compras", 2, true, null),
            BuildCubo(3, "Analitica", 1, false, 99)
        ]);
        db.Cubos.Returns(cubos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetCubos(null, null, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(3);
        AssertAnonymousProperty(data[0], "Descripcion", "Analitica");
        AssertAnonymousProperty(data[1], "Descripcion", "Compras");
        AssertAnonymousProperty(data[2], "Descripcion", "Ventas");
    }

    [Fact]
    public async Task GetCubos_CuandoFiltraSoloPorAmbiente_DevuelveCoincidencias()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cubos = MockDbSetHelper.CreateMockDbSet([
            BuildCubo(1, "Ventas", 2, false, 11),
            BuildCubo(2, "Compras", 2, true, null),
            BuildCubo(3, "Analitica", 1, false, 11)
        ]);
        db.Cubos.Returns(cubos);
        var controller = CreateController(mediator, db);

        var result = await controller.GetCubos(null, 2, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        data.Should().HaveCount(2);
        AssertAnonymousProperty(data[0], "AmbienteId", 2);
        AssertAnonymousProperty(data[1], "AmbienteId", 2);
    }

    [Fact]
    public async Task GetCuboById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cubos = MockDbSetHelper.CreateMockDbSet(new List<Cubo>());
        var campos = MockDbSetHelper.CreateMockDbSet(new List<CuboCampo>());
        var filtros = MockDbSetHelper.CreateMockDbSet(new List<CuboFiltro>());
        db.Cubos.Returns(cubos);
        db.CubosCampos.Returns(campos);
        db.CubosFiltros.Returns(filtros);
        var controller = CreateController(mediator, db);

        var result = await controller.GetCuboById(55, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.Value.Should().NotBeNull();
        AssertAnonymousProperty(notFound.Value!, "error", "Cubo 55 no encontrado.");
    }

    [Fact]
    public async Task GetCuboById_CuandoExiste_DevuelveDetalleOrdenado()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        var cubos = MockDbSetHelper.CreateMockDbSet([
            BuildCubo(5, "Ventas", 3, false, 7, "select * from ventas", "principal", 44)
        ]);
        var campos = MockDbSetHelper.CreateMockDbSet([
            BuildCuboCampo(10, 5, "cliente", "Cliente", 2),
            BuildCuboCampo(11, 5, "importe", "Importe", 1, visible: false, calculado: true)
        ]);
        var filtros = MockDbSetHelper.CreateMockDbSet([
            BuildCuboFiltro(20, 5, "anio = 2026", 1, 2),
            BuildCuboFiltro(21, 5, "mes = 1", 2, 1)
        ]);
        db.Cubos.Returns(cubos);
        db.CubosCampos.Returns(campos);
        db.CubosFiltros.Returns(filtros);
        var controller = CreateController(mediator, db);

        var result = await controller.GetCuboById(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var cubo = ok.Value!.GetType().GetProperty("Cubo")!.GetValue(ok.Value);
        AssertAnonymousProperty(cubo!, "Id", 5L);
        AssertAnonymousProperty(cubo!, "Descripcion", "Ventas");
        AssertAnonymousProperty(cubo!, "AmbienteId", 3);
        AssertAnonymousProperty(cubo!, "OrigenDatos", "select * from ventas");
        AssertAnonymousProperty(cubo!, "Observacion", "principal");
        AssertAnonymousProperty(cubo!, "UsuarioAltaId", 7L);

        var camposData = ok.Value!.GetType().GetProperty("Campos")!.GetValue(ok.Value)
            .Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        camposData.Should().HaveCount(2);
        AssertAnonymousProperty(camposData[0], "Id", 11L);
        AssertAnonymousProperty(camposData[0], "Orden", 1);
        AssertAnonymousProperty(camposData[0], "Visible", false);
        AssertAnonymousProperty(camposData[0], "Calculado", true);
        AssertAnonymousProperty(camposData[1], "Id", 10L);

        var filtrosData = ok.Value!.GetType().GetProperty("Filtros")!.GetValue(ok.Value)
            .Should().BeAssignableTo<System.Collections.IEnumerable>().Subject.Cast<object>().ToList();
        filtrosData.Should().HaveCount(2);
        AssertAnonymousProperty(filtrosData[0], "Id", 21L);
        AssertAnonymousProperty(filtrosData[0], "Orden", 1);
        AssertAnonymousProperty(filtrosData[1], "Id", 20L);
    }

    [Fact]
    public async Task AnalisisMensual_DevuelveOkYMandaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var db = Substitute.For<IApplicationDbContext>();
        List<AnalisisMensualDto> data = [new(2026, 2, 3100m, 12)];
        mediator.Send(Arg.Any<GetAnalisisMensualQuery>(), Arg.Any<CancellationToken>())
            .Returns(data);
        var controller = CreateController(mediator, db);

        var result = await controller.AnalisisMensual(9, 2026, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
        await mediator.Received(1).Send(
            Arg.Is<GetAnalisisMensualQuery>(query => query.SucursalId == 9 && query.Anio == 2026),
            Arg.Any<CancellationToken>());
    }

    private static ThorController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new ThorController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static Cubo BuildCubo(long id, string descripcion, int ambienteId, bool esSistema, long? usuarioAltaId, string? origenDatos = null, string? observacion = null, long? menuCuboId = null)
    {
        var entity = Cubo.Crear(descripcion, origenDatos, observacion, ambienteId, menuCuboId, null, esSistema, null, usuarioAltaId);
        entity.GetType().GetProperty(nameof(Cubo.Id))!.SetValue(entity, id);
        return entity;
    }

    private static CuboCampo BuildCuboCampo(long id, long cuboId, string sourceName, string descripcion, int orden, bool visible = true, bool calculado = false)
    {
        var entity = CuboCampo.Crear(cuboId, sourceName, descripcion, orden: orden, visible: visible, calculado: calculado);
        entity.GetType().GetProperty(nameof(CuboCampo.Id))!.SetValue(entity, id);
        return entity;
    }

    private static CuboFiltro BuildCuboFiltro(long id, long cuboId, string filtro, int operador, int orden)
    {
        var entity = CuboFiltro.Crear(cuboId, filtro, operador, orden);
        entity.GetType().GetProperty(nameof(CuboFiltro.Id))!.SetValue(entity, id);
        return entity;
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
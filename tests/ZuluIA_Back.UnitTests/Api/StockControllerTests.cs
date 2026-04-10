using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Stock.Commands;
using ZuluIA_Back.Application.Features.Stock.DTOs;
using ZuluIA_Back.Application.Features.Stock.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class StockControllerTests
{
    [Fact]
    public async Task GetByItem_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetStockByItemQuery>(), Arg.Any<CancellationToken>())
            .Returns((StockResumenDto?)null);
        var controller = CreateController(mediator);

        var result = await controller.GetByItem(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByItem_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetStockByItemQuery>(), Arg.Any<CancellationToken>())
            .Returns(new StockResumenDto
            {
                ItemId = 7,
                ItemCodigo = "ITEM-7",
                ItemDescripcion = "Producto 7",
                StockTotal = 12.5m,
                PorDeposito =
                [
                    new StockPorDepositoDto
                    {
                        DepositoId = 10,
                        DepositoDescripcion = "Principal",
                        Cantidad = 12.5m
                    }
                ]
            });
        var controller = CreateController(mediator);

        var result = await controller.GetByItem(7, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<StockResumenDto>().Which.ItemId.Should().Be(7);
    }

    [Fact]
    public async Task GetByDeposito_CuandoHayDatos_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetStockByDepositoQuery>(), Arg.Any<CancellationToken>())
            .Returns([
                new StockItemDto { Id = 1, ItemId = 7, ItemCodigo = "ITEM-7", ItemDescripcion = "Producto 7", DepositoId = 5, DepositoDescripcion = "Principal", Cantidad = 3m },
                new StockItemDto { Id = 2, ItemId = 8, ItemCodigo = "ITEM-8", ItemDescripcion = "Producto 8", DepositoId = 5, DepositoDescripcion = "Principal", Cantidad = 1m }
            ]);
        var controller = CreateController(mediator);

        var result = await controller.GetByDeposito(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<StockItemDto>>().Subject.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByItemDeposito_CuandoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(db: BuildDb(stock: Array.Empty<StockItem>()));

        var result = await controller.GetByItemDeposito(7, 10, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByItemDeposito_CuandoExiste_DevuelveProjectionOk()
    {
        var updatedAt = new DateTimeOffset(2026, 3, 21, 12, 0, 0, TimeSpan.Zero);
        var controller = CreateController(db: BuildDb(stock:
        [
            BuildStockItem(1, 7, 10, 8m, updatedAt),
            BuildStockItem(2, 7, 11, 5m, updatedAt.AddHours(-1))
        ]));

        var result = await controller.GetByItemDeposito(7, 10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 1L);
        AssertAnonymousProperty(ok.Value!, "ItemId", 7L);
        AssertAnonymousProperty(ok.Value!, "DepositoId", 10L);
        AssertAnonymousProperty(ok.Value!, "Cantidad", 8m);
        AssertAnonymousProperty(ok.Value!, "UpdatedAt", updatedAt);
    }

    [Fact]
    public async Task GetBajoMinimo_CuandoFiltra_DevuelveOkYMandaQuery()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetStockBajoMinimoQuery>(), Arg.Any<CancellationToken>())
            .Returns([
                new StockBajoMinimoDto
                {
                    ItemId = 7,
                    ItemCodigo = "ITEM-7",
                    ItemDescripcion = "Producto 7",
                    DepositoId = 10,
                    DepositoDescripcion = "Principal",
                    CantidadActual = 2m,
                    StockMinimo = 5m,
                    Diferencia = 3m
                }
            ]);
        var controller = CreateController(mediator);

        var result = await controller.GetBajoMinimo(3, 10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IReadOnlyList<StockBajoMinimoDto>>().Subject.Should().ContainSingle();
        await mediator.Received(1).Send(
            Arg.Is<GetStockBajoMinimoQuery>(q => q.SucursalId == 3 && q.DepositoId == 10),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetResumen_CuandoHayDatos_DevuelveTotalesCalculados()
    {
        var db = BuildDb(
            stock:
            [
                BuildStockItem(1, 100, 10, 2m),
                BuildStockItem(2, 101, 10, 0m),
                BuildStockItem(3, 102, 11, 5m),
                BuildStockItem(4, 103, 12, 9m)
            ],
            items:
            [
                BuildItem(100, "A", "Alfa", 5m, true, true),
                BuildItem(101, "B", "Beta", 1m, true, true),
                BuildItem(102, "C", "Gamma", 3m, true, true),
                BuildItem(103, "D", "Delta", 2m, true, true)
            ],
            depositos:
            [
                BuildDeposito(10, 5, "Principal", true),
                BuildDeposito(11, 5, "Secundario", true),
                BuildDeposito(12, 5, "Inactivo", false),
                BuildDeposito(13, 6, "Otra Sucursal", true)
            ]);
        var controller = CreateController(db: db);

        var result = await controller.GetResumen(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "sucursalId", 5L);
        AssertAnonymousProperty(ok.Value!, "totalItemsConStock", 2);
        AssertAnonymousProperty(ok.Value!, "itemsBajoMinimo", 2);
        AssertAnonymousProperty(ok.Value!, "itemsSinStock", 1);
        AssertAnonymousProperty(ok.Value!, "totalDepositos", 2);
    }

    [Fact]
    public async Task Ajuste_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AjusteStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("No se puede ajustar a un valor negativo."));
        var controller = CreateController(mediator);

        var result = await controller.Ajuste(new AjusteStockCommand(7, 10, -1m, "conteo"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("valor negativo");
    }

    [Fact]
    public async Task Ajuste_CuandoTieneExito_DevuelveOkConMovimientoId()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AjusteStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(33L));
        var controller = CreateController(mediator);

        var result = await controller.Ajuste(new AjusteStockCommand(7, 10, 8m, "conteo"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "movimientoId", 33L);
    }

    [Fact]
    public async Task Transferencia_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<TransferenciaStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Stock insuficiente."));
        var controller = CreateController(mediator);

        var result = await controller.Transferencia(new TransferenciaStockCommand(7, 10, 11, 5m, "traslado"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("Stock insuficiente");
    }

    [Fact]
    public async Task Transferencia_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<TransferenciaStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.Transferencia(new TransferenciaStockCommand(7, 10, 11, 5m, "traslado"), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task StockInicial_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<StockInicialCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<int>("Debe informar al menos un ítem."));
        var controller = CreateController(mediator);

        var result = await controller.StockInicial(new StockInicialCommand([], "inicio"), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("al menos un ítem");
    }

    [Fact]
    public async Task StockInicial_CuandoTieneExito_DevuelveOkConCantidadProcesada()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<StockInicialCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(2));
        var controller = CreateController(mediator);

        var result = await controller.StockInicial(
            new StockInicialCommand([new StockInicialItemDto(7, 10, 3m), new StockInicialItemDto(8, 10, 2m)], "inicio"),
            CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "itemsProcesados", 2);
    }

    [Fact]
    public async Task GetMovimientos_CuandoFiltraYPagina_DevuelveResultadoEsperado()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetMovimientosStockPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<MovimientoStockDto>(
            [
                new MovimientoStockDto
                {
                    Id = 1,
                    ItemId = 7,
                    ItemCodigo = "ITEM-7",
                    ItemDescripcion = "Producto 7",
                    DepositoId = 10,
                    DepositoDescripcion = "Principal",
                    Fecha = new DateTimeOffset(2026, 3, 21, 12, 0, 0, TimeSpan.Zero),
                    TipoMovimiento = TipoMovimientoStock.AjustePositivo.ToString(),
                    Cantidad = 2m,
                    SaldoResultante = 8m,
                    Observacion = "conteo"
                }
            ],
            1,
            1,
            2));
        var controller = CreateController(mediator);

        var result = await controller.GetMovimientos(7, 10, null, new DateOnly(2026, 3, 20), new DateOnly(2026, 3, 21), 1, 1, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var paged = ok.Value.Should().BeOfType<PagedResult<MovimientoStockDto>>().Subject;
        paged.Items.Should().ContainSingle();
        paged.Items[0].Id.Should().Be(1L);
        paged.Items[0].TipoMovimiento.Should().Be(TipoMovimientoStock.AjustePositivo.ToString());
        paged.Items[0].DepositoDescripcion.Should().Be("Principal");
        paged.Page.Should().Be(1);
        paged.PageSize.Should().Be(1);
        paged.TotalCount.Should().Be(2);
        paged.TotalPages.Should().Be(2);
        await mediator.Received(1).Send(
            Arg.Is<GetMovimientosStockPagedQuery>(q =>
                q.Page == 1 &&
                q.PageSize == 1 &&
                q.ItemId == 7 &&
                q.DepositoId == 10 &&
                q.Tipo == null &&
                q.Desde == new DateOnly(2026, 3, 20) &&
                q.Hasta == new DateOnly(2026, 3, 21)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetInventarios_CuandoSoloAbiertos_DevuelveFiltradosYOrdenados()
    {
        var db = BuildDb(inventarios:
        [
            BuildInventario(1, 9, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero), null, 100),
            BuildInventario(2, 8, new DateTimeOffset(2026, 3, 20, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 3, 21, 18, 0, 0, TimeSpan.Zero), 90),
            BuildInventario(3, 7, new DateTimeOffset(2026, 3, 22, 10, 0, 0, TimeSpan.Zero), null, 110)
        ]);
        var controller = CreateController(db: db);

        var result = await controller.GetInventarios(true, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 3L);
        AssertAnonymousProperty(items[0], "Cerrado", false);
        AssertAnonymousProperty(items[1], "Id", 1L);
    }

    [Fact]
    public async Task GetInventarioById_CuandoNoExiste_DevuelveNotFound()
    {
        var inventarios = MockDbSetHelper.CreateMockDbSet<InventarioConteo>();
        var controller = CreateController(db: BuildDb(inventariosDbSet: inventarios));

        var result = await controller.GetInventarioById(7, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Inventario 7 no encontrado");
    }

    [Fact]
    public async Task GetInventarioById_CuandoExiste_DevuelveOk()
    {
        var inventario = BuildInventario(7, 9, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero), null, 100);
        var inventarios = MockDbSetHelper.CreateMockDbSet(new[] { inventario });
        var controller = CreateController(db: BuildDb(inventariosDbSet: inventarios));

        var result = await controller.GetInventarioById(7, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(inventario);
    }

    [Fact]
    public async Task CreateInventario_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateInventarioConteoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Ya existe un inventario para la auditoria 100."));
        var controller = CreateController(mediator);

        var result = await controller.CreateInventario(
            new CreateInventarioConteoRequest(9, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero), 100),
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("auditoria 100");
    }

    [Fact]
    public async Task CreateInventario_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateInventarioConteoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(17L));
        var controller = CreateController(mediator);

        var result = await controller.CreateInventario(
            new CreateInventarioConteoRequest(9, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero), 100),
            CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetInventarioConteoById");
        AssertAnonymousProperty(created.Value!, "Id", 17L);
    }

    [Fact]
    public async Task CerrarInventario_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CerrarInventarioConteoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Inventario 17 no encontrado."));
        var controller = CreateController(mediator);

        var result = await controller.CerrarInventario(17, new CerrarInventarioRequest(new DateTimeOffset(2026, 3, 21, 18, 0, 0, TimeSpan.Zero)), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value!.ToString().Should().Contain("Inventario 17 no encontrado");
    }

    [Fact]
    public async Task CerrarInventario_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CerrarInventarioConteoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("El inventario ya fue cerrado."));
        var controller = CreateController(mediator);

        var result = await controller.CerrarInventario(17, new CerrarInventarioRequest(new DateTimeOffset(2026, 3, 21, 18, 0, 0, TimeSpan.Zero)), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value!.ToString().Should().Contain("ya fue cerrado");
    }

    [Fact]
    public async Task CerrarInventario_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CerrarInventarioConteoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator);

        var result = await controller.CerrarInventario(17, new CerrarInventarioRequest(new DateTimeOffset(2026, 3, 21, 18, 0, 0, TimeSpan.Zero)), CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static StockController CreateController(IMediator? mediator = null, IApplicationDbContext? db = null)
    {
        var controller = new StockController(mediator ?? Substitute.For<IMediator>(), db ?? BuildDb())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<StockItem>? stock = null,
        IEnumerable<Item>? items = null,
        IEnumerable<Deposito>? depositos = null,
        IEnumerable<MovimientoStock>? movimientos = null,
        IEnumerable<InventarioConteo>? inventarios = null,
        DbSet<InventarioConteo>? inventariosDbSet = null)
    {
        var db = Substitute.For<IApplicationDbContext>();

        var stockDbSet = MockDbSetHelper.CreateMockDbSet(stock);
        var itemsDbSet = MockDbSetHelper.CreateMockDbSet(items);
        var depositosDbSet = MockDbSetHelper.CreateMockDbSet(depositos);
        var movimientosDbSet = MockDbSetHelper.CreateMockDbSet(movimientos);
        var inventariosSet = inventariosDbSet ?? MockDbSetHelper.CreateMockDbSet(inventarios);

        db.Stock.Returns(stockDbSet);
        db.Items.Returns(itemsDbSet);
        db.Depositos.Returns(depositosDbSet);
        db.MovimientosStock.Returns(movimientosDbSet);
        db.InventariosConteo.Returns(inventariosSet);
        return db;
    }

    private static StockItem BuildStockItem(long id, long itemId, long depositoId, decimal cantidad, DateTimeOffset? updatedAt = null)
    {
        var entity = StockItem.Crear(itemId, depositoId, cantidad);
        SetProperty(entity, nameof(StockItem.Id), id);
        SetProperty(entity, nameof(StockItem.UpdatedAt), updatedAt ?? new DateTimeOffset(2026, 3, 21, 9, 0, 0, TimeSpan.Zero));
        return entity;
    }

    private static Item BuildItem(long id, string codigo, string descripcion, decimal stockMinimo, bool manejaStock, bool activo)
    {
        var entity = Item.Crear(codigo, descripcion, 1, 1, 1, true, false, false, manejaStock, 1m, 2m, null, stockMinimo, null, null, null, null, null, 1);
        SetProperty(entity, nameof(Item.Id), id);
        SetProperty(entity, nameof(Item.Activo), activo);
        SetProperty(entity, nameof(Item.ManejaStock), manejaStock);
        SetProperty(entity, nameof(Item.StockMinimo), stockMinimo);
        return entity;
    }

    private static Deposito BuildDeposito(long id, long sucursalId, string descripcion, bool activo)
    {
        var entity = Deposito.Crear(sucursalId, descripcion);
        SetProperty(entity, nameof(Deposito.Id), id);
        SetProperty(entity, nameof(Deposito.Activo), activo);
        return entity;
    }

    private static MovimientoStock BuildMovimiento(
        long id,
        long itemId,
        long depositoId,
        DateTimeOffset fecha,
        TipoMovimientoStock tipo,
        decimal cantidad,
        decimal saldo,
        string observacion)
    {
        var entity = MovimientoStock.Crear(itemId, depositoId, tipo, cantidad, saldo, observacion: observacion);
        SetProperty(entity, nameof(MovimientoStock.Id), id);
        SetProperty(entity, nameof(MovimientoStock.Fecha), fecha);
        return entity;
    }

    private static InventarioConteo BuildInventario(long id, long usuarioId, DateTimeOffset apertura, DateTimeOffset? cierre, int nroAuditoria)
    {
        var entity = InventarioConteo.Crear(usuarioId, apertura, nroAuditoria);
        SetProperty(entity, nameof(InventarioConteo.Id), id);
        SetProperty(entity, nameof(InventarioConteo.FechaCierre), cierre);
        SetProperty(entity, nameof(InventarioConteo.FechaAlta), apertura.AddHours(1));
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
using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

public class MovimientosStockControllerTests
{
    [Fact]
    public async Task GetAll_ParseaTipoYEnviaQueryCorrecta()
    {
        var mediator = Substitute.For<IMediator>();
        var db = BuildDb();
        var paged = new PagedResult<MovimientoStockDto>(
            [new MovimientoStockDto { Id = 1, ItemId = 10, DepositoId = 20, TipoMovimiento = "CompraRecepcion" }],
            2,
            25,
            40);
        mediator.Send(Arg.Any<GetMovimientosStockPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator, db);

        var result = await controller.GetAll(2, 25, 10, 20, "comprarecepcion", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetMovimientosStockPagedQuery>(query =>
                query.Page == 2 &&
                query.PageSize == 25 &&
                query.ItemId == 10 &&
                query.DepositoId == 20 &&
                query.Tipo == TipoMovimientoStock.CompraRecepcion &&
                query.Desde == new DateOnly(2026, 3, 1) &&
                query.Hasta == new DateOnly(2026, 3, 31)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAll_CuandoTipoEsInvalido_EnviaQueryConTipoNull()
    {
        var mediator = Substitute.For<IMediator>();
        var db = BuildDb();
        mediator.Send(Arg.Any<GetMovimientosStockPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<MovimientoStockDto>([], 1, 20, 0));
        var controller = CreateController(mediator, db);

        await controller.GetAll(1, 20, null, null, "inventado", null, null, CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<GetMovimientosStockPagedQuery>(query => query.Tipo == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByOrigen_CuandoFaltaOrigenTabla_DevuelveBadRequest()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.GetByOrigen(" ", 10, CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "El parámetro origenTabla es obligatorio.");
    }

    [Fact]
    public async Task GetByOrigen_FiltraYOrdenaPorFechaDesc()
    {
        MovimientoStock[] movimientos =
        [
            BuildMovimientoStock(1, 10, 20, TipoMovimientoStock.CompraRecepcion, 5m, 10m, new DateTimeOffset(2026, 3, 20, 10, 0, 0, TimeSpan.Zero), "comprobantes", 100, "uno", 7),
            BuildMovimientoStock(2, 10, 20, TipoMovimientoStock.AjustePositivo, 2m, 12m, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero), "comprobantes", 100, "dos", 8),
            BuildMovimientoStock(3, 10, 20, TipoMovimientoStock.AjusteNegativo, -1m, 11m, new DateTimeOffset(2026, 3, 22, 10, 0, 0, TimeSpan.Zero), "pagos", 100, "tres", 9)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(movimientos: movimientos));

        var result = await controller.GetByOrigen("comprobantes", 100, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Id", 2L);
        AssertAnonymousProperty(items[0], "TipoMovimiento", "AjustePositivo");
        AssertAnonymousProperty(items[1], "Id", 1L);
        AssertAnonymousProperty(items[1], "CreatedBy", 7L);
    }

    [Fact]
    public void GetTipos_DevuelveCatalogoEsperado()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = controller.GetTipos();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(Enum.GetValues<TipoMovimientoStock>().Length);
        var compra = items.Single(x => object.Equals(GetAnonymousProperty(x, "valor"), "CompraRecepcion"));
        AssertAnonymousProperty(compra, "descripcion", "Recepción de Compra");
        AssertAnonymousProperty(compra, "esIngreso", true);
        var egreso = items.Single(x => object.Equals(GetAnonymousProperty(x, "valor"), "VentaDespacho"));
        AssertAnonymousProperty(egreso, "esIngreso", false);
    }

    [Fact]
    public async Task GetEstadisticas_AgrupaYFiltraPorSucursal()
    {
        MovimientoStock[] movimientos =
        [
            BuildMovimientoStock(1, 10, 20, TipoMovimientoStock.CompraRecepcion, 5m, 10m, new DateTimeOffset(2026, 3, 20, 10, 0, 0, TimeSpan.Zero)),
            BuildMovimientoStock(2, 10, 20, TipoMovimientoStock.CompraRecepcion, 7m, 17m, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero)),
            BuildMovimientoStock(3, 10, 21, TipoMovimientoStock.AjusteNegativo, -2m, 15m, new DateTimeOffset(2026, 3, 22, 10, 0, 0, TimeSpan.Zero)),
            BuildMovimientoStock(4, 11, 30, TipoMovimientoStock.CompraRecepcion, 3m, 3m, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero))
        ];
        Deposito[] depositos =
        [
            BuildDeposito(20, 1, "Central"),
            BuildDeposito(21, 1, "Secundario"),
            BuildDeposito(30, 2, "Externo")
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(movimientos: movimientos, depositos: depositos));

        var result = await controller.GetEstadisticas(1, new DateOnly(2026, 3, 20), new DateOnly(2026, 3, 22), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "sucursalId", 1L);
        var stats = ((IEnumerable)ok.Value!.GetType().GetProperty("estadisticas")!.GetValue(ok.Value)!).Cast<object>().ToList();
        stats.Should().HaveCount(2);
        var compra = stats.Single(x => object.Equals(GetAnonymousProperty(x, "TipoMovimiento"), "CompraRecepcion"));
        AssertAnonymousProperty(compra, "ItemId", 10L);
        AssertAnonymousProperty(compra, "TotalMovimientos", 2);
        AssertAnonymousProperty(compra, "TotalCantidad", 12m);
        var ajuste = stats.Single(x => object.Equals(GetAnonymousProperty(x, "TipoMovimiento"), "AjusteNegativo"));
        AssertAnonymousProperty(ajuste, "TotalCantidad", -2m);
    }

    [Fact]
    public async Task GetAtributos_CuandoMovimientoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.GetAtributos(99, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAtributos_CuandoExiste_DevuelveAtributos()
    {
        MovimientoStock[] movimientos =
        [
            BuildMovimientoStock(5, 10, 20, TipoMovimientoStock.StockInicial, 10m, 10m, new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero))
        ];
        MovimientoStockAtributo[] atributos =
        [
            BuildMovimientoStockAtributo(1, 5, 7, "LOTE-1"),
            BuildMovimientoStockAtributo(2, 5, 8, "SERIE-2"),
            BuildMovimientoStockAtributo(3, 6, 9, "OTRO")
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(movimientos: movimientos, atributos: atributos));

        var result = await controller.GetAtributos(5, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "MovimientoStockId", 5L);
        AssertAnonymousProperty(items[1], "Valor", "SERIE-2");
    }

    [Fact]
    public async Task AddAtributo_CuandoMovimientoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddMovimientoStockAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("Movimiento de stock 5 no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddAtributo(5, new AddMovAtributoRequest(7, "LOTE"), CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "Movimiento de stock 5 no encontrado.");
    }

    [Fact]
    public async Task AddAtributo_CuandoFallaPorRegla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddMovimientoStockAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El valor es requerido."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddAtributo(5, new AddMovAtributoRequest(7, " "), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        AssertAnonymousProperty(badRequest.Value!, "error", "El valor es requerido.");
    }

    [Fact]
    public async Task AddAtributo_CuandoTieneExito_DevuelveCreatedAtAction()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddMovimientoStockAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(11L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddAtributo(5, new AddMovAtributoRequest(7, "LOTE"), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(MovimientosStockController.GetAtributos));
        created.RouteValues!["id"].Should().Be(5L);
        AssertAnonymousProperty(created.Value!, "Id", 11L);
    }

    [Fact]
    public async Task UpdateAtributo_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateMovimientoStockAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Atributo de movimiento 7 no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdateAtributo(5, 7, new UpdateMovAtributoRequest("nuevo"), CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "Atributo de movimiento 7 no encontrado.");
    }

    [Fact]
    public async Task UpdateAtributo_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateMovimientoStockAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdateAtributo(5, 7, new UpdateMovAtributoRequest("nuevo"), CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "Id", 7L);
        AssertAnonymousProperty(ok.Value!, "Valor", "nuevo");
    }

    [Fact]
    public async Task DeleteAtributo_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteMovimientoStockAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Atributo de movimiento 7 no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeleteAtributo(5, 7, CancellationToken.None);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        AssertAnonymousProperty(notFound.Value!, "error", "Atributo de movimiento 7 no encontrado.");
    }

    [Fact]
    public async Task DeleteAtributo_CuandoTieneExito_DevuelveNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteMovimientoStockAtributoCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeleteAtributo(5, 7, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    private static MovimientosStockController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new MovimientosStockController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<MovimientoStock>? movimientos = null,
        IEnumerable<MovimientoStockAtributo>? atributos = null,
        IEnumerable<Deposito>? depositos = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var movimientosDbSet = MockDbSetHelper.CreateMockDbSet(movimientos);
        var atributosDbSet = MockDbSetHelper.CreateMockDbSet(atributos);
        var depositosDbSet = MockDbSetHelper.CreateMockDbSet(depositos);
        db.MovimientosStock.Returns(movimientosDbSet);
        db.MovimientosStockAtributos.Returns(atributosDbSet);
        db.Depositos.Returns(depositosDbSet);
        return db;
    }

    private static MovimientoStock BuildMovimientoStock(long id, long itemId, long depositoId, TipoMovimientoStock tipo, decimal cantidad, decimal saldoResultante, DateTimeOffset fecha, string? origenTabla = null, long? origenId = null, string? observacion = null, long? createdBy = null)
    {
        var entity = MovimientoStock.Crear(itemId, depositoId, tipo, cantidad, saldoResultante, origenTabla, origenId, observacion, createdBy);
        SetProperty(entity, nameof(MovimientoStock.Id), id);
        SetProperty(entity, nameof(MovimientoStock.Fecha), fecha);
        SetProperty(entity, nameof(MovimientoStock.CreatedAt), fecha);
        SetProperty(entity, nameof(MovimientoStock.CreatedBy), createdBy);
        return entity;
    }

    private static MovimientoStockAtributo BuildMovimientoStockAtributo(long id, long movimientoStockId, long atributoId, string valor)
    {
        var entity = MovimientoStockAtributo.Crear(movimientoStockId, atributoId, valor);
        SetProperty(entity, nameof(MovimientoStockAtributo.Id), id);
        return entity;
    }

    private static Deposito BuildDeposito(long id, long sucursalId, string descripcion)
    {
        var entity = Deposito.Crear(sucursalId, descripcion);
        SetProperty(entity, nameof(Deposito.Id), id);
        return entity;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object expectedValue)
    {
        GetAnonymousProperty(item, propertyName).Should().Be(expectedValue);
    }

    private static object? GetAnonymousProperty(object item, string propertyName)
    {
        return item.GetType().GetProperty(propertyName)!.GetValue(item);
    }
}
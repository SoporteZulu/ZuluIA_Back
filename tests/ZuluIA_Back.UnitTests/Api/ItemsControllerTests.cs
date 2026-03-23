using System.Collections;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Api.Controllers;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Commands;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.Items.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Api;

public class ItemsControllerTests
{
    [Fact]
    public async Task GetAll_EnviaQueryCorrectaYDevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var paged = new PagedResult<ItemListDto>(
        [new ItemListDto { Id = 1, Codigo = "A1", Descripcion = "Articulo" }],
            2,
            25,
            40);
        mediator.Send(Arg.Any<GetItemsPagedQuery>(), Arg.Any<CancellationToken>())
            .Returns(paged);
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.GetAll(2, 25, "art", 3, true, false, true, false, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(paged);
        await mediator.Received(1).Send(
            Arg.Is<GetItemsPagedQuery>(q =>
                q.Page == 2 && q.PageSize == 25 && q.Search == "art" && q.CategoriaId == 3 &&
                q.SoloActivos == true && q.SoloConStock == false && q.SoloProductos == true && q.SoloServicios == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetItemByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns((ItemDto?)null);
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.GetById(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByCodigo_CuandoExiste_ConsultaMediatorYDevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ItemDto { Id = 9, Codigo = "A-001", Descripcion = "Articulo" };
        mediator.Send(Arg.Any<GetItemByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator, BuildDb(items: [BuildItem(9, "A-001", "Articulo", "123") ]));

        var result = await controller.GetByCodigo("a-001", CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(Arg.Is<GetItemByIdQuery>(q => q.Id == 9), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByCodigoBarras_CuandoNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(items: [BuildItem(9, "A-001", "Articulo", "123") ]));

        var result = await controller.GetByCodigoBarras("999", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetPrecio_CuandoExiste_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        var dto = new ItemPrecioDto { Id = 9, Codigo = "A-001", Descripcion = "Articulo", PrecioVenta = 150, MonedaId = 1 };
        mediator.Send(Arg.Any<GetItemPrecioQuery>(), Arg.Any<CancellationToken>())
            .Returns(dto);
        var controller = CreateController(mediator, BuildDb());
        var fecha = new DateOnly(2026, 3, 21);

        var result = await controller.GetPrecio(9, 5, 1, fecha, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        await mediator.Received(1).Send(Arg.Is<GetItemPrecioQuery>(q => q.ItemId == 9 && q.ListaPreciosId == 5 && q.MonedaId == 1 && q.Fecha == fecha), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStock_OrdenaPorDepositoDefaultYCalculaTotal()
    {
        StockItem[] stock =
        [
            BuildStock(1, 9, 2, 3),
            BuildStock(2, 9, 1, 5),
            BuildStock(3, 10, 1, 100)
        ];
        Deposito[] depositos =
        [
            BuildDeposito(1, 10, "Central", true),
            BuildDeposito(2, 10, "Secundario", false)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(stock: stock, depositos: depositos));

        var result = await controller.GetStock(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        AssertAnonymousProperty(ok.Value!, "itemId", 9L);
        AssertAnonymousProperty(ok.Value!, "totalStock", 8m);
        var depositosPayload = (IEnumerable)ok.Value!.GetType().GetProperty("depositos")!.GetValue(ok.Value!)!;
        var items = depositosPayload.Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "DepositoDescripcion", "Central");
        AssertAnonymousProperty(items[1], "DepositoDescripcion", "Secundario");
    }

    [Fact]
    public async Task Create_CuandoTieneExito_DevuelveCreatedAtRoute()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(20L));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Create(BuildCreateCommand(), CancellationToken.None);

        var created = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        created.RouteName.Should().Be("GetItemById");
        created.RouteValues!["id"].Should().Be(20L);
        AssertAnonymousProperty(created.Value!, "id", 20L);
    }

    [Fact]
    public async Task Update_CuandoIdNoCoincide_DevuelveBadRequest()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb());

        var result = await controller.Update(9, BuildUpdateCommand(10), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdatePrecios_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateItemPreciosCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Precio invalido."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdatePrecios(9, new UpdatePreciosRequest(-1, 100), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoTieneStock_DevuelveConflict()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(stock: [BuildStock(1, 9, 1, 3)]));

        var result = await controller.Delete(9, CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Delete_CuandoNoTieneStock_EnviaCommandYDevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb(stock: [BuildStock(1, 9, 1, 0)]));

        var result = await controller.Delete(9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(Arg.Is<DeleteItemCommand>(c => c.Id == 9), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activar_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ActivateItemCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.Activar(9, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
        await mediator.Received(1).Send(
            Arg.Is<ActivateItemCommand>(command => command.Id == 9),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetComponentes_CuandoItemNoExiste_DevuelveNotFound()
    {
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(items: [BuildItem(10, "B-001", "Otro", null)]));

        var result = await controller.GetComponentes(9, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetComponentes_CuandoExiste_DevuelveLista()
    {
        var controller = CreateController(
            Substitute.For<IMediator>(),
            BuildDb(
                items: [BuildItem(9, "A-001", "Articulo", null)],
                componentes: [BuildComponente(1, 9, 50, 2, 1), BuildComponente(2, 9, 51, 1, null), BuildComponente(3, 10, 52, 1, null)]));

        var result = await controller.GetComponentes(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "ComponenteId", 50L);
    }

    [Fact]
    public async Task AddComponente_CuandoDuplicado_DevuelveConflict()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddItemComponenteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("El componente ya esta asignado a este item."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddComponente(9, new AddItemComponenteRequest(50, 2, 1), CancellationToken.None);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task UpdateComponente_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateItemComponenteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Componente no encontrado."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdateComponente(9, 1, new UpdateItemComponenteRequest(3, 1), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteComponente_CuandoTieneExito_DevuelveNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteItemComponenteCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeleteComponente(9, 1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetUnidadesManipulacion_FiltraPorItem()
    {
        UnidadManipulacion[] unidades =
        [
            BuildUnidadManipulacion(1, 9, "Caja", 10, 1, 5),
            BuildUnidadManipulacion(2, 9, "Pallet", 100, 1, null),
            BuildUnidadManipulacion(3, 10, "Otro", 1, 1, null)
        ];
        var controller = CreateController(Substitute.For<IMediator>(), BuildDb(unidadesManipulacion: unidades));

        var result = await controller.GetUnidadesManipulacion(9, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = ((IEnumerable)ok.Value!).Cast<object>().ToList();
        items.Should().HaveCount(2);
        AssertAnonymousProperty(items[0], "Descripcion", "Caja");
    }

    [Fact]
    public async Task AddUnidadManipulacion_CuandoFalla_DevuelveBadRequest()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<AddUnidadManipulacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<long>("La cantidad debe ser mayor a cero."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.AddUnidadManipulacion(9, new UnidadManipulacionRequest("Caja", 0, 1, null), CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateUnidadManipulacion_CuandoNoExiste_DevuelveNotFound()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateUnidadManipulacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Unidad de manipulacion no encontrada."));
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.UpdateUnidadManipulacion(9, 1, new UnidadManipulacionRequest("Caja", 10, 1, null), CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteUnidadManipulacion_CuandoTieneExito_DevuelveOk()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<DeleteUnidadManipulacionCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        var controller = CreateController(mediator, BuildDb());

        var result = await controller.DeleteUnidadManipulacion(9, 1, CancellationToken.None);

        result.Should().BeOfType<OkResult>();
    }

    private static ItemsController CreateController(IMediator mediator, IApplicationDbContext db)
    {
        return new ItemsController(mediator, db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static IApplicationDbContext BuildDb(
        IEnumerable<Item>? items = null,
        IEnumerable<StockItem>? stock = null,
        IEnumerable<Deposito>? depositos = null,
        IEnumerable<ItemComponente>? componentes = null,
        IEnumerable<UnidadManipulacion>? unidadesManipulacion = null)
    {
        var db = Substitute.For<IApplicationDbContext>();
        var itemsDbSet = MockDbSetHelper.CreateMockDbSet(items);
        var stockDbSet = MockDbSetHelper.CreateMockDbSet(stock);
        var depositosDbSet = MockDbSetHelper.CreateMockDbSet(depositos);
        var componentesDbSet = MockDbSetHelper.CreateMockDbSet(componentes);
        var unidadesDbSet = MockDbSetHelper.CreateMockDbSet(unidadesManipulacion);
        db.Items.Returns(itemsDbSet);
        db.Stock.Returns(stockDbSet);
        db.Depositos.Returns(depositosDbSet);
        db.ItemsComponentes.Returns(componentesDbSet);
        db.UnidadesManipulacion.Returns(unidadesDbSet);
        return db;
    }

    private static CreateItemCommand BuildCreateCommand()
    {
        return new CreateItemCommand("A-001", "Articulo", null, "123", 1, 2, 1, true, false, false, true, 3, 100, 150, 1, 10, null, 10);
    }

    private static UpdateItemCommand BuildUpdateCommand(long id)
    {
        return new UpdateItemCommand(id, "Articulo", null, "123", 1, 2, 1, true, false, false, true, 3, 100, 150, 1, 10, null);
    }

    private static Item BuildItem(long id, string codigo, string descripcion, string? codigoBarras)
    {
        var entity = Item.Crear(codigo, descripcion, 1, 2, 1, true, false, false, true, 100, 150, 3, 1, 10, codigoBarras, null, null, 10, null);
        SetProperty(entity, nameof(Item.Id), id);
        return entity;
    }

    private static StockItem BuildStock(long id, long itemId, long depositoId, decimal cantidad)
    {
        var entity = StockItem.Crear(itemId, depositoId, cantidad);
        SetProperty(entity, nameof(StockItem.Id), id);
        return entity;
    }

    private static Deposito BuildDeposito(long id, long sucursalId, string descripcion, bool esDefault)
    {
        var entity = Deposito.Crear(sucursalId, descripcion, esDefault);
        SetProperty(entity, nameof(Deposito.Id), id);
        return entity;
    }

    private static ItemComponente BuildComponente(long id, long itemPadreId, long componenteId, decimal cantidad, long? unidadMedidaId)
    {
        var entity = ItemComponente.Crear(itemPadreId, componenteId, cantidad, unidadMedidaId);
        SetProperty(entity, nameof(ItemComponente.Id), id);
        return entity;
    }

    private static UnidadManipulacion BuildUnidadManipulacion(long id, long itemId, string descripcion, decimal cantidad, long unidadMedidaId, long? tipoUnidadManipulacionId)
    {
        var entity = UnidadManipulacion.Crear(itemId, descripcion, cantidad, unidadMedidaId, tipoUnidadManipulacionId);
        SetProperty(entity, nameof(UnidadManipulacion.Id), id);
        return entity;
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);
    }

    private static void AssertAnonymousProperty(object item, string propertyName, object? expectedValue)
    {
        item.GetType().GetProperty(propertyName)!.GetValue(item).Should().Be(expectedValue);
    }
}
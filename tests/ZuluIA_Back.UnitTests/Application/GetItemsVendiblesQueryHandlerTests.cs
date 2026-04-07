using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Queries;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class GetItemsVendiblesQueryHandlerTests
{
    [Fact]
    public async Task Handle_CuandoHayVendibles_RetornaSelectorComercial()
    {
        var db = Substitute.For<IApplicationDbContext>();

        var producto = Item.Crear("P001", "Producto Uno", 10, 21, 1, true, false, false, true, 100m, 150m, null, 0m, null, "7791", null, null, null, null);
        var servicio = Item.Crear("S001", "Servicio Uno", 10, 21, 1, false, true, false, false, 0m, 80m, null, 0m, null, null, null, null, null, null);
        var noVendible = Item.Crear("NV01", "No vendible", 10, 21, 1, true, false, false, true, 100m, 120m, null, 0m, null, null, null, null, null, null);

        SetId(producto, 1L);
        SetId(servicio, 2L);
        SetId(noVendible, 3L);
        SetPrivateProperty(producto, nameof(Item.CodigoAlternativo), "ALT-P001");
        SetPrivateProperty(producto, nameof(Item.CategoriaId), 11L);
        SetPrivateProperty(producto, nameof(Item.MarcaId), 12L);
        noVendible.ActualizarConfiguracionVentas(false, true, null, false, null);

        var unidad = UnidadMedida.Crear("UN", "Unidad");
        SetId(unidad, 10L);

        var categoria = CategoriaItem.Crear(null, "CAT", "Categoría ventas", 1, null, null);
        SetId(categoria, 11L);

        var marca = MarcaComercial.Crear("MAR", "Marca ventas", null);
        SetId(marca, 12L);

        var stock = StockItem.Crear(1L, 1L, 8m);
        var tipoComprobante = CreateTipoComprobante(1L, esVenta: true, afectaStock: false);
        var comprobanteReservado = Comprobante.Crear(1, null, 1, 1, 1, DateOnly.FromDateTime(DateTime.Today), null, 1, 30, 1, null, null);
        SetId(comprobanteReservado, 101L);
        var itemReservado = ComprobanteItem.Crear(101L, 1L, "Producto Uno", 3m, 0, 100, 0, 21, 21, null, 1);

        db.Items.Returns(MockDbSetHelper.CreateMockDbSet<Item>([producto, servicio, noVendible]));
        db.UnidadesMedida.Returns(MockDbSetHelper.CreateMockDbSet<UnidadMedida>([unidad]));
        db.CategoriasItems.Returns(MockDbSetHelper.CreateMockDbSet<CategoriaItem>([categoria]));
        db.MarcasComerciales.Returns(MockDbSetHelper.CreateMockDbSet<MarcaComercial>([marca]));
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet<StockItem>([stock]));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ComprobanteItem>([itemReservado]));
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<Comprobante>([comprobanteReservado]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet<TipoComprobante>([tipoComprobante]));

        var handler = new GetItemsVendiblesQueryHandler(db, new ItemCommercialStockService(db));

        var result = await handler.Handle(new GetItemsVendiblesQuery("uno", false, 10), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(x => x.Codigo).Should().Contain(["P001", "S001"]);
        result.Should().OnlyContain(x => x.EsVendible);
        var productoResult = result.Single(x => x.Codigo == "P001");
        productoResult.StockDisponible.Should().Be(5m);
        productoResult.CodigoAlternativo.Should().Be("ALT-P001");
        productoResult.CategoriaId.Should().Be(11L);
        productoResult.CategoriaDescripcion.Should().Be("Categoría ventas");
        productoResult.MarcaId.Should().Be(12L);
        productoResult.MarcaDescripcion.Should().Be("Marca ventas");
        result.Single(x => x.Codigo == "S001").ManejaStock.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CuandoSoloConStock_ExcluyeProductosSinDisponiblePeroMantieneServicios()
    {
        var db = Substitute.For<IApplicationDbContext>();

        var productoSinStock = Item.Crear("P002", "Producto sin stock", 10, 21, 1, true, false, false, true, 100m, 150m, null, 0m, null, null, null, null, null, null);
        var servicio = Item.Crear("S002", "Servicio vigente", 10, 21, 1, false, true, false, false, 0m, 80m, null, 0m, null, null, null, null, null, null);
        SetId(productoSinStock, 4L);
        SetId(servicio, 5L);

        var unidad = UnidadMedida.Crear("UN", "Unidad");
        SetId(unidad, 10L);

        db.Items.Returns(MockDbSetHelper.CreateMockDbSet<Item>([productoSinStock, servicio]));
        db.UnidadesMedida.Returns(MockDbSetHelper.CreateMockDbSet<UnidadMedida>([unidad]));
        db.CategoriasItems.Returns(MockDbSetHelper.CreateMockDbSet<CategoriaItem>());
        db.MarcasComerciales.Returns(MockDbSetHelper.CreateMockDbSet<MarcaComercial>());
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet<StockItem>());
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ComprobanteItem>());
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<Comprobante>());
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet<TipoComprobante>());

        var handler = new GetItemsVendiblesQueryHandler(db, new ItemCommercialStockService(db));

        var result = await handler.Handle(new GetItemsVendiblesQuery(null, true, 10), CancellationToken.None);

        result.Should().ContainSingle();
        result.Single().Codigo.Should().Be("S002");
    }

    private static void SetId<T>(T entity, long id)
        where T : class
    {
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
    }

    private static void SetPrivateProperty<T>(T entity, string propertyName, object? value)
        where T : class
    {
        entity.GetType().GetProperty(propertyName)!.SetValue(entity, value);
    }

    private static TipoComprobante CreateTipoComprobante(long id, bool esVenta, bool afectaStock)
    {
        var entity = (TipoComprobante)Activator.CreateInstance(typeof(TipoComprobante), nonPublic: true)!;
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
        entity.GetType().GetProperty("Codigo")!.SetValue(entity, "TEST");
        entity.GetType().GetProperty("Descripcion")!.SetValue(entity, "Tipo Test");
        entity.GetType().GetProperty("EsVenta")!.SetValue(entity, esVenta);
        entity.GetType().GetProperty("AfectaStock")!.SetValue(entity, afectaStock);
        return entity;
    }
}
using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Queries;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class GetItemByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenItemHasOperationalStock_ReturnsOperationalMetrics()
    {
        var repo = Substitute.For<IItemRepository>();
        var db = Substitute.For<IApplicationDbContext>();

        var item = Item.Crear(
            "PROD001",
            "Producto detalle",
            10,
            20,
            30,
            true,
            false,
            false,
            true,
            100m,
            150m,
            1L,
            2L,
            5m,
            50m,
            "779999",
            "Obs",
            "AFIP01",
            null,
            true,
            false,
            25m,
            12m,
            true,
            false,
            userId: null,
            alicuotaIvaCompraId: 21L,
            impuestoInternoId: 70L);

        SetId(item, 100L);
        repo.GetByIdAsync(100L, Arg.Any<CancellationToken>()).Returns(item);

        var categoria = CategoriaItem.Crear(null, "CAT", "Categoría", 1, null, null);
        SetId(categoria, 1L);
        var marca = MarcaComercial.Crear("MAR", "Marca", null);
        SetId(marca, 2L);
        var unidad = UnidadMedida.Crear("UN", "Unidad");
        SetId(unidad, 10L);
        var alicuota = CreateAlicuota(20L, 5, "IVA 21%", 21);
        var alicuotaCompra = CreateAlicuota(21L, 6, "IVA Compra 10%", 10);
        var impuestoInterno = CreateImpuesto(70L, "Interno");
        var stock = StockItem.Crear(100L, 1L, 16m);
        var tipoComprobante = CreateTipoComprobante(1L, esVenta: true, afectaStock: false);
        var comprobanteReservado = Comprobante.Crear(1, null, 1, 1, 1, DateOnly.FromDateTime(DateTime.Today), null, 1, 30, 1, null, null);
        SetId(comprobanteReservado, 501L);
        var comprobanteComprometido = Comprobante.Crear(1, null, 1, 1, 2, DateOnly.FromDateTime(DateTime.Today), null, 1, 30, 1, null, null);
        SetId(comprobanteComprometido, 502L);
        SetEstado(comprobanteComprometido, EstadoComprobante.Emitido);
        var itemReservado = ComprobanteItem.Crear(501L, 100L, "Producto detalle", 4m, 0, 100, 0, 20, 21, null, 1);
        var itemComprometido = ComprobanteItem.Crear(502L, 100L, "Producto detalle", 3m, 0, 100, 0, 20, 21, null, 1);
        var transferenciaEnTransito = TransferenciaDeposito.Crear(1L, 1L, 90L, DateOnly.FromDateTime(DateTime.Today), null, null);
        SetId(transferenciaEnTransito, 701L);
        transferenciaEnTransito.AgregarDetalle(100L, 2m);
        transferenciaEnTransito.Despachar(fechaDespacho: DateOnly.FromDateTime(DateTime.Today), userId: null);
        var lista = CreateListaPrecios(801L, "Mayorista", 30L, true, new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
        var listaItem = CreateListaPreciosItem(901L, 801L, 100L, 140m, 10m);
        var atributoComercial = AtributoComercial.Crear("COLOR", "Color", TipoDatoAtributoComercial.Texto, null);
        SetId(atributoComercial, 1001L);
        var itemAtributo = ItemAtributoComercial.Crear(100L, 1001L, "Rojo", null);
        SetId(itemAtributo, 1101L);
        var componente = Item.Crear("COMP001", "Componente pack", 10, 20, 30, true, false, false, true, 10m, 20m, null, 0m, null, null, null, null, null, null);
        SetId(componente, 200L);
        var itemComponente = ItemComponente.Crear(100L, 200L, 2m, 10L);
        SetId(itemComponente, 1201L);

        db.CategoriasItems.Returns(MockDbSetHelper.CreateMockDbSet<CategoriaItem>([categoria]));
        db.MarcasComerciales.Returns(MockDbSetHelper.CreateMockDbSet<MarcaComercial>([marca]));
        db.UnidadesMedida.Returns(MockDbSetHelper.CreateMockDbSet<UnidadMedida>([unidad]));
        db.AlicuotasIva.Returns(MockDbSetHelper.CreateMockDbSet<AlicuotaIva>([alicuota, alicuotaCompra]));
        db.Impuestos.Returns(MockDbSetHelper.CreateMockDbSet<Impuesto>([impuestoInterno]));
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet<StockItem>([stock]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet<TipoComprobante>([tipoComprobante]));
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<Comprobante>([comprobanteReservado, comprobanteComprometido]));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ComprobanteItem>([itemReservado, itemComprometido]));
        db.TransferenciasDeposito.Returns(MockDbSetHelper.CreateMockDbSet<TransferenciaDeposito>([transferenciaEnTransito]));
        db.TransferenciasDepositoDetalles.Returns(MockDbSetHelper.CreateMockDbSet<TransferenciaDepositoDetalle>(transferenciaEnTransito.Detalles));
        db.ListasPrecios.Returns(MockDbSetHelper.CreateMockDbSet<ListaPrecios>([lista]));
        db.ListaPreciosItems.Returns(MockDbSetHelper.CreateMockDbSet<ListaPreciosItem>([listaItem]));
        db.AtributosComerciales.Returns(MockDbSetHelper.CreateMockDbSet<AtributoComercial>([atributoComercial]));
        db.ItemsAtributosComerciales.Returns(MockDbSetHelper.CreateMockDbSet<ItemAtributoComercial>([itemAtributo]));
        db.Items.Returns(MockDbSetHelper.CreateMockDbSet<Item>([item, componente]));
        db.ItemsComponentes.Returns(MockDbSetHelper.CreateMockDbSet<ItemComponente>([itemComponente]));

        var handler = new GetItemByIdQueryHandler(repo, db, new ItemCommercialStockService(db));

        var result = await handler.Handle(new GetItemByIdQuery(100L), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Stock.Should().Be(16m);
        result.StockReservado.Should().Be(4m);
        result.StockComprometido.Should().Be(3m);
        result.StockDisponible.Should().Be(9m);
        result.StockEnTransito.Should().Be(2m);
        result.ListasPrecios.Should().ContainSingle();
        result.ListasPrecios[0].ListaDescripcion.Should().Be("Mayorista");
        result.ListasPrecios[0].Precio.Should().Be(140m);
        result.ListasPrecios[0].DescuentoPct.Should().Be(10m);
        result.ListasPrecios[0].PrecioFinal.Should().Be(126m);
        result.AtributosComerciales.Should().ContainSingle();
        result.AtributosComerciales[0].AtributoCodigo.Should().Be("COLOR");
        result.AtributosComerciales[0].Valor.Should().Be("Rojo");
        result.Componentes.Should().ContainSingle();
        result.Componentes[0].ComponenteCodigo.Should().Be("COMP001");
        result.Componentes[0].Cantidad.Should().Be(2m);
    }

    private static void SetId<T>(T entity, long id)
        where T : class
    {
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
    }

    private static void SetEstado(Comprobante entity, EstadoComprobante estado)
    {
        entity.GetType().GetProperty("Estado")!.SetValue(entity, estado);
    }

    private static AlicuotaIva CreateAlicuota(long id, short codigo, string descripcion, long porcentaje)
    {
        var entity = (AlicuotaIva)Activator.CreateInstance(typeof(AlicuotaIva), nonPublic: true)!;
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
        entity.GetType().GetProperty("Codigo")!.SetValue(entity, codigo);
        entity.GetType().GetProperty("Descripcion")!.SetValue(entity, descripcion);
        entity.GetType().GetProperty("Porcentaje")!.SetValue(entity, porcentaje);
        return entity;
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

    private static Impuesto CreateImpuesto(long id, string descripcion)
    {
        var entity = (Impuesto)Activator.CreateInstance(typeof(Impuesto), nonPublic: true)!;
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
        entity.GetType().GetProperty("Descripcion")!.SetValue(entity, descripcion);
        return entity;
    }

    private static ListaPrecios CreateListaPrecios(long id, string descripcion, long monedaId, bool activa, DateOnly? vigenciaDesde, DateOnly? vigenciaHasta)
    {
        var entity = (ListaPrecios)Activator.CreateInstance(typeof(ListaPrecios), nonPublic: true)!;
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
        entity.GetType().GetProperty("Descripcion")!.SetValue(entity, descripcion);
        entity.GetType().GetProperty("MonedaId")!.SetValue(entity, monedaId);
        entity.GetType().GetProperty("Activa")!.SetValue(entity, activa);
        entity.GetType().GetProperty("VigenciaDesde")!.SetValue(entity, vigenciaDesde);
        entity.GetType().GetProperty("VigenciaHasta")!.SetValue(entity, vigenciaHasta);
        return entity;
    }

    private static ListaPreciosItem CreateListaPreciosItem(long id, long listaId, long itemId, decimal precio, decimal descuentoPct)
    {
        var entity = (ListaPreciosItem)Activator.CreateInstance(typeof(ListaPreciosItem), nonPublic: true)!;
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
        entity.GetType().GetProperty("ListaId")!.SetValue(entity, listaId);
        entity.GetType().GetProperty("ItemId")!.SetValue(entity, itemId);
        entity.GetType().GetProperty("Precio")!.SetValue(entity, precio);
        entity.GetType().GetProperty("DescuentoPct")!.SetValue(entity, descuentoPct);
        entity.GetType().GetProperty("UpdatedAt")!.SetValue(entity, DateTimeOffset.UtcNow);
        return entity;
    }
}

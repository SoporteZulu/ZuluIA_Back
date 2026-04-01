using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Queries;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Application.Features.ListasPrecios.Services;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class GetItemPrecioQueryHandlerTests
{
    [Fact]
    public async Task Handle_CuandoExistePrecioEspecialCliente_DebePriorizarloSobreLaLista()
    {
        var repo = Substitute.For<IItemRepository>();
        var db = Substitute.For<IApplicationDbContext>();

        var item = Item.Crear(
            "ITEM001",
            "Item precio",
            1,
            1,
            1,
            true,
            false,
            false,
            true,
            100m,
            200m,
            5L,
            0m,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
        SetId(item, 100L);

        var alicuota = CreateAlicuota(1L, 21);
        var precioEspecial = PrecioEspecialCliente.Crear(100L, 10L, 1L, 150m, 10m, null, null, null, null);
        SetId(precioEspecial, 1L);

        repo.GetByIdAsync(100L, Arg.Any<CancellationToken>()).Returns(item);
        db.AlicuotasIva.Returns(MockDbSetHelper.CreateMockDbSet<AlicuotaIva>([alicuota]));
        db.PreciosEspecialesClientes.Returns(MockDbSetHelper.CreateMockDbSet<PrecioEspecialCliente>([precioEspecial]));
        db.PreciosEspecialesCanales.Returns(MockDbSetHelper.CreateMockDbSet<PrecioEspecialCanal>());
        db.PreciosEspecialesVendedores.Returns(MockDbSetHelper.CreateMockDbSet<PrecioEspecialVendedor>());
        db.ListasPreciosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<ListaPrecioPersona>());
        db.ListasPrecios.Returns(MockDbSetHelper.CreateMockDbSet<ListaPrecios>());
        db.ListaPreciosItems.Returns(MockDbSetHelper.CreateMockDbSet<ListaPreciosItem>());
        db.ListasPreciosPromociones.Returns(MockDbSetHelper.CreateMockDbSet<ListaPreciosPromocion>());
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Stock.StockItem>());
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ComprobanteItem>());
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<Comprobante>());
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet<TipoComprobante>());

        var sut = new GetItemPrecioQueryHandler(repo, db, new PrecioListaResolutionService(db), new ItemCommercialStockService(db));

        var result = await sut.Handle(
            new GetItemPrecioQuery(100L, null, 1L, DateOnly.FromDateTime(DateTime.Today), 10L),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.PrecioVenta.Should().Be(135m);
        result.EsVendible.Should().BeTrue();
        result.StockDisponible.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_CuandoLaListaHijaNoTienePrecio_DebeResolverDesdeLaListaPadreConPromocion()
    {
        var repo = Substitute.For<IItemRepository>();
        var db = Substitute.For<IApplicationDbContext>();
        var fecha = DateOnly.FromDateTime(DateTime.Today);

        var item = Item.Crear(
            "ITEM002",
            "Item heredado",
            1,
            1,
            1,
            true,
            false,
            false,
            true,
            100m,
            220m,
            7L,
            0m,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
        SetId(item, 200L);

        var alicuota = CreateAlicuota(1L, 21);
        var listaPadre = ListaPrecios.Crear("Padre", 1L, fecha.AddDays(-10), fecha.AddDays(10), null);
        SetId(listaPadre, 1L);
        var listaHija = ListaPrecios.Crear("Hija", 1L, fecha.AddDays(-10), fecha.AddDays(10), null, false, 1L, 1, null);
        SetId(listaHija, 2L);

        var itemListaPadre = CreateListaPreciosItem(10L, 1L, 200L, 100m, 0m);
        var promocion = ListaPreciosPromocion.Crear(
            1L,
            "Promo heredada",
            5m,
            fecha.AddDays(-1),
            fecha.AddDays(1),
            200L,
            null,
            null,
            null,
            null,
            null);
        SetId(promocion, 20L);

        repo.GetByIdAsync(200L, Arg.Any<CancellationToken>()).Returns(item);
        db.AlicuotasIva.Returns(MockDbSetHelper.CreateMockDbSet<AlicuotaIva>([alicuota]));
        db.PreciosEspecialesClientes.Returns(MockDbSetHelper.CreateMockDbSet<PrecioEspecialCliente>());
        db.PreciosEspecialesCanales.Returns(MockDbSetHelper.CreateMockDbSet<PrecioEspecialCanal>());
        db.PreciosEspecialesVendedores.Returns(MockDbSetHelper.CreateMockDbSet<PrecioEspecialVendedor>());
        db.ListasPreciosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<ListaPrecioPersona>());
        db.ListasPrecios.Returns(MockDbSetHelper.CreateMockDbSet<ListaPrecios>([listaPadre, listaHija]));
        db.ListaPreciosItems.Returns(MockDbSetHelper.CreateMockDbSet<ListaPreciosItem>([itemListaPadre]));
        db.ListasPreciosPromociones.Returns(MockDbSetHelper.CreateMockDbSet<ListaPreciosPromocion>([promocion]));
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Stock.StockItem>());
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ComprobanteItem>());
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<Comprobante>());
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet<TipoComprobante>());

        var sut = new GetItemPrecioQueryHandler(repo, db, new PrecioListaResolutionService(db), new ItemCommercialStockService(db));

        var result = await sut.Handle(
            new GetItemPrecioQuery(200L, 2L, 1L, fecha),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.PrecioVenta.Should().Be(95m);
    }

    [Fact]
    public async Task Handle_CuandoElItemNoEsVendible_RetornaNull()
    {
        var repo = Substitute.For<IItemRepository>();
        var db = Substitute.For<IApplicationDbContext>();

        var item = Item.Crear(
            "ITEM003",
            "Item inactivo",
            1,
            1,
            1,
            true,
            false,
            false,
            true,
            100m,
            200m,
            5L,
            0m,
            null,
            null,
            null,
            null,
            null,
            null,
            null);

        item.Desactivar(null);
        SetId(item, 300L);

        repo.GetByIdAsync(300L, Arg.Any<CancellationToken>()).Returns(item);

        var sut = new GetItemPrecioQueryHandler(repo, db, new PrecioListaResolutionService(db), new ItemCommercialStockService(db));

        var result = await sut.Handle(
            new GetItemPrecioQuery(300L, null, null, DateOnly.FromDateTime(DateTime.Today)),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CuandoManejaStock_RetornaSnapshotConsistenteDeStock()
    {
        var repo = Substitute.For<IItemRepository>();
        var db = Substitute.For<IApplicationDbContext>();

        var item = Item.Crear(
            "ITEM004",
            "Item con stock",
            1,
            1,
            1,
            true,
            false,
            false,
            true,
            100m,
            200m,
            5L,
            0m,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
        SetId(item, 400L);

        var alicuota = CreateAlicuota(1L, 21);
        var stock = ZuluIA_Back.Domain.Entities.Stock.StockItem.Crear(400L, 1L, 10m);
        var tipoComprobante = CreateTipoComprobante(1L, esVenta: true, afectaStock: false);
        var comprobanteReservado = Comprobante.Crear(1, null, 1, 1, 1, DateOnly.FromDateTime(DateTime.Today), null, 1, 30, 1, null, null);
        SetId(comprobanteReservado, 501L);
        var comprobanteComprometido = Comprobante.Crear(1, null, 1, 1, 2, DateOnly.FromDateTime(DateTime.Today), null, 1, 30, 1, null, null);
        SetId(comprobanteComprometido, 502L);
        SetEstado(comprobanteComprometido, EstadoComprobante.Emitido);
        var itemReservado = ComprobanteItem.Crear(501L, 400L, "Item con stock", 2m, 0, 100, 0, 1, 21, null, 1);
        var itemComprometido = ComprobanteItem.Crear(502L, 400L, "Item con stock", 3m, 0, 100, 0, 1, 21, null, 1);

        repo.GetByIdAsync(400L, Arg.Any<CancellationToken>()).Returns(item);
        db.AlicuotasIva.Returns(MockDbSetHelper.CreateMockDbSet<AlicuotaIva>([alicuota]));
        db.PreciosEspecialesClientes.Returns(MockDbSetHelper.CreateMockDbSet<PrecioEspecialCliente>());
        db.PreciosEspecialesCanales.Returns(MockDbSetHelper.CreateMockDbSet<PrecioEspecialCanal>());
        db.PreciosEspecialesVendedores.Returns(MockDbSetHelper.CreateMockDbSet<PrecioEspecialVendedor>());
        db.ListasPreciosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<ListaPrecioPersona>());
        db.ListasPrecios.Returns(MockDbSetHelper.CreateMockDbSet<ListaPrecios>());
        db.ListaPreciosItems.Returns(MockDbSetHelper.CreateMockDbSet<ListaPreciosItem>());
        db.ListasPreciosPromociones.Returns(MockDbSetHelper.CreateMockDbSet<ListaPreciosPromocion>());
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet<ZuluIA_Back.Domain.Entities.Stock.StockItem>([stock]));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ComprobanteItem>([itemReservado, itemComprometido]));
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<Comprobante>([comprobanteReservado, comprobanteComprometido]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet<TipoComprobante>([tipoComprobante]));

        var sut = new GetItemPrecioQueryHandler(repo, db, new PrecioListaResolutionService(db), new ItemCommercialStockService(db));

        var result = await sut.Handle(
            new GetItemPrecioQuery(400L, null, null, DateOnly.FromDateTime(DateTime.Today)),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Stock.Should().Be(10m);
        result.StockReservado.Should().Be(2m);
        result.StockComprometido.Should().Be(3m);
        result.StockDisponible.Should().Be(5m);
    }

    private static void SetId<T>(T entity, long id)
        where T : class
    {
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
    }

    private static AlicuotaIva CreateAlicuota(long id, long porcentaje)
    {
        var entity = (AlicuotaIva)Activator.CreateInstance(typeof(AlicuotaIva), nonPublic: true)!;
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
        entity.GetType().GetProperty("Codigo")!.SetValue(entity, (short)1);
        entity.GetType().GetProperty("Descripcion")!.SetValue(entity, "IVA Test");
        entity.GetType().GetProperty("Porcentaje")!.SetValue(entity, porcentaje);
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

    private static void SetEstado(Comprobante entity, EstadoComprobante estado)
    {
        entity.GetType().GetProperty("Estado")!.SetValue(entity, estado);
    }
}

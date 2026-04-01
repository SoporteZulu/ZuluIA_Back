using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Queries;
using ZuluIA_Back.Application.Features.ListasPrecios.Services;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Referencia;
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

        var sut = new GetItemPrecioQueryHandler(repo, db, new PrecioListaResolutionService(db));

        var result = await sut.Handle(
            new GetItemPrecioQuery(100L, null, 1L, DateOnly.FromDateTime(DateTime.Today), 10L),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.PrecioVenta.Should().Be(135m);
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

        var sut = new GetItemPrecioQueryHandler(repo, db, new PrecioListaResolutionService(db));

        var result = await sut.Handle(
            new GetItemPrecioQuery(200L, 2L, 1L, fecha),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.PrecioVenta.Should().Be(95m);
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
}

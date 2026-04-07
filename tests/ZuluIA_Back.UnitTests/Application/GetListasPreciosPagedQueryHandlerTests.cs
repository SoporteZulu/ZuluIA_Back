using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.ListasPrecios.Queries;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class GetListasPreciosPagedQueryHandlerTests
{
    [Fact]
    public async Task Handle_CuandoFiltraSoloVigentes_DebeRetornarSoloListasVigentes()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var vigente = ListaPrecios.Crear("Mayorista", 1L, hoy.AddDays(-2), hoy.AddDays(2), null, true, null, 10, "vigente");
        SetId(vigente, 1L);

        var vencida = ListaPrecios.Crear("Vieja", 1L, hoy.AddDays(-10), hoy.AddDays(-1), null);
        SetId(vencida, 2L);

        db.ListasPrecios.Returns(MockDbSetHelper.CreateMockDbSet<ListaPrecios>([vigente, vencida]));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([CreateMoneda(1L, "Peso", "$") ]));
        db.ListaPreciosItems.Returns(MockDbSetHelper.CreateMockDbSet<ListaPreciosItem>());
        db.ListasPreciosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<ListaPrecioPersona>());
        db.ListasPreciosPromociones.Returns(MockDbSetHelper.CreateMockDbSet<ListaPreciosPromocion>());

        var sut = new GetListasPreciosPagedQueryHandler(db);

        var result = await sut.Handle(
            new GetListasPreciosPagedQuery(SoloVigentes: true, Fecha: hoy),
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Descripcion.Should().Be("Mayorista");
        result.Items[0].EsPorDefecto.Should().BeTrue();
        result.Items[0].EstaVigenteHoy.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DebeMapearContadoresYListaPadre()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var padre = ListaPrecios.Crear("Base", 1L, hoy.AddDays(-10), hoy.AddDays(10), null);
        SetId(padre, 10L);

        var hija = ListaPrecios.Crear("Canal Distribuidor", 1L, hoy.AddDays(-10), hoy.AddDays(10), null, false, 10L, 5, "obs");
        SetId(hija, 20L);

        var moneda = CreateMoneda(1L, "Peso", "$ ");
        var item = CreateListaPreciosItem(100L, 20L, 500L, 120m, 0m);
        var persona = ListaPrecioPersona.Crear(20L, 900L);
        SetId(persona, 200L);
        var promocion = ListaPreciosPromocion.Crear(20L, "Promo", 10m, hoy.AddDays(-1), hoy.AddDays(1), null, null, null, null, null, null);
        SetId(promocion, 300L);

        db.ListasPrecios.Returns(MockDbSetHelper.CreateMockDbSet<ListaPrecios>([padre, hija]));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([moneda]));
        db.ListaPreciosItems.Returns(MockDbSetHelper.CreateMockDbSet<ListaPreciosItem>([item]));
        db.ListasPreciosPersonas.Returns(MockDbSetHelper.CreateMockDbSet<ListaPrecioPersona>([persona]));
        db.ListasPreciosPromociones.Returns(MockDbSetHelper.CreateMockDbSet<ListaPreciosPromocion>([promocion]));

        var sut = new GetListasPreciosPagedQueryHandler(db);

        var result = await sut.Handle(
            new GetListasPreciosPagedQuery(Search: "Distribuidor"),
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].ListaPadreDescripcion.Should().Be("Base");
        result.Items[0].TieneHerencia.Should().BeTrue();
        result.Items[0].CantidadItems.Should().Be(1);
        result.Items[0].CantidadPersonasAsignadas.Should().Be(1);
        result.Items[0].CantidadPromocionesActivas.Should().Be(1);
        result.Items[0].MonedaDescripcion.Should().Be("Peso");
    }

    private static void SetId<T>(T entity, long id)
        where T : class
    {
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
    }

    private static Moneda CreateMoneda(long id, string descripcion, string simbolo)
    {
        var entity = (Moneda)Activator.CreateInstance(typeof(Moneda), nonPublic: true)!;
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
        entity.GetType().GetProperty("Codigo")!.SetValue(entity, descripcion[..Math.Min(3, descripcion.Length)].ToUpperInvariant());
        entity.GetType().GetProperty("Descripcion")!.SetValue(entity, descripcion);
        entity.GetType().GetProperty("Simbolo")!.SetValue(entity, simbolo);
        entity.GetType().GetProperty("Activa")!.SetValue(entity, true);
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

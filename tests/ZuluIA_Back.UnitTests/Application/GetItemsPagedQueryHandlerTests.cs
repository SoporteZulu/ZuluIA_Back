using FluentAssertions;
using NSubstitute;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Queries;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.UnitTests.Helpers;

namespace ZuluIA_Back.UnitTests.Application;

public class GetItemsPagedQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenItemHasCommercialData_ReturnsEnrichedSalesFields()
    {
        var repo = Substitute.For<IItemRepository>();
        var db = Substitute.For<IApplicationDbContext>();

        var item = Item.Crear(
            codigo: "PROD001",
            descripcion: "Producto ventas",
            unidadMedidaId: 10,
            alicuotaIvaId: 20,
            monedaId: 30,
            esProducto: true,
            esServicio: false,
            esFinanciero: false,
            manejaStock: true,
            precioCosto: 100m,
            precioVenta: 150m,
            categoriaId: 1L,
            marcaId: 2L,
            stockMinimo: 5m,
            stockMaximo: 50m,
            codigoBarras: "779999",
            descripcionAdicional: "Obs",
            codigoAfip: "AFIP01",
            sucursalId: null,
            aplicaVentas: true,
            aplicaCompras: false,
            porcentajeGanancia: 25m,
            porcentajeMaximoDescuento: 12m,
            esRpt: true,
            esSistema: false,
            userId: null,
            puntoReposicion: 7m,
            stockSeguridad: 2m,
            peso: 1.25m,
            volumen: 0.5m,
            codigoAlternativo: "ALT001",
            esTrazable: true,
            permiteFraccionamiento: true,
            diasVencimientoLimite: 45,
            depositoDefaultId: 90L,
            alicuotaIvaCompraId: 21L,
            impuestoInternoId: 70L);

        SetId(item, 100L);

        repo.GetPagedAsync(
                1,
                20,
                "prod",
                1L,
                true,
                true,
                true,
                false,
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Item>([item], 1, 20, 1));

        var categoria = CategoriaItem.Crear(null, "CAT", "Categoría", 1, null, null);
        SetId(categoria, 1L);

        var marca = MarcaComercial.Crear("MAR", "Marca", null);
        SetId(marca, 2L);

        var unidad = UnidadMedida.Crear("UN", "Unidad");
        SetId(unidad, 10L);

        var alicuota = CreateAlicuota(20L, 5, "IVA 21%", 21);
        var alicuotaCompra = CreateAlicuota(21L, 6, "IVA Compra 10.5%", 10);
        var moneda = CreateMoneda(30L, "PES", "Peso argentino", "$");
        var deposito = Deposito.Crear(1L, "Depósito principal", true);
        var impuestoInterno = Impuesto.Crear("II", "Impuesto interno test", 3.5m);
        SetId(deposito, 90L);
        SetId(impuestoInterno, 70L);

        var stock = StockItem.Crear(100L, 1L, 16m);
        var tipoComprobante = CreateTipoComprobante(1L, esVenta: true, afectaStock: false);
        var comprobanteReservado = Comprobante.Crear(1, null, 1, 1, 1, DateOnly.FromDateTime(DateTime.Today), null, 1, 30, 1, null, null);
        SetId(comprobanteReservado, 501L);
        var comprobanteComprometido = Comprobante.Crear(1, null, 1, 1, 2, DateOnly.FromDateTime(DateTime.Today), null, 1, 30, 1, null, null);
        SetId(comprobanteComprometido, 502L);
        SetEstado(comprobanteComprometido, EstadoComprobante.Emitido);
        var itemReservado = ComprobanteItem.Crear(501L, 100L, "Producto ventas", 4m, 0, 100, 0, 20, 21, null, 1);
        var itemComprometido = ComprobanteItem.Crear(502L, 100L, "Producto ventas", 3m, 0, 100, 0, 20, 21, null, 1);
        var transferenciaEnTransito = TransferenciaDeposito.Crear(1L, 1L, 90L, DateOnly.FromDateTime(DateTime.Today), null, null);
        SetId(transferenciaEnTransito, 701L);
        transferenciaEnTransito.AgregarDetalle(100L, 2m);
        transferenciaEnTransito.Despachar(fechaDespacho: DateOnly.FromDateTime(DateTime.Today), userId: null);
        var componentePack = ItemComponente.Crear(100L, 200L, 2m, 10L);
        SetId(componentePack, 1201L);

        db.CategoriasItems.Returns(MockDbSetHelper.CreateMockDbSet([categoria]));
        db.MarcasComerciales.Returns(MockDbSetHelper.CreateMockDbSet([marca]));
        db.UnidadesMedida.Returns(MockDbSetHelper.CreateMockDbSet([unidad]));
        db.AlicuotasIva.Returns(MockDbSetHelper.CreateMockDbSet<AlicuotaIva>([alicuota, alicuotaCompra]));
        db.Impuestos.Returns(MockDbSetHelper.CreateMockDbSet<Impuesto>([impuestoInterno]));
        db.Monedas.Returns(MockDbSetHelper.CreateMockDbSet<Moneda>([moneda]));
        db.Depositos.Returns(MockDbSetHelper.CreateMockDbSet<Deposito>([deposito]));
        db.Stock.Returns(MockDbSetHelper.CreateMockDbSet<StockItem>([stock]));
        db.ComprobantesItems.Returns(MockDbSetHelper.CreateMockDbSet<ComprobanteItem>([itemReservado, itemComprometido]));
        db.Comprobantes.Returns(MockDbSetHelper.CreateMockDbSet<Comprobante>([comprobanteReservado, comprobanteComprometido]));
        db.TiposComprobante.Returns(MockDbSetHelper.CreateMockDbSet<TipoComprobante>([tipoComprobante]));
        db.TransferenciasDeposito.Returns(MockDbSetHelper.CreateMockDbSet<TransferenciaDeposito>([transferenciaEnTransito]));
        db.TransferenciasDepositoDetalles.Returns(MockDbSetHelper.CreateMockDbSet<TransferenciaDepositoDetalle>(transferenciaEnTransito.Detalles));
        db.ItemsComponentes.Returns(MockDbSetHelper.CreateMockDbSet<ItemComponente>([componentePack]));

        var handler = new GetItemsPagedQueryHandler(repo, db);

        var result = await handler.Handle(
            new GetItemsPagedQuery(
                Page: 1,
                PageSize: 20,
                Search: "prod",
                CategoriaId: 1L,
                SoloActivos: true,
                SoloConStock: true,
                SoloProductos: true,
                SoloServicios: false),
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        var dto = result.Items.Single();
        dto.Id.Should().Be(100L);
        dto.CategoriaDescripcion.Should().Be("Categoría");
        dto.MarcaDescripcion.Should().Be("Marca");
        dto.UnidadMedidaDescripcion.Should().Be("Unidad");
        dto.DescripcionAdicional.Should().Be("Obs");
        dto.AlicuotaIvaId.Should().Be(20L);
        dto.AlicuotaIvaDescripcion.Should().Be("IVA 21%");
        dto.AlicuotaIvaPorcentaje.Should().Be(21m);
        dto.AlicuotaIvaCompraId.Should().Be(21L);
        dto.AlicuotaIvaCompraDescripcion.Should().Be("IVA Compra 10.5%");
        dto.AlicuotaIvaCompraPorcentaje.Should().Be(10m);
        dto.ImpuestoInternoId.Should().Be(70L);
        dto.ImpuestoInternoDescripcion.Should().Be("Impuesto interno test");
        dto.MonedaId.Should().Be(30L);
        dto.MonedaSimbol.Should().Be("$");
        dto.CodigoAlternativo.Should().Be("ALT001");
        dto.EsFinanciero.Should().BeFalse();
        dto.PrecioCosto.Should().Be(100m);
        dto.PrecioVenta.Should().Be(125m);
        dto.Stock.Should().Be(16m);
        dto.StockReservado.Should().Be(4m);
        dto.StockComprometido.Should().Be(3m);
        dto.StockDisponible.Should().Be(9m);
        dto.StockEnTransito.Should().Be(2m);
        dto.StockMinimo.Should().Be(5m);
        dto.StockMaximo.Should().Be(50m);
        dto.PuntoReposicion.Should().Be(7m);
        dto.StockSeguridad.Should().Be(2m);
        dto.Peso.Should().Be(1.25m);
        dto.Volumen.Should().Be(0.5m);
        dto.EsTrazable.Should().BeTrue();
        dto.PermiteFraccionamiento.Should().BeTrue();
        dto.DiasVencimientoLimite.Should().Be(45);
        dto.DepositoDefaultId.Should().Be(90L);
        dto.DepositoDefaultDescripcion.Should().Be("Depósito principal");
        dto.CodigoAfip.Should().Be("AFIP01");
        dto.CreatedAt.Should().NotBe(default);
        dto.AplicaVentas.Should().BeTrue();
        dto.AplicaCompras.Should().BeFalse();
        dto.PorcentajeGanancia.Should().Be(25m);
        dto.PorcentajeMaximoDescuento.Should().Be(12m);
        dto.EsRpt.Should().BeTrue();
        dto.EsPack.Should().BeTrue();
        dto.CantidadComponentes.Should().Be(1);
        dto.PrecioVentaCalculado.Should().Be(125m);
        dto.PuedeEditar.Should().BeTrue();
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

    private static Moneda CreateMoneda(long id, string codigo, string descripcion, string simbolo)
    {
        var entity = (Moneda)Activator.CreateInstance(typeof(Moneda), nonPublic: true)!;
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
        entity.GetType().GetProperty("Codigo")!.SetValue(entity, codigo);
        entity.GetType().GetProperty("Descripcion")!.SetValue(entity, descripcion);
        entity.GetType().GetProperty("Simbolo")!.SetValue(entity, simbolo);
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
}

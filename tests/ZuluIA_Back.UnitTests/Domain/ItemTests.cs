using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Events.Items;

namespace ZuluIA_Back.UnitTests.Domain;

public class ItemTests
{
    [Fact]
    public void Crear_ConDatosValidos_DebeCrearItem()
    {
        var item = Item.Crear(
            "PROD001", "Notebook 15 pulgadas",
            1, 1, 1,
            true, false, true,
            true, 800000m, 1200000m,
            null, 0m, null,
            null, null, null, null, null);

        item.Codigo.Should().Be("PROD001");
        item.Descripcion.Should().Be("Notebook 15 pulgadas");
        item.EsProducto.Should().BeTrue();
        item.EsServicio.Should().BeFalse();
        item.ManejaStock.Should().BeTrue();
        item.PrecioCosto.Should().Be(800000m);
        item.PrecioVenta.Should().Be(1200000m);
        item.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_ConCamposLogisticosYStockOperativo_DebePersistirlos()
    {
        var item = Item.Crear(
            "PRODLOG", "Producto logístico",
            1, 1, 1,
            true, false, false,
            true, 100m, 150m,
            null, 5m, 50m,
            null, null, null, null, null,
            10m, 3m, 1.5m, 0.8m);

        item.PuntoReposicion.Should().Be(10m);
        item.StockSeguridad.Should().Be(3m);
        item.Peso.Should().Be(1.5m);
        item.Volumen.Should().Be(0.8m);
    }

    [Fact]
    public void Crear_ConCamposAvanzados_DebePersistirlos()
    {
        var item = Item.Crear(
            "PRODAV", "Producto avanzado",
            1, 1, 1,
            true, false, false,
            true, 100m, 150m,
            null, 5m, 50m,
            null, null, null, null, null,
            10m, 3m, 1.5m, 0.8m,
            "ALT001", true, true, 30, 9L);

        item.CodigoAlternativo.Should().Be("ALT001");
        item.EsTrazable.Should().BeTrue();
        item.PermiteFraccionamiento.Should().BeTrue();
        item.DiasVencimientoLimite.Should().Be(30);
        item.DepositoDefaultId.Should().Be(9L);
    }

    [Fact]
    public void Crear_ConCamposFiscalesCompra_DebePersistirlos()
    {
        var item = Item.Crear(
            "PRODFISC", "Producto fiscal",
            1, 1, 1,
            true, false, false,
            true, 100m, 150m,
            null, 5m, 50m,
            null, null, null, null, null,
            null, null, null, null,
            null, false, false, null, null,
            2L, 7L);

        item.AlicuotaIvaCompraId.Should().Be(2L);
        item.ImpuestoInternoId.Should().Be(7L);
    }

    [Fact]
    public void Crear_TrazableSinStock_LanzaExcepcion()
    {
        var act = () => Item.Crear(
            "PRODTR", "Producto trazable inválido",
            1, 1, 1,
            true, false, false,
            false, 100m, 150m,
            null, 0m, null,
            null, null, null, null, null,
            null, null, null, null,
            null, true, false, null, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*trazable*");
    }

    [Fact]
    public void Crear_ComoServicio_NoDebeManejearStock()
    {
        var item = Item.Crear(
            "SERV001", "Servicio de Instalación",
            1, 1, 1,
            false, true, true,
            false, 0m, 25000m,
            null, 0m, null,
            null, null, null, null, null);

        item.EsServicio.Should().BeTrue();
        item.ManejaStock.Should().BeFalse();
    }

    [Fact]
    public void Crear_ProductoYServicioAlMismoTiempo_DebeLanzarExcepcion()
    {
        var act = () => Item.Crear(
            "PROD001", "Producto",
            1, 1, 1,
            true, true, false,
            false, 0m, 0m,
            null, 0m, null,
            null, null, null, null, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*producto y servicio*");
    }

    [Fact]
    public void Crear_CodigoConEspacios_DebeNormalizarse()
    {
        var item = Item.Crear(
            "  prod001  ", "Producto",
            1, 1, 1,
            true, false, false,
            false, 0m, 0m,
            null, 0m, null,
            null, null, null, null, null);

        item.Codigo.Should().Be("PROD001");
    }

    [Fact]
    public void Crear_DebeGenerarDomainEventItemCreado()
    {
        var item = Item.Crear(
            "PROD001", "Producto",
            1, 1, 1,
            true, false, false,
            false, 0m, 0m,
            null, 0m, null,
            null, null, null, null, null);

        item.DomainEvents.Should().ContainSingle();
        item.DomainEvents.First().Should().BeOfType<ItemCreadoEvent>();
    }


    [Fact]
    public void ActualizarPrecios_ConPreciosValidos_DebeActualizar()
    {
        var item = Item.Crear(
            "PROD001", "Producto",
            1, 1, 1,
            true, false, false,
            true, 100m, 200m,
            null, 0m, null,
            null, null, null, null, null);

        item.ClearDomainEvents();
        item.ActualizarPrecios(150m, 250m, null);

        item.PrecioCosto.Should().Be(150m);
        item.PrecioVenta.Should().Be(250m);
    }

    [Fact]
    public void ActualizarPrecios_DebeGenerarDomainEventPrecioActualizado()
    {
        var item = Item.Crear(
            "PROD001", "Producto",
            1, 1, 1,
            true, false, false,
            true, 100m, 200m,
            null, 0m, null,
            null, null, null, null, null);

        item.ClearDomainEvents();
        item.ActualizarPrecios(150m, 300m, null);

        item.DomainEvents.Should().ContainSingle();
        var evt = item.DomainEvents.First() as PrecioItemActualizadoEvent;
        evt!.PrecioAnterior.Should().Be(200m);
        evt.PrecioNuevo.Should().Be(300m);
    }

    [Fact]
    public void Desactivar_ItemActivo_DebeDesactivarlo()
    {
        var item = Item.Crear(
            "PROD001", "Producto",
            1, 1, 1,
            true, false, false,
            true, 0m, 0m,
            null, 0m, null,
            null, null, null, null, null);

        item.Desactivar(null);

        item.Activo.Should().BeFalse();
    }

    [Fact]
    public void Activar_ItemDesactivado_DebeReactivarloYLimpiarDeletedAt()
    {
        var item = Item.Crear(
            "PROD001", "Producto",
            1, 1, 1,
            true, false, false,
            true, 0m, 0m,
            null, 0m, null,
            null, null, null, null, null);

        item.Desactivar(null);
        item.Activar(null);

        item.Activo.Should().BeTrue();
        item.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Crear_ConPorcentajeGanancia_DebeCalcularPrecioVenta()
    {
        var item = Item.Crear(
            "PROD002", "Producto con margen",
            1, 1, 1,
            true, false, false,
            true, 100m, 0m,
            null, null, 0m, null,
            null, null, null, null,
            true, true, 25m, null, false, false, null);

        item.PrecioVenta.Should().Be(125m);
    }

    [Fact]
    public void Desactivar_ItemSistema_LanzaExcepcion()
    {
        var item = Item.Crear(
            "SIST001", "Producto sistema",
            1, 1, 1,
            true, false, false,
            true, 100m, 150m,
            null, null, 0m, null,
            null, null, null, null,
            true, true, null, null, false, true, null);

        var act = () => item.Desactivar(null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*item del sistema*");
    }

    // ── Actualizar ──────────────────────────────────────────────────────────

    private static Item CrearItemBase() => Item.Crear(
        "PROD001", "Producto Original",
        1, 1, 1,
        true, false, false,
        true, 100m, 200m,
        null, 5m, 50m,
        null, null, null, null, null);

    [Fact]
    public void Actualizar_ConDatosValidos_ActualizaCampos()
    {
        var item = CrearItemBase();

        item.Actualizar(
            "Producto Actualizado", "Detalle adicional", "789012",
            2, 2, 2,
            true, false, false,
            true, 10L,
            "ABC123", 10m, 100m,
            null, null,
            12m, 4m, 1.2m, 0.7m,
            "ALT002", true, true, 20, 8L);

        item.Descripcion.Should().Be("Producto Actualizado");
        item.DescripcionAdicional.Should().Be("Detalle adicional");
        item.CodigoBarras.Should().Be("789012");
        item.UnidadMedidaId.Should().Be(2);
        item.ManejaStock.Should().BeTrue();
        item.StockMinimo.Should().Be(10m);
        item.StockMaximo.Should().Be(100m);
        item.PuntoReposicion.Should().Be(12m);
        item.StockSeguridad.Should().Be(4m);
        item.Peso.Should().Be(1.2m);
        item.Volumen.Should().Be(0.7m);
        item.CodigoAlternativo.Should().Be("ALT002");
        item.EsTrazable.Should().BeTrue();
        item.PermiteFraccionamiento.Should().BeTrue();
        item.DiasVencimientoLimite.Should().Be(20);
        item.DepositoDefaultId.Should().Be(8L);
    }

    [Fact]
    public void Actualizar_PesoNegativo_LanzaExcepcion()
    {
        var item = CrearItemBase();

        var act = () => item.Actualizar(
            "Producto", null, null,
            1, 1, 1,
            true, false, false,
            true, null,
            null, 10m, 20m,
            null, null,
            null, null, -1m, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*peso*negativo*");
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var item = CrearItemBase();

        var act = () => item.Actualizar(
            "   ", null, null,
            1, 1, 1,
            true, false, false,
            true, null,
            null, 0m, null,
            null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ProductoYServicioAlMismoTiempo_LanzaExcepcion()
    {
        var item = CrearItemBase();

        var act = () => item.Actualizar(
            "Producto", null, null,
            1, 1, 1,
            true, true, false,
            true, null,
            null, 0m, null,
            null, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*producto y servicio*");
    }

    [Fact]
    public void Actualizar_StockMinimoNegativo_LanzaExcepcion()
    {
        var item = CrearItemBase();

        var act = () => item.Actualizar(
            "Producto", null, null,
            1, 1, 1,
            true, false, false,
            true, null,
            null, -1m, null,
            null, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*stock mínimo*negativo*");
    }

    [Fact]
    public void Actualizar_StockMaximoMenorAlMinimo_LanzaExcepcion()
    {
        var item = CrearItemBase();

        var act = () => item.Actualizar(
            "Producto", null, null,
            1, 1, 1,
            true, false, false,
            true, null,
            null, 10m, 5m,
            null, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*stock máximo*menor*");
    }

    [Fact]
    public void Actualizar_ComoServicio_ManejaStockEsFalse()
    {
        var item = CrearItemBase();

        item.Actualizar(
            "Servicio", null, null,
            1, 1, 1,
            false, true, false,
            true, null,
            null, 0m, null,
            null, null);

        item.ManejaStock.Should().BeFalse();
    }

    // ── ActualizarStock ──────────────────────────────────────────────────────

    [Fact]
    public void ActualizarStock_ConDatosValidos_ActualizaValores()
    {
        var item = CrearItemBase();

        item.ActualizarStock(20m, 200m, null);

        item.StockMinimo.Should().Be(20m);
        item.StockMaximo.Should().Be(200m);
    }

    [Fact]
    public void ActualizarStock_StockMinimoNegativo_LanzaExcepcion()
    {
        var item = CrearItemBase();

        var act = () => item.ActualizarStock(-1m, null, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*stock mínimo*negativo*");
    }

    [Fact]
    public void ActualizarStock_StockMaximoMenorAlMinimo_LanzaExcepcion()
    {
        var item = CrearItemBase();

        var act = () => item.ActualizarStock(50m, 10m, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*stock máximo*menor*");
    }

    // ── Activar ──────────────────────────────────────────────────────────────

    [Fact]
    public void Activar_ItemDesactivado_DebeActivarlo()
    {
        var item = CrearItemBase();
        item.Desactivar(null);
        item.Activo.Should().BeFalse();

        item.Activar(null);

        item.Activo.Should().BeTrue();
    }

}
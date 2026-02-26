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
            800000m, 1200000m,
            null, null);

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
    public void Crear_ComoServicio_NoDebeManejearStock()
    {
        var item = Item.Crear(
            "SERV001", "Servicio de Instalación",
            1, 1, 1,
            false, true, true,
            0m, 25000m,
            null, null);

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
            0m, 0m,
            null, null);

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
            0m, 0m,
            null, null);

        item.Codigo.Should().Be("PROD001");
    }

    [Fact]
    public void Crear_DebeGenerarDomainEventItemCreado()
    {
        var item = Item.Crear(
            "PROD001", "Producto",
            1, 1, 1,
            true, false, false,
            0m, 0m,
            null, null);

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
            100m, 200m,
            null, null);

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
            100m, 200m,
            null, null);

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
            0m, 0m,
            null, null);

        item.Desactivar(null);

        item.Activo.Should().BeFalse();
    }
}
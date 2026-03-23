using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Events.Stock;

namespace ZuluIA_Back.UnitTests.Domain;

public class StockItemTests
{
    [Fact]
    public void Crear_ConCantidadInicial_DebeCrearStock()
    {
        var stock = StockItem.Crear(1, 1, 100m);

        stock.ItemId.Should().Be(1);
        stock.DepositoId.Should().Be(1);
        stock.Cantidad.Should().Be(100m);
    }

    [Fact]
    public void Incrementar_CantidadPositiva_DebeAumentarStock()
    {
        var stock = StockItem.Crear(1, 1, 100m);

        stock.Incrementar(50m);

        stock.Cantidad.Should().Be(150m);
    }

    [Fact]
    public void Incrementar_CantidadCero_DebeLanzarExcepcion()
    {
        var stock = StockItem.Crear(1, 1, 100m);

        var act = () => stock.Incrementar(0m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Incrementar_CantidadNegativa_DebeLanzarExcepcion()
    {
        var stock = StockItem.Crear(1, 1, 100m);

        var act = () => stock.Incrementar(-10m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Decrementar_CantidadValida_DebeReducirStock()
    {
        var stock = StockItem.Crear(1, 1, 100m);

        stock.Decrementar(30m);

        stock.Cantidad.Should().Be(70m);
    }

    [Fact]
    public void Decrementar_SinStockSuficiente_DebeLanzarExcepcion()
    {
        var stock = StockItem.Crear(1, 1, 10m);

        var act = () => stock.Decrementar(50m, permitirNegativo: false);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Stock insuficiente*");
    }

    [Fact]
    public void Decrementar_PermitirNegativo_DebeDejarStockNegativo()
    {
        var stock = StockItem.Crear(1, 1, 10m);

        stock.Decrementar(50m, permitirNegativo: true);

        stock.Cantidad.Should().Be(-40m);
    }

    [Fact]
    public void AjustarStock_DebeGenerarDomainEvent()
    {
        var stock = StockItem.Crear(1, 1, 100m);

        stock.AjustarStock(200m, "Ajuste de inventario");

        stock.Cantidad.Should().Be(200m);
        stock.DomainEvents.Should().ContainSingle();
        stock.DomainEvents.First().Should().BeOfType<StockAjustadoEvent>();
    }

    [Fact]
    public void AjustarStock_EventoDebeContenerValoresCorrectos()
    {
        var stock = StockItem.Crear(1, 1, 100m);

        stock.AjustarStock(200m, "Test");

        var evt = stock.DomainEvents.First() as StockAjustadoEvent;
        evt!.Anterior.Should().Be(100m);
        evt.Nuevo.Should().Be(200m);
        evt.Motivo.Should().Be("Test");
    }

    // ── AjustarA ──────────────────────────────────────────────────────────────

    [Fact]
    public void AjustarA_ConCantidadValida_ActualizaCantidadYRetornaDiferencia()
    {
        var stock = StockItem.Crear(1, 1, 100m);

        var diferencia = stock.AjustarA(150m);

        stock.Cantidad.Should().Be(150m);
        diferencia.Should().Be(50m);
    }

    [Fact]
    public void AjustarA_CantidadMenorQueActual_RetornaDiferenciaNegativa()
    {
        var stock = StockItem.Crear(1, 1, 100m);

        var diferencia = stock.AjustarA(60m);

        stock.Cantidad.Should().Be(60m);
        diferencia.Should().Be(-40m);
    }

    [Fact]
    public void AjustarA_CantidadNegativa_LanzaExcepcion()
    {
        var stock = StockItem.Crear(1, 1, 100m);

        var act = () => stock.AjustarA(-1m);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*negativa*");
    }

    [Fact]
    public void AjustarA_DebeGenerarDomainEvent()
    {
        var stock = StockItem.Crear(1, 1, 100m);

        stock.AjustarA(200m, "Inventario anual");

        stock.DomainEvents.Should().ContainSingle();
        var evt = stock.DomainEvents.First() as StockAjustadoEvent;
        evt!.Anterior.Should().Be(100m);
        evt.Nuevo.Should().Be(200m);
        evt.Motivo.Should().Be("Inventario anual");
    }

    [Fact]
    public void Ingresar_CantidadPositiva_IncrementaStock()
    {
        var stock = StockItem.Crear(1, 1, 10m);
        stock.Ingresar(5m);
        stock.Cantidad.Should().Be(15m);
    }

    [Fact]
    public void Ingresar_CantidadCero_LanzaExcepcion()
    {
        var stock = StockItem.Crear(1, 1, 10m);
        var act = () => stock.Ingresar(0m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ingresar_CantidadNegativa_LanzaExcepcion()
    {
        var stock = StockItem.Crear(1, 1, 10m);
        var act = () => stock.Ingresar(-3m);
        act.Should().Throw<InvalidOperationException>();
    }
}
using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Events.Comprobantes;

namespace ZuluIA_Back.UnitTests.Domain;

public class ComprobanteTests
{
    private static Comprobante CrearComprobante() =>
        Comprobante.Crear(1, 1, 1, 1, 1, DateOnly.FromDateTime(DateTime.Today), 1, 1, 1m, null);

    private static ComprobanteItem CrearItem(decimal precio = 1000m, decimal iva = 21m) =>
        ComprobanteItem.Crear(0, 1, "Producto Test", 1m, precio, 0m, 1, iva, null, 1);

    [Fact]
    public void Crear_ConDatosValidos_DebeCrearEnEstadoBorrador()
    {
        var comp = CrearComprobante();

        comp.Estado.Should().Be(EstadoComprobante.Borrador);
        comp.Items.Should().BeEmpty();
        comp.Total.Should().Be(0m);
    }

    [Fact]
    public void AgregarItem_EnBorrador_DebeAgregarItem()
    {
        var comp = CrearComprobante();
        var item = CrearItem();

        comp.AgregarItem(item);

        comp.Items.Should().HaveCount(1);
        comp.NetoGravado.Should().BeGreaterThan(0);
        comp.IvaRi.Should().BeGreaterThan(0);
        comp.Total.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AgregarItem_DespuesDeEmitir_DebeLanzarExcepcion()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null, null, null);

        var act = () => comp.AgregarItem(CrearItem());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Emitir_ConItems_DebeEmitirYGenerarEvento()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.ClearDomainEvents();

        comp.Emitir("12345678901234", DateOnly.FromDateTime(DateTime.Today.AddDays(30)), null);

        comp.Estado.Should().Be(EstadoComprobante.Emitido);
        comp.Cae.Should().Be("12345678901234");
        comp.DomainEvents.Should().ContainSingle();
        comp.DomainEvents.First().Should().BeOfType<ComprobanteEmitidoEvent>();
    }

    [Fact]
    public void Emitir_SinItems_DebeLanzarExcepcion()
    {
        var comp = CrearComprobante();

        var act = () => comp.Emitir(null, null, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*sin ítems*");
    }

    [Fact]
    public void Emitir_ComprobanteYaEmitido_DebeLanzarExcepcion()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null, null, null);

        var act = () => comp.Emitir(null, null, null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Anular_ComprobanteEmitido_DebeAnularYGenerarEvento()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null, null, null);
        comp.ClearDomainEvents();

        comp.Anular(null);

        comp.Estado.Should().Be(EstadoComprobante.Anulado);
        comp.DomainEvents.Should().ContainSingle();
        comp.DomainEvents.First().Should().BeOfType<ComprobanteAnuladoEvent>();
    }

    [Fact]
    public void Anular_ComprobanteYaAnulado_DebeLanzarExcepcion()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null, null, null);
        comp.Anular(null);

        var act = () => comp.Anular(null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AplicarPago_PagoTotal_DebeDejarSaldoCeroYEstadoPagado()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000m, 21m));
        comp.Emitir(null, null, null);

        comp.AplicarPago(comp.Total);

        comp.Saldo.Should().Be(0m);
        comp.Estado.Should().Be(EstadoComprobante.Pagado);
    }

    [Fact]
    public void AplicarPago_PagoParcial_DebeDejarSaldoPositivoYEstadoParcial()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000m, 21m));
        comp.Emitir(null, null, null);

        comp.AplicarPago(comp.Total / 2);

        comp.Saldo.Should().BeGreaterThan(0);
        comp.Estado.Should().Be(EstadoComprobante.PagadoParcial);
    }
}
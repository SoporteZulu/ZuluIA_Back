using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Events.Comprobantes;

namespace ZuluIA_Back.UnitTests.Domain;

public class ComprobanteTests
{
    private static Comprobante CrearComprobante() =>
        Comprobante.Crear(1, 1, 1, 1, 1, DateOnly.FromDateTime(DateTime.Today), null, 1, 1, 1m, string.Empty, null);

    private static ComprobanteItem CrearItem(long precio = 1000, long iva = 21) =>
        ComprobanteItem.Crear(0, 1, "Producto Test", 1m, precio, 0, 1, iva, 0, null, (short)1);

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
        comp.Emitir(null);

        var act = () => comp.AgregarItem(CrearItem());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Emitir_ConItems_DebeEmitirYGenerarEvento()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.ClearDomainEvents();

        comp.Emitir(null);

        comp.Estado.Should().Be(EstadoComprobante.Emitido);
        comp.Cae.Should().Be("12345678901234");
        comp.DomainEvents.Should().ContainSingle();
        comp.DomainEvents.First().Should().BeOfType<ComprobanteEmitidoEvent>();
    }

    [Fact]
    public void Emitir_SinItems_DebeLanzarExcepcion()
    {
        var comp = CrearComprobante();

        var act = () => comp.Emitir(null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*sin ítems*");
    }

    [Fact]
    public void Emitir_ComprobanteYaEmitido_DebeLanzarExcepcion()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null);

        var act = () => comp.Emitir(null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Anular_ComprobanteEmitido_DebeAnularYGenerarEvento()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem());
        comp.Emitir(null);
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
        comp.Emitir(null);
        comp.Anular(null);

        var act = () => comp.Anular(null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AplicarPago_PagoTotal_DebeDejarSaldoCeroYEstadoPagado()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 21));
        comp.Emitir(null);

        comp.AplicarPago(comp.Total);

        comp.Saldo.Should().Be(0m);
        comp.Estado.Should().Be(EstadoComprobante.Pagado);
    }

    [Fact]
    public void AplicarPago_PagoParcial_DebeDejarSaldoPositivoYEstadoParcial()
    {
        var comp = CrearComprobante();
        comp.AgregarItem(CrearItem(1000, 21));
        comp.Emitir(null);

        comp.AplicarPago(comp.Total / 2);

        comp.Saldo.Should().BeGreaterThan(0);
        comp.Estado.Should().Be(EstadoComprobante.PagadoParcial);
    }
}
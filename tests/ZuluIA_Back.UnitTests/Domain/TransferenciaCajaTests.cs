using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.UnitTests.Domain;

public class TransferenciaCajaTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static TransferenciaCaja RegistrarValida() =>
        TransferenciaCaja.Registrar(1L, 10L, 20L, Hoy, 5_000m, 1L, 1m, "Traspaso", null);

    // ── Registrar ────────────────────────────────────────────────────────────

    [Fact]
    public void Registrar_ConDatosValidos_DebeRegistrar()
    {
        var t = RegistrarValida();

        t.SucursalId.Should().Be(1L);
        t.CajaOrigenId.Should().Be(10L);
        t.CajaDestinoId.Should().Be(20L);
        t.Importe.Should().Be(5_000m);
        t.Anulada.Should().BeFalse();
    }

    [Fact]
    public void Registrar_OrigenIgualDestino_DebeLanzarExcepcion()
    {
        var act = () => TransferenciaCaja.Registrar(1L, 10L, 10L, Hoy, 5_000m, 1L, 1m, null, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*misma*");
    }

    [Fact]
    public void Registrar_ImporteCero_DebeLanzarExcepcion()
    {
        var act = () => TransferenciaCaja.Registrar(1L, 10L, 20L, Hoy, 0m, 1L, 1m, null, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*mayor a cero*");
    }

    [Fact]
    public void Registrar_ImporteNegativo_DebeLanzarExcepcion()
    {
        var act = () => TransferenciaCaja.Registrar(1L, 10L, 20L, Hoy, -1m, 1L, 1m, null, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*mayor a cero*");
    }

    [Fact]
    public void Registrar_CotizacionCeroONegativa_DebeClamearAUno()
    {
        var t = TransferenciaCaja.Registrar(1L, 10L, 20L, Hoy, 500m, 1L, 0m, null, null);

        t.Cotizacion.Should().Be(1m);
    }

    [Fact]
    public void Registrar_CotizacionNegativa_DebeClamearAUno()
    {
        var t = TransferenciaCaja.Registrar(1L, 10L, 20L, Hoy, 500m, 1L, -5m, null, null);

        t.Cotizacion.Should().Be(1m);
    }

    // ── Anular ───────────────────────────────────────────────────────────────

    [Fact]
    public void Anular_PrimeraVez_DebeAnularTransferencia()
    {
        var t = RegistrarValida();

        t.Anular(null);

        t.Anulada.Should().BeTrue();
    }

    [Fact]
    public void Anular_YaAnulada_DebeLanzarExcepcion()
    {
        var t = RegistrarValida();
        t.Anular(null);

        var act = () => t.Anular(null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*ya está anulada*");
    }

    // ── AsignarMovimientos ───────────────────────────────────────────────────

    [Fact]
    public void AsignarMovimientos_DebeAsignarAmbosIds()
    {
        var t = RegistrarValida();

        t.AsignarMovimientos(100L, 200L);

        t.MovimientoOrigenId.Should().Be(100L);
        t.MovimientoDestinoId.Should().Be(200L);
    }
}

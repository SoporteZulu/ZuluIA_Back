using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

/// <summary>
/// Tests for Cobro + CobroMedio and Pago + PagoMedio domain entities.
/// These are the core financial collection/disbursement entities.
/// </summary>
public class CobroPagoTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    // ─── CobroMedio ──────────────────────────────────────────────────────────

    [Fact]
    public void CobroMedio_Crear_ConDatosValidos_DebeCrear()
    {
        var medio = CobroMedio.Crear(1L, 10L, 5L, null, 1000m, 1L, 1m);

        medio.CobroId.Should().Be(1L);
        medio.CajaId.Should().Be(10L);
        medio.FormaPagoId.Should().Be(5L);
        medio.Importe.Should().Be(1000m);
    }

    [Fact]
    public void CobroMedio_Crear_ImporteCero_DebeLanzarExcepcion()
    {
        var act = () => CobroMedio.Crear(1L, 10L, 5L, null, 0m, 1L, 1m);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*mayor a 0*");
    }

    [Fact]
    public void CobroMedio_Crear_ImporteNegativo_DebeLanzarExcepcion()
    {
        var act = () => CobroMedio.Crear(1L, 10L, 5L, null, -500m, 1L, 1m);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*mayor a 0*");
    }

    [Fact]
    public void CobroMedio_Crear_CotizacionCeroONegativa_DebeClamearAUno()
    {
        var medio = CobroMedio.Crear(1L, 10L, 5L, null, 500m, 1L, 0m);

        medio.Cotizacion.Should().Be(1m);
    }

    // ─── Cobro ───────────────────────────────────────────────────────────────

    [Fact]
    public void Cobro_Crear_ConDatosValidos_DebeCrearActivo()
    {
        var cobro = Cobro.Crear(1L, 2L, Hoy, 1L, 1m, "Obs", null);

        cobro.SucursalId.Should().Be(1L);
        cobro.TerceroId.Should().Be(2L);
        cobro.Estado.Should().Be(EstadoCobro.Activo);
        cobro.Total.Should().Be(0m);
        cobro.Medios.Should().BeEmpty();
    }

    [Fact]
    public void Cobro_Crear_CotizacionCeroONegativa_DebeClamearAUno()
    {
        var cobro = Cobro.Crear(1L, 2L, Hoy, 1L, -1m, null, null);

        cobro.Cotizacion.Should().Be(1m);
    }

    [Fact]
    public void Cobro_AgregarMedio_DebeActualizarTotal()
    {
        var cobro = Cobro.Crear(1L, 2L, Hoy, 1L, 1m, null, null);
        var medio = CobroMedio.Crear(cobro.Id, 10L, 5L, null, 500m, 1L, 2m);

        cobro.AgregarMedio(medio);

        cobro.Total.Should().Be(1000m);   // 500 * 2 cotizacion
        cobro.Medios.Should().HaveCount(1);
    }

    [Fact]
    public void Cobro_AgregarMedio_CobroAnulado_DebeLanzarExcepcion()
    {
        var cobro = Cobro.Crear(1L, 2L, Hoy, 1L, 1m, null, null);
        cobro.Anular(null);

        var medio = CobroMedio.Crear(cobro.Id, 10L, 5L, null, 500m, 1L, 1m);
        var act = () => cobro.AgregarMedio(medio);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*inactivo*");
    }

    [Fact]
    public void Cobro_AgregarVariosMedios_DebeAcumularTotal()
    {
        var cobro = Cobro.Crear(1L, 2L, Hoy, 1L, 1m, null, null);
        cobro.AgregarMedio(CobroMedio.Crear(cobro.Id, 10L, 5L, null, 400m, 1L, 1m));
        cobro.AgregarMedio(CobroMedio.Crear(cobro.Id, 10L, 5L, null, 600m, 1L, 1m));

        cobro.Total.Should().Be(1000m);
        cobro.Medios.Should().HaveCount(2);
    }

    [Fact]
    public void Cobro_Anular_PrimeraVez_DebeAnular()
    {
        var cobro = Cobro.Crear(1L, 2L, Hoy, 1L, 1m, null, null);

        cobro.Anular(null);

        cobro.Estado.Should().Be(EstadoCobro.Anulado);
    }

    [Fact]
    public void Cobro_Anular_YaAnulado_DebeLanzarExcepcion()
    {
        var cobro = Cobro.Crear(1L, 2L, Hoy, 1L, 1m, null, null);
        cobro.Anular(null);

        var act = () => cobro.Anular(null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*ya está anulado*");
    }

    [Fact]
    public void Cobro_AsignarCierre_DebeSetearNroCierre()
    {
        var cobro = Cobro.Crear(1L, 2L, Hoy, 1L, 1m, null, null);

        cobro.AsignarCierre(7, null);

        cobro.NroCierre.Should().Be(7);
    }

    // ─── PagoMedio ───────────────────────────────────────────────────────────

    [Fact]
    public void PagoMedio_Crear_ConDatosValidos_DebeCrear()
    {
        var medio = PagoMedio.Crear(1L, 10L, 5L, null, 1500m, 1L, 1m);

        medio.PagoId.Should().Be(1L);
        medio.CajaId.Should().Be(10L);
        medio.FormaPagoId.Should().Be(5L);
        medio.Importe.Should().Be(1500m);
    }

    [Fact]
    public void PagoMedio_Crear_ImporteCero_DebeLanzarExcepcion()
    {
        var act = () => PagoMedio.Crear(1L, 10L, 5L, null, 0m, 1L, 1m);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*mayor a 0*");
    }

    [Fact]
    public void PagoMedio_Crear_CotizacionCeroONegativa_DebeClamearAUno()
    {
        var medio = PagoMedio.Crear(1L, 10L, 5L, null, 500m, 1L, -3m);

        medio.Cotizacion.Should().Be(1m);
    }

    // ─── Pago ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Pago_Crear_ConDatosValidos_DebeCrearActivo()
    {
        var pago = Pago.Crear(1L, 2L, Hoy, 1L, 1m, "Obs pago", null);

        pago.SucursalId.Should().Be(1L);
        pago.TerceroId.Should().Be(2L);
        pago.Estado.Should().Be(EstadoPago.Activo);
        pago.Total.Should().Be(0m);
        pago.Medios.Should().BeEmpty();
    }

    [Fact]
    public void Pago_Crear_CotizacionCeroONegativa_DebeClamearAUno()
    {
        var pago = Pago.Crear(1L, 2L, Hoy, 1L, 0m, null, null);

        pago.Cotizacion.Should().Be(1m);
    }

    [Fact]
    public void Pago_AgregarMedio_DebeActualizarTotal()
    {
        var pago = Pago.Crear(1L, 2L, Hoy, 1L, 1m, null, null);
        var medio = PagoMedio.Crear(pago.Id, 10L, 5L, null, 800m, 1L, 1.5m);

        pago.AgregarMedio(medio);

        pago.Total.Should().Be(1200m);   // 800 * 1.5 cotizacion
        pago.Medios.Should().HaveCount(1);
    }

    [Fact]
    public void Pago_AgregarMedio_PagoAnulado_DebeLanzarExcepcion()
    {
        var pago = Pago.Crear(1L, 2L, Hoy, 1L, 1m, null, null);
        pago.Anular(null);

        var medio = PagoMedio.Crear(pago.Id, 10L, 5L, null, 500m, 1L, 1m);
        var act = () => pago.AgregarMedio(medio);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*inactivo*");
    }

    [Fact]
    public void Pago_Anular_PrimeraVez_DebeAnular()
    {
        var pago = Pago.Crear(1L, 2L, Hoy, 1L, 1m, null, null);

        pago.Anular(null);

        pago.Estado.Should().Be(EstadoPago.Anulado);
    }

    [Fact]
    public void Pago_Anular_YaAnulado_DebeLanzarExcepcion()
    {
        var pago = Pago.Crear(1L, 2L, Hoy, 1L, 1m, null, null);
        pago.Anular(null);

        var act = () => pago.Anular(null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*ya está anulado*");
    }
}

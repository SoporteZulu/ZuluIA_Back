using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

/// <summary>
/// Tests for CajaCuentaBancaria and Cheque domain entities.
/// Covers cash register lifecycle and payment instrument rules.
/// </summary>
public class CajaChequeTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    // ─── CajaCuentaBancaria ──────────────────────────────────────────────────

    private static CajaCuentaBancaria CrearCaja() =>
        CajaCuentaBancaria.Crear(1L, 1L, "Caja Principal", 1L, true, null, null);

    [Fact]
    public void CajaCuentaBancaria_Crear_ConDatosValidos_DebeCrear()
    {
        var caja = CrearCaja();

        caja.SucursalId.Should().Be(1L);
        caja.Descripcion.Should().Be("Caja Principal");
        caja.Activa.Should().BeTrue();
        caja.EstaAbierta.Should().BeFalse();
        caja.NroCierreActual.Should().Be(0);
    }

    [Fact]
    public void CajaCuentaBancaria_Crear_DescripcionVacia_DebeLanzarExcepcion()
    {
        var act = () => CajaCuentaBancaria.Crear(1L, 1L, "   ", 1L, true, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CajaCuentaBancaria_AbrirCaja_DebeHabilitarOperacion()
    {
        var caja = CrearCaja();

        caja.AbrirCaja(Hoy, 1000m, null);

        caja.EstaAbierta.Should().BeTrue();
        caja.FechaUltimaApertura.Should().Be(Hoy);
        caja.SaldoApertura.Should().Be(1000m);
    }

    [Fact]
    public void CajaCuentaBancaria_AbrirCaja_YaAbierta_DebeLanzarExcepcion()
    {
        var caja = CrearCaja();
        caja.AbrirCaja(Hoy, 500m, null);

        var act = () => caja.AbrirCaja(Hoy, 200m, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*ya está abierta*");
    }

    [Fact]
    public void CajaCuentaBancaria_CerrarArqueo_IncrementaNroCierre()
    {
        var caja = CrearCaja();
        caja.AbrirCaja(Hoy, 0m, null);

        var nro = caja.CerrarArqueo(null);

        nro.Should().Be(1);
        caja.NroCierreActual.Should().Be(1);
        caja.EstaAbierta.Should().BeFalse();
    }

    [Fact]
    public void CajaCuentaBancaria_CerrarArqueo_SinAbrir_DebeLanzarExcepcion()
    {
        var caja = CrearCaja();

        var act = () => caja.CerrarArqueo(null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*no está abierta*");
    }

    [Fact]
    public void CajaCuentaBancaria_AbrirCerrarAbrir_DebePermitirCicloRepetido()
    {
        var caja = CrearCaja();

        caja.AbrirCaja(Hoy, 0m, null);
        caja.CerrarArqueo(null);
        caja.AbrirCaja(Hoy, 0m, null);  // segundo ciclo

        caja.EstaAbierta.Should().BeTrue();
        caja.NroCierreActual.Should().Be(1);
    }

    [Fact]
    public void CajaCuentaBancaria_Desactivar_DebeDesactivar()
    {
        var caja = CrearCaja();

        caja.Desactivar(null);

        caja.Activa.Should().BeFalse();
    }

    [Fact]
    public void CajaCuentaBancaria_Activar_DebeActivarNuevamente()
    {
        var caja = CrearCaja();
        caja.Desactivar(null);

        caja.Activar(null);

        caja.Activa.Should().BeTrue();
    }

    // ─── Cheque ──────────────────────────────────────────────────────────────

    private static Cheque CrearChequeValido() =>
        Cheque.Crear(1L, null, "00001234", "Banco Nacional", Hoy, Hoy.AddDays(30), 5000m, 1L, null, null);

    [Fact]
    public void Cheque_Crear_ConDatosValidos_DebeQuedarEnCartera()
    {
        var cheque = CrearChequeValido();

        cheque.NroCheque.Should().Be("00001234");
        cheque.Banco.Should().Be("Banco Nacional");
        cheque.Importe.Should().Be(5000m);
        cheque.Estado.Should().Be(EstadoCheque.Cartera);
    }

    [Fact]
    public void Cheque_Crear_ImporteCero_DebeLanzarExcepcion()
    {
        var act = () => Cheque.Crear(1L, null, "00001234", "Banco Nacional",
            Hoy, Hoy.AddDays(30), 0m, 1L, null, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*mayor a 0*");
    }

    [Fact]
    public void Cheque_Crear_FechaVencimientoAnteriorAEmision_DebeLanzarExcepcion()
    {
        var act = () => Cheque.Crear(1L, null, "00001234", "Banco Nacional",
            Hoy, Hoy.AddDays(-1), 500m, 1L, null, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*no puede ser anterior*");
    }

    [Fact]
    public void Cheque_Crear_NroChequeVacio_DebeLanzarExcepcion()
    {
        var act = () => Cheque.Crear(1L, null, "  ", "Banco Nacional",
            Hoy, Hoy.AddDays(30), 500m, 1L, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Cheque_Depositar_DesdeBancoCartera_DebeDepositarse()
    {
        var cheque = CrearChequeValido();

        cheque.Depositar(Hoy, Hoy.AddDays(3), null);

        cheque.Estado.Should().Be(EstadoCheque.Depositado);
        cheque.FechaDeposito.Should().Be(Hoy);
    }

    [Fact]
    public void Cheque_Depositar_NoDesdeCartera_DebeLanzarExcepcion()
    {
        var cheque = CrearChequeValido();
        cheque.Depositar(Hoy, null, null);

        var act = () => cheque.Depositar(Hoy, null, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cartera*");
    }

    [Fact]
    public void Cheque_Acreditar_DesdeDepositado_DebeAcreditarse()
    {
        var cheque = CrearChequeValido();
        cheque.Depositar(Hoy, null, null);

        cheque.Acreditar(Hoy.AddDays(3), null);

        cheque.Estado.Should().Be(EstadoCheque.Acreditado);
    }

    [Fact]
    public void Cheque_Acreditar_NoDesdeDepositado_DebeLanzarExcepcion()
    {
        var cheque = CrearChequeValido();

        var act = () => cheque.Acreditar(Hoy.AddDays(3), null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*depositados*");
    }

    [Fact]
    public void Cheque_Rechazar_DebeRechazarse()
    {
        var cheque = CrearChequeValido();
        cheque.Depositar(Hoy, null, null);

        cheque.Rechazar("Sin fondos", null);

        cheque.Estado.Should().Be(EstadoCheque.Rechazado);
    }

    [Fact]
    public void Cheque_Rechazar_YaRechazado_DebeLanzarExcepcion()
    {
        var cheque = CrearChequeValido();
        cheque.Depositar(Hoy, null, null);
        cheque.Rechazar(null, null);

        var act = () => cheque.Rechazar(null, null);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*ya está rechazado*");
    }

    [Fact]
    public void Cheque_Entregar_DesdeBancoCartera_DebeEntregarse()
    {
        var cheque = CrearChequeValido();

        cheque.Entregar(null, null, null);

        cheque.Estado.Should().Be(EstadoCheque.Entregado);
    }

    [Fact]
    public void Cheque_SetObservacion_ConEspacios_Recorta()
    {
        var cheque = CrearChequeValido();
        cheque.SetObservacion("  nota  ");
        cheque.Observacion.Should().Be("nota");
    }

    [Fact]
    public void Cheque_SetObservacion_Null_AsignaNulo()
    {
        var cheque = CrearChequeValido();
        cheque.SetObservacion(null);
        cheque.Observacion.Should().BeNull();
    }
}

using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class ChequeTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static Cheque CrearChequeTercero() =>
        Cheque.Crear(1, null, "123", "Banco Test", Hoy, Hoy.AddDays(10), 100m, 1,
            TipoCheque.Tercero, true, false, "Titular Test", null, null, null, null, null, null);

    [Fact]
    public void Crear_ConDatosValidos_DebeIniciarEnCartera()
    {
        var cheque = CrearChequeTercero();

        cheque.Estado.Should().Be(EstadoCheque.Cartera);
    }

    [Fact]
    public void Depositar_DesdeCartera_DebeQuedarDepositado()
    {
        var cheque = CrearChequeTercero();

        cheque.Depositar(Hoy, Hoy.AddDays(2), null);

        cheque.Estado.Should().Be(EstadoCheque.Depositado);
        cheque.FechaDeposito.Should().Be(Hoy);
    }

    [Fact]
    public void Acreditar_DesdeDepositado_DebeQuedarAcreditado()
    {
        var cheque = CrearChequeTercero();
        cheque.Depositar(Hoy, Hoy.AddDays(2), null);

        cheque.Acreditar(Hoy.AddDays(2), null);

        cheque.Estado.Should().Be(EstadoCheque.Acreditado);
    }

    [Fact]
    public void Entregar_ConTercero_DebeQuedarEntregadoYAsignarTercero()
    {
        var cheque = CrearChequeTercero();

        cheque.Entregar(55, "Entrega a proveedor", null);

        cheque.Estado.Should().Be(EstadoCheque.Entregado);
        cheque.TerceroId.Should().Be(55);
        cheque.Observacion.Should().Be("Entrega a proveedor");
    }

    [Fact]
    public void Rechazar_DebeQuedarRechazadoYGuardarConcepto()
    {
        var cheque = CrearChequeTercero();

        cheque.Rechazar("Sin fondos", "Observación", null);

        cheque.Estado.Should().Be(EstadoCheque.Rechazado);
        cheque.ConceptoRechazo.Should().Be("Sin fondos");
    }

    [Fact]
    public void Endosar_AlaOrden_DebeQuedarEndosado()
    {
        var cheque = CrearChequeTercero();

        cheque.Endosar(77, "Endoso a tercero", null);

        cheque.Estado.Should().Be(EstadoCheque.Endosado);
        cheque.TerceroId.Should().Be(77);
    }

    [Fact]
    public void Anular_ChequePropioEnCartera_DebeQuedarAnulado()
    {
        var cheque = Cheque.Crear(1, null, "999", "Banco Test", Hoy, Hoy.AddDays(10), 100m, 1,
            TipoCheque.Propio, false, false, null, null, null, 5, null, null, null);

        cheque.Anular("Error de emisión", null);

        cheque.Estado.Should().Be(EstadoCheque.Anulado);
        cheque.Observacion.Should().Contain("ANULADO");
    }
}

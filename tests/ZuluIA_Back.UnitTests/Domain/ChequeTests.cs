using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class ChequeTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    private static Cheque CrearCheque() =>
        Cheque.Crear(1, null, "123", "Banco Test", Hoy, Hoy.AddDays(10), 100m, 1, null, null);

    [Fact]
    public void Crear_ConDatosValidos_DebeIniciarEnCartera()
    {
        var cheque = CrearCheque();

        cheque.Estado.Should().Be(EstadoCheque.Cartera);
    }

    [Fact]
    public void Depositar_DesdeCartera_DebeQuedarDepositado()
    {
        var cheque = CrearCheque();

        cheque.Depositar(Hoy, Hoy.AddDays(2), null);

        cheque.Estado.Should().Be(EstadoCheque.Depositado);
        cheque.FechaDeposito.Should().Be(Hoy);
    }

    [Fact]
    public void Acreditar_DesdeDepositado_DebeQuedarAcreditado()
    {
        var cheque = CrearCheque();
        cheque.Depositar(Hoy, Hoy.AddDays(2), null);

        cheque.Acreditar(Hoy.AddDays(2), null);

        cheque.Estado.Should().Be(EstadoCheque.Acreditado);
    }

    [Fact]
    public void Entregar_ConTercero_DebeQuedarEntregadoYAsignarTercero()
    {
        var cheque = CrearCheque();

        cheque.Entregar(55, "Entrega a proveedor", null);

        cheque.Estado.Should().Be(EstadoCheque.Entregado);
        cheque.TerceroId.Should().Be(55);
        cheque.Observacion.Should().Be("Entrega a proveedor");
    }

    [Fact]
    public void Rechazar_DebeQuedarRechazado()
    {
        var cheque = CrearCheque();

        cheque.Rechazar("Sin fondos", null);

        cheque.Estado.Should().Be(EstadoCheque.Rechazado);
    }
}

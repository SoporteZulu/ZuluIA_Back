using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class TesoreriaMovimientoTests
{
    [Fact]
    public void Registrar_ConDatosValidos_DebeCrearMovimientoActivo()
    {
        var movimiento = TesoreriaMovimiento.Registrar(
            1, 2, DateOnly.FromDateTime(DateTime.Today),
            TipoOperacionTesoreria.Vale,
            SentidoMovimientoTesoreria.Egreso,
            100m, 1, 1m, null, "VALE", null, "Caja chica", null);

        movimiento.Anulado.Should().BeFalse();
        movimiento.Importe.Should().Be(100m);
    }

    [Fact]
    public void Anular_PrimeraVez_DebeMarcarloAnulado()
    {
        var movimiento = TesoreriaMovimiento.Registrar(
            1, 2, DateOnly.FromDateTime(DateTime.Today),
            TipoOperacionTesoreria.Reintegro,
            SentidoMovimientoTesoreria.Ingreso,
            100m, 1, 1m, null, "REINTEGRO", null, null, null);

        movimiento.Anular(null);

        movimiento.Anulado.Should().BeTrue();
    }
}

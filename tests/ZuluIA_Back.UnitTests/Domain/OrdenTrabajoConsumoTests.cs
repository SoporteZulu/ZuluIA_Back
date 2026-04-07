using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Produccion;

namespace ZuluIA_Back.UnitTests.Domain;

public class OrdenTrabajoConsumoTests
{
    [Fact]
    public void Registrar_DebePersistirCantidades()
    {
        var consumo = OrdenTrabajoConsumo.Registrar(1, 2, 3, 10m, 9.5m, 4, null, null);

        consumo.CantidadPlanificada.Should().Be(10m);
        consumo.CantidadConsumida.Should().Be(9.5m);
    }
}

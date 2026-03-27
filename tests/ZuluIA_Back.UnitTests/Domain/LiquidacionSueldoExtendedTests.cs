using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.RRHH;

namespace ZuluIA_Back.UnitTests.Domain;

public class LiquidacionSueldoExtendedTests
{
    [Fact]
    public void RegistrarImputacion_Parcial_DebeActualizarSaldo()
    {
        var liq = LiquidacionSueldo.Crear(1, 1, 2025, 1, 100m, 150m, 20m, 1, null);
        liq.RegistrarImputacion(50m, new DateOnly(2025, 1, 31));
        liq.ImporteImputado.Should().Be(50m);
        liq.SaldoPendiente.Should().Be(80m);
        liq.Pagada.Should().BeFalse();
    }

    [Fact]
    public void Actualizar_SinImputaciones_DebeRecalcularNeto()
    {
        var liq = LiquidacionSueldo.Crear(1, 1, 2025, 1, 100m, 150m, 20m, 1, null);

        liq.Actualizar(120m, 180m, 30m, 1, "ok");

        liq.Neto.Should().Be(150m);
        liq.SueldoBasico.Should().Be(120m);
    }
}

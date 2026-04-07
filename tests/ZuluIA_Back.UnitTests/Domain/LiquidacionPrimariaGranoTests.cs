using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Fiscal;

namespace ZuluIA_Back.UnitTests.Domain;

public class LiquidacionPrimariaGranoTests
{
    [Fact]
    public void Crear_DebeCalcularTotal()
    {
        var liquidacion = LiquidacionPrimariaGrano.Crear(1, DateOnly.FromDateTime(DateTime.Today), "LPG-1", "Soja", 10m, 15m, null, null);
        liquidacion.Total.Should().Be(150m);
    }
}

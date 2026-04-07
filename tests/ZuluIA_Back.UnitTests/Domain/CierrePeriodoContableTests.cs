using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Fiscal;

namespace ZuluIA_Back.UnitTests.Domain;

public class CierrePeriodoContableTests
{
    [Fact]
    public void Crear_DebeGuardarRango()
    {
        var cierre = CierrePeriodoContable.Crear(1, 2, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), null, null);
        cierre.EjercicioId.Should().Be(1);
        cierre.SucursalId.Should().Be(2);
    }
}

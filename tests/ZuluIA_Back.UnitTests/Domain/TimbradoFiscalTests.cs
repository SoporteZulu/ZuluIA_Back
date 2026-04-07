using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.UnitTests.Domain;

public class TimbradoFiscalTests
{
    [Fact]
    public void VigentePara_DebeResponderSegunRango()
    {
        var timbrado = TimbradoFiscal.Crear(1, 2, "TIM-1", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), null, null);
        timbrado.VigentePara(new DateOnly(2025, 1, 15)).Should().BeTrue();
        timbrado.VigentePara(new DateOnly(2025, 2, 1)).Should().BeFalse();
    }

    [Fact]
    public void Desactivar_DebeInactivarTimbrado()
    {
        var timbrado = TimbradoFiscal.Crear(1, 2, "TIM-1", new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), null, null);

        timbrado.Desactivar("obs", null);

        timbrado.Activo.Should().BeFalse();
    }
}

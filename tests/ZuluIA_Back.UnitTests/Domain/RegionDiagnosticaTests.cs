using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Geografia;

namespace ZuluIA_Back.UnitTests.Domain;

public class RegionDiagnosticaTests
{
    [Fact]
    public void Crear_DebeNormalizarCodigo()
    {
        var region = RegionDiagnostica.Crear(" rg-1 ", "Región 1", null);
        region.Codigo.Should().Be("RG-1");
    }
}

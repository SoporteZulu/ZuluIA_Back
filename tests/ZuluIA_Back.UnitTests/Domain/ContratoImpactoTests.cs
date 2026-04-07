using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Contratos;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

public class ContratoImpactoTests
{
    [Fact]
    public void Registrar_DebeGuardarTipoEImporte()
    {
        var impacto = ContratoImpacto.Registrar(1, TipoImpactoContrato.Financiero, new DateOnly(2025, 1, 1), 100m, "Impacto", null);
        impacto.Tipo.Should().Be(TipoImpactoContrato.Financiero);
        impacto.Importe.Should().Be(100m);
    }
}
